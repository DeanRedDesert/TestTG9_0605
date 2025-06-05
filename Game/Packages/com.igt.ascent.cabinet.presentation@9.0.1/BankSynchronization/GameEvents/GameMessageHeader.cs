// -----------------------------------------------------------------------
// <copyright file = "GameMessageHeader.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Header data including all information about a game message necessary for validating the message when it is
    /// received.
    /// </summary>
    [DataContract]
    public class GameMessageHeader
    {
        /// <summary>
        /// Gets the theme id of the sending game.
        /// </summary>
        [DataMember]
        public string SenderThemeId { get; private set; }

        /// <summary>
        /// Gets the serialized game message.
        /// </summary>
        [DataMember]
        public byte[] GameMessage { get; private set; }

        /// <summary>
        /// Gets the message type of the included message.
        /// </summary>
        [DataMember]
        public string MessageType { get; private set; }

        /// <summary>
        /// Gets the message version of the included message.
        /// </summary>
        [DataMember]
        public int MessageVersion { get; private set; }

        /// <summary>
        /// Gets the version of this header.
        /// </summary>
        [DataMember]
        public int HeaderVersion { get; private set; }

        /// <summary>
        /// Gets the length of the game message without compression.
        /// </summary>
        /// <remarks>
        /// Version 1 of the header uses a compressed game message and this is used to uncompress it.
        /// </remarks>
        [DataMember]
        public int UncompressedMessageLength { get; private set; }

        /// <summary>
        /// Constructs a GameMessageHeader.
        /// </summary>
        /// <param name="senderThemeId">Theme id of the sending game.</param>
        /// <param name="gameMessage">The serialized game message.</param>
        /// <param name="messageType">The message type of the included message.</param>
        /// <param name="messageVersion">The message version of the included message.</param>
        /// <param name="headerVersion">The header version.</param>
        /// <param name="uncompressedMessageLength">The original length of the message without compression.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="senderThemeId"/>, <paramref name="gameMessage"/>, or 
        /// <paramref name="messageType"/> is null.
        /// </exception>
        public GameMessageHeader(string senderThemeId, byte[] gameMessage, string messageType, 
            int messageVersion, int headerVersion, int uncompressedMessageLength)
        {
            SenderThemeId = senderThemeId ?? throw new ArgumentNullException(nameof(senderThemeId));
            GameMessage = gameMessage ?? throw new ArgumentNullException(nameof(gameMessage));
            MessageType = messageType ?? throw new ArgumentNullException(nameof(messageType));
            MessageVersion = messageVersion;
            HeaderVersion = headerVersion;
            UncompressedMessageLength = uncompressedMessageLength == 0 ? gameMessage.Length : uncompressedMessageLength;
        }
    }
}
