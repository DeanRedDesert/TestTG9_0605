//-----------------------------------------------------------------------
// <copyright file = "IHaptic.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Interface for the haptic category of the CSI.
    /// </summary>
    public interface IHaptic
    {
        /// <summary>
        /// Get a list of haptic devices available to be acquired through the CSI.
        /// </summary>
        /// <returns>
        /// Configuration information including all present haptic devices.
        /// </returns>
        IEnumerable<HapticDevice> GetAvailableHapticDevices();

        /// <summary>
        /// Queries the specified haptic device for its current status
        /// </summary>
        /// <returns>
        /// A HapticDeviceStatus enumeration value to indicate if the device is in a failure state or not
        /// </returns>
        HapticDeviceStatus GetHapticDeviceStatus (string deviceId);
    }
}