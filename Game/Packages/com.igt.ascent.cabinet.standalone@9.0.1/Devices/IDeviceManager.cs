//-----------------------------------------------------------------------
// <copyright file = "IDeviceManager.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;
    using IgtUsbDevice;

    /// <summary>
    /// Interface for managing devices.
    /// </summary>
    public interface IDeviceManager
    {
        /// <summary>
        /// Event indicating a device has been connected.
        /// </summary>
        event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <summary>
        /// Event indicating a device has been removed.
        /// </summary>
        event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <summary>
        /// Get the identifiers of all connected devices that are
        /// supported by device manager.
        /// </summary>
        /// <returns>Identifiers of all connected devices.</returns>
        List<DeviceIdentifier> GetConnectedDevices();

        /// <summary>
        /// Get the <see cref="UsbDeviceData"/> of all devices of the
        /// specified type that are connected.
        /// </summary>
        /// <param name="deviceType">The type of devices to get data for.</param>
        /// <returns>
        /// The list of <see cref="UsbDeviceData"/> of all devices of
        /// the specified type that are connected.
        /// </returns>
        List<UsbDeviceData> GetDeviceData(DeviceType deviceType);

        /// <summary>
        /// Get the <see cref="UsbDeviceData"/> of a device of the specified type
        /// and with the specified id.
        /// </summary>
        /// <param name="deviceType">The type of the device to get data for.</param>
        /// <param name="deviceId">The ID of the device to get data for.</param>
        /// <returns>
        /// The <see cref="UsbDeviceData"/> of the specified device.
        /// Null if the specified device is not available.
        /// </returns>
        UsbDeviceData GetDeviceData(DeviceType deviceType, string deviceId);

        /// <summary>
        /// Notify Device Manager that a hardware device has been created.
        /// </summary>
        /// <param name="device">The hardware device to register.</param>
        void RegisterDevice(DeviceBase device);

        /// <summary>
        /// Notify Device Manager that a hardware device has been deleted.
        /// </summary>
        /// <param name="device">The hardware device to unregister.</param>
        void UnregisterDevice(DeviceBase device);

        /// <summary>
        /// Update device statuses and raise the appropriate events.
        /// </summary>
        void Update();
    }
}
