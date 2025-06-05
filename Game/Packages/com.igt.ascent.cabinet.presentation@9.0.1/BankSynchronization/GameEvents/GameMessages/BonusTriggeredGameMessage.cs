// -----------------------------------------------------------------------
// <copyright file = "BonusTriggeredGameMessage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents.GameMessages
{
    /// <summary>
    /// Game message for a bonus being triggered.
    /// </summary>
    public class BonusTriggeredGameMessage : IGameMessage
    {
        /// <inheritdoc />
        public int MessageVersion => 1;

        /// <summary>
        /// Gets or sets the identifier of the bonus triggered.
        /// </summary>
        public int BonusId { get; set; }

        /// <summary>
        /// Gets or sets if the trigger was a retrigger or not.
        /// </summary>
        public bool Retrigger { get; set; }
    }
}
