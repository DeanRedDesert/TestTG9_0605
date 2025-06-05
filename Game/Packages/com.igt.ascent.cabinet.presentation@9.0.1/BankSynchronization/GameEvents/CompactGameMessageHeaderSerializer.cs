// -----------------------------------------------------------------------
// <copyright file = "CompactGameMessageHeaderSerializer.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;

    /// <summary>
    /// Compact serialization helper for game message headers.
    /// It uses a base 64 string algorithm to encode/decode game message header and its game message body to/from string type.
    /// Encoded string will have following structure:
    ///     "Convert.ToBase64String(GameMessageHeader) + message separator + Convert.ToBase64String(GameMessageBody)"
    /// </summary>
    public class CompactGameMessageHeaderSerializer : GameMessageHeaderSerializer
    {
        /// <summary>
        /// Game Message Separator which separate serialized message header and its serialized body.
        /// </summary>
        private const char MessageSeparator = '-';

        /// <inheritdoc />
        public override string SerializeGameMessageHeader(GameMessageHeader header)
        {
            if(header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            var headerWithoutGameMessage = new GameMessageHeader(
                header.SenderThemeId,
                new byte[0],
                header.MessageType,
                header.MessageVersion,
                header.HeaderVersion,
                header.UncompressedMessageLength
            );

            // Serialize the header.
            return Convert.ToBase64String(CompressMessage(SerializeMessage(headerWithoutGameMessage)))
                   + MessageSeparator
                   + Convert.ToBase64String(header.GameMessage);
        }

        /// <inheritdoc />
        public override GameMessageHeader DeserializeGameMessageHeader(string header)
        {
            if(string.IsNullOrEmpty(header))
            {
                throw new ArgumentException("Header is null or empty.", nameof(header));
            }

            GameMessageHeader deserializedHeader;
            try
            {
                deserializedHeader = UnpackGameMessageHeader(header);
            }
            catch(Exception)
            {
                // If compact deserializer fails, then try with the standard game message header deserialization.
                deserializedHeader = base.DeserializeGameMessageHeader(header);
            }

            return deserializedHeader;
        }

        /// <summary>
        /// Unpack and deserialize game message header.
        /// </summary>
        /// <param name="header">Base 64 encoded string with pure game message header and game message body.</param>
        /// <exception cref="ArgumentException">Thrown if error occurs when unpacking the message header. </exception>
        /// <returns>Unpacked and deserialized game message header.</returns>
        private GameMessageHeader UnpackGameMessageHeader(string header)
        {
            var messageParts = header.Split(MessageSeparator);
            if(messageParts.Length != 2)
            {
                throw new ArgumentException($"{nameof(header)} string should contain only two parts separated with '{MessageSeparator}'.");
            }

            var gameMessageHeaderArray = Convert.FromBase64String(messageParts[0]);
            var gameMessageArray = Convert.FromBase64String(messageParts[1]);
            var gameMessageHeader = DeserializeMessage(DecompressMessage(gameMessageHeaderArray));

            GameMessageHeader completeGameMessageHeader = null;
            if(gameMessageHeader != null)
            {
                // Add message body to the game message header.
                completeGameMessageHeader = new GameMessageHeader(gameMessageHeader.SenderThemeId,
                                                                  gameMessageArray,
                                                                  gameMessageHeader.MessageType,
                                                                  gameMessageHeader.MessageVersion,
                                                                  gameMessageHeader.HeaderVersion,
                                                                  gameMessageHeader.UncompressedMessageLength);
            }

            return completeGameMessageHeader;
        }

        /// <summary>
        /// Compresses a byte array message.
        /// </summary>
        /// <param name="message">Byte array message to compress.</param>
        /// <exception cref="ArgumentException">Thrown if input message is null. </exception>
        /// <returns>The compressed message.</returns>
        private static byte[] CompressMessage(byte[] message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            // Compress the message into the underlying output stream.
            var outStream = new MemoryStream();
            using(var compressionStream = new DeflateStream(outStream, CompressionMode.Compress))
            {

                compressionStream.Write(message, 0, message.Length);
                compressionStream.Close();
            }
            return outStream.ToArray();
        }

        /// <summary>
        /// Decompresses a byte array message.
        /// </summary>
        /// <param name="message">The byte array message to decompress.</param>
        /// <exception cref="ArgumentException">Thrown if message is null or its length is zero. </exception>
        /// <returns>The decompressed message.</returns>
        private byte[] DecompressMessage(byte[] message)
        {
            if(message == null || message.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", nameof(message));
            }

            // Decompress the message into the underlying output stream.
            var uncompressedStream = new MemoryStream();
            var compressedStream = new MemoryStream(message);
            using(var ds = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                ds.CopyTo(uncompressedStream);
            }
            return uncompressedStream.ToArray();
        }

        /// <summary>
        /// Serialize the game message header.
        /// </summary>
        /// <param name="messageHeader">Game message header to serialize.</param>
        /// <returns>The serialized message.</returns>
        /// <exception cref="ArgumentException">Thrown if message header is null. </exception>
        private byte[] SerializeMessage(GameMessageHeader messageHeader)
        {
            if(messageHeader == null)
            {
                throw new ArgumentNullException(nameof(messageHeader));
            }

            // Serialize the message into the underlying output stream.
            using(var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(GameMessageHeader));
                serializer.WriteObject(stream, messageHeader);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize to game message header.
        /// </summary>
        /// <param name="serializedMessage">The message to deserialize.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="serializedMessage"/> is null or empty.</exception>
        /// <returns>Returns the deserialized game message header or null if deserialization fails.</returns>
        private GameMessageHeader DeserializeMessage(byte[] serializedMessage)
        {
            if(serializedMessage == null || serializedMessage.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", nameof(serializedMessage));
            }

            GameMessageHeader deserializedHeader = null;
            using(var stream = new MemoryStream(serializedMessage))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(GameMessageHeader));
                    deserializedHeader = (GameMessageHeader)serializer.ReadObject(stream);
                }
                catch(SerializationException)
                {
                    // Corrupted data would cause a serialization exception, however this should not cause a crash.
                    // Use null message and false return to indicate failure.
                }
            }
            return deserializedHeader;
        }
    }
}
