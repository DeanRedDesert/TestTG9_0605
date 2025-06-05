// -----------------------------------------------------------------------
// <copyright file = "BigWinGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a big win.
    /// </summary>
    public class BigWinGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;

        /// <summary>
        /// Gets or sets the identifier of the big win.
        /// </summary>
        public int BigWinId { get; set; }
    }
}