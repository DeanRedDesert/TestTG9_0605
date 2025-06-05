//-----------------------------------------------------------------------
// <copyright file = "ReelAttribute.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;

    /// <summary>
    /// The motion attribute for the reel.
    /// </summary>
    [Serializable]
    internal enum ReelAttribute : byte
    {
        /// <summary>
        /// Cock the reel before spin.
        /// </summary>
        Cock,

        /// <summary>
        /// Bounce the reel when stopping.
        /// </summary>
        Bounce,

        /// <summary>
        /// Shake the reel with low amplitude.
        /// </summary>
        ShakeLow,

        /// <summary>
        /// Shake the reel with medium amplitude.
        /// </summary>
        ShakeMedium,

        /// <summary>
        /// Shake the reel with high amplitude.
        /// </summary>
        ShakeHigh,

        /// <summary>
        /// Shake the reel with Max amplitude.
        /// </summary>
        ShakeMax,

        /// <summary>
        /// Hover around the final stop between two specified limits.
        /// </summary>
        HoverCustom,

        /// <summary>
        /// Hover around the final stop between 3 above and 1 below.
        /// </summary>
        HoverTop,

        /// <summary>
        /// Hover around the final stop between 2 above and 2 below.
        /// </summary>
        HoverCenter,

        /// <summary>
        /// Hover around the final stop between 1 above and 3 below.
        /// </summary>
        HoverBottom

        // All = 0xFF
    }
}
