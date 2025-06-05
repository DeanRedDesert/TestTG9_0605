//-----------------------------------------------------------------------
// <copyright file = "GlobalQueryCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Query code applicable to all IGT USB devices.
    /// </summary>
    [Serializable]
    internal enum GlobalQueryCode : byte
    {
        /// <summary>
        /// Get all true statuses of a device.
        /// </summary>
        GetStatus = 0xFF,

        /// <summary>
        /// Get string data records of a device.
        /// </summary>
        GetFeatureData = 0xFE
    }
}
