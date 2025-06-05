//-----------------------------------------------------------------------
// <copyright file = "ReelQueryCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;

    /// <summary>
    /// Query code to query IGT reel devices.
    /// </summary>
    [Serializable]
    internal enum ReelQueryCode : byte
    {
        /// <summary>
        /// Get all deceleration profiles supported by the reel.
        /// </summary>
        GetDecelerationProfiles = 0x00,

        /// <summary>
        /// Get all speeds supported by the reel.
        /// </summary>
        GetSpeeds = 0x01,

        /// <summary>
        /// Get all accel / decel profiles supported by the reel.
        /// </summary>
        GetAccelDecelProfiles = 0x03,

        /// <summary>
        /// Get the current reel status.
        /// </summary>
        GetStatus = 0xFF
    }
}