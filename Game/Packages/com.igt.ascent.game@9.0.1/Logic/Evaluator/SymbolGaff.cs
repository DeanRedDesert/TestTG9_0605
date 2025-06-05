//-----------------------------------------------------------------------
// <copyright file = "SymbolGaff.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// This class is for determining the virtual stops of a gaff from a pattern and list of symbols.
    /// </summary>
    public static class SymbolGaff
    {
        /// <summary>
        /// Any symbol constant that will be replaced by a random number when getting stops.
        /// </summary>
        public const string AnySymbol = "ANY";

        /// <summary>
        /// Determine the virtual stops required to generate a symbol window which has the requested symbols on the
        /// requested pattern.
        /// </summary>
        /// <param name="section">Paytable section to generate the window from.</param>
        /// <param name="pattern">Pattern that the symbols need to appear on. ex: "Line 1"</param>
        /// <param name="symbols">
        /// A list of symbols in the order of the cell population definitions. In a slot game this is normally the
        /// order of the reels.
        /// </param>
        /// <returns>A list of virtual stops.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static IEnumerable<int> GetVirtualStops(SlotPaytableSection section, string pattern,
                                                       IEnumerable<string> symbols)
        {
            if (section == null)
            {
                throw new ArgumentNullException("section", "Argument may not be null.");
            }
            if (pattern == null)
            {
                throw new ArgumentNullException("pattern", "Argument may not be null.");
            }
            if (symbols == null)
            {
                throw new ArgumentNullException("symbols", "Argument may not be null");
            }

            var random = new Random();

            var desiredVirtualStops = new List<int>();

            var desiredPattern = GetDesiredPattern(section, pattern);

            //Get a position from each cluster.
            var positions = desiredPattern.Cluster.Select(cluster => cluster.Cells.First()).ToList();

            //Determine the virtual stop for each population mapping and add it to the desired stops list.
            for (var populationIndex = 0;
                 populationIndex < section.SymbolWindow.PopulationEntry.Count && populationIndex < symbols.Count();
                 populationIndex++)
            {
                var populationEntry = section.SymbolWindow.PopulationEntry[populationIndex];
                var stripIdentifier = populationEntry.stripName;
                var selectedStrip = GetSelectedStrip(section, stripIdentifier);

                var symbol = symbols.ElementAt(populationIndex);
                var physicalStop = symbol == AnySymbol ? random.Next(selectedStrip.Stop.Count) :
                    GetDesiredPhysicalStop(symbols.ElementAt(populationIndex), selectedStrip);

                desiredVirtualStops.Add(GetDesiredVirtualStop(selectedStrip, populationEntry,
                                                              positions[populationIndex], physicalStop));
            }

            return desiredVirtualStops;
        }

        /// <summary>
        /// Get the reel strip associated with the given section and identifier.
        /// </summary>
        /// <param name="section">The section to get the reel strip from.</param>
        /// <param name="stripIdentifier">The identifier to get a reel strip for.</param>
        /// <returns>The reel strip for the specified identifier.</returns>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown when the requested strip cannot be found.
        /// </exception>
        private static Strip GetSelectedStrip(SlotPaytableSection section, string stripIdentifier)
        {
            var selectedStrip =
                (from strip in section.StripList.Strip where strip.name == stripIdentifier select strip).
                    FirstOrDefault();

            if (selectedStrip == null)
            {
                throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                        "Requested strip: {0} was not present in strip list: {1}",
                                                                        stripIdentifier,
                                                                        section.StripList.name));
            }
            return selectedStrip;
        }

        /// <summary>
        /// Get the requested pattern from the given section. Line, scatter, and multiway patterns are checked.
        /// </summary>
        /// <remarks>
        /// Line patterns are searched before scatter patterns. And scatter patterns are searched before multiway
        /// patterns. If there is a duplicate pattern name in more than one pattern, the first pattern searched
        /// will be returned.
        /// </remarks>
        /// <param name="section">The section to get the pattern from.</param>
        /// <param name="pattern">The desired pattern name.</param>
        /// <returns>The pattern definition for the requested pattern.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested pattern cannot be found.</exception>
        private static PatternListPattern GetDesiredPattern(SlotPaytableSection section, string pattern)
        {
            var desiredPattern = GetDesiredLinePattern(section, pattern) ??
                                GetDesiredScatterPattern(section, pattern) ??
                                GetDesiredMultiWayPattern(section, pattern);

            if (desiredPattern == null)
            {
                throw new KeyNotFoundException("Could not find desired pattern for gaff: " + pattern);
            }
            return desiredPattern;
        }

        /// <summary>
        /// Get the requested line pattern from the given section.
        /// </summary>
        /// <param name="section">The section to get the pattern from.</param>
        /// <param name="pattern">The desired pattern name.</param>
        /// <returns>The pattern definition for the requested pattern.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested pattern cannot be found.</exception>
        private static PatternListPattern GetDesiredLinePattern(SlotPaytableSection section, string pattern)
        {
            PatternListPattern desiredPattern = null;

            if (section.LinePatternList != null)
            {
                desiredPattern = section.LinePatternList.Pattern.AsEnumerable().Where
                    (listPattern => listPattern.name == pattern).Select
                    (listPattern => listPattern).FirstOrDefault();
            }
            return desiredPattern;
        }

        /// <summary>
        /// Get the requested scatter pattern from the given section.
        /// </summary>
        /// <param name="section">The section to get the pattern from.</param>
        /// <param name="pattern">The desired pattern name.</param>
        /// <returns>The pattern definition for the requested pattern.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested pattern cannot be found.</exception>
        private static PatternListPattern GetDesiredScatterPattern(SlotPaytableSection section, string pattern)
        {
            PatternListPattern desiredPattern = null;

            if (section.ScatterPatternList != null)
            {
                desiredPattern = section.ScatterPatternList.Pattern.AsEnumerable().Where
                    (listPattern => listPattern.name == pattern).Select
                    (listPattern => listPattern).FirstOrDefault();
            }
            return desiredPattern;
        }

        /// <summary>
        /// Get the requested multiway pattern from the given section.
        /// </summary>
        /// <param name="section">The section to get the pattern from.</param>
        /// <param name="pattern">The desired pattern name.</param>
        /// <returns>The pattern definition for the requested pattern.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested pattern cannot be found.</exception>
        private static PatternListPattern GetDesiredMultiWayPattern(SlotPaytableSection section, string pattern)
        {
            PatternListPattern desiredPattern = null;

            if (section.MultiwayPatternList != null)
            {
                desiredPattern = section.MultiwayPatternList.Pattern.AsEnumerable().Where
                    (listPattern => listPattern.name == pattern).Select
                    (listPattern => listPattern).FirstOrDefault();
            }
            return desiredPattern;
        }

        /// <summary>
        /// Get the physical stop for the specified population index and symbol.
        /// </summary>
        /// <param name="symbol">The symbol to locate a physical stop for.</param>
        /// <param name="selectedStrip">The reel strip to get the stop from.</param>
        /// <returns>The physical stop for the specified symbol.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the requested symbol cannot be found.</exception>
        private static int GetDesiredPhysicalStop(string symbol, Strip selectedStrip)
        {
            var physicalStop = 0;

            StopType desiredStop = null;
            //Find stop of desired symbol.
            for (var currentStopIndex = 0; currentStopIndex < selectedStrip.Stop.Count; currentStopIndex++)
            {
                var currentStop = selectedStrip.Stop[currentStopIndex];
                if (currentStop.id == symbol)
                {
                    desiredStop = currentStop;
                    physicalStop = currentStopIndex;
                    break;
                }
            }

            if (desiredStop == null)
            {
                throw new KeyNotFoundException("Could not find desired symbol: " + symbol +
                                               " on strip: " + selectedStrip.name);
            }

            return physicalStop;
        }

        /// <summary>
        /// Get a virtual stop for the given physical stop. The virtual stop will be adjusted for the desired position
        /// of the physical stop within the window.
        /// </summary>
        /// <param name="selectedStrip">The reel strip the physical stop is associated with.</param>
        /// <param name="populationEntry">The population entry the stop is to appear in.</param>
        /// <param name="position">The desired position within the population entry.</param>
        /// <param name="physicalStop">The physical stop to get a virtual stop for.</param>
        /// <returns>A virtual stop for the given physical stop adjusted for the desired population position.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when requested location cannot be found in the given population entry.
        /// </exception>
        private static int GetDesiredVirtualStop(Strip selectedStrip,
                                                 CellPopulationPopulationEntry populationEntry, Cell position,
                                                 int physicalStop)
        {
            var offset = 0;
            var cellFound = false;
            var desiredVirtualStop = 0;

            foreach (var cell in populationEntry.Cells)
            {
                if (cell.Equals(position))
                {
                    cellFound = true;
                    break;
                }
                offset++;
            }

            offset = offset - populationEntry.stripOffsetIndex;

            if (cellFound)
            {
                //Adjust the stop for the offset in the symbol window.
                physicalStop = (selectedStrip.Stop.Count - offset + physicalStop) % selectedStrip.Stop.Count;

                desiredVirtualStop = selectedStrip.GetWeightForStopIndex(physicalStop);
            }
            else
            {
                throw new KeyNotFoundException("Could not find cell for gaff: " + position + " in population: " +
                                               populationEntry.name);
            }
            return desiredVirtualStop;
        }
    }
}
