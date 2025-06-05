//-----------------------------------------------------------------------
// <copyright file = "DeviceFeatureNumber.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Enumeration used by the device driver to
    /// indicate a feature.
    /// </summary>
    [Serializable]
    public enum DeviceFeatureNumber : ushort
    {
        /// <summary>
        /// Common feature present in every peripherals.
        /// </summary>
        FeatureZero = 0,

        /// <summary>
        /// Mechanical reels.
        /// </summary>
        Reel = 100,

        /// <summary>
        /// Peripheral lights.
        /// </summary>
        Light = 101,

        /// <summary>
        /// Button panel.
        /// </summary>
        ButtonPanel = 114,

        /// <summary>
        /// The streaming peripheral lights.
        /// </summary>
        StreamingLight = 121
    }
}
