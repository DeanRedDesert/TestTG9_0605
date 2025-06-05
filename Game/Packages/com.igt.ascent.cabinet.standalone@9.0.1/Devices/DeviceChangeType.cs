//-----------------------------------------------------------------------
// <copyright file = "DeviceChangeType.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    /// <summary>
    /// Enumeration which indicates the type of a device change.
    /// </summary>
    internal enum DeviceChangeType
    {
        /// <summary>
        /// A device has been inserted.
        /// </summary>
        Arrival,

        /// <summary>
        /// A device has been removed.
        /// </summary>
        Removal
    }
}
