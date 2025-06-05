// -----------------------------------------------------------------------
// <copyright file = "IMechanicalReelService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using CabinetServices;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;
    using Communication.Cabinet.MechanicalReels;

    /// <summary>
    /// This interface defines a cabinet service to access mechanical reels.
    /// </summary>
    public interface IMechanicalReelService : ICabinetService
    {
        #region Events

        /// <summary>
        /// Event raised when the mechanical reel device is acquired.
        /// </summary>
        event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceAcquiredEvent;

        /// <summary>
        /// Event raised when the mechanical reel device is released.
        /// </summary>
        event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceReleasedEvent;

        /// <summary>
        /// Event raised when the mechanical reel device is connected. This is typically
        /// unexpected behavior during game play indicating that the device has regained communication with the
        /// EGM after it is lost due to a failure at the hardware level.
        /// </summary>
        event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceConnectedEvent;

        /// <summary>
        /// Event raised when a mechanical reel device has been removed. This is typically
        /// unexpected behavior during game play indicating that the device has lost communication with the
        /// EGM due to a failure at the hardware level.
        /// </summary>
        event EventHandler<ReelDeviceStatusChangedEventArgs> ReelDeviceRemovedEvent;

        #endregion

        /// <summary>
        /// Gets a flag indicating if a mechanical reel device is acquired.
        /// </summary>
        bool IsDeviceAcquired { get; }

        /// <summary>
        /// Gets a flag indicating if a mechanical reel device is connected.
        /// </summary>
        bool IsDeviceConnected { get; }

        /// <summary>
        /// Get the device implementation for the specified reel device hardware.
        /// </summary>
        /// <param name="deviceType">Type of reel device to acquire.</param>
        /// <param name="priority">Client priority.</param>
        /// <returns>A reference to the requested device.</returns>
        /// <remarks>The device return may not be acquired successfully. Check the DeviceAcquired flag.</remarks>
        /// <exception cref="CabinetDoesNotSupportReelsInterfaceException">
        /// Thrown if the cabinet lib passed in <see cref="ICabinetLib"/>
        /// does not support <see cref="IMechanicalReels"/> interface.
        /// </exception>
        MechanicalReelDevice GetMechanicalReelDevice(Hardware deviceType, Priority priority);

        /// <summary>
        /// Releases the mechanical reel device.
        /// </summary>
        void ReleaseDevice(string reelFeatureId);

        /// <summary>
        /// Sets a mechanical reel device as a required device. This will cause the Foundation
        /// to tilt if the device is not connected.
        /// </summary>
        /// <param name="deviceType">The <see cref="Hardware"/> reel type device to mark as required.</param>
        /// <param name="requiredNumberOfReels">The number of reels the device should have.</param>
        /// <returns>The reel command result from the require device call.</returns>
        /// <exception cref="CabinetDoesNotSupportReelsInterfaceException">
        /// Thrown if the cabinet does not support the mechanical reels interface.
        /// </exception>
        ReelCommandResult RequireDevice(Hardware deviceType, int requiredNumberOfReels);

        /// <summary>
        /// Enables the device implementation for the specified reel device hardware.
        /// Precondition: The device implementation for the specified reel device hardware must have been retrieved using GetMechanicalReelDevice.
        /// </summary>
        /// <param name="deviceType">The <see cref="Hardware"/> type reel device to enable.</param>
        /// <returns>True if the device implementation was enabled.
        /// Returns false if no device implementation exists at this moment, or the hardware type does not support being enabled.</returns>
        bool EnableReelDevice(Hardware deviceType);

        /// <summary>
        /// Disables usage of the specified reel device hardware.
        /// Precondition: The device implementation for the specified reel device hardware must have been retrieved using GetMechanicalReelDevice.
        /// </summary>
        /// <param name="deviceType">The <see cref="Hardware"/> type reel device to disable.</param>
        /// <returns>True if the device implementation was disabled.
        /// Returns false if no device implementation exists at this moment, or the hardware type does not support being disabled.</returns>
        bool DisableReelDevice(Hardware deviceType);
    }
}
