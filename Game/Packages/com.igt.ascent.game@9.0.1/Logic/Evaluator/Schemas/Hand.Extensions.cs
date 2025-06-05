// -----------------------------------------------------------------------
// <copyright file = "Hand.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the Hand class.
    /// </summary>
    public partial class Hand : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, DealtCardList);
            CompactSerializer.WriteList(stream, DrawnCardList);
            CompactSerializer.WriteList(stream, CardHeldList);
            CompactSerializer.Write(stream, numberOfCardsToDeal);
            CompactSerializer.Write(stream, numberOfCardsToDraw);
            CompactSerializer.Write(stream, numberOfCardsToEvaluate);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            DealtCardList = CompactSerializer.ReadListInt(stream);
            DrawnCardList = CompactSerializer.ReadListInt(stream);
            CardHeldList = CompactSerializer.ReadListBool(stream);
            numberOfCardsToDeal = CompactSerializer.ReadUint(stream);
            numberOfCardsToDraw = CompactSerializer.ReadUint(stream);
            numberOfCardsToEvaluate = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
