// -----------------------------------------------------------------------
// <copyright file = "HotStreakGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a player on a hot streak.
    /// </summary>
    public class HotStreakGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;
    }
}