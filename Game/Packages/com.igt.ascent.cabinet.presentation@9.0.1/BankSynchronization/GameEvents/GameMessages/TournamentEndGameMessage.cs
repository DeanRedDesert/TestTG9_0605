// -----------------------------------------------------------------------
// <copyright file = "TournamentEndGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a tournament ending.
    /// </summary>
    public class TournamentEndGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;

        /// <summary>
        /// Gets or sets the time at which the tournament will end.
        /// </summary>
        public long EndTime { get; set; }
    }
}