// -----------------------------------------------------------------------
// <copyright file = "IGameMessageHeaderSerializer.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    using System;

    /// <summary>
    /// Interface for serializing game message headers.
    /// </summary>
    public interface IGameMessageHeaderSerializer
    {
        /// <summary>
        /// Serializes a game message header.
        /// </summary>
        /// <param name="header">The game message header to serialize.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="header"/> is null.</exception>
        /// <returns>Returns the JSON string data of the serialized header.</returns>
        string SerializeGameMessageHeader(GameMessageHeader header);

        /// <summary>
        /// Deserializes a game message header.
        /// </summary>
        /// <param name="header">The game message header string to deserialize.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="header"/> is null or empty.</exception>
        /// <returns>Returns the deserialized header or null if the deserialization fails.</returns>
        GameMessageHeader DeserializeGameMessageHeader(string header);
    }
}
