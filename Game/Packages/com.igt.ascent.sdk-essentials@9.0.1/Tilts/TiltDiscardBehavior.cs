//-----------------------------------------------------------------------
// <copyright file = "TiltDiscardBehavior.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;

    /// <summary>
    /// An enumeration defining the discard behavior for a tilt.
    /// </summary>
    [Serializable]
    public enum TiltDiscardBehavior
    {
        /// <summary>
        /// The tilt will never be discarded.
        /// </summary>
        Never,

       /// <summary>
        /// The tilt will be discarded when the game is inactivated, or otherwise terminated.
        /// </summary>
        OnGameTermination,
    }
}
