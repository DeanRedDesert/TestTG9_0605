// -----------------------------------------------------------------------
// <copyright file = "Card.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the card class.
    /// </summary>
    public partial class Card : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, index);
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, suit);
            CompactSerializer.Write(stream, rank);
            CompactSerializer.Write(stream, isWild);
            CompactSerializer.Write(stream, isBonus);
            CompactSerializer.Write(stream, displayAttribute);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            index = CompactSerializer.ReadUint(stream);
            name = CompactSerializer.ReadString(stream);
            suit = CompactSerializer.ReadUint(stream);
            rank = CompactSerializer.ReadUint(stream);
            isWild = CompactSerializer.ReadBool(stream);
            isBonus = CompactSerializer.ReadBool(stream);
            displayAttribute = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
