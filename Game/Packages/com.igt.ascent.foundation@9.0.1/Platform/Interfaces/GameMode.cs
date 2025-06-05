// -----------------------------------------------------------------------
// <copyright file = "GameMode.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Type to indicate the game mode.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// The game mode is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// The game is in play mode.
        /// </summary>
        Play,

        /// <summary>
        /// The game is in history mode.
        /// </summary>
        History,

        /// <summary>
        /// The game is in utility mode.
        /// </summary>
        Utility
    }
}
