// -----------------------------------------------------------------------
// <copyright file = "AcquisitionState.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    /// <summary>
    /// Different ownership states available for a device.
    /// </summary>
    public enum AcquisitionState
    {
        /// <summary>
        /// This device is not owned and has not been requested.
        /// </summary>
        NotRequested,

        /// <summary>
        /// This device is owned and may be used.
        /// </summary>
        Acquired,

        /// <summary>
        /// This device has been requested, and is connected, but isn't acquired yet.
        /// </summary>
        AcquirePending,

        /// <summary>
        /// This device has been requested, but isn't connected to the cabinet.
        /// </summary>
        RequestedNotConnected
    }
}