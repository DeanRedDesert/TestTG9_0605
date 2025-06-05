//-----------------------------------------------------------------------
// <copyright file = "XmlMessageDeserializationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Thrown when there is an error de-serializing a message.
    /// </summary>
    [Serializable]
    public class XmlMessageDeserializationException : Exception
    {
        /// <summary>
        /// The XML message which could not be de-serialized.
        /// </summary>
        public string XmlMessage { get; private set; }

        /// <summary>
        /// The application message number.
        /// </summary>
        public uint MessageNumber { get; private set; }

        /// <summary>
        /// The API category the message is intended for.
        /// </summary>
        public MessageCategory Category { get; private set; }

        /// <summary>
        /// The channel the message was sent on.
        /// </summary>
        public Channel Channel { get; private set; }

        /// <summary>
        /// The transaction identifier for the message.
        /// </summary>
        public uint TransactionIdentifier { get; private set; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "Error de-serializing message. MessageNumber: {0} Category: {1} Channel: {2}  Transaction ID: {3} XmlMessage: {4}";

        /// <summary>
        /// Construct an instance of the exception with the given information.
        /// </summary>
        /// <param name="messageNumber">The sequential message number.</param>
        /// <param name="category">The message category.</param>
        /// <param name="channel">The channel that the message was sent on.</param>
        /// <param name="transactionIdentifier">The transaction identifier.</param>
        /// <param name="xmlMessage">The XML message being decoded when the error was encountered.</param>
        /// <param name="innerException">Exception encountered during deserialization.</param>
        public XmlMessageDeserializationException(uint messageNumber, MessageCategory category, Channel channel, uint transactionIdentifier, string xmlMessage, Exception innerException)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, messageNumber, category, channel, transactionIdentifier, xmlMessage), innerException)
        {
            Category = category;
            Channel = channel;
            MessageNumber = messageNumber;
            TransactionIdentifier = transactionIdentifier;
            XmlMessage = xmlMessage;
        }
    }
}
