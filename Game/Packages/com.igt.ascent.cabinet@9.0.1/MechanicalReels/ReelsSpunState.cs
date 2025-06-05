//-----------------------------------------------------------------------
// <copyright file = "ReelsSpunState.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Defines the states that reels currently affected by a spin command can be in.
    /// </summary>
    public enum ReelsSpunState
    {
        /// <summary>
        /// The idle state of the reel shelf, before any command is issued, and after the 'AllStopped'
        /// event is posted.
        /// </summary>
        AllWaitingForCommand,

        /// <summary>
        /// All affected reels have been started and are either accelerating or at constant speed.
        /// </summary>
        AllSpinningUp,

        /// <summary>
        /// All affected reels have reached constant speed, or a combination of constant and stopped speeds.
        /// </summary>
        AllCompletedSpinUp,

        /// <summary>
        /// All affected reels are either decelerating or have stopped.
        /// </summary>
        AllSpinningDown,

        /// <summary>
        /// All affected reels spun without attributes have stopped.
        /// </summary>
        AllStopped,

        /// <summary>
        /// All affected reels have started moving irregularly.
        /// </summary>
        AllMovingIrregularly
    }
}
