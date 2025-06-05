// -----------------------------------------------------------------------
// <copyright file = "IGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Interface for game messages.
    /// </summary>
    public interface IGameMessage
    {
        /// <summary>
        /// Gets and sets the message version.
        /// </summary>
        int MessageVersion { get; }
    }
}

