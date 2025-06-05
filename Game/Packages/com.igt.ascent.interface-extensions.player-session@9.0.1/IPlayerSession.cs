//-----------------------------------------------------------------------
// <copyright file = "IPlayerSession.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;

    /// <summary>
    /// This interface defines APIs for player session interface extension.
    /// </summary>
    public interface IPlayerSession
    {
        /// <summary>
        /// Event raised when the player session status has changed.
        /// </summary>
        event EventHandler<PlayerSessionStatusChangedEventArgs> PlayerSessionStatusChangedEvent;

        /// <summary>
        /// Gets the current player session status.
        /// </summary>
        PlayerSessionStatus PlayerSessionStatus { get; }

        /// <summary>
        /// Gets the flag indicating whether or not the session timer display is enabled.
        /// </summary>
        bool SessionTimerDisplayEnabled { get; }
    }
}
