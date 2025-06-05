//-----------------------------------------------------------------------
// <copyright file = "LightCommandCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;

    /// <summary>
    /// Command code to control IGT light devices.
    /// </summary>
    [Serializable]
    internal enum LightCommandCode : byte
    {
        /// <summary>
        /// Start a light sequence.
        /// </summary>
        StartSequence = 0x00,

        /// <summary>
        /// Turn off lights.
        /// </summary>
        LightsOff = 0x01,

        /// <summary>
        /// Set the brightness for the specified lights.
        /// </summary>
        RandomMonochrome = 0x02,

        /// <summary>
        /// Set the brightness for a series of lights.
        /// </summary>
        ConsecutiveMonochrome = 0x03,

        /// <summary>
        /// Set the color for the specified lights.
        /// </summary>
        RandomRgb = 0x04,

        /// <summary>
        /// Set the color for a series of lights.
        /// </summary>
        ConsecutiveRgb = 0x05,

        /// <summary>
        /// Bitwise control the brightness or color for a series of lights.
        /// </summary>
        BitwiseControl = 0x06
    }
}
