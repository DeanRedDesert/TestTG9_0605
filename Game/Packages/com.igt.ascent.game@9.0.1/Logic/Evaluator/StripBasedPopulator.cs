//-----------------------------------------------------------------------
// <copyright file = "StripBasedPopulator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Schemas;

    /// <summary>
    /// The purpose of the cell based populator is too populate a cell
    /// population given a set of random numbers, a set of reel strips,
    /// and a strip mapping. The most common use of this is to populate
    /// a symbol window for a slot game. In the case of a normal slot game
    /// each cell population outcome entry represents a reel on the screen
    /// and each cell in that entry represents a visible symbol on that reel.
    /// </summary>
    public static class StripBasedPopulator
    {
        /// <summary>
        /// Select random numbers and use them to create and populate a symbol window.
        /// </summary>
        /// <param name="stripList">Reel strips to use to populate the symbol window.</param>
        /// <param name="windowDefinition">A cell population which defines the extents of the symbol window.</param>
        /// <param name="randomNumberGenerator">A source of random numbers.</param>
        /// <returns>A populated CellPopulationOutcome</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when any of the parameters to this function are null.
        /// </exception>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if a strip requested in the symbol window is not present in the strip list.
        /// </exception>
        public static CellPopulationOutcome CreateCellPopulationOutcome(StripList stripList,
                                                                        CellPopulation windowDefinition,
                                                                        IRandomNumbers randomNumberGenerator)
        {
            if(stripList == null)
            {
                throw new ArgumentNullException("stripList", "Parameter may not be null");
            }
            if(windowDefinition == null)
            {
                throw new ArgumentNullException("windowDefinition", "Parameter may not be null");
            }
            if(randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator", "Parameter may not be null");
            }

            var requests = new List<RandomValueRequest>();

            //Build a random number request based off of the reel strips.
            foreach(var population in windowDefinition.PopulationEntry)
            {
                var strip = MatchStrip(stripList, population);
                var stripRange = strip.TotalWeight - 1;
                requests.Add(new RandomValueRequest(1, 0, stripRange));
            }

            var virtualStops = randomNumberGenerator.GetRandomNumbers(requests);

            return CreateCellPopulationOutcome(stripList, windowDefinition, virtualStops);
        }

        /// <summary>
        /// Use a list of stop numbers to create and populate a symbol window.
        /// </summary>
        /// <param name="stripList">Reel strips to use to populate the symbol window.</param>
        /// <param name="windowDefinition">A cell population which defines the extents of the symbol window.</param>
        /// <param name="stopNumbers">A list of stop numbers.</param>
        /// <returns>A populated CellPopulationOutcome</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when any of the parameters to this function are null.
        /// </exception>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if a strip requested in the symbol window is not present in the strip list.
        /// </exception>
        public static CellPopulationOutcome CreateCellPopulationOutcome(StripList stripList,
                                                                        CellPopulation windowDefinition,
                                                                        ICollection<int> stopNumbers)
        {
            if(stripList == null)
            {
                throw new ArgumentNullException("stripList", "Parameter may not be null");
            }
            if(windowDefinition == null)
            {
                throw new ArgumentNullException("windowDefinition", "Parameter may not be null");
            }
            if(stopNumbers == null)
            {
                throw new ArgumentNullException("stopNumbers", "Parameter may not be null");
            }

            var populatedOutcome = new CellPopulationOutcome {name = "SymbolWindow_" + windowDefinition.name};

            //Name for the populated window to use for debugging.

            var strips =
                windowDefinition.PopulationEntry.Select(population => MatchStrip(stripList, population)).ToList();

            for(var index = 0; index < windowDefinition.PopulationEntry.Count; index++)
            {
                var population = windowDefinition.PopulationEntry[index];
                populatedOutcome.PopulationEntryList.Add(
                    CreatePopulationEntryByVirtualStop(strips[index],
                                                       population,
                                                       stopNumbers.ElementAt(index)));
            }

            return populatedOutcome;
        }

        /// <summary>
        /// Select random numbers and use them to update and populate a symbol window based on an existed one.
        /// </summary>
        /// <param name="basedSymbolWindow">Symbol window to update</param>
        /// <param name="stripList">Reel strips to use to populate the symbol window.</param>
        /// <param name="windowDefinition">A cell population which defines the extents of the symbol window.</param>
        /// <param name="randomNumberGenerator">A source of random numbers.</param>
        /// <param name="outcomeIndexsToUpdate">A collection to indicate the indexs of entries that need to update</param>
        /// <returns>A populated CellPopulationOutcome</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when any of the parameters to this function are null.
        /// </exception>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if a strip requested in the symbol window is not present in the strip list.
        /// </exception>
        public static CellPopulationOutcome UpdateCellPopulationOutcome(CellPopulationOutcome basedSymbolWindow,
                                                                        StripList stripList,
                                                                        CellPopulation windowDefinition,
                                                                        IRandomNumbers randomNumberGenerator,
                                                                        ICollection<int> outcomeIndexsToUpdate)
        {
            if(basedSymbolWindow == null)
            {
                throw new ArgumentNullException("basedSymbolWindow", "Parameter may not be null");
            }
            if(stripList == null)
            {
                throw new ArgumentNullException("stripList", "Parameter may not be null");
            }
            if(windowDefinition == null)
            {
                throw new ArgumentNullException("windowDefinition", "Parameter may not be null");
            }
            if(randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator", "Parameter may not be null");
            }
            if(outcomeIndexsToUpdate == null)
            {
                throw new ArgumentNullException("outcomeIndexsToUpdate", "Parameter may not be null");
            }

            var populatedOutcome = new CellPopulationOutcome
                                       {
                                           //Name for the populated window to use for debugging.
                                           name = "SymbolWindow_" + windowDefinition.name
                                       };

            //populate symbol window based on existed one
            for(var index = 0; index < windowDefinition.PopulationEntry.Count; index++)
            {
                if(outcomeIndexsToUpdate.Contains(index))
                {
                    var populationEntry = CreatePopulationEntry(stripList,
                                                                windowDefinition.PopulationEntry[index],
                                                                randomNumberGenerator);
                    populatedOutcome.PopulationEntryList.Add(populationEntry);
                }
                else
                {
                    populatedOutcome.PopulationEntryList.Add(basedSymbolWindow.PopulationEntryList[index]);
                }
            }

            return populatedOutcome;
        }

        /// <summary>
        /// Shift the symbols in given symbol window up or down over specified positions. 
        /// </summary>
        /// <param name="basedSymbolWindow">The given symbol window.</param>
        /// <param name="stripList">Reel strips to use to populate the symbol window.</param>
        /// <param name="windowDefinition">A cell population which defines the extents of the symbol window.</param>
        /// <param name="shiftPositions">A list of integer numbers indicate the positions to shift for each PopulationEntry in the given symbol window.
        /// Positive value indicates the direction is forward; negative value indicates the direction is backward.</param>
        /// <returns>The new symbols window after the shift.</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when any of the parameters is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// This exception is thrown if the elements count of <paramref name="shiftPositions"/> not equals
        /// the population entries count of <paramref name="basedSymbolWindow"/>.
        /// </exception>
        public static CellPopulationOutcome UpdateCellPopulationOutcome(
                                                CellPopulationOutcome basedSymbolWindow,
                                                StripList stripList,
                                                CellPopulation windowDefinition,
                                                List<int> shiftPositions)
        {
            if(basedSymbolWindow == null)
            {
                throw new ArgumentNullException("basedSymbolWindow", "Parameter may not be null");
            }

            if(stripList == null)
            {
                throw new ArgumentNullException("stripList", "Parameter may not be null");
            }

            if(windowDefinition == null)
            {
                throw new ArgumentNullException("windowDefinition", "Parameter may not be null");
            }

            if(shiftPositions == null)
            {
                throw new ArgumentNullException("shiftPositions", "Parameter may not be null");
            }

            if(shiftPositions.Count != basedSymbolWindow.PopulationEntryList.Count)
            {
                throw new ArgumentException("The elements count of shiftPositions must equal the population entry count of basedSymbolWindow", "shiftPositions");
            }

            var populatedOutcome = new CellPopulationOutcome
                                       {
                                           //Name for the populated window to use for debugging.
                                           name = "SymbolWindow_" + windowDefinition.name
                                       };

            for(int index = 0; index < shiftPositions.Count; index++)
            {
                if(shiftPositions[index] != 0)
                {
                    // match the strip
                    var strip = MatchStrip(stripList, windowDefinition.PopulationEntry[index]);
                    // shift the population entry
                    var updatedPopulationEntry =
                        UpdatePopulationEntry(basedSymbolWindow.PopulationEntryList[index],
                                              strip,
                                              windowDefinition.PopulationEntry[index],
                                              shiftPositions[index]);
                    populatedOutcome.PopulationEntryList.Add(updatedPopulationEntry);
                }
                else
                {
                    populatedOutcome.PopulationEntryList.Add(basedSymbolWindow.PopulationEntryList[index]);
                }

            }

            return populatedOutcome;
        }

        /// <summary>
        /// Shift the symbols in given PopulationEntry up or down over specified position.
        /// </summary>
        /// <param name="basedPopulationEntry">The given PopulationEntry.</param>
        /// <param name="strip">The strip based off which this PopulationEntry was generated.</param>
        /// <param name="populationEntryDefinition">The PopulationEntry definition based off which this PopulationEntry was generated.</param>
        /// <param name="shiftPosition">Indicates how many symbols the PopulationEntry should shift over. 
        /// Positive value indicates the direction is forward; negative value indicates the direction is backward.</param>
        /// <returns>The new PopulationEntry after the shift.</returns>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown if any of the reference parameters is null.
        /// </exception>
        private static PopulationEntry UpdatePopulationEntry(PopulationEntry basedPopulationEntry,
                                                             Strip strip,
                                                             CellPopulationPopulationEntry populationEntryDefinition,
                                                             int shiftPosition)
        {
            if(basedPopulationEntry == null)
            {
                throw new ArgumentNullException("basedPopulationEntry", "Parameter may not be null");
            }

            if(strip == null)
            {
                throw new ArgumentNullException("strip", "Parameter may not be null");
            }

            if(populationEntryDefinition == null)
            {
                throw new ArgumentNullException("populationEntryDefinition", "Parameter may not be null");
            }

            int physicalStop = (int)basedPopulationEntry.OutcomeCellList[0].stop - shiftPosition;
            int totalStops = strip.Stop.Count;

            if(physicalStop >= totalStops)
            {
                physicalStop = physicalStop % totalStops;
            }
            else if(physicalStop < 0)
            {
                physicalStop += totalStops;
            }

            return CreatePopulationEntry(strip, populationEntryDefinition, physicalStop);
        }


        /// <summary>
        /// Select random numbers and use them to create and populate a symbol reel.
        /// </summary>
        /// <param name="stripList">Reel strips to use to populate the symbol window.</param>
        /// <param name="reelDefinition">A cell population which defines the extents of the reel.</param>
        /// <param name="randomNumberGenerator">A source of random numbers.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown when any of the parameters to this function is null.
        /// </exception>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if a strip requested in the symbol reel is not present in the strip list.
        /// </exception>
        private static PopulationEntry CreatePopulationEntry(StripList stripList,
                                                             CellPopulationPopulationEntry reelDefinition,
                                                             IRandomNumbers randomNumberGenerator)
        {
            if(stripList == null)
            {
                throw new ArgumentNullException("stripList", "Parameter may not be null");
            }
            if(reelDefinition == null)
            {
                throw new ArgumentNullException("reelDefinition", "Parameter may not be null");
            }
            if(randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator", "Parameter may not be null");
            }

            var strip = MatchStrip(stripList, reelDefinition);

            //Build a random number request based off of the reel strips.
            var stripRange = strip.TotalWeight - 1;
            var request = new RandomValueRequest(1, 0, stripRange);

            var virtualStops = randomNumberGenerator.GetRandomNumbers(request);
            var populatedEntry = CreatePopulationEntryByVirtualStop(strip,
                                                                    reelDefinition,
                                                                    virtualStops.ElementAt(0));

            return populatedEntry;
        }

        /// <summary>
        /// Construct and populate a population entry for a single strip by specified physical stop.
        /// </summary>
        /// <param name="strip">A reel strip to get symbols from.</param>
        /// <param name="populationEntry">The definition of the entry.</param>
        /// <param name="physicalStop">The physical stop to populate from.</param>
        /// <returns>A populated PopulationEntry.</returns>
        private static PopulationEntry CreatePopulationEntry(Strip strip,
                                                             CellPopulationPopulationEntry populationEntry,
                                                             int physicalStop)
        {
            var outcomeCells = new List<OutcomeCell>();
            int totalStops = strip.Stop.Count;

            foreach(var cell in populationEntry.Cells)
            {
                if(physicalStop >= totalStops)
                {
                    physicalStop = 0;
                }

                StopType stripStop = strip.Stop[physicalStop];

                var outcomeCell = new OutcomeCell
                {
                    symbolID = stripStop.id,
                    Cell = cell,
                    stop = (uint)physicalStop,
                    stopSpecified = true
                };

                outcomeCells.Add(outcomeCell);

                physicalStop++;
            }

            var outcomePopulationEntry = new PopulationEntry
            {
                OutcomeCellList = outcomeCells,
                name = populationEntry.name
            };

            return outcomePopulationEntry;
        }

        /// <summary>
        /// Construct and populate a population entry for a single strip by specified virtual stop.
        /// </summary>
        /// <param name="strip">A reel strip to get symbols from.</param>
        /// <param name="populationEntry">The definition of the entry.</param>
        /// <param name="virtualStop">The virtual stop to populate from.</param>
        /// <returns>A populated PopulationEntry.</returns>
        private static PopulationEntry CreatePopulationEntryByVirtualStop(Strip strip, 
                                                                          CellPopulationPopulationEntry populationEntry,
                                                                          int virtualStop)
        {
            int totalStops = strip.Stop.Count;

            int physicalStop = strip.GetStopIndexForWeight(virtualStop);

            checked
            {
                //Adjust the physical stop for the population offset.
                physicalStop = (totalStops - populationEntry.stripOffsetIndex + physicalStop) % totalStops;
            }

            return CreatePopulationEntry(strip, populationEntry, physicalStop);
        }

        /// <summary>
        /// Find the matched strip according to reel definition
        /// </summary>
        /// <param name="stripList">Strip list</param>
        /// <param name="population">Reel definition</param>
        /// <returns>Matched strip</returns>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if a strip requested in the symbol reel is not present in the strip list.
        /// </exception>
        private static Strip MatchStrip(StripList stripList, CellPopulationPopulationEntry population)
        {
            var selectedStrip =
                    (from strip in stripList.Strip where strip.name == population.stripName select strip).
                        FirstOrDefault();

            if(selectedStrip == null)
            {
                throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                        "Requested strip: {0} from population: {1} was not present in strip list: {2}",
                                                                        population.stripName, population.name,
                                                                        stripList.name));
            }
            return selectedStrip;
        }
    }
}