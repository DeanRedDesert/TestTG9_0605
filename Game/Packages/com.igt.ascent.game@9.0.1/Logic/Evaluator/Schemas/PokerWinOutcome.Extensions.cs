// -----------------------------------------------------------------------
// <copyright file = "PokerWinOutcome.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the PokerWinOutcome class.
    /// </summary>
    public partial class PokerWinOutcome : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, PokerWinOutcomeItems);
            CompactSerializer.WriteList(stream, HandList);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            PokerWinOutcomeItems = CompactSerializer.ReadListSerializable<PokerWinOutcomeItem>(stream);
            HandList = CompactSerializer.ReadListSerializable<Hand>(stream);
        }

        #endregion
    }
}
