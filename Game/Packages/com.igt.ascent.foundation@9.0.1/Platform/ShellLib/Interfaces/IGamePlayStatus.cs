// -----------------------------------------------------------------------
// <copyright file = "IGamePlayStatus.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for game play status.
    /// </summary>
    public interface IGamePlayStatus
    {
        /// <summary>
        /// Occurs when the first coplayer game starts progress or
        /// the last coplayer game ends progress.
        /// </summary>
        event EventHandler<GameInProgressChangedEventArgs> GameInProgressChangedEvent;

        /// <summary>
        /// Occurs when the game focus is changed.
        /// </summary>
        event EventHandler<GameFocusChangedEventArgs> GameFocusChangedEvent;
        
        /// <summary>
        /// Gets the flag indicating whether or not any coplayer has a game currently in progress.
        /// </summary>
        bool GameInProgress { get; }

        /// <summary>
        /// Gets the current game focus.
        /// Null if no game is in focus.
        /// </summary>
        GameFocus GameFocus { get; }
    }
}