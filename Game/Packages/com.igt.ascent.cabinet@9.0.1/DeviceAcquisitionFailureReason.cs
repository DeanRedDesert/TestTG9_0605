//-----------------------------------------------------------------------
// <copyright file = "DeviceAcquisitionFailureReason.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Enumeration which indicates the reason that a device acquisition failed.
    /// </summary>
    public enum DeviceAcquisitionFailureReason
    {
        /// <summary>
        /// The device is not connected and the request has been discarded.
        /// </summary>
        DeviceNotConnected,

        /// <summary>
        /// A higher priority client has control of the device. The request has been queued and when no higher priority
        /// clients desire the device control will be given to this client.
        /// </summary>
        RequestQueued,

        /// <summary>
        /// The device is in a tilted state.
        /// </summary>
        DeviceTilted
    }
}
