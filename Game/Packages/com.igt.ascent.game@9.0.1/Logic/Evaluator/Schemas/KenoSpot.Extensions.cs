// -----------------------------------------------------------------------
// <copyright file = "KenoSpot.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the keno spot class.
    /// </summary>
    public partial class KenoSpot : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, index);
            CompactSerializer.Write(stream, isPicked);
            CompactSerializer.Write(stream, isHit);
            CompactSerializer.Write(stream, isDrawn);
            CompactSerializer.Write(stream, isWild);
            CompactSerializer.Write(stream, isBonus);
            CompactSerializer.Write(stream, displayAttribute);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            index = CompactSerializer.ReadUint(stream);
            isPicked = CompactSerializer.ReadBool(stream);
            isHit = CompactSerializer.ReadBool(stream);
            isDrawn = CompactSerializer.ReadBool(stream);
            isWild = CompactSerializer.ReadBool(stream);
            isBonus = CompactSerializer.ReadBool(stream);
            displayAttribute = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
