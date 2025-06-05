//-----------------------------------------------------------------------
// <copyright file = "MultiwayPatternPopulator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using Schemas;

    /// <summary>
    /// The multiway pattern populator populates patterns from a pattern list with the symbols from a cell population.
    /// Unlike the regular PatternPopulator the multiway populator maintains cluster information so that it may be
    /// used during multiway prize evaluation. Typically each cluster in the pattern represents a multiway column.
    /// </summary>
    public static class MultiwayPatternPopulator
    {
        /// <summary>
        /// Creates and populates a list of CellPopulationOutcomes. A cell population outcome will be created for
        /// each pattern in the pattern list.
        /// </summary>
        /// <param name="symbolWindow">The populated symbol window to generate populated patterns from.</param>
        /// <param name="patternList">List of patterns to populate.</param>
        /// <returns>A populated list of patterns.</returns>
        public static IList<CellPopulationOutcome> PopulatePatterns(CellPopulationOutcome symbolWindow,
                                                                    PatternList patternList)
        {
            if (symbolWindow == null)
            {
                throw new ArgumentNullException("symbolWindow", "Parameter may not be null");
            }

            if (patternList == null)
            {
                throw new ArgumentNullException("patternList", "Parameter may not be null");
            }

            var populatedPatternList = new List<CellPopulationOutcome>();

            foreach (var pattern in patternList.Pattern)
            {
                var populatedPattern = new CellPopulationOutcome {name = pattern.name};

                foreach (var cluster in pattern.Cluster)
                {
                    var populatedEntry = new PopulationEntry {name = cluster.name};

                    foreach (var cell in cluster.Cells)
                    {
                        populatedEntry.OutcomeCellList.Add(symbolWindow.GetOutcomeCellByCell(cell));
                    }

                    populatedPattern.PopulationEntryList.Add(populatedEntry);
                }

                populatedPatternList.Add(populatedPattern);
            }

            return populatedPatternList;
        }
    }
}
