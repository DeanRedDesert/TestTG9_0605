//-----------------------------------------------------------------------
// <copyright file = "WeightedBlockStrip.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using CompactSerialization;

    /// <summary>
    /// Definition of a blocked reel strip including weights and frequency distortion. 
    /// </summary>
    [Serializable]
    public class WeightedBlockStrip : ICompactSerializable
    {
        #region Properties

        /// <summary>
        /// A List of block IDs. This represents the reel strip. 
        /// Each block ID references to the SymbolBlock defined in the BlockDefinitions.
        /// </summary>
        public IList<int> BlockStrip { get; private set; }

        /// <summary>
        /// Get a dictionary representing ID of the symbol block as key and the weight as value.
        /// The weight is proportional to how often the block will be shown as a stop symbol.
        /// </summary>
        public IDictionary<int, double> WeightTotals { get; private set; }

        /// <summary>
        /// Gets a dictionary containing the block ID as key and a frequency distortion factor as value.
        /// This frequency distortion factor influences how often a symbol or block is shown during spin.
        /// Due to jurisdictional reasons, this factor might or might not be applied from presentation.
        /// Frequency distortion must be greater 0.
        /// </summary>
        /// <remarks>
        /// All key/value pairs are optional. Not every block ID has to be presented here and the frequency distortion can also be an empty dictionary.
        /// A non-existing key value pair means that the block ID should be shown as often as defined by its weights.
        /// This is equivalent to a frequency distortion of 1.0.
        /// </remarks>
        // ReSharper disable once MemberCanBePrivate.Global
        public IDictionary<int, double> FrequencyDistortion { get; private set; }

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public WeightedBlockStrip()
        {
            BlockStrip = new List<int>();
            WeightTotals = new Dictionary<int, double>();
            FrequencyDistortion = new Dictionary<int, double>();
        }

        #region ICompactSerializable

        /// <inheritdoc />
        public void Serialize(System.IO.Stream stream)
        {
            DistortionSerializationHelper.WriteList(stream, BlockStrip);
            DistortionSerializationHelper.WriteDictionary(stream, WeightTotals);
            DistortionSerializationHelper.WriteDictionary(stream, FrequencyDistortion);
        }

        /// <inheritdoc />
        public void Deserialize(System.IO.Stream stream)
        {
            BlockStrip = DistortionSerializationHelper.ReadListInt(stream);
            WeightTotals = DistortionSerializationHelper.ReadDictionary(stream);
            FrequencyDistortion = DistortionSerializationHelper.ReadDictionary(stream);
        }

        #endregion
    }
}