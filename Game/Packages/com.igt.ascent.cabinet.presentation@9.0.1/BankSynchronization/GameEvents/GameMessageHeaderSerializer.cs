// -----------------------------------------------------------------------
// <copyright file = "GameMessageHeaderSerializer.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Text;

    /// <summary>
    /// Serialization helper for game message headers.
    /// </summary>
    public class GameMessageHeaderSerializer : IGameMessageHeaderSerializer
    {
        /// <inheritdoc />
        public virtual string SerializeGameMessageHeader(GameMessageHeader header)
        {
            if(header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            // Serialize the header.
            using(var stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(GameMessageHeader));
                serializer.WriteObject(stream, header);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <inheritdoc />
        public virtual GameMessageHeader DeserializeGameMessageHeader(string header)
        {
            if(string.IsNullOrEmpty(header))
            {
                throw new ArgumentException("Header is null or empty.", nameof(header));
            }

            GameMessageHeader deserializedHeader = null;
            using(var stream = new MemoryStream(Encoding.UTF8.GetBytes(header)))
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
