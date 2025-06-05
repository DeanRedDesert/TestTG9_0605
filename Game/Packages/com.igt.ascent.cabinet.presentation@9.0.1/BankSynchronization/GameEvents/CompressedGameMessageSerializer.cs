// -----------------------------------------------------------------------
// <copyright file = "CompressedGameMessageSerializer.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using GameMessages;

    /// <summary>
    /// Serialization and compression helper for game messages.
    /// </summary>
    public class CompressedGameMessageSerializer : GameMessageSerializer
    {
        /// <summary>
        /// Serializes and compresses a game message.
        /// </summary>
        /// <param name="message">The game message to serialize.</param>
        /// <param name="uncompressedMessageLength">The length of the message before compression.</param>
        /// <returns>Returns the compressed byte array representation of the json serialized array.</returns>
        public byte[] SerializeGameMessage(IGameMessage message, out int uncompressedMessageLength)
        {
            var serializedMessage = SerializeGameMessage(message);
            uncompressedMessageLength = serializedMessage.Length;
            return CompressGameMessage(serializedMessage);
        }

        /// <summary>
        /// Decompresses and deserializes a game message.
        /// </summary>
        /// <param name="gameMessage">The game message to decompress and deserialize</param>
        /// <param name="messageType">The type of the game message being deserialized.</param>
        /// <param name="uncompressedMessageLength">The length of the message before compression.</param>
        /// <returns>Returns the decompressed and deserialized game message or null if the deserialization fails.</returns>
        public IGameMessage DeserializeGameMessage(byte[] gameMessage, Type messageType, int uncompressedMessageLength)
        {
            var decompressedGameMessage = DecompressGameMessage(gameMessage, uncompressedMessageLength);
            return DeserializeGameMessage(decompressedGameMessage, messageType);
        }

        /// <summary>
        /// Compresses a serialized game message.
        /// </summary>
        /// <param name="serializedMessage">The serialized game message to compress.</param>
        /// <returns>The compressed serialized message.</returns>
        /// <exception cref="ArgumentException">Thrown if message is null or message length is zero. </exception>
        private static byte[] CompressGameMessage(byte[] serializedMessage)
        {
            if (serializedMessage == null || serializedMessage.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", nameof(serializedMessage));
            }

            var outStream = new MemoryStream();

            // Compress the message into the underlying output stream.
            using (var compressionStream = new DeflateStream(outStream, CompressionMode.Compress))
            {
                compressionStream.Write(serializedMessage, 0, serializedMessage.Length);
                compressionStream.Close();
            }
            return outStream.ToArray();
        }

        /// <summary>
        /// Decompresses a compressed serialized game message.
        /// </summary>
        /// <param name="compressedMessage">The compressed serialized game message to decompress.</param>
        /// <param name="uncompressedMessageLength">The length of the message before compression.</param>
        /// <returns>The decompressed serialized message.</returns>
        /// <exception cref="ArgumentException">Thrown if compressedMessage is null or its length is zero. </exception>
        /// <exception cref="ArgumentException">Thrown if uncompressedMessageLength is zero. </exception>
        private static byte[] DecompressGameMessage(byte[] compressedMessage, int uncompressedMessageLength)
        {
            if (compressedMessage == null || compressedMessage.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", nameof(compressedMessage));
            }

            if (uncompressedMessageLength == 0)
            {
                throw new ArgumentException("Uncompressed message length is specified as 0.", nameof(uncompressedMessageLength));
            }

            var decompressedBytes = new byte[uncompressedMessageLength];

            // Wrap the compressed message in a memory stream.
            var outStream = new MemoryStream(compressedMessage);

            // Decompress the message into the output byte array.
            using (var decompressionStream = new DeflateStream(outStream, CompressionMode.Decompress))
            {
                decompressionStream.Read(decompressedBytes, 0, uncompressedMessageLength);
                decompressionStream.Close();
            }
            return decompressedBytes;
        }
    }
}
