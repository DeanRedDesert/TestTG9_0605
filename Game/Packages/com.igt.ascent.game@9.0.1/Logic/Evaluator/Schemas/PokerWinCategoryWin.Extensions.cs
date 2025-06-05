// -----------------------------------------------------------------------
// <copyright file = "PokerWinCategoryWin.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the PokerWinCategoryWin class.
    /// </summary>
    public partial class PokerWinCategoryWin : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, ProgressiveLevel);
            CompactSerializer.WriteList(stream, Trigger);
            CompactSerializer.Write(stream, winLevelIndex);
            CompactSerializer.Write(stream, multiplier);
            CompactSerializer.Write(stream, betRange);
            CompactSerializer.Write(stream, betBoundaryLower);
            CompactSerializer.Write(stream, betBoundaryUpper);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
            multiplier = CompactSerializer.ReadUint(stream);
            betRange = CompactSerializer.ReadEnum<PokerWinCategoryWinBetRange>(stream);
            betBoundaryLower = CompactSerializer.ReadUint(stream);
            betBoundaryUpper = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
