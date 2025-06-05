// -----------------------------------------------------------------------
// <copyright file = "IGameModeQuery.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This interface defines the method to query the game mode.
    /// </summary>
    public interface IGameModeQuery
    {
        /// <summary>
        /// Gets the current game mode.
        /// </summary>
        GameMode GameMode { get; }
    }
}
