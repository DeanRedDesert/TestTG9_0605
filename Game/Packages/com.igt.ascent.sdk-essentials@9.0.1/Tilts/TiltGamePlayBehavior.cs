//-----------------------------------------------------------------------
// <copyright file = "TiltGamePlayBehavior.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    using System;

    /// <summary>
    /// An enumeration describing the game play behavior for the tilt.
    /// </summary>
    [Serializable]
    public enum TiltGamePlayBehavior
    {
        /// <summary>
        /// The tilt will block game play from starting.
        /// </summary>
        Blocking,

        /// <summary>
        /// The tilt will allow game play to start.
        /// </summary>
        NonBlocking,
    }
}