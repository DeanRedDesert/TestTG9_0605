// -----------------------------------------------------------------------
// <copyright file = "GameMessageSerializer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using GameMessages;

    /// <summary>
    /// Serialization helper for game messages.
    /// </summary>
    public class GameMessageSerializer
    {
        /// <summary>
        /// Serializes a game message.
        /// </summary>
        /// <param name="message">The game message to serialize.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="message"/> is null.</exception>
        /// <returns>Returns the byte array representation of the json serialized array.</returns>
        public byte[] SerializeGameMessage(IGameMessage message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            using(var stream = new MemoryStream())
            {
                var type = message.GetType();
                var serializer = new DataContractJsonSerializer(type);
                serializer.WriteObject(stream, message);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserializes a game message.
        /// </summary>
        /// <param name="gameMessage">The game message to deserialize</param>
        /// <param name="messageType">The type of the game message being deserialized.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="gameMessage"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="messageType"/> is does not implement IGameMessage.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="messageType"/> is null.</exception>
        /// <returns>Returns the deserialized game message or null if the deserialization fails.</returns>
        public IGameMessage DeserializeGameMessage(byte[] gameMessage, Type messageType)
        {
            if(gameMessage == null || gameMessage.Length == 0)
            {
                throw new ArgumentException("Message is null or empty.", nameof(gameMessage));
            }
            if(messageType == null)
            {
                throw new ArgumentNullException(nameof(messageType));
            }
            if(messageType.GetInterfaces().Contains(typeof(IGameMessage)) == false)
            {
                throw new ArgumentException("Type does not implement IGameMessage.", nameof(messageType));
            }

            IGameMessage deserializedMessage = null;

            using(var stream = new MemoryStream(gameMessage))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(messageType);
                    deserializedMessage = serializer.ReadObject(stream) as IGameMessage;
                }
                catch(SerializationException)
                {
                    // Corrupted data would cause a serialization exception, however this should not cause a crash.
                    // Use null message and false return to indicate failure.
                }
            }
            return deserializedMessage;
        }
    }
}
