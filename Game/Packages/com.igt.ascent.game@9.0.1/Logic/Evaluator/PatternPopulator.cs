//-----------------------------------------------------------------------
// <copyright file = "PatternPopulator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// The pattern populator populates patterns from a pattern list with
    /// the symbols from a cell population. The most common use in a slot
    /// game is populating a set of lines and scatters with information
    /// from the symbol window. So if given a symbol window with the following
    /// content:<br/>
    ///  -----------<br/>
    /// | K | Q | J |<br/>
    /// | Q | J | A |<br/>
    /// | A | Q | J |<br/>
    ///  -----------<br/>
    /// And line definitions for 3 lines consisting of the top middle
    /// and bottom row. Then the resulting output would be three cell
    /// cell populations. The first one containing K,Q,J the second
    /// containing Q,J,A and the third containing A,Q,J.
    /// </summary>
    public static class PatternPopulator
    {
        /// <summary>
        /// Creates and populates a CellPopulationOutcome with patterns.
        /// </summary>
        /// <param name="symbolWindow">A symbol window to get symbols for population.</param>
        /// <param name="patternList">A list of patterns to populate based on.</param>
        /// <returns>A populated pattern list.</returns>
        /// <exception cref="ArgumentNullException">This exception is thrown if any of the parameters are null.</exception>
        public static CellPopulationOutcome PopulatePatterns(CellPopulationOutcome symbolWindow,
                                                             PatternList patternList)
        {
            if(symbolWindow == null)
            {
                throw new ArgumentNullException("symbolWindow", "Parameter may not be null");
            }

            if(patternList == null)
            {
                throw new ArgumentNullException("patternList", "Parameter may not be null");
            }

            var populatedPatternList = new CellPopulationOutcome {name = patternList.name};

            foreach(var pattern in patternList.Pattern)
            {
                var populationEntry = new PopulationEntry {name = pattern.name};

                foreach(var cell in pattern.Cluster.SelectMany(cluster => cluster.Cells))
                {
                    populationEntry.OutcomeCellList.Add(symbolWindow.GetOutcomeCellByCell(cell));
                }

                populatedPatternList.PopulationEntryList.Add(populationEntry);
            }

            return populatedPatternList;
        }
    }
}
