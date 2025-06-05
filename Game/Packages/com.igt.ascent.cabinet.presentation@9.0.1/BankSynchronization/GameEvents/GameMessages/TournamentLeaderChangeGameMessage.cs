// -----------------------------------------------------------------------
// <copyright file = "TournamentLeaderChangeGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a tournament leader changing.
    /// </summary>
    public class TournamentLeaderChangeGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;
    }
}