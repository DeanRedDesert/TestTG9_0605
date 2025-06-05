//-----------------------------------------------------------------------
// <copyright file = "DistortionPaytableProvider.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;
    using Services;

    /// <summary>
    /// Provider which exposes reel strips as a list of blocks including
    /// block, weight and distortion specific information.
    /// </summary>
    public class DistortionPaytableProvider
    {

        #region Nested Classes and Fields.

        /// <summary>
        /// Data structure needed for efficient search of existing blocks inside the reel strip.
        /// </summary>
        private class SymbolToBlockId
        {
            public int? BlockId;
            public readonly Dictionary<string, SymbolToBlockId> NextSymbol = new Dictionary<string, SymbolToBlockId>();
        }

        private SymbolToBlockId symbolToBlockIdRoot;
        private int nextFreeBlockId;

        #endregion

        #region Game Services.

        /// <summary>
        /// Gets the definition of each block per Block ID.
        /// Key is the Block ID, value is the definition of the block.
        /// </summary>
        [GameService]
        // ReSharper disable once MemberCanBePrivate.Global
        public IDictionary<int, SymbolBlock> BlockDefinitions { get; private set;  }

        /// <summary>
        /// Gets the symbol type per symbol ID.
        /// Key is the symbol ID, value is the symbol type.
        /// </summary>
        [GameService]
        // ReSharper disable once MemberCanBePrivate.Global
        public IDictionary<string, SymbolType> SymbolTypes { get; private set; }

        /// <summary>
        /// Gets or sets all reel strips containing block, weight and distortion information.
        /// </summary>
        [GameService]
        // ReSharper disable once MemberCanBePrivate.Global
        public IList<WeightedBlockStrip> WeightedBlockStrips { get; set; }

        /// <summary>
        /// Gets or sets the definition of how reels are linked together.
        /// Key is the index of the master reel.
        /// Value is a list of indices of the linked reels linked to the master reel.
        /// </summary>
        [GameService]
        // ReSharper disable once MemberCanBePrivate.Global
        public IDictionary<int, IList<int>> LinkedReelDefinition { get; set; }

        #endregion

        #region Constructors.

        /// <summary>
        /// Create a paytable provider for distortion without any arguments.
        /// </summary>
        public DistortionPaytableProvider()
            : this(new Dictionary<int, SymbolBlock>(), new Dictionary<string, SymbolType>())
        {
        }

        /// <summary>
        /// Create a paytable provider for distortion with a given Paytable Section.
        /// </summary>
        /// <param name="paytableSection">The paytable section to use for the provider.</param>
        public DistortionPaytableProvider(SlotPaytableSection paytableSection)
            : this(paytableSection, new Dictionary<int, SymbolBlock>(), new Dictionary<string, SymbolType>())
        {
        }

        /// <summary>
        /// Create a paytable provider with a given paytable section and given block definitions.
        /// All symbols available on the reel strip that are not defined in block definitions will
        /// be added to the block definitions as one symbol size blocks and represented as such in the block strip.
        /// </summary>
        /// <param name="paytableSection">The paytable section to use for the provider.</param>
        /// <param name="blockDefinitions">Predefined structure of the blocks used in the game. Key is the block Id, value is the block definition.</param>
        /// <param name="symbolTypes">Definition of the symboltype per symbolID.</param>
        /// <remarks>
        /// Blocks given in the blockDefinitions will be automatically considered.
        /// Any sequence of symbols on the reel strip that corresponds to any predefined block will be identified and marked as one block.
        /// In case that two or more blocks match the sequence (e.g., reel strip is "AAA", Blocks are "AAA" and "AA") then the longer block is taken.
        /// Any symbol or sequence not defined in this structure will be automatically defined as blocks with one symbol.
        /// </remarks>
        public DistortionPaytableProvider(SlotPaytableSection paytableSection, 
                                          IDictionary<int, SymbolBlock> blockDefinitions,
                                           IDictionary<string, SymbolType> symbolTypes) 
            : this(blockDefinitions, symbolTypes)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection");
            }

            if(paytableSection.SymbolWindow == null || paytableSection.SymbolWindow.PopulationEntry == null)
            {
                throw new PaytableException("SymbolWindow", "A SlotPaytableSection must contain a SymbolWindow.");
            }

            if(paytableSection.StripList == null || paytableSection.StripList.Strip == null)
            {
                throw new PaytableException("StripList", "A SlotPaytableSection must contain a StripList");
            }

            SetBlockStrips(paytableSection.SymbolWindow.PopulationEntry, paytableSection.StripList.Strip);
        }

        /// <summary>
        /// Create a paytable provider with given block definitions.
        /// </summary>
        /// <param name="blockDefinitions">Predefined structure of the blocks used in the game.</param>
        /// <param name="symbolTypes">Definition of the symboltype per symbol ID.</param>
        public DistortionPaytableProvider(IDictionary<int, SymbolBlock> blockDefinitions, IDictionary<string, SymbolType> symbolTypes)
        {
            if(blockDefinitions == null)
            {
                throw new ArgumentNullException("blockDefinitions");
            }
            if(symbolTypes == null)
            {
                throw new ArgumentNullException("symbolTypes");
            }
            WeightedBlockStrips = new List<WeightedBlockStrip>();
            BlockDefinitions = new Dictionary<int, SymbolBlock>(blockDefinitions);
            SymbolTypes = new Dictionary<string, SymbolType>(symbolTypes);
            LinkedReelDefinition = new Dictionary<int, IList<int>>();

            InitInternalDatastructure();
        }

        #endregion

        #region Private Methods.

        /// <summary>
        /// Update private members on initialization of the class.
        /// </summary>
        private void InitInternalDatastructure()
        {
            symbolToBlockIdRoot = new SymbolToBlockId();
            nextFreeBlockId = 0;
            foreach(var block in BlockDefinitions)
            {
                var current = symbolToBlockIdRoot;
                foreach(var symbol in block.Value.Symbols)
                {
                    if(!current.NextSymbol.ContainsKey(symbol))
                    {
                        current.NextSymbol[symbol] = new SymbolToBlockId();
                    }
                    current = current.NextSymbol[symbol];
                }
                current.BlockId = block.Key;
                nextFreeBlockId = Math.Max(block.Key + 1, nextFreeBlockId);
            }
        }

        /// <summary>
        /// Set the block strips specified by the mappings to the provider and the global block definition.
        /// </summary>
        /// <param name="mappings">Mappings to determine strip order.</param>
        /// <param name="stripList">Strips to add to the provider.</param>
        /// <exception cref="PaytableException">Thrown if the give mappings do not have at least 1 entry or mappings and stripList do not match.</exception>
        private void SetBlockStrips(IEnumerable<CellPopulationPopulationEntry> mappings, ICollection<Strip> stripList)
        {
            WeightedBlockStrips.Clear();
            if(stripList != null && stripList.Any())
            {
                foreach(var reelMapping in mappings)
                {
                    var reelStrip = stripList.FirstOrDefault(strip => strip.name == reelMapping.stripName);
                    if(reelStrip == null)
                    {
                        throw new PaytableException("StripList", "The StripList did not contain the strip referenced in the mapping (" + reelMapping.stripName + ").");
                    }

                    // Add it to the final data structure.
                    WeightedBlockStrips.Add(GetBlockStripFromReelStrip(reelStrip.Stop));
                }
            }
        }

        /// <summary>
        /// Convert a reel strip to a block strip using and updating the global block info.
        /// </summary>
        /// <param name="strip">Reel strip to convert.</param>
        /// <returns>The equivalent block strip.</returns>
        private WeightedBlockStrip GetBlockStripFromReelStrip(IList<StopType> strip)
        {
            var blockStrip = new WeightedBlockStrip();

            for (var reelStripIndex = 0; 
                reelStripIndex < strip.Count; 
                reelStripIndex += AddNextBlock(reelStripIndex, strip, blockStrip))
            {
            }

            return blockStrip;
        }

        /// <summary>
        /// Add one block starting at startIndex from the strip to block strip.
        /// </summary>
        /// <param name="startIndex">Index to start with.</param>
        /// <param name="strip">The strip the block should be identified.</param>
        /// <param name="blockStrip">The blockstrip the found block will be set.</param>
        /// <returns>Length of the block that has been added.</returns>
        private int AddNextBlock(int startIndex, IList<StopType> strip, WeightedBlockStrip blockStrip)
        {
            var currentSymbolToBlockId = symbolToBlockIdRoot;
            int? validBlockId = null;
            var validBlockSize = 0;
            double validBlockWeight = 0;
            var weightSum = 0;

            // Identify next valid block.
            for(var innerStripIndex = startIndex;
                innerStripIndex < strip.Count
                && currentSymbolToBlockId.NextSymbol.TryGetValue(strip[innerStripIndex].id, out currentSymbolToBlockId);
                innerStripIndex++)
            {
                weightSum += strip[innerStripIndex].weight;

                if(currentSymbolToBlockId.BlockId.HasValue)
                {
                    // We have identified a valid block. Remember this one as fallback but 
                    // the search continues since we are interested in the longest possible block.
                    validBlockId = currentSymbolToBlockId.BlockId;
                    validBlockSize = innerStripIndex - startIndex + 1;
                    validBlockWeight = (double)weightSum / validBlockSize;
                }
            }

            // If no valid block has been found, create a new block consisting of just a single symbol.
            if(!validBlockId.HasValue)
            {
                validBlockId = AddBlockWithSingleSymbolToBlockDefinitions(strip[startIndex].id);
                validBlockSize = 1;
                validBlockWeight = strip[startIndex].weight;
            }

            // Add the found block to the block strip.
            AddBlockIdToBlockStrip(blockStrip, validBlockId.Value, validBlockWeight);
            return validBlockSize;
        }

        /// <summary>
        /// Add weight for a specific block ID to the block strip structure.
        /// </summary>
        /// <param name="blockStrip">The block strip structure.</param>
        /// <param name="blockId">The block ID to add.</param>
        /// <param name="weight">The weight to add.</param>
        private static void AddBlockIdToBlockStrip(WeightedBlockStrip blockStrip, int blockId, double weight)
        {
            blockStrip.BlockStrip.Add(blockId);
            if(!blockStrip.WeightTotals.ContainsKey(blockId))
            {
                blockStrip.WeightTotals[blockId] = weight;
            }
            else
            {
                blockStrip.WeightTotals[blockId] += weight;
            }
        }

        /// <summary>
        /// Add a block with a single symbol to the Block Definitions and its search tree "symbolToBlockId".
        /// </summary>
        /// <param name="symbolToAdd">The symbol to add.</param>
        /// <returns>Block ID the symbol has been added.</returns>
        private int AddBlockWithSingleSymbolToBlockDefinitions(string symbolToAdd)
        {
            SymbolToBlockId nextSymbol;
            if(!symbolToBlockIdRoot.NextSymbol.TryGetValue(symbolToAdd, out nextSymbol))
            {
                nextSymbol = new SymbolToBlockId();
                symbolToBlockIdRoot.NextSymbol[symbolToAdd] = nextSymbol;
            }
            var hasValue = nextSymbol.BlockId.HasValue;
            var blockId = hasValue
                ? nextSymbol.BlockId.Value
                : nextFreeBlockId;
            if(!hasValue)
            {
                // Not existing yet: Add this new block.
                var symbolBlock = new SymbolBlock(new List<string>(new [] { symbolToAdd }));
                BlockDefinitions.Add(nextFreeBlockId, symbolBlock);
                nextSymbol.BlockId = blockId;
                nextFreeBlockId++;
            }
            return blockId;
        }

        #endregion
    }
}
