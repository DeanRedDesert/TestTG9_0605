//-----------------------------------------------------------------------
// <copyright file = "GlobalCommandCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Command code applicable to all IGT USB devices.
    /// </summary>
    [Serializable]
    internal enum GlobalCommandCode : byte
    {
        /// <summary>
        /// Reset the device.
        /// </summary>
        Reset = 0xFF,

        /// <summary>
        /// Reserved.
        /// </summary>
        Reserved = 0xFE,

        /// <summary>
        /// Clear the device status.
        /// </summary>
        ClearStatus = 0xFD,

        /// <summary>
        /// Do self test.
        /// </summary>
        SelfTest = 0xFC
    }
}
