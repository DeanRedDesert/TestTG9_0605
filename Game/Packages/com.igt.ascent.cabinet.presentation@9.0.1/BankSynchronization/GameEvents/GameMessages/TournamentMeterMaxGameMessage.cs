// -----------------------------------------------------------------------
// <copyright file = "TournamentMeterMaxGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a tournament meter being maxed.
    /// </summary>
    public class TournamentMeterMaxGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;
    }
}