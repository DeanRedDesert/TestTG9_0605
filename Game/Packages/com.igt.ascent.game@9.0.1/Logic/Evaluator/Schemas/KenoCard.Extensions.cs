// -----------------------------------------------------------------------
// <copyright file = "KenoCard.Extensions.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Extensions to the Keno Card class.
    /// </summary>
    public partial class KenoCard : ICompactSerializable
    {
        #region ICompactSerializable Implementation

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.WriteList(stream, KenoSpotsList);
            CompactSerializer.WriteList(stream, KenoSpotsDrawnList);
            CompactSerializer.Write(stream, kenoCardIndex);
            CompactSerializer.Write(stream, numberOfSpotsMarked);
            CompactSerializer.Write(stream, numberOfSpotsToDraw);
            CompactSerializer.Write(stream, numberOfSpotsHit);
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            KenoSpotsList = CompactSerializer.ReadListSerializable<KenoSpot>(stream);
            KenoSpotsDrawnList = CompactSerializer.ReadListInt(stream);
            kenoCardIndex = CompactSerializer.ReadUint(stream);
            numberOfSpotsMarked = CompactSerializer.ReadUint(stream);
            numberOfSpotsToDraw = CompactSerializer.ReadUint(stream);
            numberOfSpotsHit = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
