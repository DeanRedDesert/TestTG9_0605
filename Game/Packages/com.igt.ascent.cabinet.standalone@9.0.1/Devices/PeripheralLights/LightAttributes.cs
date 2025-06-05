//-----------------------------------------------------------------------
// <copyright file = "LightAttributes.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;

    /// <summary>
    /// Enumeration indicating the attributes of a light group.
    /// </summary>
    [Flags]
    internal enum LightAttributes : ushort
    {
        /// <summary>
        /// The hardware controls the lights using
        /// separate red, green and blue brightness.
        /// </summary>
        RgbControl = 0x8000,

        /// <summary>
        /// The hardware provides feedback to confirm
        /// that the lights are working.
        /// </summary>
        HardwareFeedback = 0x4000,

        /// <summary>
        /// This light group can track other devices.
        /// </summary>
        DeviceTracking = 0x2000,

        /// <summary>
        /// This group can show game (or bonus game) outcome.
        /// </summary>
        OutcomeDisplay = 0x1000,

        /// <summary>
        /// This group is autonomous capable.
        /// </summary>
        Autonomous = 0x0800
    }
}
