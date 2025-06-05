//-----------------------------------------------------------------------
// <copyright file = "WinProcessor.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// The purpose of the win processor is to filter a set of wins down to those which are the best for the set of
    /// patterns and prizes.
    /// </summary>
    public static class WinProcessor
    {
        /// <summary>
        /// Execute the best static strategy. The first step is to determine the best pay for a given prize on a given
        /// pattern. So prize A may have 3, 4, and 5 symbol variants on the same pattern. The best of these pays will
        /// be determined and the suboptimal pays removed. The next step is then to determine the best prize on a given
        /// pattern. In the best static strategy the best prize, as determined by the available static data, is awarded.
        /// Progressives are assumed to be of more value than any normal win or trigger. Normal wins and triggers are
        /// prioritized based on their values and average bonus pay. If there is a line which has multiple awards which
        /// are the same value, then those awards are prioritized by symbol count and then by order in the paytable.
        /// So if a line were to have an award for 3 symbols and an award for 5 symbols and those rewards had equivalent
        /// values, then the 5 symbol award would have priority.
        /// </summary>
        /// <param name="winOutcome"> The win outcome on which to execute the pay strategy. </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if the winOutcome parameter is null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static void ExecuteBestStaticMode(WinOutcome winOutcome, ProgressiveLevels progressiveLevels)
        {
            if (winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null.");
            }

            var bestPays = FindBestWinPerPrizePerPattern(winOutcome, progressiveLevels);
            var bestPatternPay = FindBestPrizePerPattern(bestPays, progressiveLevels);

            //Find all wins that are not in the bestPatternPay list. ToList must be used to create a list which is
            //separate from the win outcome list. If the list is not separated, then it cannot be iterated because it will
            //become invalid after a win outcome item is remo
            var winsToRemove =
                (from win in winOutcome.WinOutcomeItems where bestPatternPay[win.Pattern.name] != win select win).ToList
                    ();

            foreach (var win in winsToRemove)
            {
                winOutcome.WinOutcomeItems.Remove(win);
            }

            //Update the name of each win outcome item to include the pattern it was on. This will make the name unique
            //which simplifies further processing of wins.
            foreach (var winOutcomeitem in winOutcome.WinOutcomeItems)
            {
                winOutcomeitem.name += "_" + winOutcomeitem.Pattern.name;
            }
        }


        /// <summary>
        /// Execute the best win per prize. In this strategy each prize is filtered down to the best pay for that prize
        /// regardless of what pattern it is on. This strategy is most useful for multiway.
        /// </summary>
        /// <param name="winOutcome"> The win outcome on which to execute the pay strategy. </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if the winOutcome parameter is null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static void ExecuteBestWinPerPrize(WinOutcome winOutcome, ProgressiveLevels progressiveLevels)
        {
            if (winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null.");
            }

            var bestPays = FindBestWinPerPrize(winOutcome, progressiveLevels);

            //Find all wins that are not in the bestPatternPay list. ToList must be used to create a list which is
            //separate from the win outcome list. If the list is not separated, then it cannot be iterated because it will
            //become invalid after a win outcome item is removed.
            var winsToRemove =
                (from win in winOutcome.WinOutcomeItems where bestPays[win.Prize.prizeName] != win select win).ToList();

            foreach (var win in winsToRemove)
            {
                winOutcome.WinOutcomeItems.Remove(win);
            }
        }

        /// <summary>
        /// Executes the best pay per way strategy, which guarantees that the best win per individual way will be awarded.
        /// </summary>
        /// <param name="winOutcome">The win outcome on which to execute this strategy.</param>
        /// <param name="rows">The number of rows in the symbol window.</param>
        /// <param name="cols">The number of columns in the symbol window.</param>
        /// <param name="progressiveLevels">The progressive levels from the paytable.</param>
        public static void ExecuteBestPayPerWay(
            WinOutcome winOutcome,
            int rows,
            int cols,
            ProgressiveLevels progressiveLevels)
        {
            ExecuteBestPayPerWay(winOutcome, rows, cols, progressiveLevels, null);
        }

        /// <summary>
        /// Executes the best pay per way strategy and collects the <see cref="Way"/> objects that were removed
        /// in a dictionary keyed by the name of the prize that the ways were removed from.
        /// </summary>
        /// <param name="winOutcome">The win outcome on which to execute this strategy.</param>
        /// <param name="rows">The number of rows in the symbol window.</param>
        /// <param name="cols">The number of columns in the symbol window.</param>
        /// <param name="progressiveLevels">The progressive levels from the paytable.</param>
        /// <param name="removedWays">
        /// (optional) If <paramref name="removedWays"/> is not <b>null</b>, it will be populated with lists of 
        /// <see cref="Way"/> objects that were removed from prizes. The key for each list is the name of the prize
        /// that the ways were removed from.
        /// </param>
        /// <devdoc>
        /// This method is primarily for development. If you don't need the information, consider calling 
        /// <see cref="ExecuteBestPayPerWay(IGT.Game.Core.Logic.Evaluator.Schemas.WinOutcome,int,int,IGT.Game.Core.Logic.Evaluator.Schemas.ProgressiveLevels)"/> 
        /// instead.
        /// </devdoc>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="winOutcome"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if either <paramref name="rows"/> or <paramref name="cols"/> is less than 1.
        /// </exception>
        /// <exception cref="EvaluationException">
        /// Thrown if the <see cref="WinOutcome"/> contains a multiway prize with a multiplier value of 0.
        /// </exception>
        public static void ExecuteBestPayPerWay(
            WinOutcome winOutcome,
            int rows,
            int cols,
            ProgressiveLevels progressiveLevels,
            IDictionary<string, List<Way>> removedWays)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome");
            }
            if(rows < 1)
            {
                throw new ArgumentOutOfRangeException("rows", rows, "Value must be greater than 0.");
            }
            if (cols < 1)
            {
                throw new ArgumentOutOfRangeException("cols", cols, "Value must be greater than 0.");
            }

            // Only filter ways if there's more than one win.
            if (winOutcome.WinOutcomeItems.Count < 2)
            {
                return;
            }

            var winsToRemove = new List<WinOutcomeItem>();
            var ways = new WayTree(rows, cols);
            var orderedWins = winOutcome.WinOutcomeItems.OrderByDescending(
                win => win,
                new WinOutcomeComparer(progressiveLevels));
            
            foreach(var winOutcomeItem in orderedWins)
            {
                var matchedWays = winOutcomeItem.MatchedWays;
                if(matchedWays != null && matchedWays.Count > 0)
                {
                    // Use the way tree to determine if a way has already been seen.
                    var waysToRemove = matchedWays.Where(way => !ways.Add(way)).ToList();
                    foreach(var way in waysToRemove)
                    {
                        matchedWays.Remove(way);
                    }
                    if(matchedWays.Count == 0)
                    {
                        winsToRemove.Add(winOutcomeItem);
                    }
                    else
                    {
                        if(winOutcomeItem.Prize.multiplier == 0)
                        {
                            var message =
                                string.Format(
                                    "The multiway prize {0} has a multiplier of 0, which is disallowed.",
                                    winOutcomeItem.Prize.prizeName);
                            throw new EvaluationException(message);
                        }

                        // Update the multiplier and win amount to account for the reduced ways.
                        var payPerWay = winOutcomeItem.Prize.winAmount / winOutcomeItem.Prize.multiplier;
                        winOutcomeItem.Prize.multiplier = (uint)matchedWays.Count;
                        winOutcomeItem.Prize.winAmount = payPerWay * winOutcomeItem.Prize.multiplier;

                        // Update the matched cells after removing the ways.
                        var matchedCells = matchedWays.SelectMany(way => way.Cells)
                                                      .GroupBy(x => new { x.Cell.layer, x.Cell.row, x.Cell.column })
                                                      .Select(g => g.First())
                                                      .ToList();

                        winOutcomeItem.Pattern.Cluster.Cells.Clear();
                        winOutcomeItem.Pattern.SymbolList.Clear();
                        foreach (var matchedCell in matchedCells)
                        {
                            winOutcomeItem.Pattern.Cluster.Cells.Add(matchedCell.Cell);
                            winOutcomeItem.Pattern.SymbolList.Add(matchedCell.symbolID);
                        }
                    }
                    
                    // If the caller cares about the removed ways then add these ones.
                    if(removedWays != null && waysToRemove.Count > 0)
                    {
                        if(!removedWays.ContainsKey(winOutcomeItem.Prize.prizeName))
                        {
                            removedWays.Add(winOutcomeItem.Prize.prizeName, new List<Way>());
                        }
                        removedWays[winOutcomeItem.Prize.prizeName].AddRange(waysToRemove);
                    }
                }
            }
            
            // Remove any wins with ways that were reduced to 0.
            foreach(var winOutcomeItem in winsToRemove)
            {
                winOutcome.WinOutcomeItems.Remove(winOutcomeItem);
            }
        }

        /// <summary>
        /// Find the best prize per pattern. If a pattern has multiple different prizes, then this function determines
        /// which of those prizes is the best.
        /// </summary>
        /// <param name="bestPays">
        /// List of pays which has been filtered such that there will only be one instance of any given prize per pattern.
        /// So if originally there were 2 instances of "Prize 1" for a pattern, only the better of the 2 should be in this
        /// list.
        /// </param>
        /// <param name="progressiveLevels">Progressive level information used for comparing wins.</param>
        /// <returns>A dictionary of patterns and their associated best prize.</returns>
        /// <exception cref="ArgumentNullException">Thrown is the bestPays parameter is null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static Dictionary<string, WinOutcomeItem> FindBestPrizePerPattern(
            IDictionary<string, WinOutcomeItem> bestPays, ProgressiveLevels progressiveLevels)
        {
            if (bestPays == null)
            {
                throw new ArgumentNullException("bestPays", "Parameter may not be null.");
            }

            var bestPatternPay = new Dictionary<string, WinOutcomeItem>();
            foreach (var win in bestPays)
            {
                var patternName = win.Value.Pattern.name;
                if (bestPatternPay.ContainsKey(patternName))
                {
                    if (CompareWinOutcomeItems(bestPatternPay[patternName], win.Value, progressiveLevels))
                    {
                        bestPatternPay[patternName] = win.Value;
                    }
                }
                else
                {
                    bestPatternPay[patternName] = win.Value;
                }
            }

            return bestPatternPay;
        }


        /// <summary>
        /// From the win outcome find the best of each prize pattern combination. For instance, if prize "Prize 1" is hit
        /// multiple times on the pattern "Pattern 1" this function will determine which instance of "Prize 1" on "Pattern 1"
        /// is the best.
        /// </summary>
        /// <param name="winOutcome">The win outcome to filter.</param>
        /// <param name="progressiveLevels">Progressive level information used for comparing wins.</param>
        /// <returns>A list of the best prize/pattern combinations.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the winOutcome parameter is null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static Dictionary<string, WinOutcomeItem> FindBestWinPerPrizePerPattern(WinOutcome winOutcome,
                                                                                       ProgressiveLevels
                                                                                           progressiveLevels)
        {
            if (winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }

            var bestPays = new Dictionary<string, WinOutcomeItem>();

            foreach (var win in winOutcome.WinOutcomeItems)
            {
                //Build a name which represents a prize on a given pattern.
                //All the different pay combinations which have the same prize which
                //appear on the same pattern will have the same qualified name.
                var qualifiedName = win.Prize.prizeName + "_" + win.Pattern.name;

                if (!bestPays.ContainsKey(qualifiedName))
                {
                    bestPays[qualifiedName] = win;
                }
                else if (CompareWinOutcomeItems(bestPays[qualifiedName], win, progressiveLevels))
                {
                    bestPays[qualifiedName] = win;
                }
            }

            return bestPays;
        }

        /// <summary>
        /// From the win outcome find the best pay for each prize.
        /// </summary>
        /// <param name="winOutcome">The win outcome to filter.</param>
        /// <param name="progressiveLevels">Progressive level information used for comparing wins.</param>
        /// <returns>A list of the best prizes.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the winOutcome parameter is null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static Dictionary<string, WinOutcomeItem> FindBestWinPerPrize(WinOutcome winOutcome,
                                                                             ProgressiveLevels
                                                                                 progressiveLevels)
        {
            if (winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }

            var bestPays = new Dictionary<string, WinOutcomeItem>();

            foreach (var win in winOutcome.WinOutcomeItems)
            {
                if (!bestPays.ContainsKey(win.Prize.prizeName))
                {
                    bestPays[win.Prize.prizeName] = win;
                }
                else if (CompareWinOutcomeItems(bestPays[win.Prize.prizeName], win, progressiveLevels))
                {
                    bestPays[win.Prize.prizeName] = win;
                }
            }

            return bestPays;
        }

        /// <summary> Compare two win outcome items to see if the second is better. </summary>
        /// <param name="item1"> Item to use as a base for comparison. </param>
        /// <param name="item2"> Item being compared. </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <returns> True if item2 is better than item1. </returns>
        /// <exception cref="ArgumentNullException">Thrown if item1 or item2 are null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static bool CompareWinOutcomeItems(WinOutcomeItem item1, WinOutcomeItem item2,
                                                  ProgressiveLevels progressiveLevels)
        {
            return WinOutcomeComparer.CompareWinOutcomeItems(item1, item2, progressiveLevels) < 0;
        }


        /// <summary> Compare the win amounts of 2 win outcome items to see which is better. </summary>
        /// <param name="item1">Item to use as a base for comparison. </param>
        /// <param name="item2">Item being compared. </param>
        /// <exception cref="ArgumentNullException">Thrown if item1 or item2 are null.</exception>
        /// <returns> True if item2 is than item1. </returns>
        public static bool CompareWinOutcomeItemAmounts(WinOutcomeItem item1, WinOutcomeItem item2)
        {
            return WinOutcomeComparer.CompareWinOutcomeItemAmounts(item1, item2) < 0;
        }

        /// <summary> Compare the progressive levels of 2 win outcome items to see which is better. </summary>
        /// <param name="item1"> Item to use as a base for comparison. </param>
        /// <param name="item2"> Item being compared. </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <returns> True if item2 is better than item1. </returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static bool CompareProgressiveLevels(
            WinOutcomeItem item1,
            WinOutcomeItem item2,
            ProgressiveLevels progressiveLevels)
        {
            return WinOutcomeComparer.CompareProgressiveLevels(item1, item2, progressiveLevels) < 0;
        }
    }
}
