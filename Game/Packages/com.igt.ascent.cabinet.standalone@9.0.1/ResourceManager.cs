//-----------------------------------------------------------------------
// <copyright file = "ResourceManager.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas;
    using Devices;
    using MechanicalReels;

    /// <summary>
    /// This class manages resources for standalone cabinet lib,
    /// including installed interfaces, connected devices (both
    /// virtual and hardware ones), device acquisition and release.
    /// </summary>
    internal sealed class ResourceManager
    {
        #region Events

        /// <summary>
        /// Event indicating that a device has been acquired.
        /// </summary>
        public event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// Event indicating that a device has been released.
        /// </summary>
        public event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <summary>
        /// Event indicating a device has been connected.
        /// </summary>
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <summary>
        /// Event indicating a device has been removed.
        /// </summary>
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        #endregion

        #region Private Fields

        /// <summary>
        /// List of the interfaces installed in the game.
        /// The interfaces can be supported by either a virtual or a hardware implementation.
        /// </summary>
        private readonly Dictionary<Type, object> installedInterfaces = new Dictionary<Type, object>();

        /// <summary>
        /// List of devices that can be accessed through the installed interfaces.
        /// </summary>
        private readonly HashSet<DeviceIdentifier> installedDevices = new HashSet<DeviceIdentifier>();

        /// <summary>
        /// List of devices the game currently has control of.
        /// </summary>
        private readonly HashSet<DeviceIdentifier> acquiredDevices = new HashSet<DeviceIdentifier>();

        /// <summary>
        /// List of device types whose interfaces are supported by a hardware implementation.
        /// </summary>
        private readonly List<DeviceType> hardwareSupportedDevices = new List<DeviceType>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize an instance of Resource Manager that supports
        /// a list of device interfaces requested by the game, and
        /// does not work together with a device manager.
        /// </summary>
        /// <remarks>
        /// In <paramref name="requestedInterfaces"/>, if the object supporting an interface
        /// is null, it means that the interface requires a hardware implementation.
        /// Otherwise, the object is a virtual implementation of the interface.
        /// </remarks>
        /// <param name="requestedInterfaces">
        /// The list of interfaces requested by the game, and
        /// the corresponding objects supporting the interfaces.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="requestedInterfaces"/> asks for a hardware
        /// support for an interface that does not have a hardware implementation.
        /// </exception>
        public ResourceManager(Dictionary<Type, object> requestedInterfaces)
            : this(requestedInterfaces, null)
        {
        }

        /// <summary>
        /// Initialize an instance of Resource Manager that supports
        /// a list of device interfaces requested by the game, and
        /// works together with a device manager.
        /// </summary>
        /// <remarks>
        /// In <paramref name="requestedInterfaces"/>, if the object supporting an interface
        /// is null, it means that the interface requires a hardware implementation.
        /// Otherwise, the object is a virtual implementation of the interface.
        /// </remarks>
        /// <param name="requestedInterfaces">
        /// The list of interfaces requested by the game, and
        /// the corresponding objects supporting the interfaces.
        /// </param>
        /// <param name="deviceManager">
        /// The device manager required by the hardware interface implementations.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="requestedInterfaces"/> asks for a hardware
        /// support for an interface that does not have a hardware implementation.
        /// </exception>
        public ResourceManager(IDictionary<Type, object> requestedInterfaces,
                               IDeviceManager deviceManager)
        {
            if(requestedInterfaces != null)
            {
                // Install the interfaces.
                foreach(var requestedInterface in requestedInterfaces)
                {
                    if(requestedInterface.Value != null)
                    {
                        // If it is a virtual implementation, simply add to the list.
                        AddInterface(requestedInterface.Key, requestedInterface.Value);
                    }
                    else
                    {
                        // If it needs a hardware implementation, create one.
                        var hardwareSupport = CreateHardwareSupport(requestedInterface.Key, deviceManager);

                        if(hardwareSupport == null)
                        {
                            throw new ArgumentException($"Interface {requestedInterface.Key.Name} is not supported.");
                        }

                        AddInterface(requestedInterface.Key, hardwareSupport);
                    }
                }

                // Subscribe to Device Manager events in order to filter them
                // based on whether a device is requested by the game.
                if(deviceManager != null)
                {
                    deviceManager.DeviceConnectedEvent += HandleDeviceConnected;
                    deviceManager.DeviceRemovedEvent += HandleDeviceRemoved;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a list of the installed devices.
        /// </summary>
        /// <returns>List of all the currently installed devices.</returns>
        public IList<DeviceIdentifier> GetInstalledDevices()
        {
            return installedDevices.ToList();
        }

        /// <summary>
        /// Attempt to get the the specified interface.
        /// </summary>
        /// <typeparam name="TInterface">The type of the interface to get.</typeparam>
        /// <returns>The interface if available, otherwise null.</returns>
        public TInterface GetInterface<TInterface>() where TInterface : class
        {
            TInterface result = null;

            if(installedInterfaces.ContainsKey(typeof(TInterface)))
            {
                result = installedInterfaces[typeof(TInterface)] as TInterface;
            }

            return result;
        }

        /// <summary>
        /// Acquire a control of a device.
        /// </summary>
        /// <param name="deviceType">The type of the device to acquire.</param>
        /// <param name="deviceId">The id of the device to acquire.</param>
        /// <returns>
        /// An <see cref="AcquireDeviceResult"/> indicating if
        /// the device was acquired and if not why.
        /// </returns>
        public AcquireDeviceResult AcquireDevice(DeviceType deviceType, string deviceId)
        {
            AcquireDeviceResult result;

            var deviceIdentifier = new DeviceIdentifier(deviceType, deviceId);

            // Only an installed device can be acquired.
            if(installedDevices.Contains(deviceIdentifier))
            {
                acquiredDevices.Add(deviceIdentifier);
                result = new AcquireDeviceResult(true, null);

                // Raise the Device Acquired event.
                DeviceAcquiredEvent?.Invoke(this, new DeviceAcquiredEventArgs(deviceIdentifier.DeviceType,
                                                                              deviceIdentifier.DeviceId));
            }
            else
            {
                result = new AcquireDeviceResult(false, DeviceAcquisitionFailureReason.DeviceNotConnected);
            }

            return result;
        }

        /// <summary>
        /// Release the specified device.
        /// </summary>
        /// <param name="deviceType">The device to release.</param>
        /// <param name="deviceId">The device ID of the device to release.</param>
        public void ReleaseDevice(DeviceType deviceType, string deviceId)
        {
            var deviceIdentifier = new DeviceIdentifier(deviceType, deviceId);

            acquiredDevices.Remove(deviceIdentifier);
        }

        /// <summary>
        /// Check if the specified device is acquired.
        /// </summary>
        /// <param name="deviceType">Device to check for having been acquired.</param>
        /// <param name="deviceId">The ID of the device to check if it has been acquired.</param>
        /// <returns>True if the device has been acquired.</returns>
        public bool DeviceAcquired(DeviceType deviceType, string deviceId)
        {
            var deviceIdentifier = new DeviceIdentifier(deviceType, deviceId);

            return acquiredDevices.Contains(deviceIdentifier);
        }

        /// <summary>
        /// Add a new interface.
        /// </summary>
        /// <param name="interfaceType">The interface to add.</param>
        /// <param name="interfaceSupport">An object which supports the interface.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="interfaceType"/> or <paramref name="interfaceSupport"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="interfaceType"/> is already installed.
        /// </exception>
        public void AddInterface(Type interfaceType, object interfaceSupport)
        {
            if(interfaceType == null)
            {
                throw new ArgumentNullException(nameof(interfaceType));
            }

            if(installedInterfaces.ContainsKey(interfaceType))
            {
                throw new InvalidOperationException($"Interface {interfaceType.Name} is already installed, cannot be added again.");
            }

            // Add to the installed interfaces list.
            installedInterfaces[interfaceType] = interfaceSupport ?? throw new ArgumentNullException(nameof(interfaceSupport));

            // Check what devices are supported by the interface,
            // add to installed devices list.
            RetrieveInstalledDevices(interfaceSupport);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Instantiate a hardware instance that supports the specified interface.
        /// </summary>
        /// <param name="type">The type of the interface to support.</param>
        /// <param name="deviceManager">The device manager providing device data.</param>
        /// <returns>
        /// The hardware implementation of the interface.
        /// Null if the interface type is not supported.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceManager"/> is null.
        /// </exception>
        private object CreateHardwareSupport(Type type, IDeviceManager deviceManager)
        {
            if(deviceManager == null)
            {
                throw new ArgumentNullException(nameof(deviceManager));
            }

            object result = null;

            // Check if the interface type is supported.
            if(type == typeof(IPeripheralLights))
            {
                // Mark the device type as hardware supported.
                hardwareSupportedDevices.Add(DeviceType.Light);

                result = new HardwarePeripheralLights(deviceManager);
            }
            else if(type == typeof(IMechanicalReels))
            {
                // Mark the device type as hardware supported.
                hardwareSupportedDevices.Add(DeviceType.Reel);

                result = new HardwareMechanicalReels(deviceManager);
            }
            else if(type == typeof(IStreamingLights))
            {
                // Mark the device type as hardware supported.
                hardwareSupportedDevices.Add(DeviceType.StreamingLight);

                result = new HardwareStreamingLights(deviceManager);
            }

            return result;
        }

        /// <summary>
        /// Retrieve device identifiers from an interface implementation,
        /// and add them to the installed devices list.
        /// </summary>
        /// <param name="interfaceSupport">An object which supports the interface.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="interfaceSupport"/> is null.
        /// </exception>
        private void RetrieveInstalledDevices(object interfaceSupport)
        {
            switch(interfaceSupport)
            {
                case null:
                    throw new ArgumentNullException(nameof(interfaceSupport));
                case IPeripheralLights peripheralLights:
                {
                    foreach(var description in peripheralLights.GetLightDevices())
                    {
                        installedDevices.Add(new DeviceIdentifier(DeviceType.Light, description.FeatureId));
                    }

                    break;
                }

                case IMechanicalReels mechanicalReels:
                {
                    foreach(var description in mechanicalReels.GetReelDevices())
                    {
                        installedDevices.Add(new DeviceIdentifier(DeviceType.Reel, description.FeatureId));
                    }

                    break;
                }

                case IStreamingLights streamingLights:
                {
                    foreach(var description in streamingLights.GetLightDevices())
                    {
                        installedDevices.Add(new DeviceIdentifier(DeviceType.StreamingLight, description.FeatureId));
                    }

                    break;
                }

                case IMonitor monitorSupport:
                {
                    foreach(var monitor in monitorSupport.GetComposition().Monitors)
                    {
                        installedDevices.Add(new DeviceIdentifier(DeviceType.Monitor, monitor.DeviceId));
                    }

                    break;
                }

                case IVideoTopper _:
                    installedDevices.Add(new DeviceIdentifier(DeviceType.VideoTopper, null));
                    break;
                case ITouchScreen _:
                    installedDevices.Add(new DeviceIdentifier(DeviceType.TouchScreen, null));
                    break;
            }
        }

        /// <summary>
        /// Handle Device Connected event from the device manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceConnected(object sender, DeviceConnectedEventArgs eventArgs)
        {
            var deviceIdentifier = new DeviceIdentifier(eventArgs.DeviceName, eventArgs.DeviceId);

            // Only handle the devices that are used by the game.
            if(hardwareSupportedDevices.Contains(deviceIdentifier.DeviceType))
            {
                installedDevices.Add(deviceIdentifier);

                // Raise the Device Connected event.
                DeviceConnectedEvent?.Invoke(this, new DeviceConnectedEventArgs(deviceIdentifier.DeviceType,
                                                                   deviceIdentifier.DeviceId));
            }
        }

        /// <summary>
        /// Handle Device Removed event from the device manager.
        /// Release the device if the removed one has been acquired by the game.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs eventArgs)
        {
            var deviceIdentifier = new DeviceIdentifier(eventArgs.DeviceName, eventArgs.DeviceId);

            // Only handle the devices that are used by the game.
            if(hardwareSupportedDevices.Contains(deviceIdentifier.DeviceType)
               && installedDevices.Contains(deviceIdentifier))
            {
                // Remove from the installed devices list.
                installedDevices.Remove(deviceIdentifier);

                // Raise the Device Removed event.
                var removedHandler = DeviceRemovedEvent;
                removedHandler?.Invoke(this, new DeviceRemovedEventArgs(deviceIdentifier.DeviceType,
                                                                        deviceIdentifier.DeviceId));

                // If it has been acquired by the game...
                if(acquiredDevices.Contains(deviceIdentifier))
                {
                    // Release the device.
                    acquiredDevices.Remove(deviceIdentifier);

                    // Raise the Device Released event.
                    var releasedHandler = DeviceReleasedEvent;
                    releasedHandler?.Invoke(this, new DeviceReleasedEventArgs(deviceIdentifier.DeviceType,
                                                                              deviceIdentifier.DeviceId,
                                                                              DeviceAcquisitionFailureReason.DeviceNotConnected));
                }
            }
        }

        #endregion
    }
}
