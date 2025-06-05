//-----------------------------------------------------------------------
// <copyright file = "UsbDataFormat.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Enumeration used to specify the format of data
    /// in USB messages.
    /// </summary>
    [Serializable]
    internal enum UsbDataFormat : byte
    {
        /// <summary>
        /// Null-terminated ASCII string.
        /// </summary>
        Ascii = 0,

        /// <summary>
        /// Binary data.
        /// </summary>
        Binary = 1,

        /// <summary>
        /// BCD data.
        /// </summary>
        Bcd = 2,

        /// <summary>
        /// Unicode data.
        /// </summary>
        Unicode = 3,

        /// <summary>
        /// Feature specific.  Typically contains mixed data formats.
        /// </summary>
        FeatureSpecific = 0xFF
    }
}
