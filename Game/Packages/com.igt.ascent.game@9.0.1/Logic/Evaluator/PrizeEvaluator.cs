//-----------------------------------------------------------------------
// <copyright file = "PrizeEvaluator.cs" company = "IGT">
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
    /// The purpose of the prize evaluator is to determine if any patterns in a set of populated patterns fulfills the
    /// requirements of a set of prizes. So if a pattern was populated with [S1, S1, S1, S1] and there was a prize
    /// which had a required symbol of S1, and had pays for 3, 4, and 5 symbol counts, then a potential win would be
    /// added to the win outcome for each of those counts. After a prize evaluator is executed a win processor will
    /// normally be executed and filter the wins.
    /// </summary>
    public static class PrizeEvaluator
    {
        /// <summary>
        /// The PrizeEvaluatorCore contains the core functionality, which performs all the actual work. Each function
        /// in PrizeEvaluator validates its arguments, then calls PrizeEvaluatorCore to do the actual work. For speed,
        /// the PrizeEvaluatorCore functions do not validate their arguments.  Hence, PrizeEvaluatorCore is private.
        /// </summary>
        private static class PrizeEvaluatorCore
        {
            /// <summary>
            /// Evaluate the given prize against the given pattern.
            /// </summary>
            /// <param name="prize">The prize to evaluate</param>
            /// <param name="patternCells">The cells of the pattern to evaluate the prize against.</param>
            /// <param name="symbols">The symbols to be evaluated (extracted from the patternCells).</param>
            /// <param name="winOutcome">A win outcome to add wins to.</param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            /// <exception cref="EvaluatorConfigurationException">
            /// Thrown if the order strategy of the prize is not supported.
            /// </exception>
            public static void EvaluatePrize(SlotPrize prize,
                                             IList<OutcomeCell> patternCells,
                                             List<string> symbols,
                                             WinOutcome winOutcome,
                                             RequiredWinOutcomeItemData winInformation)
            {
                switch (prize.OrderStrategy)
                {
                    case SlotPrizeOrderStrategy.Ordered:
                        {
                            EvaluateSymbols(prize, patternCells, symbols, true, winOutcome, winInformation);
                            break;
                        }
                    case SlotPrizeOrderStrategy.Unordered:
                        {
                            EvaluateSymbols(prize, patternCells, symbols, false, winOutcome, winInformation);
                            break;
                        }
                    default:
                        {
                            throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                                    "Unsupported evaluation order strategy: {0}",
                                                                                    prize.OrderStrategy));
                        }
                }
            }

            /// <summary>
            /// Evaluate the given pattern against the given prize.
            /// </summary>
            /// <param name="prize">
            /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
            /// will be added to the win outcome.
            /// </param>
            /// <param name="patternCells">
            /// A collection of cells which is representative of the symbols on a pattern (such as a line). This is the
            /// pattern which will be evaluated against the prize.
            /// </param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="ordered">
            /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
            /// as an ordered pattern.
            /// </param>
            /// <param name="winOutcome">
            /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
            /// the job of the processor to remove pays which are not eligible for the bet.
            /// </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            /// <exception cref="EvaluatorConfigurationException">
            /// Thrown if the pay strategy of the prize is not supported.
            /// </exception>
            public static void EvaluateSymbols(SlotPrize prize,
                                               IList<OutcomeCell> patternCells,
                                               List<string> symbols,
                                               bool ordered,
                                               WinOutcome winOutcome,
                                               RequiredWinOutcomeItemData winInformation)
            {
                switch (prize.PayStrategy)
                {
                    case SlotPrizePayStrategy.PayLeft:
                        {
                            EvaluateAdjacentSymbols(prize, patternCells, symbols, true, ordered, winOutcome, winInformation);
                        }
                        break;
                    case SlotPrizePayStrategy.PayRight:
                        {
                            EvaluateAdjacentSymbols(prize, patternCells, symbols, false, ordered, winOutcome, winInformation);
                        }
                        break;
                    case SlotPrizePayStrategy.PayBoth:
                        {
                            EvaluateAdjacentSymbols(prize, patternCells, symbols, true, ordered, winOutcome, winInformation);
                            EvaluateAdjacentSymbols(prize, patternCells, symbols, false, ordered, winOutcome, winInformation);
                        }
                        break;
                    case SlotPrizePayStrategy.PayGroup:
                        {
                            EvaluateGroupSymbols(prize, patternCells, symbols, ordered, winOutcome, winInformation);
                        }
                        break;
                    case SlotPrizePayStrategy.PayAny:
                        {
                            EvaluateNonAdjacentSymbols(prize, patternCells, symbols, ordered, winOutcome, winInformation);
                        }
                        break;
                    default:
                        {
                            throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                                    "Unsupported evaluation pay strategy: {0}",
                                                                                    prize.PayStrategy));
                        }
                }
            }

            /// <summary>
            /// Evaluate the given pattern against the given prize. This function evaluates prizes which require that winning
            /// symbols be adjacent. For instance if a prize requires that S3 appear 3 times on the pattern, then the three
            /// instances must be adjacent to each other, and they must start at either the beginning or end of the pattern
            /// based on the order parameter.
            /// </summary>
            /// <param name="prize">
            /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
            ///  will be added to the win outcome.
            /// </param>
            /// <param name="patternCells">
            /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is
            ///  the pattern which will be evaluated against the prize.
            /// </param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="forward">
            /// Indicates if the evaluation should be done in forward order.  In a normal line pattern this usually equates to PayLeft.
            /// If forward is false, the evaluate is done in reverse order.   In a normal line pattern this usually equates to PayRight.
            /// </param>
            /// <param name="ordered">
            /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
            /// as an ordered pattern.
            /// </param>
            /// <param name="winOutcome"> Any eligible wins will be placed in this outcome. </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            public static void EvaluateAdjacentSymbols(SlotPrize prize,
                                                       IList<OutcomeCell> patternCells,
                                                       IList<string> symbols,
                                                       bool forward,
                                                       bool ordered,
                                                       WinOutcome winOutcome,
                                                       RequiredWinOutcomeItemData winInformation)
            {
                var startIndex = forward ? 0 : patternCells.Count - 1;

                if (ordered)
                {
                    EvaluateOrderedSymbols(prize, patternCells, symbols, startIndex, forward, true, winOutcome, winInformation);
                }
                else
                {
                    EvaluateUnorderedSymbols(prize, patternCells, symbols, startIndex, forward, true, winOutcome, winInformation);
                }
            }

            /// <summary>
            /// Evaluate the given pattern against the given prize. This function evaluates symbols that are not required
            /// to be adjacent in a pattern. Two cells are considered non-adjacent if another cell lies between them in the
            /// given list of <see cref="OutcomeCell"/> objects.
            /// </summary>
            /// <param name="prize">
            /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
            /// will be added to the win outcome.
            /// </param>
            /// <param name="patternCells">
            /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is the
            /// pattern which will be evaluated against the prize.
            /// </param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="ordered">
            /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
            /// as an ordered pattern.
            /// </param>
            /// <param name="winOutcome"> Any eligible wins will be placed in this outcome. </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            public static void EvaluateNonAdjacentSymbols(SlotPrize prize,
                                                          IList<OutcomeCell> patternCells,
                                                          IList<string> symbols,
                                                          bool ordered,
                                                          WinOutcome winOutcome,
                                                          RequiredWinOutcomeItemData winInformation)
            {
                if (ordered)
                {
                    EvaluateOrderedSymbols(prize, patternCells, symbols, 0, true, false, winOutcome, winInformation);
                }
                else
                {
                    EvaluateUnorderedSymbols(prize, patternCells, symbols, 0, true, false, winOutcome, winInformation);
                }
            }

            /// <summary>
            /// Evaluate the given pattern against the given prize. The symbols in the
            /// population outcome need to be contiguous, but can appear anywhere in the population. For instance
            /// a prize which required 3 S1 symbols to appear within a pattern of 5 would win in the following scenarios:
            /// [S1 S1 S1 XX XX] [XX S1 S1 S1 XX] [XX XX S1 S1 S1].
            /// </summary>
            /// <param name="prize">
            /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
            /// will be added to the win outcome.
            /// </param>
            /// <param name="patternCells">
            /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is the
            /// pattern which will be evaluated against the prize.
            /// </param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="ordered">
            /// Specifies whether the locations of the symbols matter for evaluation.
            /// </param>
            /// <param name="winOutcome">
            /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
            /// the job of the processor to remove pays which are not eligible for the bet.
            /// </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            public static void EvaluateGroupSymbols(SlotPrize prize,
                                                    IList<OutcomeCell> patternCells,
                                                    List<string> symbols,
                                                    bool ordered,
                                                    WinOutcome winOutcome,
                                                    RequiredWinOutcomeItemData winInformation)
            {
                var maxStartIndex = symbols.Count - prize.PrizePay.Select(prizePay => prizePay.count).Min();

                for (var startIndex = 0; startIndex <= maxStartIndex; startIndex++)
                {
                    var symbolsSubset = symbols.GetRange(startIndex, symbols.Count - startIndex);
                    var patternCellsSubset = patternCells.Skip(startIndex).Take(symbols.Count - startIndex).ToList();

                    if (ordered)
                    {
                        EvaluateOrderedSymbols(prize, patternCellsSubset, symbolsSubset, 0, true, true, winOutcome,
                            winInformation);
                    }
                    else
                    {
                        EvaluateUnorderedSymbols(prize, patternCellsSubset, symbolsSubset, 0, true, true, winOutcome,
                            winInformation);
                    }
                }
            }

            /// <summary>
            /// Count the occurrences of each prize symbol within the pattern. Each symbol in the prize must be at the index
            /// specified in the prize.
            /// </summary>
            /// <param name="prize">Prize to evaluate against the pattern.</param>
            /// <param name="patternCells">Pattern to count the prize symbols in.</param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="startIndex">
            /// The index in symbols to start evaluating at.
            /// </param>
            /// <param name="forward">
            /// Indicates if the evaluation should be done in forward order.  In a normal line pattern this usually equates to PayLeft.
            /// If forward is false, the evaluate is done in reverse order.   In a normal line pattern this usually equates to PayRight.
            /// </param>
            /// <param name="adjacent">
            /// Flag indicating if winning symbols must be adjacent to each other. If this is true, then counting stops
            /// as soon as a pattern position without a prize symbol is encountered.
            /// </param>
            /// <param name="winOutcome">
            /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
            /// the job of the processor to remove pays which are not eligible for the bet.
            /// </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            public static void EvaluateOrderedSymbols(SlotPrize prize,
                                                      IList<OutcomeCell> patternCells,
                                                      IList<string> symbols,
                                                      int startIndex,
                                                      bool forward,
                                                      bool adjacent,
                                                      WinOutcome winOutcome,
                                                      RequiredWinOutcomeItemData winInformation)
            {
                var prizeSymbols = prize.PrizeSymbol;
                var symbolCounts = new Dictionary<string, int>();
                var matchedIndexes = new List<int>();
                var direction = forward ? 1 : -1;

                for (var index = startIndex; index >= 0 && index < symbols.Count; index += direction)
                {
                    var desiredSymbol = symbols[index];
                    var matchedSymbol =
                        prizeSymbols.Any(prizeSymbol => prizeSymbol.index == index && prizeSymbol.id == desiredSymbol);

                    if (matchedSymbol)
                    {
                        matchedIndexes.Add(index);
                        if (symbolCounts.ContainsKey(desiredSymbol))
                        {
                            symbolCounts[desiredSymbol]++;
                        }
                        else
                        {
                            symbolCounts[desiredSymbol] = 1;
                        }

                        AddWinOutcomeItems(patternCells, prize, matchedIndexes, winInformation, winOutcome,
                                           symbolCounts);
                    }
                    else if (adjacent)
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Count the occurrences of each prize symbol within the pattern.
            /// </summary>
            /// <param name="prize">Prize to evaluate against the pattern.</param>
            /// <param name="patternCells">Pattern to count the prize symbols in.</param>
            /// <param name="symbols">
            /// The symbols to be evaluated (extracted from the patternCells).
            /// </param>
            /// <param name="startIndex">
            /// The index in symbols to start evaluating at.
            /// </param>
            /// <param name="forward">
            /// Indicates if the evaluation should be done in forward order.  In a normal line pattern this usually equates to PayLeft.
            /// If forward is false, the evaluate is done in reverse order.   In a normal line pattern this usually equates to PayRight.
            /// </param>
            /// <param name="adjacent">
            /// Flag indicating if winning symbols must be adjacent to each other. If this is true, then counting stops
            /// as soon as a pattern position without a prize symbol is encountered.
            /// </param>
            /// <param name="winOutcome">
            /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
            /// the job of the processor to remove pays which are not eligible for the bet.
            /// </param>
            /// <param name="winInformation">
            /// Object containing information which is required to add a win outcome. This data is not used for any
            /// evaluation purposes.
            /// </param>
            public static void EvaluateUnorderedSymbols(SlotPrize prize,
                                                        IList<OutcomeCell> patternCells,
                                                        IList<string> symbols,
                                                        int startIndex,
                                                        bool forward,
                                                        bool adjacent,
                                                        WinOutcome winOutcome,
                                                        RequiredWinOutcomeItemData winInformation)
            {
                var prizeSymbols = prize.PrizeSymbol;
                var symbolCounts = new Dictionary<string, int>();
                var matchedIndexes = new List<int>();
                var direction = forward ? 1 : -1;

                for (var index = startIndex; index >= 0 && index < symbols.Count; index += direction)
                {
                    var desiredSymbol = symbols[index];
                    var matched = false;

                    if (prizeSymbols.Any(prizeSymbol => prizeSymbol.id == desiredSymbol))
                    {
                        if (symbolCounts.ContainsKey(desiredSymbol))
                        {
                            symbolCounts[desiredSymbol]++;
                        }
                        else
                        {
                            symbolCounts[desiredSymbol] = 1;
                        }

                        matchedIndexes.Add(index);
                        matched = true;

                        AddWinOutcomeItems(patternCells, prize, matchedIndexes, winInformation, winOutcome,
                                           symbolCounts);
                    }

                    // The items must be contiguous in a pay right or left evaluation,
                    // so the counting needs to stop when a match is not found.
                    if (!matched && adjacent)
                    {
                        break;
                    }
                }
            }

            /// <summary>
            /// Add each eligible pay for a prize and pattern combination to the win outcome.
            /// </summary>
            /// <param name="patternCells"> The population entry used to fill the win outcome pattern information. </param>
            /// <param name="prize"> The prize containing pays to be added if eligible. </param>
            /// <param name="matchedIndexes"> A list of the symbol indexes which matched in this prize. </param>
            /// <param name="winInformation"> Object containing information which is required to add a win outcome. </param>
            /// <param name="winOutcome"> The win outcome to add pays to. </param>
            /// <param name="symbolCounts">Dictionary of symbols to the counts associated with those symbols.</param>
            public static void AddWinOutcomeItems(IList<OutcomeCell> patternCells, SlotPrize prize,
                                                  IList<int> matchedIndexes, RequiredWinOutcomeItemData winInformation,
                                                  WinOutcome winOutcome, IDictionary<string, int> symbolCounts)
            {
                foreach (var prizePay in prize.PrizePay.Where(prizePay => matchedIndexes.Count == prizePay.count))
                {
                    if (SymbolCountsMet(prize, symbolCounts))
                    {
                        var winOutcomeName = prize.name + "_" + prizePay.count;

                        var winOutcomeItem = new WinOutcomeItem
                        {
                            name = winOutcomeName,
                            Pattern = new Pattern(),
                            Prize = new Prize()
                        };

                        winOutcomeItem.Pattern.Cluster = new Cluster();

                        for (var payIndex = 0; payIndex < prizePay.count; payIndex++)
                        {
                            var outcomeCell = patternCells[matchedIndexes[payIndex]];
                            winOutcomeItem.Pattern.Cluster.Cells.Add(outcomeCell.Cell);
                            winOutcomeItem.Pattern.SymbolList.Add(outcomeCell.symbolID);
                        }

                        winOutcomeItem.Pattern.patternListName = winInformation.PatternListName;
                        winOutcomeItem.Pattern.name = winInformation.PopulationName;
                        winOutcomeItem.Pattern.count = (uint)prizePay.count;

                        winOutcomeItem.Prize.prizeScaleName = winInformation.PrizeScaleName;
                        winOutcomeItem.Prize.prizeName = prize.name;

                        winOutcome.WinOutcomeItems.Add(winOutcomeItem);
                    }
                }
            }

            /// <summary> Tell if the required counts of each symbol in the prize are met. </summary>
            /// <param name="prize"> Prize to check required counts against. </param>
            /// <param name="symbolCounts">
            /// A dictionary which contains each winning symbol found and the number of times the symbol appeared.
            /// </param>
            /// <returns>True if the required counts are met.</returns>
            public static bool SymbolCountsMet(SlotPrize prize, IDictionary<string, int> symbolCounts)
            {
                bool countsMet;

                // A prize with grouped symbols will specify both the index and a required count for all symbols.
                if (prize.PrizeSymbol.Any(symbol => symbol.indexSpecified &&          // Grouped symbol notation will specify an index, but so do ordered combos.
                                                   symbol.requiredCountSpecified &&  // Grouped symbol notation will specify a required count, but ordered combos may also.
                                                   symbol.requiredCount > 0))      // Grouped symbol notation will have a count > 0 somewhere - but ordered combos will specify a count of 0, if any.
                {
                    countsMet = CheckGroupedSymbolCounts(prize, symbolCounts);
                }
                else
                {
                    countsMet =
                        prize.PrizeSymbol.All(
                            symbol =>
                                !symbol.requiredCountSpecified ||  // If symbol's required count is not specified, the count is met.
                                symbol.requiredCount == 0 ||     // If symbol's required count is 0, the count is met.
                                (symbolCounts.ContainsKey(symbol.id) &&               // If symbolCounts contains the symbol...
                                symbolCounts[symbol.id] >= symbol.requiredCount));  // ... and there is at least the required number of that symbol, the count is met.
                }

                return countsMet;
            }

            /// <summary>Check that the required counts of each symbol group in the prize are met.</summary>
            /// <param name="prize"> Prize to check required counts against. </param>
            /// <param name="symbolCounts">
            /// A dictionary which contains each winning symbol found and the number of times the symbol appeared.
            /// </param>
            /// <returns>True if the required group counts are met.</returns>
            private static bool CheckGroupedSymbolCounts(SlotPrize prize, IDictionary<string, int> symbolCounts)
            {
                var requiredCountPerGroup = new Dictionary<uint, uint>();
                uint totalRequiredCount = 0;

                // Build a list of required counts - one for for each group.
                foreach (var symbol in prize.PrizeSymbol)
                {
                    if (requiredCountPerGroup.ContainsKey(symbol.index))
                    {
                        requiredCountPerGroup[symbol.index] += symbol.requiredCount;
                    }
                    else
                    {
                        requiredCountPerGroup[symbol.index] = symbol.requiredCount;
                    }

                    totalRequiredCount += symbol.requiredCount;
                }

                // Attempt to match the symbols to groups in a way that satisfies each group's required count.
                return totalRequiredCount <= symbolCounts.Values.Sum() &&
                       MatchSymbolsToGroups(prize.PrizeSymbol, symbolCounts, requiredCountPerGroup, totalRequiredCount);
            }

            /// <summary>
            /// Match the given symbols to the given symbol groups. This looks for all groups that the next
            /// given symbol can match.  If none are found, it returns false. For each group found, this assigns
            /// the symbol to that group, then recursively searches for matches for all remaining symbols.
            /// </summary>
            /// <param name="prizeSymbols">A collection of prize symbols that define the symbol groups.</param>
            /// <param name="symbolCounts">
            /// A dictionary which contains each winning symbol found and the number of times the symbol appeared.
            /// </param>
            /// <param name="requiredCountPerGroup">
            /// A dictionary which contains each group and the number of matching symbols required for that group.
            /// </param>
            /// <param name="totalRequiredCount">The total number of matching symbols required for all groups.</param>
            /// <returns>True if the given symbols can be used to match the required group counts.</returns>
            /// <remarks>
            /// If <paramref name="totalRequiredCount"/> is greater than the sum of <paramref name="symbolCounts"/>,
            /// this function will fail. The caller should check for that case before calling MatchSymbolsToGroups().
            /// Checking once before calling this function is more efficient than checking inside this recursive function.
            /// </remarks>
            private static bool MatchSymbolsToGroups(IList<PrizeSymbol> prizeSymbols,
                                                     IDictionary<string, int> symbolCounts,
                                                     IDictionary<uint, uint> requiredCountPerGroup,
                                                     uint totalRequiredCount)
            {
                var matched = false;

                // Get the next symbol to place in a group
                var symbolId = symbolCounts.First(kvp => kvp.Value > 0).Key;

                // Decrement the symbol count, indicating that one instance of this symbolId has been placed.
                symbolCounts[symbolId]--;

                // Check each group that the symbol might be placed in
                for (uint groupIndex = 0; groupIndex < requiredCountPerGroup.Count; ++groupIndex)
                {
                    if (requiredCountPerGroup[groupIndex] > 0 &&                          // The group must have a positive required count.
                       prizeSymbols.Any(prizeSymbol => prizeSymbol.index == groupIndex && // There must be a symbol in that group...
                                                       prizeSymbol.id == symbolId))       // ... with the same id as symbolId.
                    {
                        // If this is the last symbol to place in a group, then we're done and it matched.
                        if (totalRequiredCount == 1)
                        {
                            matched = true;
                            break;
                        }

                        // Decrement the required count for this group, since we have assigned the current symbol to it.
                        requiredCountPerGroup[groupIndex]--;

                        matched = MatchSymbolsToGroups(prizeSymbols, symbolCounts, requiredCountPerGroup, totalRequiredCount - 1);

                        // Restore the count for future attempts within this loop.
                        requiredCountPerGroup[groupIndex]++;

                        if(matched)
                        {
                            break;
                        }
                    }
                }

                // Restore the symbolCount for future attempts by this function's caller.
                symbolCounts[symbolId]++;

                return matched;
            }
        }

        #region Public methods

        /// <summary>
        /// Evaluate all of the patterns in a cell population outcome. Each entry is treated as a populated pattern.
        /// </summary>
        /// <param name="prizeScale"> The prize scale to evaluate against the patterns. </param>
        /// <param name="patterns"> The list of populated patterns. </param>
        /// <returns> A win outcome containing all winning prizes. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if prizeScale or patterns is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if prizeScale contains any null prizes.
        /// Thrown if patterns contains any null patterns, or patterns with null OutcomeCellLists.
        /// </exception>
        public static WinOutcome EvaluatePatterns(SlotPrizeScale prizeScale, CellPopulationOutcome patterns)
        {
            if(prizeScale == null)
            {
                throw new ArgumentNullException(nameof(prizeScale), "Parameter cannot be null");
            }
            if(prizeScale.Prize.Any(prize => prize == null))
            {
                throw new ArgumentException("PrizeScale cannot contain null entries");
            }
            if(patterns == null)
            {
                throw new ArgumentNullException(nameof(patterns), "Parameter cannot be null");
            }
            if(patterns.PopulationEntryList.Any(pattern => pattern?.OutcomeCellList == null))
            {
                throw new ArgumentException("Patterns cannot contain null entries or null OutcomeCellLists");
            }

            var wins = new WinOutcome();

            foreach(var pattern in patterns.PopulationEntryList)
            {
                var patternName = pattern.name;
                var symbols = pattern.OutcomeCellList.Select(t => t.symbolID).ToList();

                foreach(
                    var prize in
                        prizeScale.Prize.Where(
                            prize => prize.EligiblePattern == "ALL" || prize.EligiblePattern == patternName))
                {
                    PrizeEvaluatorCore.EvaluatePrize(prize, pattern.OutcomeCellList, symbols, wins,
                        new RequiredWinOutcomeItemData(prizeScale.name, patterns.name, patternName));
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
        /// <exception cref="ArgumentNullException">
        /// Thrown if any parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the pattern contains a null OutcomeCellList.
        /// </exception>
        public static void EvaluatePrize(SlotPrize prize, WinOutcome wins, PopulationEntry pattern,
                                         RequiredWinOutcomeItemData winInformation)
        {
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(wins == null)
            {
                throw new ArgumentNullException(nameof(wins), "Parameter may not be null");
            }
            if(pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern), "Parameter may not be null");
            }
            if(pattern.OutcomeCellList == null)
            {
                throw new ArgumentException("Pattern may not contain a null OutcomeCellLists");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var symbols = pattern.OutcomeCellList.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluatePrize(prize, pattern.OutcomeCellList, symbols, wins, winInformation);
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
        /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is the
        /// pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="ordered">
        /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
        /// as an ordered pattern.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the winOutcome, prize, pattern, or winInformation parameter is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the pattern contains a null OutcomeCellList.
        /// </exception>
        public static void EvaluateSymbols(WinOutcome winOutcome, SlotPrize prize, PopulationEntry pattern,
                                           RequiredWinOutcomeItemData winInformation, bool ordered)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern), "Parameter may not be null");
            }
            if(pattern.OutcomeCellList == null)
            {
                throw new ArgumentException("Pattern may not contain a null OutcomeCellLists");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var symbols = pattern.OutcomeCellList.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateSymbols(prize, pattern.OutcomeCellList, symbols, true, winOutcome, winInformation);
        }

        /// <summary>
        /// Evaluate the given pattern against the given prize. This function evaluates prizes which require that winning
        /// symbols be adjacent. For instance if a prize requires that S3 appear 3 times on the pattern, then the three
        /// instances must be adjacent to each other, and they must start at either the beginning or end of the pattern
        /// based on the order parameter.
        /// </summary>
        /// <param name="winOutcome"> Any eligible wins will be placed in this outcome. </param>
        /// <param name="prize">
        /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
        ///  will be added to the win outcome.
        /// </param>
        /// <param name="patternCells">
        /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is
        ///  the pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="reverse">
        /// Indicate if the evaluation should be done in reverse order. In a normal line pattern this usually equates
        /// to pay right.
        /// </param>
        /// <param name="ordered">
        /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
        /// as an ordered pattern.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the winOutcome, prize, patternCells, or winInformation parameter is null.
        /// </exception>
        /// TODO: does this API need updated? reverse is never used
        public static void EvaluateAdjacentSymbols(WinOutcome winOutcome, SlotPrize prize,
                                                   IList<OutcomeCell> patternCells,
                                                   RequiredWinOutcomeItemData winInformation,
                                                   bool reverse, bool ordered)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Pattern may not contain a null OutcomeCellLists");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var symbols = patternCells.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateAdjacentSymbols(prize, patternCells, symbols, true, ordered, winOutcome, winInformation);
        }

        /// <summary>
        /// Evaluate the given pattern against the given prize. This function evaluates symbols that are not required
        /// to be adjacent in a pattern. Two cells are considered non-adjacent if another cell lies between them in the
        /// given list of <see cref="OutcomeCell"/> objects.
        /// </summary>
        /// <param name="winOutcome"> Any eligible wins will be placed in this outcome. </param>
        /// <param name="prize">
        /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
        /// will be added to the win outcome.
        /// </param>
        /// <param name="patternCells">
        /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is the
        /// pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="ordered">
        /// Indicate if this evaluation is ordered. In an unordered evaluation the symbols in the prize are not treated
        /// as an ordered pattern.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the winOutcome, prize, patternCells, or winInformation parameter is null.
        /// </exception>
        public static void EvaluateNonAdjacentSymbols(WinOutcome winOutcome, SlotPrize prize,
                                                      IList<OutcomeCell> patternCells,
                                                      RequiredWinOutcomeItemData winInformation, 
                                                      bool ordered)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Pattern may not contain a null OutcomeCellLists");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var symbols = patternCells.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateNonAdjacentSymbols(prize, patternCells, symbols, ordered, winOutcome, winInformation);
        }

        /// <summary>
        /// Evaluate the given pattern against the given prize. The symbols in the
        /// population outcome need to be contiguous, but can appear anywhere in the population. For instance
        /// a prize which required 3 S1 symbols to appear within a pattern of 5 would win in the following scenarios:
        /// [S1 S1 S1 XX XX] [XX S1 S1 S1 XX] [XX XX S1 S1 S1].
        /// </summary>
        /// <param name="prize">
        /// This is the prize which the population outcome will be evaluated against. Any eligible pays in this prize
        /// will be added to the win outcome.
        /// </param>
        /// <param name="patternCells">
        /// A cell population entry which is representative of the symbols on a pattern (such as a line). This is the
        /// pattern which will be evaluated against the prize.
        /// </param>
        /// <param name="winOutcome">
        /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
        /// the job of the processor to remove pays which are not eligible for the bet.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="ordered">
        /// Specifies whether the locations of the symbols matter for evaluation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the winOutcome, prize, patternCells, or winInformation parameter is null.
        /// </exception>
        public static void EvaluateGroupSymbols(WinOutcome winOutcome, SlotPrize prize,
                                                List<OutcomeCell> patternCells,
                                                RequiredWinOutcomeItemData winInformation, bool ordered)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Parameter may not be null.");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var symbols = patternCells.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateGroupSymbols(prize, patternCells, symbols, ordered, winOutcome, winInformation);
        }

        /// <summary>
        /// Count the occurrences of each prize symbol within the pattern.
        /// </summary>
        /// <param name="prize">Prize to evaluate against the pattern.</param>
        /// <param name="patternCells">Pattern to count the prize symbols in.</param>
        /// <param name="adjacent">
        /// Flag indicating if winning symbols must be adjacent to each other. If this is true, then counting stops
        /// as soon as a pattern position without a prize symbol is encountered.
        /// </param>
        /// <param name="winOutcome">
        /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
        /// the job of the processor to remove pays which are not eligible for the bet.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="originalPattern">Pattern used when adding the win outcome item.</param>
        /// <returns>A dictionary of symbols to the number of occurrences of the symbol.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the prize, patternCells, winOutcome, or winInformation parameter is null.
        /// </exception>
        /// TODO: does this API need updated? originalPattern is unused
        public static void EvaluateUnorderedSymbols(SlotPrize prize,
                                                    IEnumerable<KeyValuePair<int, OutcomeCell>> patternCells,
                                                    bool adjacent, WinOutcome winOutcome,
                                                    RequiredWinOutcomeItemData winInformation,
                                                    IList<OutcomeCell> originalPattern)
        {
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Parameter may not be null");
            }
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var pattern = patternCells.Select(t => t.Value).ToList();
            var symbols = pattern.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateUnorderedSymbols(prize, pattern, symbols, 0, true, adjacent, winOutcome, winInformation);
        }

        /// <summary>
        /// Count the occurrences of each prize symbol within the pattern. Each symbol in the prize must be at the index
        /// specified in the prize.
        /// </summary>
        /// <param name="prize">Prize to evaluate against the pattern.</param>
        /// <param name="patternCells">Pattern to count the prize symbols in.</param>
        /// <param name="winOutcome">
        /// Any eligible wins will be placed in this outcome. Bet requirements are not taken into account, and it is
        /// the job of the processor to remove pays which are not eligible for the bet.
        /// </param>
        /// <param name="winInformation">
        /// Object containing information which is required to add a win outcome. This data is not used for any
        /// evaluation purposes.
        /// </param>
        /// <param name="originalPattern">Pattern used when adding the win outcome item.</param>
        /// <param name="adjacent">
        /// Flag indicating if winning symbols must be adjacent to each other. If this is true, then counting stops
        /// as soon as a pattern position without a prize symbol is encountered.
        /// </param>
        /// <returns>A dictionary of symbols to the number of occurrences of the symbol.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the prize, patternCells, winOutcome, or winInformation parameter is null.
        /// </exception>
        /// TODO: does this API need updated? originalPattern and adjacent are unused
        public static void EvaluateOrderedSymbols(SlotPrize prize,
                                                  IEnumerable<KeyValuePair<int, OutcomeCell>> patternCells,
                                                  WinOutcome winOutcome,
                                                  RequiredWinOutcomeItemData winInformation,
                                                  IList<OutcomeCell> originalPattern, 
                                                  bool adjacent)
        {
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Parameter may not be null");
            }
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }

            var pattern = patternCells.Select(t => t.Value).ToList();
            var symbols = pattern.Select(t => t.symbolID).ToList();
            PrizeEvaluatorCore.EvaluateOrderedSymbols(prize, pattern, symbols, 0, true, true, winOutcome, winInformation);
        }

        /// <summary>
        /// Add each eligible pay for a prize and pattern combination to the win outcome.
        /// </summary>
        /// <param name="patternCells"> The population entry used to fill the win outcome pattern information. </param>
        /// <param name="prize"> The prize containing pays to be added if eligible. </param>
        /// <param name="matchedIndexes"> A list of the symbol indexes which matched in this prize. </param>
        /// <param name="winInformation"> Object containing information which is required to add a win outcome. </param>
        /// <param name="winOutcome"> The win outcome to add pays to. </param>
        /// <param name="symbolCounts">Dictionary of symbols to the counts associated with those symbols.</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static void AddWinOutcomeItems(IList<OutcomeCell> patternCells, SlotPrize prize,
                                              IList<int> matchedIndexes, RequiredWinOutcomeItemData winInformation,
                                              WinOutcome winOutcome, Dictionary<string, int> symbolCounts)
        {
            if(patternCells == null)
            {
                throw new ArgumentNullException(nameof(patternCells), "Parameter may not be null");
            }
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(matchedIndexes == null)
            {
                throw new ArgumentNullException(nameof(matchedIndexes), "Parameter may not be null");
            }
            if(winInformation == null)
            {
                throw new ArgumentNullException(nameof(winInformation), "Parameter may not be null");
            }
            if(winOutcome == null)
            {
                throw new ArgumentNullException(nameof(winOutcome), "Parameter may not be null");
            }
            if(symbolCounts == null)
            {
                throw new ArgumentNullException(nameof(symbolCounts), "Parameter may not be null");
            }

            // Call this to do the actual work.
            PrizeEvaluatorCore.AddWinOutcomeItems(patternCells, prize, matchedIndexes, winInformation, winOutcome, symbolCounts);
        }

        /// <summary> Check that the required counts of each symbol in the prize are met. </summary>
        /// <param name="prize"> Prize to check required counts against. </param>
        /// <param name="symbolCounts">
        /// A dictionary which contains each winning symbol found and the number of times the symbol appeared.
        /// </param>
        /// <returns>True if the required counts are met.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
        public static bool CheckSymbolCounts(SlotPrize prize, IDictionary<string, int> symbolCounts)
        {
            if(prize == null)
            {
                throw new ArgumentNullException(nameof(prize), "Parameter may not be null");
            }
            if(symbolCounts == null)
            {
                throw new ArgumentNullException(nameof(symbolCounts), "Parameter may not be null");
            }

            // Call this to do the actual work.
            return PrizeEvaluatorCore.SymbolCountsMet(prize, symbolCounts);
        }

        /// <summary>
        /// Create an enumerator for a collection which returns ItemIndexPairs.
        /// </summary>
        /// <typeparam name="T">Type of item in the collection.</typeparam>
        /// <param name="list">Collection to enumerate.</param>
        /// <param name="reverse">If true the collection will be enumerated in descending order.</param>
        /// <returns>An IEnumerable of ItemIndexPairs</returns>
        public static IEnumerable<KeyValuePair<int, T>> CreateIndexedEnumerator<T>(IList<T> list, bool reverse)
        {
            return reverse ? CreateReverseIndexedEnumerator(list) : CreateForwardIndexedEnumerator(list);
        }

        /// <summary>
        /// Create an enumerator for a collection which returns ItemIndexPairs. The collection will be iterated in
        /// descending order.
        /// </summary>
        /// <typeparam name="T">Type of item in the collection.</typeparam>
        /// <param name="list">Collection to enumerate.</param>
        /// <returns>An IEnumerable of ItemIndexPairs</returns>
        public static IEnumerable<KeyValuePair<int, T>> CreateReverseIndexedEnumerator<T>(IList<T> list)
        {
            var count = list.Count;
            for(var index = count - 1; index >= 0; --index)
            {
                yield return new KeyValuePair<int, T>(index, list[index]);
            }
        }

        /// <summary>
        /// Create an enumerator for a collection which returns ItemIndexPairs. The collection will be iterated in
        /// ascending order.
        /// </summary>
        /// <typeparam name="T">Type of item in the collection.</typeparam>
        /// <param name="list">Collection to enumerate.</param>
        /// <returns>An IEnumerable of ItemIndexPairs</returns>
        public static IEnumerable<KeyValuePair<int, T>> CreateForwardIndexedEnumerator<T>(IList<T> list)
        {
            var count = list.Count;
            for(var index = 0; index < count; index++)
            {
                yield return new KeyValuePair<int, T>(index, list[index]);
            }
        }

        #endregion
    }
}
