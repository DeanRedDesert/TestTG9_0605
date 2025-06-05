// -----------------------------------------------------------------------
// <copyright file = "PokerWinCategory.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the PokerWinCategory class.
    /// </summary>
    public partial class PokerWinCategory : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Win);
            CompactSerializer.WriteList(stream, OddsList);
            CompactSerializer.Write(stream, name);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Win = CompactSerializer.ReadListSerializable<PokerWinCategoryWin>(stream);
            OddsList = CompactSerializer.ReadListSerializable<PokerWinCategoryOddsList>(stream);
            name = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}
