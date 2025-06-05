// -----------------------------------------------------------------------
// <copyright file = "DeviceServiceBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// Basic device service that handles acquiring and releasing devices, along with connect and disconnect events.
    /// </summary>
    public abstract class DeviceServiceBase : CabinetServiceBase, IDeviceService
    {
        /// <summary>
        /// A place holder string for panel device Id, since it might be null from foundation previous to G series.
        /// Copied from Standard.CabinetLib.
        /// </summary>
        private const string NoDeviceId = "NoDeviceId";

        /// <summary>
        /// The current <see cref="AcquisitionState"/> of all requested devices.
        /// </summary>
        private readonly Dictionary<string, AcquisitionState> acquiredDevices = new Dictionary<string, AcquisitionState>();

        #region IDeviceService
        
        /// <inheritdoc/>
        public bool Acquire(string deviceId)
        {
            VerifyCabinetIsConnected();

            // If we are acquired or pending, there's nothing to do.
            var state = GetAcquisitionState(deviceId);
            if(state == AcquisitionState.Acquired || state == AcquisitionState.AcquirePending)
            {
                return IsAcquired(deviceId);
            }

            var response = CabinetLib.RequestAcquireDevice(ServiceDeviceType, deviceId, ClientPriority);

            if(response.Acquired)
            {
                // We've acquired the device! Notify listeners
                SetAcquisitionState(deviceId, AcquisitionState.Acquired);
                OnDeviceAcquired(deviceId);
                Raise(DeviceAcquired, new DeviceEventArgs(ServiceDeviceType, deviceId));
            }
            else if(response.Reason == DeviceAcquisitionFailureReason.DeviceNotConnected)
            {
                // We'll need to request it when (if) it connects
                SetAcquisitionState(deviceId, AcquisitionState.RequestedNotConnected);
            }
            else
            {
                // We'll get it automatically, eventually
                SetAcquisitionState(deviceId, AcquisitionState.AcquirePending);
            }

            return IsAcquired(deviceId);
        }
        
        /// <inheritdoc/>
        public void Release(string deviceId)
        {
            VerifyCabinetIsConnected();

            var state = GetAcquisitionState(deviceId);

            // Disconnected and released devices don't need to be released.
            if(state == AcquisitionState.Acquired || state == AcquisitionState.AcquirePending)
            {
                CabinetLib.ReleaseDevice(ServiceDeviceType, deviceId);

                if(state == AcquisitionState.Acquired)
                {
                    // Only raise if we had managed to acquire the device before releasing.
                    Raise(DeviceReleased, new DeviceEventArgs(ServiceDeviceType, deviceId));
                }
            }

            // This device is marked as released
            SetAcquisitionState(deviceId, AcquisitionState.NotRequested);
        }

        /// <inheritdoc/>
        public bool IsAcquired(string deviceId)
        {
            return GetAcquisitionState(deviceId) == AcquisitionState.Acquired;
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetConnectedDeviceIdentifiers()
        {
            VerifyCabinetIsConnected();
            return
                CabinetLib.GetConnectedDevices()
                    .Where(device => device.DeviceType == ServiceDeviceType)
                    .Select(device => device.DeviceId);
        }

        /// <inheritdoc/>
        public event EventHandler<DeviceEventArgs> DeviceAcquired;

        /// <inheritdoc/>
        public event EventHandler<DeviceEventArgs> DeviceReleased;

        #endregion

        #region ICabinetServiceRestricted

        /// <inheritdoc/>
        protected override void OnPostConnect()
        {
            base.OnPostConnect();

            // Remove existing handlers so we don't double up on handlers by accident
            CabinetLib.DeviceAcquiredEvent -= DeviceAcquiredEventHandler;
            CabinetLib.DeviceAcquiredEvent += DeviceAcquiredEventHandler;
            CabinetLib.DeviceReleasedEvent -= DeviceReleasedEventHandler;
            CabinetLib.DeviceReleasedEvent += DeviceReleasedEventHandler;
            CabinetLib.DeviceConnectedEvent -= DeviceConnectedEventHandler;
            CabinetLib.DeviceConnectedEvent += DeviceConnectedEventHandler;
            CabinetLib.DeviceRemovedEvent -= DeviceRemovedEventHandler;
            CabinetLib.DeviceRemovedEvent += DeviceRemovedEventHandler;
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            CabinetLib.DeviceAcquiredEvent -= DeviceAcquiredEventHandler;
            CabinetLib.DeviceReleasedEvent -= DeviceReleasedEventHandler;
            CabinetLib.DeviceConnectedEvent -= DeviceConnectedEventHandler;
            CabinetLib.DeviceRemovedEvent -= DeviceRemovedEventHandler;

            // Copy the keys because Release modifies the dictionary
            foreach(var deviceId in GetAssociatedDeviceIds())
            {
                Release(deviceId);
            }

            base.Disconnect();
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Priority of this service.
        /// </summary>
        protected Priority ClientPriority { get; }

        /// <summary>
        /// Device type of this service.
        /// </summary>
        protected DeviceType ServiceDeviceType { get; }

        /// <summary>
        /// Construct a service using a specific device type and priority.
        /// </summary>
        /// <param name="clientPriority">Priority of this service.</param>
        /// <param name="serviceDeviceType">Device type of this service.</param>
        protected DeviceServiceBase(Priority clientPriority, DeviceType serviceDeviceType)
        {
            ClientPriority = clientPriority;
            ServiceDeviceType = serviceDeviceType;
        }

        /// <summary>
        /// Get the <see cref="AcquisitionState"/> of a device by its ID.
        /// </summary>
        /// <param name="deviceId">ID of the device to look up. May be null.</param>
        /// <returns>The last known state. <see cref="AcquisitionState.NotRequested"/> if unknown.</returns>
        protected AcquisitionState GetAcquisitionState(string deviceId)
        {
            var key = deviceId ?? NoDeviceId;
            return acquiredDevices.ContainsKey(key)
                ? acquiredDevices[key]
                : AcquisitionState.NotRequested;
        }

        /// <summary>
        /// Gets a copy of all device ids that have been accessed by this service.
        /// </summary>
        /// <returns>An array of associated device ids.</returns>
        protected IEnumerable<string> GetAssociatedDeviceIds()
        {
            // Convert stored NoDeviceId back to null and create a copy for enumeration.
            return acquiredDevices.Keys.Select(deviceId => deviceId == NoDeviceId ? null : deviceId).ToArray();
        }

        /// <summary>
        /// Custom handler for when a device of this service's type is acquired.
        /// </summary>
        /// <param name="deviceId">Identifier of the device.</param>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void OnDeviceAcquired(string deviceId)
        { }

        /// <summary>
        /// Custom handler for when a device of this service's type is released.
        /// </summary>
        /// <param name="deviceId">Identifier of the device.</param>
        /// <param name="reason">Reason for the release.</param>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void OnDeviceReleased(string deviceId, DeviceAcquisitionFailureReason reason)
        { }

        /// <summary>
        /// Custom handler for when a device of this service's type has connected.
        /// </summary>
        /// <param name="deviceId">Identifier of the device.</param>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void OnDeviceConnected(string deviceId)
        { }

        /// <summary>
        /// Custom handler for when a device of this service's type has been removed.
        /// </summary>
        /// <param name="deviceId">Identifier of the device.</param>
        // ReSharper disable once UnusedParameter.Global
        protected virtual void OnDeviceRemoved(string deviceId)
        { }

        #endregion

        #region Private methods

        /// <summary>
        /// Set the <see cref="AcquisitionState"/> of a specified device.
        /// </summary>
        /// <param name="deviceId">Specific device to set.</param>
        /// <param name="state">New state of the specified device.</param>
        private void SetAcquisitionState(string deviceId, AcquisitionState state)
        {
            var key = deviceId ?? NoDeviceId;
            acquiredDevices[key] = state;
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceAcquiredEvent"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Description of the event, including device type and ID.</param>
        private void DeviceAcquiredEventHandler(object sender, DeviceAcquiredEventArgs args)
        {
            if(args.DeviceName == ServiceDeviceType)
            {
                SetAcquisitionState(args.DeviceId, AcquisitionState.Acquired);

                OnDeviceAcquired(args.DeviceId);

                Raise(DeviceAcquired, new DeviceEventArgs(args.DeviceName, args.DeviceId));
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceReleasedEvent"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Description of the event, including device type and ID, and cause of the release.</param>
        private void DeviceReleasedEventHandler(object sender, DeviceReleasedEventArgs args)
        {
            if(args.DeviceName == ServiceDeviceType)
            {
                // We'll get a released event if we own the device and it gets disconnected
                SetAcquisitionState(args.DeviceId,
                    args.Reason == DeviceAcquisitionFailureReason.DeviceNotConnected
                        ? AcquisitionState.RequestedNotConnected
                        : AcquisitionState.AcquirePending);

                OnDeviceReleased(args.DeviceId, args.Reason);

                Raise(DeviceReleased, new DeviceEventArgs(args.DeviceName, args.DeviceId));
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceConnectedEvent"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Description of the event, including device type and ID.</param>
        private void DeviceConnectedEventHandler(object sender, DeviceConnectedEventArgs args)
        {
            if(args.DeviceName == ServiceDeviceType)
            {
                // Notify the device connected event first, so that the button service
                // has a chance to update the button panels before proceeding pending animations and lamp state
                // after the button panel is acquired.
                OnDeviceConnected(args.DeviceId);

                if(GetAcquisitionState(args.DeviceId) == AcquisitionState.RequestedNotConnected)
                {
                    Acquire(args.DeviceId);
                }
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceRemovedEvent"/>.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="args">Description of the event, including device type and ID.</param>
        private void DeviceRemovedEventHandler(object sender, DeviceRemovedEventArgs args)
        {
            if(args.DeviceName == ServiceDeviceType)
            {
                var state = GetAcquisitionState(args.DeviceId);
                if(state != AcquisitionState.NotRequested)
                {
                    SetAcquisitionState(args.DeviceId, AcquisitionState.RequestedNotConnected);
                }

                OnDeviceRemoved(args.DeviceId);

                if(state == AcquisitionState.Acquired)
                {
                    // Only raise the released event if we owned the device when it got removed
                    Raise(DeviceReleased, new DeviceEventArgs(args.DeviceName, args.DeviceId));
                }
            }
        }

        /// <summary>
        /// Raises a specified event with arguments if there are any attached handlers.
        /// </summary>
        /// <typeparam name="TEventArgs">Type of the event arguments.</typeparam>
        /// <param name="eventDelegate">Event delegate to check and raise.</param>
        /// <param name="args">Arguments to supply with the event.</param>
        private void Raise<TEventArgs>(EventHandler<TEventArgs> eventDelegate, TEventArgs args) where TEventArgs : EventArgs
        {
            eventDelegate?.Invoke(this, args);
        }

        #endregion
    }
}