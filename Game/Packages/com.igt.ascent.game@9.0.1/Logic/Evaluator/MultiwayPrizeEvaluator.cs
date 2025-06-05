//-----------------------------------------------------------------------
// <copyright file = "MultiwayPrizeEvaluator.cs" company = "IGT">
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
    /// The purpose of the multiway prize evaluator is to determine if any patterns in a set of populated patterns
    /// fulfills the requirements of a set of prizes. Unlike a line prize evaluation where the pay count is determined
    /// by the number of matching symbols, the pay count in multiway is determined by the number of population entries
    /// which contain matching symbols. Each population entry in a pattern typically represents a multiway column.
    /// The number of symbols matched in each column is tracked and then the matches per column are multiplied together
    /// and stored in the multiplier field of the win outcome. The bet processor strategy would typically multiply the
    /// multiplier by the bet on the pattern (the pattern representing the active mask).
    /// </summary>
    public static class MultiwayPrizeEvaluator
    {
        /// <summary>
        /// Evaluate all of the patterns in a cell population outcome. Each CellPopulationOutcome is treated as a
        ///  populated pattern.
        /// </summary>
        /// <param name="prizeScale"> The prize scale to evaluate against the patterns. </param>
        /// <param name="patterns"> The list of populated patterns. </param>
        /// <param name="patternListName">Name of the pattern list. Used when creating wins.</param>
        /// <returns> A win outcome containing all winning prizes. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameters are null.
        /// </exception>
        public static WinOutcome EvaluatePatterns(SlotPrizeScale prizeScale,
                                                  IList<CellPopulationOutcome> patterns, string patternListName)
        {
            if(prizeScale == null)
            {
                throw new ArgumentNullException("prizeScale", "Argument may not be null");
            }
            if(patterns == null)
            {
                throw new ArgumentNullException("patterns", "Argument may not be null");
            }
            if(patternListName == null)
            {
                throw new ArgumentNullException("patternListName", "Argument may not be null");
            }

            var wins = new WinOutcome();

            foreach(var pattern in patterns)
            {
                var patternName = pattern.name;

                foreach(
                    var prize in
                        prizeScale.Prize.Where(
                            prize => prize.EligiblePattern == "ALL" || prize.EligiblePattern == patternName))
                {
                    EvaluatePrize(prize, wins, pattern,
                                  new RequiredWinOutcomeItemData(prizeScale.name, patternListName, patternName));
                }
            }

            return wins;
        }

        /// <summary>
        /// Evaluate the given prize against the given pattern.
        /// </summary>
        /// <param name="prize">The prize to evaluate</param>
        /// <param name="wins">A win outcome to add wins to.</param>
        /// <param name="pattern">The pattern to evaluate the prize against.</param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown if the order strategy of the prize is not supported.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameters are null.
        /// </exception>
        public static void EvaluatePrize(SlotPrize prize, WinOutcome wins, CellPopulationOutcome pattern,
                                         RequiredWinOutcomeItemData winInformation)
        {
            if(prize == null)
            {
                throw new ArgumentNullException("prize", "Parameter may not be null");
            }
            if(wins == null)
            {
                throw new ArgumentNullException("wins", "Parameter may not be null");
            }
            if(pattern == null)
            {
                throw new ArgumentNullException("pattern", "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException("winInformation", "Parameter may not be null");
            }

            if(prize.OrderStrategy == SlotPrizeOrderStrategy.Unordered)
            {
                EvaluateSymbols(wins, prize, pattern, winInformation);
            }
            else
            {
                throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                        "Unsupported evaluation order strategy: {0}",
                                                                        prize.OrderStrategy));
            }
        }

        /// <summary>
        /// Evaluate the given pattern against the given prize.
        /// </summary>
        /// <param name="winOutcome">
        /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
        /// the job of the processor to remove pays which are not eligible for the bet.
        /// </param>
        /// <param name="prize">
        /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
        /// will be added to the win outcome.
        /// </param>
        /// <param name="pattern">
        /// A list of cell population outcomes which is representative of the symbols on a pattern (a multiway mask).
        /// This is the pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameters are null.
        /// </exception>
        public static void EvaluateSymbols(WinOutcome winOutcome, SlotPrize prize,
                                           CellPopulationOutcome pattern, RequiredWinOutcomeItemData winInformation)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException("prize", "Parameter may not be null");
            }
            if(pattern == null)
            {
                throw new ArgumentNullException("pattern", "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException("winInformation", "Parameter may not be null");
            }

            switch(prize.PayStrategy)
            {
                case SlotPrizePayStrategy.PayLeft:
                    {
                        EvaluateAdjacentSymbols(prize, pattern.PopulationEntryList, winOutcome, winInformation);
                    }
                    break;
                case SlotPrizePayStrategy.PayRight:
                    {
                        EvaluateAdjacentSymbols(prize, Enumerable.Reverse(pattern.PopulationEntryList).ToList(), winOutcome,
                            winInformation);
                    }
                    break;
                case SlotPrizePayStrategy.PayBoth:
                    {
                        EvaluateAdjacentSymbols(prize, pattern.PopulationEntryList, winOutcome, winInformation);

                        EvaluateAdjacentSymbols(prize, Enumerable.Reverse(pattern.PopulationEntryList).ToList(), winOutcome,
                            winInformation);
                    }
                    break;
                case SlotPrizePayStrategy.PayGroup:
                    {
                        for(var populationEntryIndex = 0;
                            populationEntryIndex < pattern.PopulationEntryList.Count;
                            populationEntryIndex++)
                        {
                            var groupPopulationEntryList = pattern.PopulationEntryList.GetRange(populationEntryIndex,
                                pattern.PopulationEntryList.Count - populationEntryIndex);

                            EvaluateAdjacentSymbols(prize, groupPopulationEntryList, winOutcome, winInformation);
                        }
                    }
                    break;
                default:
                    {
                        throw new EvaluatorConfigurationException("PayStrategy not supported for Multiway: " +
                                                                  prize.PayStrategy);
                    }
            }
        }

        /// <summary>
        /// Evaluate the given pattern against the given prize. This function will look for matching symbols in each
        /// population entry in the order they are returned by the pattern enumerator. If any population does not
        /// contain a match then the evaluation for the prize will stop when the anyGroup flag is false.
        /// </summary>
        /// <param name="prize">
        /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
        ///  will be added to the win outcome.
        /// </param>
        /// <param name="pattern">
        /// A list of cell population outcomes which is representative of the symbols on a pattern (a multiway mask).
        /// This is the pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winOutcome"> Any eligible wins will be placed in this outcome. </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameters are null.
        /// </exception>
        public static void EvaluateAdjacentSymbols(SlotPrize prize,
                                                   IList<PopulationEntry> pattern,
                                                   WinOutcome winOutcome,
                                                   RequiredWinOutcomeItemData winInformation)
        {
            if(prize == null)
            {
                throw new ArgumentNullException("prize", "Parameter may not be null");
            }
            if(pattern == null)
            {
                throw new ArgumentNullException("pattern", "Parameter may not be null");
            }
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException("winInformation", "Parameter may not be null");
            }

            var matchedPopulations = 0;
            var ways = new List<Way>{new Way()};

            foreach(var population in pattern)
            {
                var matchedCells = population.OutcomeCellList.Where(outcomeCell => prize.PrizeSymbol.Any(prizeSymbol => prizeSymbol.id == outcomeCell.symbolID)).ToList();

                if(matchedCells.Any())
                {
                    matchedPopulations++;
                    ways = ways.SelectMany(way => matchedCells.Select(outcomeCell => new Way(way, outcomeCell))).ToList();
                    AddWinOutcomeItems(prize, winOutcome, winInformation, matchedPopulations, ways);
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Add each eligible pay for a prize and pattern combination to the win outcome.
        /// </summary>
        /// <param name="prize"> The prize containing pays to be added if eligible. </param>
        /// <param name="winOutcome"> The win outcome to add pays to. </param>
        /// <param name="winInformation"> Object containing information which is required to add a win outcome. </param>
        /// <param name="matchedPopulations">The number of populations in which there was a match.</param>
        /// <param name="ways">List of ways that the prize may be matched.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static void AddWinOutcomeItems(SlotPrize prize, WinOutcome winOutcome,
                                              RequiredWinOutcomeItemData winInformation, int matchedPopulations,
                                              IList<Way> ways)
        {
            if(prize == null)
            {
                throw new ArgumentNullException("prize", "Parameter may not be null");
            }
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException("winInformation", "Parameter may not be null");
            }
            if(ways == null)
            {
                throw new ArgumentNullException("ways", "Parameter may not be null");
            }

            List<Way> matchedWays = null;

            // Both multiway and jumbo pays use prizePay.count as the required number of matched populations. 
            foreach(var prizePay in prize.PrizePay.Where(pay => pay.count == matchedPopulations))
            {
                // Don't calculate matchedWays until it's needed.  Preserve it through multiple iterations if it is needed.
                if(matchedWays == null)
                {
                    // When support is added for ordered pays, it should just use matchedWays = ways for ordered pays.
                    matchedWays = ways.Where(way => PrizeEvaluator.CheckSymbolCounts(prize, way.SymbolCounts)).ToList();
                }

                var distinctMatchedCells = matchedWays.SelectMany(way => way.Cells).Distinct().ToList();

                // Jumbo pays specifies a total symbol count.
                if(prizePay.totalSymbolCountSpecified &&
                   // Jumbo pays total symbol count must match the number of symbols in the win.
                   prizePay.totalSymbolCount != distinctMatchedCells.Count)
                    continue;

                var winOutcomeName = prize.name + "_" + prizePay.count;

                var waysMultiplier = prizePay.totalSymbolCountSpecified
                    ? distinctMatchedCells.Count // Jumbo pays sets the multiplier to the number of symbols matched.
                    : matchedWays.Count;         // Multiways sets the multiplier to the number of ways.

                if(waysMultiplier > 0)
                {
                    var winOutcomeItem = new WinOutcomeItem
                    {
                        name = winOutcomeName,
                        Prize = new Prize
                        {
                            prizeScaleName = winInformation.PrizeScaleName,
                            prizeName = prize.name,
                            multiplier = (uint)waysMultiplier
                        },
                        Pattern = new Pattern
                        {
                            Cluster = new Cluster(),
                            count = (uint)matchedPopulations,
                            name = winInformation.PopulationName,
                            patternListName = winInformation.PatternListName
                        },
                        MatchedWays = matchedWays,
                    };

                    // Select only the cells with unique coordinates
                    foreach(var matchedCell in distinctMatchedCells)
                    {
                        winOutcomeItem.Pattern.Cluster.Cells.Add(matchedCell.Cell);
                        winOutcomeItem.Pattern.SymbolList.Add(matchedCell.symbolID);
                    }

                    winOutcome.WinOutcomeItems.Add(winOutcomeItem);
                }
            }
        }
    }
}
