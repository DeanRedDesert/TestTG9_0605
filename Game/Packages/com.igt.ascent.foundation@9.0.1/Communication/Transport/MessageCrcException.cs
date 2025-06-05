//-----------------------------------------------------------------------
// <copyright file = "MessageCrcException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that  there was a CRC error in a message header.
    /// </summary>
    public class MessageCrcException : Exception
    {
        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat =
            "Received Message has Invalid CRC. Type: {0} Message CRC: {1} Calculated CRC: {2}";

        /// <summary>
        /// The CRC contained in the message header.
        /// </summary>
        public uint MessageCrc { get; set; }

        /// <summary>
        /// A CRC calculated based on the contents of the header.
        /// </summary>
        public uint CalculatedCrc { get; set; }

        /// <summary>
        /// The type of message.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// Construct a MessageCrcException with the given parameters.
        /// </summary>
        /// <param name="type">The message type.</param>
        /// <param name="messageCrc">The CRC contained in the message header.</param>
        /// <param name="calculatedCrc">The CRC calculated based on the content of the header.</param>
        public MessageCrcException(MessageType type, uint messageCrc, uint calculatedCrc)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, type, messageCrc, calculatedCrc))
        {
            MessageType = type;
            MessageCrc = messageCrc;
            CalculatedCrc = calculatedCrc;
        }
    }
}