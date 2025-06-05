//-----------------------------------------------------------------------
// <copyright file = "ResourceManagementCategory.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Category which manages resources.
    /// </summary>
    internal class ResourceManagementCategory : CategoryBase<CsiResource>
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// Collection of devices and their status.
        /// </summary>
        private readonly Dictionary<DeviceIdentifier, DeviceStatus> requestedDevices =
            new Dictionary<DeviceIdentifier, DeviceStatus>();

        /// <summary>
        /// Collection of groups of each device and their status.
        /// </summary>
        private readonly Dictionary<DeviceIdentifier, Dictionary<uint, DeviceStatus>> requestedGroups =
            new Dictionary<DeviceIdentifier, Dictionary<uint, DeviceStatus>>();

        /// <summary>
        /// Cached list of connected devices.  Populated via <see cref="GetConnectedDevicesWithGroups"/>.
        /// </summary>
        private readonly Dictionary<DeviceIdentifier, ConnectedDevice> connectedDevicesCache =
            new Dictionary<DeviceIdentifier, ConnectedDevice>();

        /// <summary>
        /// Dummy object for thread safe transactions on list of connected devices.
        /// The list object itself is not used for locking because new memory get assigned
        /// to the list and that could break the game.
        /// </summary>
        private static object connectedDevicesLockerObject;

        /// <summary>
        /// Enumeration for stating different status of devices.
        /// </summary>
        private enum DeviceStatus
        {
            Requested,
            Acquired,
            NotInUse
        }

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="ResourceManagementCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// This is cached for internal use.
        /// </param>
        public ResourceManagementCategory(FoundationTarget target)
        {
            foundationTarget = target;

            eventHandlers[typeof(DeviceAcquiredEvent)] =
                message => HandleDeviceAcquiredEvent(message as DeviceAcquiredEvent);

            eventHandlers[typeof(DeviceReleasedEvent)] =
                message => HandleDeviceReleasedEvent(message as DeviceReleasedEvent);

            eventHandlers[typeof(DeviceConnectedEvent)] =
                message => HandleDeviceConnectedEvent(message as DeviceConnectedEvent);

            eventHandlers[typeof(DeviceRemovedEvent)] =
                message => HandleDeviceRemovedEvent(message as DeviceRemovedEvent);

            connectedDevicesLockerObject = new object();
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Event which is fired when a device is acquired.
        /// </summary>
        public event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// Event which is fired when a device is released.
        /// </summary>
        public event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <summary>
        /// Event which is fired when a device is released.
        /// </summary>
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <summary>
        /// Event which is fired when a device is connected.
        /// </summary>
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        #endregion Events

        #region Private Methods

        /// <summary>
        /// Clears the collection of connected devices, invalidating the cache.
        /// </summary>
        /// <remarks>
        /// Invalidate the cache, ensuring any subsequent call to <see cref="GetConnectedDevicesWithGroups"/>
        /// will query the foundation for an up-to-date list of connected devices.
        /// </remarks>
        private void ClearConnectedDeviceCache()
        {
            lock(connectedDevicesLockerObject)
            {
                connectedDevicesCache.Clear();
            }
        }

        /// <summary>
        /// Compare the device type against the version, in case the
        /// ResourceManagementCategory version doesn't support the device.
        /// </summary>
        /// <param name="deviceType">The <see cref="DeviceType"/> to validate.</param>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void ValidateDeviceType(DeviceType deviceType)
        {
            // Currently, MechanicalBell is the only device that requires special handling.
            // The MechanicalBell is available in version 1.13 or later, but due to temporary
            // logic in Ascent Series J, VersionMinor may return something less than 13.
            if((deviceType == DeviceType.MechanicalBell) && (VersionMinor < 13))
            {
                throw new ArgumentException(
                    $"DeviceType {DeviceType.MechanicalBell} requires ResourceManagementCategory version 13 or higher.  Please check your foundation target.");
            }
        }

        /// <summary>
        /// Get the reason that a device was not acquired.
        /// </summary>
        /// <param name="givenReason">Reason from the CSI manager..</param>
        /// <returns>Reason the device was not acquired.</returns>
        private static DeviceAcquisitionFailureReason GetDeviceNotAcquiredReason(DeviceNotAcquired givenReason)
        {
            var reason = DeviceAcquisitionFailureReason.DeviceNotConnected;
            switch(givenReason)
            {
                case DeviceNotAcquired.DeviceDisconnected:
                    break;

                case DeviceNotAcquired.InQueue:
                    reason = DeviceAcquisitionFailureReason.RequestQueued;
                    break;
            }
            return reason;
        }

        /// <summary>
        /// Handle a device acquired event.
        /// </summary>
        /// <param name="deviceAcquiredEvent">Information about the acquired device.</param>
        private void HandleDeviceAcquiredEvent(DeviceAcquiredEvent deviceAcquiredEvent)
        {
            if(DeviceAcquiredEvent != null)
            {
                var identifier = new DeviceIdentifier(deviceAcquiredEvent.DeviceType,
                                                              deviceAcquiredEvent.DeviceId);

                if(deviceAcquiredEvent.GroupList.Count > 0)
                {
                    lock(requestedGroups)
                    {
                        foreach(var group in deviceAcquiredEvent.GroupList)
                        {
                            if(requestedGroups.ContainsKey(identifier) && requestedGroups[identifier].ContainsKey(group))
                            {
                                requestedGroups[identifier][group] = DeviceStatus.Acquired;
                            }
                        }
                    }
                }
                else
                {
                    lock(requestedDevices)
                    {
                        requestedDevices[identifier] = DeviceStatus.Acquired;
                    }
                }

                DeviceAcquiredEvent(this, new DeviceAcquiredEventArgs(deviceAcquiredEvent.DeviceType, deviceAcquiredEvent.DeviceId,
                    deviceAcquiredEvent.GroupList));
            }
        }

        /// <summary>
        /// Handle a device released event.
        /// </summary>
        /// <param name="deviceReleasedEvent">Information about the released device.</param>
        private void HandleDeviceReleasedEvent(DeviceReleasedEvent deviceReleasedEvent)
        {
            if(DeviceReleasedEvent != null)
            {
                var identifier = new DeviceIdentifier(deviceReleasedEvent.DeviceType,
                                                              deviceReleasedEvent.DeviceId);

                if(deviceReleasedEvent.GroupList.Count > 0)
                {
                    lock(requestedGroups)
                    {
                        foreach(var group in deviceReleasedEvent.GroupList)
                        {
                            if(requestedGroups.ContainsKey(identifier) && requestedGroups[identifier].ContainsKey(group))
                            {
                                requestedGroups[identifier][group] = DeviceStatus.Requested;
                            }
                        }
                    }
                }
                else
                {
                    lock(requestedDevices)
                    {
                        requestedDevices[identifier] = DeviceStatus.Requested;
                    }
                }

                DeviceReleasedEvent(this,
                        new DeviceReleasedEventArgs(deviceReleasedEvent.DeviceType,
                                                    deviceReleasedEvent.DeviceId,
                                                    deviceReleasedEvent.GroupList,
                                                    GetDeviceNotAcquiredReason(deviceReleasedEvent.Reason)));
            }
        }

        /// <summary>
        /// Handle a device connected event.
        /// </summary>
        /// <param name="deviceConnectedEvent">Information about the connected device.</param>
        private void HandleDeviceConnectedEvent(DeviceConnectedEvent deviceConnectedEvent)
        {
            ClearConnectedDeviceCache();

            DeviceConnectedEvent?.Invoke(this,
                        new DeviceConnectedEventArgs(deviceConnectedEvent.DeviceType,
                                                     deviceConnectedEvent.DeviceId,
                                                     deviceConnectedEvent.GroupList));
        }

        /// <summary>
        /// Handle a device removed event.
        /// </summary>
        /// <param name="deviceRemovedEvent">Information about the removed device.</param>
        private void HandleDeviceRemovedEvent(DeviceRemovedEvent deviceRemovedEvent)
        {
            ClearConnectedDeviceCache();

            var identifier = new DeviceIdentifier(deviceRemovedEvent.DeviceType,
                                                      deviceRemovedEvent.DeviceId);
            lock(requestedDevices)
            {
                requestedDevices[identifier] = DeviceStatus.NotInUse;
            }

            lock(requestedGroups)
            {
                requestedGroups.Remove(identifier);
            }

            DeviceRemovedEvent?.Invoke(this, new DeviceRemovedEventArgs(deviceRemovedEvent.DeviceType, deviceRemovedEvent.DeviceId));
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetResourceResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();
            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="ResourceManagementCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(ResourceResponse response)
        {
            if(response.ErrorCode != ResourceErrorCode.NONE)
            {
                throw new ResourceManagementCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        /// <summary>
        /// Make a request to acquire a device.
        /// </summary>
        /// <param name="request">The request that contains the device information.</param>
        /// <returns>Response to the request.</returns>
        private AcquireDeviceResponse SendAcquireDeviceRequest(CsiResource request)
        {
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<AcquireDeviceResponse>();
            CheckResponse(response.ResourceResponse);
            return response;
        }

        /// <summary>
        /// Converts <see cref="GroupAcquiredStatus"/> to <see cref="GroupAcquisitionStatus"/>
        /// </summary>
        /// <param name="groupStatus">The group status to convert from.</param>
        /// <returns>The <see cref="GroupAcquisitionStatus"/> equivalent of the parameter.</returns>
        private static GroupAcquisitionStatus GetGroupAcquisitionStatus(GroupAcquiredStatus groupStatus)
        {
            var status = GroupAcquisitionStatus.Acquired;
            switch(groupStatus)
            {
                case GroupAcquiredStatus.Acquired:
                    status = GroupAcquisitionStatus.Acquired;
                    break;

                case GroupAcquiredStatus.InQueue:
                    status = GroupAcquisitionStatus.RequestQueued;
                    break;
            }

            return status;
        }

        #endregion Private Methods

        #region Overrides of CategoryBase<CsiResource>

        /// <inheritdoc/>
        public override Category Category => Category.CsiResource;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentS1Series))
                {
                    return 17;
                }
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentN01Series))
                {
                    return 16;
                }
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds))
                {
                    return 15;
                }
                if(foundationTarget == FoundationTarget.AscentJSeriesMps)
                {
                    return 14;
                }
                if(foundationTarget == FoundationTarget.AscentHSeriesCds)
                {
                    return 13;
                }
                return 10;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var resourceManagementMessage = message as CsiResource;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(resourceManagementMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiResource), message.GetType()));
            }
            if(resourceManagementMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Resource management message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(resourceManagementMessage.Item.GetType()))
            {
                eventHandlers[resourceManagementMessage.Item.GetType()](resourceManagementMessage.Item);
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + resourceManagementMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var resourceManagementMessage = message as CsiResource;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(resourceManagementMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiResource), message.GetType()));
            }
            if(resourceManagementMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Resource management message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + resourceManagementMessage.Item.GetType());
        }

        #endregion Overrides of CategoryBase<CsiResource>

        #region Resource Methods

        /// <summary>
        /// Request control of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device.</param>
        /// <param name="deviceId">The device id of the device to acquire.</param>
        /// <param name="priority">The client priority of the device.</param>
        /// <returns>An AcquireDeviceResult indicating if the device was acquired and if not why.</returns>
        public AcquireDeviceResult AcquireDevice(DeviceType deviceType, string deviceId, Priority priority)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(deviceType);

            var identifier = new DeviceIdentifier(deviceType, deviceId);

            lock(requestedDevices)
            {
                if(requestedDevices.ContainsKey(identifier) &&
                    requestedDevices[identifier] == DeviceStatus.Requested)
                {
                    return new AcquireDeviceResult(false, DeviceAcquisitionFailureReason.RequestQueued);
                }
            }

            var request = new CsiResource
            {
                Item = new AcquireDeviceRequest
                {
                    PriorityType = priority,
                    DeviceType = deviceType,
                    DeviceId = deviceId,
                    GroupList = null
                }
            };

            var response = SendAcquireDeviceRequest(request);
            if(response.Acquired)
            {
                lock(requestedDevices)
                {
                    requestedDevices[identifier] = DeviceStatus.Acquired;
                }

                return new AcquireDeviceResult(response.Acquired, null);
            }

            var reason = GetDeviceNotAcquiredReason(response.Reason);
            if(reason == DeviceAcquisitionFailureReason.RequestQueued)
            {
                lock(requestedDevices)
                {
                    requestedDevices[identifier] = DeviceStatus.Requested;
                }
            }

            return new AcquireDeviceResult(response.Acquired, reason);
        }

        /// <summary>
        /// Request control of groups of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to which the groups belong to.</param>
        /// <param name="deviceId">The device id of the device.</param>
        /// <param name="priority">The client priority of the device.</param>
        /// <param name="groupList">The list of groups to be acquired.</param>
        /// <returns>An <see cref="AcquireGroupsResult"/> indicating if the groups were acquired and if not why.</returns>
        public AcquireGroupsResult AcquireGroups(DeviceType deviceType, string deviceId, Priority priority, List<uint> groupList)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(deviceType);

            var groupAcquisitionStatuses = new Dictionary<uint, GroupAcquisitionStatus>();
            var identifier = new DeviceIdentifier(deviceType, deviceId);
            var acquired = false;
            var makeRequest = true;
            DeviceAcquisitionFailureReason? reason = null;

            lock(requestedGroups)
            {
                if(requestedGroups.ContainsKey(identifier))
                {
                    foreach(var group in groupList)
                    {
                        if(requestedGroups[identifier].ContainsKey(group))
                        {
                            if(requestedGroups[identifier][group] == DeviceStatus.Requested)
                            {
                                groupAcquisitionStatuses[group] = GroupAcquisitionStatus.RequestQueued;
                                reason = DeviceAcquisitionFailureReason.RequestQueued;
                                makeRequest = false;
                            }
                        }
                    }
                }
            }

            if(makeRequest)
            {
                var request = new CsiResource
                    {
                        Item = new AcquireDeviceRequest
                        {
                            PriorityType = priority,
                            DeviceType = deviceType,
                            DeviceId = deviceId,
                            GroupList = groupList
                        }
                    };

                var response = SendAcquireDeviceRequest(request);
                lock(requestedGroups)
                {
                    if(!requestedGroups.ContainsKey(identifier))
                    {
                        requestedGroups[identifier] = new Dictionary<uint, DeviceStatus>();
                    }

                    if(response.Acquired)
                    {
                        foreach(var group in groupList)
                        {
                            requestedGroups[identifier][group] = DeviceStatus.Acquired;
                        }

                        acquired = true;
                    }
                    else
                    {
                        if(response.Reason == DeviceNotAcquired.InQueue)
                        {
                            foreach(var group in response.GroupResponse)
                            {
                                var status = GetGroupAcquisitionStatus(group.AcquisitionStatus);
                                requestedGroups[identifier][group.GroupId] = DeviceStatus.Requested;
                                groupAcquisitionStatuses[group.GroupId] = status;
                            }
                        }

                        reason = GetDeviceNotAcquiredReason(response.Reason);
                    }
                }
            }

            return new AcquireGroupsResult(acquired, reason, groupAcquisitionStatuses);
        }

        /// <summary>
        /// Release control of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to release.</param>
        /// <param name="deviceId">The device ID of the device to release.</param>
        public void ReleaseDevice(DeviceType deviceType, string deviceId)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(deviceType);

            var identifier = new DeviceIdentifier(deviceType, deviceId);
            lock(requestedDevices)
            {
                if(requestedDevices.ContainsKey(identifier))
                {
                    requestedDevices[identifier] = DeviceStatus.NotInUse;
                }
            }

            var request = new CsiResource
            {
                Item = new ReleaseDeviceRequest
                {
                    DeviceType = deviceType,
                    DeviceId = deviceId,
                    GroupList = null
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<ReleaseDeviceResponse>();
            CheckResponse(response.ResourceResponse);
        }

        /// <summary>
        /// Release control of groups of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to which the groups belong to.</param>
        /// <param name="deviceId">The device ID of the device.</param>
        /// <param name="groupList">The list of groups to release.</param>
        public void ReleaseGroups(DeviceType deviceType, string deviceId, List<uint> groupList)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(deviceType);

            var identifier = new DeviceIdentifier(deviceType, deviceId);
            lock(requestedGroups)
            {
                foreach(var group in groupList)
                {
                    if(requestedGroups.ContainsKey(identifier))
                    {
                        if(requestedGroups[identifier].ContainsKey(group))
                        {
                            requestedGroups[identifier][group] = DeviceStatus.NotInUse;
                        }
                    }
                }
            }

            var request = new CsiResource
            {
                Item = new ReleaseDeviceRequest
                {
                    DeviceType = deviceType,
                    DeviceId = deviceId,
                    GroupList = groupList
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<ReleaseDeviceResponse>();
            CheckResponse(response.ResourceResponse);
        }

        /// <summary>
        /// Reset all devices to not in use.
        /// </summary>
        public void ResetAllDevices()
        {
            lock(requestedGroups)
            {
                requestedGroups.Clear();
            }

            lock(requestedDevices)
            {
                requestedDevices.Clear();
            }

            ClearConnectedDeviceCache();
        }

        /// <summary>
        /// Returns true if specified device is acquired.
        /// </summary>
        /// <param name="deviceType">Device to check for having been acquired.</param>
        /// <param name="deviceId">The ID of the device to check if it has been acquired.</param>
        /// <returns>True if the device has been acquired.</returns>
        public bool DeviceAcquired(DeviceType deviceType, string deviceId)
        {
            bool acquired;

            lock(requestedDevices)
            {
                var identifier = new DeviceIdentifier(deviceType, deviceId);

                acquired = requestedDevices.ContainsKey(identifier) &&
                           requestedDevices[identifier] == DeviceStatus.Acquired;
            }

            return acquired;
        }

        /// <summary>
        /// Returns true if specified group of a device is acquired.
        /// </summary>
        /// <param name="deviceType">The device type of the group to check for having been acquired.</param>
        /// <param name="deviceId">The ID of the device to which the group belongs.</param>
        /// <param name="groupId">The ID of the group to check if it has been acquired.</param>
        /// <returns>True if the group has been acquired.</returns>
        public bool GroupAcquired(DeviceType deviceType, string deviceId, uint groupId)
        {
            bool acquired;
            lock(requestedGroups)
            {
                var identifier = new DeviceIdentifier(deviceType, deviceId);
                acquired = requestedGroups.ContainsKey(identifier) &&
                           requestedGroups[identifier].ContainsKey(groupId) &&
                           requestedGroups[identifier][groupId] == DeviceStatus.Acquired;
            }

            return acquired;
        }

        /// <summary>
        /// Get a list of the connected devices.
        /// </summary>
        /// <returns>List of all the currently connected devices.</returns>
        public IList<DeviceIdentifier> GetConnectedDevices()
        {
            return GetConnectedDevicesWithGroups().Select(device => device.Identifier).ToList();
        }

        /// <summary>
        /// Get a list of the connected devices along with groups.
        /// </summary>
        /// <returns>List of all the currently connected devices.</returns>
        public IList<ConnectedDevice> GetConnectedDevicesWithGroups()
        {
            lock(connectedDevicesLockerObject)
            {
                if(connectedDevicesCache.Any())
                {
                    return connectedDevicesCache.Values.ToList();
                }
            }

            List<ConnectedDevice> connectedDeviceList;

            var request = new CsiResource
            {
                Item = new ConnectedDevicesRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<ConnectedDevicesResponse>();
            CheckResponse(response.ResourceResponse);

            var devices = response.Device.Select(device => new ConnectedDevice(new DeviceIdentifier(device.DeviceType, device.DeviceId),
                                                                               device.GroupList)).ToList();

            lock(connectedDevicesLockerObject)
            {
                connectedDevicesCache.Clear();
                foreach(var device in devices)
                {
                    connectedDevicesCache.Add(device.Identifier, device);
                }

                connectedDeviceList = connectedDevicesCache.Values.ToList();
            }

            return connectedDeviceList;
        }

        /// <summary>
        /// Requests event registration for a given device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to request event registration for.</param>
        /// <param name="type">The device type.</param>
        /// <exception cref="ResourceManagementCategoryException">
        /// Thrown if the device doesn't support event registration.
        /// </exception>
        public void RequestEventRegistration(string deviceId, DeviceType type)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(type);

            var request = new CsiResource
            {
                Item = new EventRegistrationRequest
                {
                    Action = EventRegistrationAction.REGISTER,
                    DeviceType = type,
                    DeviceId = deviceId,
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<EventRegistrationResponse>();

            if(response.ResourceResponse.ErrorCode == ResourceErrorCode.EVENT_REGISTRATION_NOT_SUPPORTED)
            {
                throw new ResourceManagementCategoryException(ResourceErrorCode.EVENT_REGISTRATION_NOT_SUPPORTED.ToString(),
                    $"Device Type: {type} Device ID: {deviceId ?? "(NULL)"}");
            }

            CheckResponse(response.ResourceResponse);
        }

        /// <summary>
        /// Releases the event registration of a device.
        /// </summary>
        /// <param name="deviceId">The ID of the device to release event registration on.</param>
        /// <param name="type">The device type.</param>
        public void ReleaseEventRegistration(string deviceId, DeviceType type)
        {
            // Compare the device type against the version, in case the
            // ResourceManagementCategory version doesn't support the device.
            ValidateDeviceType(type);

            var request = new CsiResource
            {
                Item = new EventRegistrationRequest
                {
                    Action = EventRegistrationAction.UNREGISTER,
                    DeviceType = type,
                    DeviceId = deviceId,
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetResourceResponse<EventRegistrationResponse>();
            CheckResponse(response.ResourceResponse);
        }

        #endregion Resource Methods
    }
}
