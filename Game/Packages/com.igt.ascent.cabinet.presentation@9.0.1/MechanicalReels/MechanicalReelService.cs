//-----------------------------------------------------------------------
// <copyright file = "MechanicalReelService.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using CabinetServices;
    using Communication.Cabinet.CSI.Schemas;
    using Communication.Cabinet.MechanicalReels;

    /// <summary>
    /// A device service that manages the creation of specific reel devices, and manages the list of connected
    /// reel devices. All individual reel devices are singletons, and are created and cached by this service.
    /// </summary>
    public class MechanicalReelService : DeviceServiceBase, IMechanicalReelService
    {
        private static IMechanicalReels mechanicalReelsInterface;

        /// <summary>
        ///  The incoming CSI featureId string lookup dictionary - string to hardware.
        /// </summary>
        /// <remarks>
        /// The leading space in any of the feature ID string is intentional.
        /// </remarks>
        private readonly Dictionary<string, Hardware> reelFeatureIdToHardwareLookup = new Dictionary<string, Hardware>
        {
            { "AVP Reel Shelf", Hardware.ReelShelf },
            { "DRS Reel Shelf", Hardware.DrsReelShelf },
            { " Nested Wheels", Hardware.NestedWheel },
            { " Single Wheel", Hardware.SingleWheel }
        };

        /// <summary>
        /// Maps hardware string representations to hardware type.
        /// </summary>
        private readonly Dictionary<string, MechanicalReelDevice> reelDevices =
            new Dictionary<string, MechanicalReelDevice>();

        /// <summary>
        /// Maps the hardware device to the number of reels required by the game.
        /// </summary>
        private readonly Dictionary<Hardware, int> requiredHardwareDeviceReelCount =
            new Dictionary<Hardware, int>();

        /// <summary>
        /// The collection of hardware devices connected that currently do not have enough reels for the game.
        /// </summary>
        private readonly HashSet<Hardware> devicesWithInsufficientReelCount = new HashSet<Hardware>();

        // Device connected internal event handler.
        private static event EventHandler<ReelDeviceStatusChangedEventArgs> OnReelDeviceConnected;

        // Device removed internal event handler.
        private static event EventHandler<ReelDeviceStatusChangedEventArgs> OnReelDeviceRemoved;

        // Device acquired internal event handler.
        private static event EventHandler<ReelDeviceStatusChangedEventArgs> OnReelDeviceAcquired;

        // Device released internal event handler.
        private static event EventHandler<ReelDeviceStatusChangedEventArgs> OnReelDeviceReleased;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="MechanicalReelService"/>.
        /// </summary>
        /// <param name="clientPriority">CSI client priority for accessing devices.</param>
        public MechanicalReelService(Priority clientPriority) : base(clientPriority, DeviceType.Reel)
        {
        }

        #endregion

        #region Overrides of DeviceServiceBase

        /// <inheritdoc />
        protected override void OnAsyncConnect()
        {
            base.OnAsyncConnect();
            mechanicalReelsInterface = CabinetLib.GetInterface<IMechanicalReels>();
            var connectedMechanicalReels = GetConnectedDeviceIdentifiers().ToList();
            IsDeviceConnected = connectedMechanicalReels.Count > 0;
        }

        /// <inheritdoc />
        protected override void OnDeviceAcquired(string deviceId)
        {
            if(!string.IsNullOrEmpty(deviceId))
            {
                base.OnDeviceAcquired(deviceId);
                var hardwareId = reelFeatureIdToHardwareLookup[deviceId];
                reelDevices.TryGetValue(deviceId, out var device);
                device?.SetAcquired(new AcquireDeviceResult(true, null));
                CheckReelCount(device);
                IsDeviceAcquired = true;
                OnReelDeviceAcquired?.Invoke(this, new ReelDeviceStatusChangedEventArgs(hardwareId));
            }
        }

        /// <inheritdoc />
        protected override void OnDeviceReleased(string deviceId, DeviceAcquisitionFailureReason reason)
        {
            if(!string.IsNullOrEmpty(deviceId))
            {
                base.OnDeviceReleased(deviceId, reason);
                var hardwareId = reelFeatureIdToHardwareLookup[deviceId];
                reelDevices.TryGetValue(deviceId, out var device);
                device?.SetAcquired(new AcquireDeviceResult(false, DeviceAcquisitionFailureReason.DeviceNotConnected));
                IsDeviceAcquired = false;
                OnReelDeviceReleased?.Invoke(this, new ReelDeviceStatusChangedEventArgs(hardwareId));
            }
        }

        /// <inheritdoc />
        protected override void OnDeviceConnected(string deviceId)
        {
            if(!string.IsNullOrEmpty(deviceId))
            {
                base.OnDeviceConnected(deviceId);
                // Let subscribers know a reel device has come online.
                PostReelDeviceConnected(deviceId);
            }
        }

        /// <inheritdoc />
        protected override void OnDeviceRemoved(string deviceId)
        {
            if(!string.IsNullOrEmpty(deviceId))
            {
                base.OnDeviceRemoved(deviceId);
                SetDeviceRemoved(deviceId);
                PostReelDeviceRemoved(deviceId);
            }
        }

        #endregion

        #region Implementation of IMechanicalReelService

        /// <inheritdoc/>
        public event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceAcquiredEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceAcquired))
                {
                    OnReelDeviceAcquired += value;
                }
            }
            remove => OnReelDeviceAcquired -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceReleasedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceReleased))
                {
                    OnReelDeviceReleased += value;
                }
            }
            remove => OnReelDeviceReleased -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceConnectedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceConnected))
                {
                    OnReelDeviceConnected += value;
                }
            }
            remove => OnReelDeviceConnected -= value;
        }

        /// <inheritdoc/>
        public event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceRemovedEvent
        {
            add
            {
                if(!IsEventHandlerAlreadySubscribed(value, OnReelDeviceRemoved))
                {
                    OnReelDeviceRemoved += value;
                }
            }
            remove => OnReelDeviceRemoved -= value;
        }

        /// <inheritdoc />
        public MechanicalReelDevice GetMechanicalReelDevice(Hardware deviceType, Priority priority)
        {
            if(mechanicalReelsInterface == null)
            {
                throw new CabinetDoesNotSupportReelsInterfaceException();
            }

            var featureId = reelFeatureIdToHardwareLookup.First(pair => pair.Value == deviceType).Key;

            var device = CachedCreate(deviceType);
            AcquireDevice(featureId);

            if(device != null)
            {
                if(device.DeviceAcquired)
                {
                    MechanicalReelLocalizedTiltHelper.ClearTilt(MechanicalReelLocalizedTiltHelper.ReelDeviceNotFoundTiltKey);
                }
                CheckReelCount(device);
            }
            else
            {
                MechanicalReelLocalizedTiltHelper.PostTilt(MechanicalReelLocalizedTiltHelper.ReelDeviceNotFoundTiltKey);
            }

            return device;
        }
        /// <inheritdoc />
        public void ReleaseDevice(string reelFeatureId)
        {
            reelDevices.TryGetValue(reelFeatureId, out var device);
            device?.SetAcquired(new AcquireDeviceResult(false, DeviceAcquisitionFailureReason.DeviceNotConnected));
            Release(reelFeatureId);
        }

        /// <inheritdoc/>
        public bool IsDeviceAcquired { get; private set; }

        /// <inheritdoc />
        public bool IsDeviceConnected { get; private set; }

        /// <inheritdoc />
        public ReelCommandResult RequireDevice(Hardware deviceType, int requiredNumberOfReels)
        {
            if(mechanicalReelsInterface == null)
            {
                throw new CabinetDoesNotSupportReelsInterfaceException();
            }

            var featureId = reelFeatureIdToHardwareLookup.First(pair => pair.Value == deviceType).Key;

            if(requiredHardwareDeviceReelCount.ContainsKey(deviceType))
            {
                requiredHardwareDeviceReelCount[deviceType] = requiredNumberOfReels;
            }
            else
            {
                requiredHardwareDeviceReelCount.Add(deviceType, requiredNumberOfReels);
            }

            return mechanicalReelsInterface.RequireDevice(featureId);
        }

        /// <inheritdoc />
        public bool EnableReelDevice(Hardware deviceType)
        {
            var reelShelf = GetCachedDeviceImplementation<ReelShelf>(deviceType);
            var valid = reelShelf != null;
            if(valid)
            {
                reelShelf.IsEnabled = true;
            }

            return valid;
        }

        /// <inheritdoc />
        public bool DisableReelDevice(Hardware deviceType)
        {
            var reelShelf = GetCachedDeviceImplementation<ReelShelf>(deviceType);
            var valid = reelShelf != null;
            if(valid)
            {
                reelShelf.IsEnabled = false;
            }

            return valid;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Get the device implementation of a reel device.
        /// Precondition: The device implementation for the specified reel device hardware must have been retrieved using GetMechanicalReelDevice.
        /// </summary>
        /// <typeparam name="T">The type of the reel device.</typeparam>
        /// <param name="deviceType">The type of the reel device.</param>
        /// <returns>True if the device implementation was disabled.
        /// Returns false if no device implementation exist at this moment or the hardware type does not support to be disabled.</returns>
        private T GetCachedDeviceImplementation<T>(Hardware deviceType) where T : MechanicalReelDevice
        {
            T result = null;
            var featureId = reelFeatureIdToHardwareLookup.Where(kvp => kvp.Value == deviceType)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
            if(featureId != null && reelDevices.ContainsKey(featureId))
            {
                result = reelDevices[featureId] as T;
            }
            return result;
        }

        /// <summary>
        /// Get an instance of a requested device type. A new instance will be created if one doesn't already exist.
        /// </summary>
        /// <param name="reelHardwareType">The <see cref="Hardware"/> type of the device to create.</param>
        /// <returns>An instance of the requested device type.</returns>
        /// <exception cref="UnsupportedHardwareException">
        /// Thrown if the device type requested is unknown.
        /// </exception>
        private MechanicalReelDevice CachedCreate(Hardware reelHardwareType)
        {
            MechanicalReelDevice newDevice = null;

            var featureId = reelFeatureIdToHardwareLookup.Where(kvp => kvp.Value == reelHardwareType)
                                                         .Select(kvp => kvp.Key)
                                                         .FirstOrDefault();

            if(featureId != null)
            {
                reelDevices.TryGetValue(featureId, out newDevice);
            }

            if(newDevice == null && featureId != null)
            {
                // Only create a device that is currently connected.
                var connectedDevices = mechanicalReelsInterface.GetReelDevices();
                var description = connectedDevices.FirstOrDefault(device => device.FeatureId == featureId);

                if(description != null)
                {
                    switch(reelHardwareType)
                    {
                        case Hardware.ReelShelf:
                            newDevice = new ReelShelf(description);
                            break;

                        case Hardware.DrsReelShelf:
                            newDevice = new ReelShelf(description);
                            break;

                        case Hardware.NestedWheel:
                            newDevice = new NestedWheels(description);
                            break;

                        case Hardware.SingleWheel:
                            newDevice = new Wheel(description);
                            break;

                        default:
                            throw new UnsupportedHardwareException(
                                $"Hardware device {reelHardwareType} is not supported.");
                    }
                }

                if(newDevice != null)
                {
                    newDevice.Initialize(mechanicalReelsInterface);
                    reelDevices.Add(featureId, newDevice);
                }
            }

            return newDevice;
        }

        /// <summary>
        /// Attempts to acquire exclusive access to a device, and set internal results accordingly.
        /// </summary>
        /// <param name="featureId">The feature ID of the device to acquire.</param>
        private void AcquireDevice(string featureId)
        {
            reelDevices.TryGetValue(featureId, out var device);
            if(device?.DeviceAcquired == false)
            {
                var result = Acquire(featureId);
                var acquisitionReason = result ?
                        (DeviceAcquisitionFailureReason?)null :
                         DeviceAcquisitionFailureReason.DeviceNotConnected;
                device.SetAcquired(new AcquireDeviceResult(result, acquisitionReason));
            }
        }

        /// <summary>
        /// Set a device to offline from the internal list of requested devices.
        /// </summary>
        /// <param name="featureId">The feature ID of the device to set offline.</param>
        private void SetDeviceRemoved(string featureId)
        {
            reelDevices.TryGetValue(featureId, out var device);
            if(device != null)
            {
                device.SetAcquired(new AcquireDeviceResult(false, DeviceAcquisitionFailureReason.DeviceNotConnected));
                device.Deinitialize();
                reelDevices.Remove(featureId);
            }
        }

        /// <summary>
        /// Post the PostReelDeviceRemovedEvent event.
        /// </summary>
        /// <param name="featureId">The feature ID of the device removed.</param>
        private void PostReelDeviceRemoved(string featureId)
        {
            if(reelFeatureIdToHardwareLookup.TryGetValue(featureId, out var hardwareId))
            {
                OnReelDeviceRemoved?.Invoke(new object(), new ReelDeviceStatusChangedEventArgs(hardwareId));
            }
        }

        /// <summary>
        /// Post the DeviceConnectedEvent event.
        /// </summary>
        /// <param name="featureId">The feature ID of the device connected.</param>
        private void PostReelDeviceConnected(string featureId)
        {
            if(reelFeatureIdToHardwareLookup.TryGetValue(featureId, out var hardwareId))
            {
                OnReelDeviceConnected?.Invoke(new object(), new ReelDeviceStatusChangedEventArgs(hardwareId));
            }
        }

        /// <summary>
        /// Checks if an event handler delegate is already in an event handler's list .
        /// </summary>
        /// <param name="prospectiveHandler">The <see cref="Delegate"/> that is going to be added.</param>
        /// <param name="handler">The <see cref="EventHandler{T}"/> where T is based on <see cref="EventArgs"/></param>.
        /// <returns>Flag indicating if this event is already handled by the prospective handler.</returns>
        private static bool IsEventHandlerAlreadySubscribed<TEventArgs>(Delegate prospectiveHandler,
                                                            EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            if(handler != null)
            {
                var invocationList = handler.GetInvocationList();

                if(invocationList.Any() && invocationList.Contains(prospectiveHandler))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks to see if the insufficient reel tilt should be posted or not.
        /// </summary>
        private void CheckReelCountTilt()
        {
            if(devicesWithInsufficientReelCount.Count > 0)
            {
                MechanicalReelLocalizedTiltHelper.PostTilt(MechanicalReelLocalizedTiltHelper.ReelCountInsufficientTiltKey);
            }
            else
            {
                MechanicalReelLocalizedTiltHelper.ClearTilt(MechanicalReelLocalizedTiltHelper.ReelCountInsufficientTiltKey);
            }
        }

        /// <summary>
        /// Checks a mechanical reel device to see if the number of reels is sufficient for the game.
        /// </summary>
        /// <param name="device">The device to check.</param>
        private void CheckReelCount(MechanicalReelDevice device)
        {
            if(device != null)
            {
                var deviceType = reelFeatureIdToHardwareLookup[device.Description.FeatureId];
                if(requiredHardwareDeviceReelCount.ContainsKey(deviceType))
                {
                    if(device.ReelCount < requiredHardwareDeviceReelCount[deviceType])
                    {
                        devicesWithInsufficientReelCount.Add(deviceType);
                    }
                    else
                    {
                        devicesWithInsufficientReelCount.Remove(deviceType);
                    }
                }
            }

            CheckReelCountTilt();
        }

        #endregion
   }
}
