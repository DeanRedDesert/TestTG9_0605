// -----------------------------------------------------------------------
// <copyright file = "KenoWinOutcomeItem.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the KenoWinOutcomeItem class.
    /// </summary>
    public partial class KenoWinOutcomeItem : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Trigger);
            CompactSerializer.WriteList(stream, ProgressiveLevel);
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, cardIndex);
            CompactSerializer.Write(stream, winLevelIndex);
            CompactSerializer.Write(stream, winAmount);
            CompactSerializer.Write(stream, nearHitProgressive);
            CompactSerializer.Write(stream, topCategoryHit);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            name = CompactSerializer.ReadString(stream);
            cardIndex = CompactSerializer.ReadUint(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
            winAmount = CompactSerializer.ReadLong(stream);
            nearHitProgressive = CompactSerializer.ReadBool(stream);
            topCategoryHit = CompactSerializer.ReadBool(stream);
        }

        #endregion
    }
}
