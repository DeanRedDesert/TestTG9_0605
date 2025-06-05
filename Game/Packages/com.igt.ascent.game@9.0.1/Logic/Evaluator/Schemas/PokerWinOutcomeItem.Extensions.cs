// -----------------------------------------------------------------------
// <copyright file = "PokerWinOutcomeItem.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the PokerWinOutcomeItem class.
    /// </summary>
    public partial class PokerWinOutcomeItem : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, Trigger);
            CompactSerializer.WriteList(stream, ProgressiveLevel);
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, handIndex);
            CompactSerializer.Write(stream, winLevelIndex);
            CompactSerializer.Write(stream, winAmount);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            Trigger = CompactSerializer.ReadListSerializable<Trigger>(stream);
            ProgressiveLevel = CompactSerializer.ReadListString(stream);
            name = CompactSerializer.ReadString(stream);
            handIndex = CompactSerializer.ReadUint(stream);
            winLevelIndex = CompactSerializer.ReadUint(stream);
            winAmount = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
