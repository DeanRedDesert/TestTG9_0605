// -----------------------------------------------------------------------
// <copyright file = "KenoWinCategoryOddsList.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the KenoWinCategoryOddsList class.
    /// </summary>
    public partial class KenoWinCategoryOddsList : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, odds);
            CompactSerializer.Write(stream, betRange);
            CompactSerializer.Write(stream, betBoundaryLower);
            CompactSerializer.Write(stream, betBoundaryUpper);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            odds = CompactSerializer.ReadFloat(stream);
            betRange = CompactSerializer.ReadEnum<KenoWinCategoryOddsListBetRange>(stream);
            betBoundaryLower = CompactSerializer.ReadUint(stream);
            betBoundaryUpper = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
