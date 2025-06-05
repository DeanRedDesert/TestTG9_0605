// -----------------------------------------------------------------------
// <copyright file = "IDeviceService.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Base interface for a device service. Exposes acquire and release functionality.
    /// </summary>
    public interface IDeviceService : ICabinetService
    {
        /// <summary>
        /// Acquire a specific device from the CSI manager.
        /// </summary>
        /// <param name="deviceId">ID of the device to acquire. Use null if no device ID is available.</param>
        /// <returns>True if the device has been acquired. If false, the caller is in queue for ownership.</returns>
        /// <remarks>
        /// Will raise <see cref="DeviceAcquired"/> when the device becomes acquired. This could
        /// occur before or after this method returns.
        /// </remarks>
        // ReSharper disable once UnusedMethodReturnValue.Global
        bool Acquire(string deviceId);

        /// <summary>
        /// Release the device.
        /// </summary>
        /// <param name="deviceId">ID of the device to release. Use null if no device ID is available.</param>
        void Release(string deviceId);

        /// <summary>
        /// Check if a specific device has been acquired.
        /// </summary>
        /// <param name="deviceId">ID of the device to query. Use null if no device ID is available.</param>
        /// <returns>True if the specified device is acquired. False otherwise.</returns>
        bool IsAcquired(string deviceId);

        /// <summary>
        /// Returns an enumeration of all currently connected devices matching this device type.
        /// </summary>
        /// <returns>All devices currently connected. This must not be cached.</returns>
        IEnumerable<string> GetConnectedDeviceIdentifiers();

        /// <summary>
        /// Raised whenever the device is acquired (at some point after calling <see cref="Acquire"/>).
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<DeviceEventArgs> DeviceAcquired;

        /// <summary>
        /// Raised whenever the device is lost (either by another client stealing it, or it disconnecting,
        /// or by calling <see cref="Release"/>).
        /// </summary>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<DeviceEventArgs> DeviceReleased;
    }
}