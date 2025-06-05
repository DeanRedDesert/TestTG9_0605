// -----------------------------------------------------------------------
// <copyright file = "MessageReceiveFailureEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;

    /// <summary>
    /// Event arguments for a received message that fails to be delivered to the game.
    /// </summary>
    public class MessageReceiveFailureEventArgs : EventArgs
    {
        /// <summary>
        /// The serialized message header data.
        /// </summary>
        public string MessageHeaderData { get; }

        /// <summary>
        /// Validation data containing exceptions, partially deserialized data, and a failure reason.
        /// </summary>
        public GameMessageValidationData ValidationData { get; }

        /// <summary>
        /// Construct an instance of the MessageReceiveFailureEventArgs.
        /// </summary>
        /// <param name="validationData">Validation data containing information about the failed header.</param>
        /// <param name="headerData">The raw/serialized message header data.</param>
        public MessageReceiveFailureEventArgs(GameMessageValidationData validationData, string headerData)
        {
            ValidationData = validationData;
            MessageHeaderData = headerData;
        }
    }
}
