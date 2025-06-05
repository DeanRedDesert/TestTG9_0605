// -----------------------------------------------------------------------
// <copyright file = "ProgressiveWinGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a progressive win.
    /// </summary>
    public class ProgressiveWinGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;

        /// <summary>
        /// Gets or sets the level of the progressive.
        /// </summary>
        public int ProgressiveGameLevel { get; set; }
    }
}
