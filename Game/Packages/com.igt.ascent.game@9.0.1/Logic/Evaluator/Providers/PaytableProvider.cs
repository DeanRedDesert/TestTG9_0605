//-----------------------------------------------------------------------
// <copyright file = "PaytableProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
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
    /// Provider which exposes paytable data.
    /// </summary>
    public class PaytableProvider
    {
        /// <summary>
        /// Get the paytable file name.
        /// </summary>
        [GameService]
        public string PaytableName { get; private set; }

        /// <summary>
        /// Get the paylines for the paytable.
        /// </summary>
        [GameService]
        public List<List<Cell>> Paylines { get; private set; }

        /// <summary>
        /// Return all reel strips.
        /// </summary>
        [GameService]
        public IList<List<string>> ReelStrips { get; private set; }

        /// <summary>
        /// Get the symbol window definition for the paytable.
        /// </summary>
        [GameService]
        public CellPopulation SymbolWindow { get; private set; }

        /// <summary>
        /// Construct a paytable provider instance with the given information.
        /// </summary>
        /// <param name="paylines">Paylines for the paytable.</param>
        /// <param name="reelStrips">Reel strips for the paytable.</param>
        /// <param name="symbolWindow">Symbol window for the paytable.</param>
        /// <remarks>
        /// This implementation is useful for alternate paytable implementations. An example being RGS paytables.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public PaytableProvider(IEnumerable<List<Cell>> paylines, IList<List<string>> reelStrips,
            CellPopulation symbolWindow) : this(paylines, reelStrips, symbolWindow, null)
        { }

        /// <summary>
        /// Construct a paytable provider instance with the given information.
        /// </summary>
        /// <param name="paylines">Paylines for the paytable.</param>
        /// <param name="reelStrips">Reel strips for the paytable.</param>
        /// <param name="symbolWindow">Symbol window for the paytable.</param>
        /// <param name="paytableName">Current paytable name.</param>
        /// <remarks>
        /// This implementation is useful for alternate paytable implementations. An example being RGS paytables.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public PaytableProvider(IEnumerable<List<Cell>> paylines, IList<List<string>> reelStrips,
            CellPopulation symbolWindow, string paytableName)
        {
            if(paylines == null)
            {
                throw new ArgumentNullException("paylines");
            }
            if(reelStrips == null)
            {
                throw new ArgumentNullException("reelStrips");
            }
            if(symbolWindow == null)
            {
                throw new ArgumentNullException("symbolWindow");
            }

            // set paytable name, check paytable name for null and initialize it.
            PaytableName = paytableName ?? string.Empty;

            //A ToList of paylines would create a new top level list, but the sublists would be references.
            //This will make a new list and then add list copies for each of the paylines. The cells are references,
            //but they are an immutable type.
            Paylines = new List<List<Cell>>();
            foreach(var payline in paylines)
            {
                Paylines.Add(payline.ToList());
            }

            //Again string is immutable, so the reference is ok.
            ReelStrips = new List<List<string>>();
            foreach(var reelStrip in reelStrips)
            {
                ReelStrips.Add(reelStrip.ToList());
            }

            AddSymbolWindow(symbolWindow);
        }

        /// <summary>
        /// Create a paytable provider.
        /// </summary>
        /// <param name="paytableSection">The paytable section to use for the provider.</param>
        public PaytableProvider(SlotPaytableSection paytableSection)
		       : this(paytableSection, null)
        {}

        /// <summary>
        /// Create a paytable provider.
        /// </summary>
        /// <param name="paytableSection">The paytable section to use for the provider.</param>
        /// <param name="paytableName">Name of the paytable file</param>
        public PaytableProvider(SlotPaytableSection paytableSection, string paytableName)
        {
            Paylines = new List<List<Cell>>();
            ReelStrips = new List<List<string>>();

            // set paytable name, check paytable name for null and initialize it.
            PaytableName = paytableName ?? string.Empty;

            ConfigurePaytableSection(paytableSection);
        }

        /// <summary>
        /// Configure the provider for the given paytable section.
        /// </summary>
        /// <param name="paytableSection"></param>
        /// <exception cref="PaytableException">
        /// Thrown when the given paytable section does not contain a SymbolWindow or StripList.
        /// </exception>
        private void ConfigurePaytableSection(SlotPaytableSection paytableSection)
        {
            //It is valid for there not to be a pattern list.
            if (paytableSection.LinePatternList != null)
            {
                AddPaylines(paytableSection.LinePatternList.Pattern);
            }

            if(paytableSection.SymbolWindow == null || paytableSection.SymbolWindow.PopulationEntry == null)
            {
                throw new PaytableException("SymbolWindow", "A SlotPaytableSection must contain a SymbolWindow.");
            }

            if(paytableSection.StripList == null || paytableSection.StripList.Strip == null)
            {
                throw new PaytableException("StripList", "A SlotPaytableSection must contain a StripList");
            }

            AddReelStrips(paytableSection.SymbolWindow.PopulationEntry, paytableSection.StripList.Strip);
            AddSymbolWindow(paytableSection.SymbolWindow);
        }

        /// <summary>
        /// Update the provider's symbol window with the given symbol window.
        /// </summary>
        /// <param name="symbolWindow">Symbol window to add to the provider.</param>
        /// <exception cref="PaytableException">
        /// Thrown if the given symbol window does not contain any population entries, or if it contains a population
        /// entry which does not contain any cells.
        /// </exception>
        private void AddSymbolWindow(CellPopulation symbolWindow)
        {
            SymbolWindow = new CellPopulation();

            if(symbolWindow.PopulationEntry == null || !symbolWindow.PopulationEntry.Any())
            {
                throw new PaytableException("SymbolWindow", "The SymbolWindow must contain at least one PopulationEntry.");
            }

            foreach (var populationEntry in symbolWindow.PopulationEntry)
            {
                var newEntry = new CellPopulationPopulationEntry
                {
                    name = populationEntry.name,
                    startPosition = populationEntry.startPosition,
                    stripName = populationEntry.stripName,
                    stripOffsetIndex = populationEntry.stripOffsetIndex
                };

                if(populationEntry.Cells == null || !populationEntry.Cells.Any())
                {
                    throw new PaytableException("SymbolWindow",
                                                "Each population entry in a symbol window must contain at least 1 cell.");
                }

                foreach (var cell in populationEntry.Cells)
                {
                    newEntry.Cells.Add(cell);
                }

                SymbolWindow.PopulationEntry.Add(newEntry);
            }
        }

        /// <summary>
        /// Add the given patterns from the paytable to the provider.
        /// </summary>
        /// <param name="patterns">Patterns to add.</param>
        /// <exception cref="PaytableException">
        /// Thrown if the given pattern list doesn't contain at least one pattern.
        /// </exception>
        private void AddPaylines(ICollection<PatternListPattern> patterns)
        {
            if(patterns == null || !patterns.Any())
            {
                throw new PaytableException("LinePatternList", "The LinePatternList must contain at least 1 pattern.");
            }

            foreach (var patternToAdd in
                patterns.Select(pattern => pattern.Cluster.SelectMany(cluster => cluster.Cells).ToList()))
            {
                Paylines.Add(patternToAdd);
            }
        }

        /// <summary>
        /// Add the reel strips specified by the mappings to the provider.
        /// </summary>
        /// <param name="mappings">Mappings to determine strip order.</param>
        /// <param name="stripList">Strips to add to the provider.</param>
        /// <exception cref="PaytableException">Thrown if the give mappings do not have at least 1 entry.</exception>
        private void AddReelStrips(IEnumerable<CellPopulationPopulationEntry> mappings, ICollection<Strip> stripList)
        {
            if(stripList == null || !stripList.Any())
            {
                throw new PaytableException("StripList", "The StripList must contain at least 1 strip.");
            }

            foreach (var reelMapping in mappings)
            {
                var reelStrip = (from strip in stripList
                                 where strip.name == reelMapping.stripName
                                 select strip).First();

                var stringStrip = (from symbol in reelStrip.Stop select symbol.id).ToList();

                ReelStrips.Add(stringStrip);
            }
        }

        /// <summary>Get the number of bet items in the paytable.</summary>
        /// <returns>The number of bet items.</returns>
        public uint NumberOfBetItems
        {
            get { return (uint)Paylines.Count; }
        }
    }
}
