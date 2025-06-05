// -----------------------------------------------------------------------
// <copyright file = "FailureReason.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    
    /// <summary>
    /// Enumeration indicating the reason a received game message was not sent to the game.
    /// </summary>
    public enum FailureReason
    {
        /// <summary>
        /// Game message header failed to deserialize.
        /// </summary>
        ErrorDeserializingHeader,

        /// <summary>
        /// No events were registered for the game message type.
        /// </summary>
        NoRegisteredListeners,

        /// <summary>
        /// Game message was not a known type.
        /// </summary>
        NotAKnownType,

        /// <summary>
        /// The message version was invalid.
        /// </summary>
        MessageVersionIncompatible,

        /// <summary>
        /// The game message failed to deserialize.
        /// </summary>
        ErrorDeserializingMessage,

        /// <summary>
        /// The sender key did not match any registered messages.
        /// </summary>
        SenderKeyMismatch,

        /// <summary>
        /// No error.
        /// </summary>
        None
    }
}