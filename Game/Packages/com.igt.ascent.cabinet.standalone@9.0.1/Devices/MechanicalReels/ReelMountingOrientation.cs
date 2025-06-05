//-----------------------------------------------------------------------
// <copyright file = "ReelMountingOrientation.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;

    /// <summary>
    /// Enumeration used to specify the orientation of how reels are mounted.
    /// </summary>
    [Serializable]
    public enum ReelMountingOrientation : byte
    {
        /// <summary>
        /// Normal, default orientation.
        /// </summary>
        Normal,

        /// <summary>
        /// Slant top orientation.  The reel feature will automatically
        /// reverse the spin direction and adjusts stop position for
        /// better player view of the results.
        /// </summary>
        SlantTop
    }
}
