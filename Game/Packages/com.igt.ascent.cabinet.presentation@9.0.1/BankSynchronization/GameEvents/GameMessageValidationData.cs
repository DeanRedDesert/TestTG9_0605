// -----------------------------------------------------------------------
// <copyright file = "GameMessageValidationData.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    /// <summary>
    /// Data structure containing information about the message validation process.
    /// </summary>
    public class GameMessageValidationData
    {
        /// <summary>
        /// Constructs an instance of <see cref="GameMessageValidationData"/>
        /// </summary>
        /// <param name="failureReason">The failure reason associated with this validation.</param>
        /// <param name="header">The game message header, if any was deserialized.</param>
        public GameMessageValidationData(FailureReason failureReason, GameMessageHeader header)
        {
            FailureReason = failureReason;
            ReceivedHeader = header;
        }
        /// <summary>
        /// The reason the message receive failed.
        /// </summary>
        public FailureReason FailureReason
        {
            get;
        }

        /// <summary>
        /// The deserialized message header data. Null if deserialization failed.
        /// </summary>
        public GameMessageHeader ReceivedHeader
        {
            get;
        }
    }
}