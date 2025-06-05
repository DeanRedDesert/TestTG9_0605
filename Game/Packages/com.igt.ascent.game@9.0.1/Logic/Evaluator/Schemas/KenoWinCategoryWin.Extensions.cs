// -----------------------------------------------------------------------
// <copyright file = "KenoWinCategoryWin.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the KenoWinCategoryWin class.
    /// </summary>
    public partial class KenoWinCategoryWin : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Trigger);
            CompactSerializer.Write(stream, winLevelIndex);
            CompactSerializer.Write(stream, multiplier);
            CompactSerializer.Write(stream, progressiveBetRange);
            CompactSerializer.Write(stream, progressiveBetRangeSpecified);
            CompactSerializer.Write(stream, betRange);
            CompactSerializer.Write(stream, betBoundaryLower);
            CompactSerializer.Write(stream, betBoundaryUpper);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
            multiplier = CompactSerializer.ReadUint(stream);
            progressiveBetRange = CompactSerializer.ReadEnum<KenoWinCategoryWinProgressiveBetRange>(stream);
            progressiveBetRangeSpecified = CompactSerializer.ReadBool(stream);
            betRange = CompactSerializer.ReadEnum<KenoWinCategoryWinBetRange>(stream);
            betBoundaryLower = CompactSerializer.ReadUint(stream);
            betBoundaryUpper = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
