//-----------------------------------------------------------------------
// <copyright file = "WinOutcomeComparer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// An implementation of <see cref="IComparer{T}"/> that compares <see cref="WinOutcomeItem"/> objects.
    /// </summary>
    /// <remarks>
    /// This implementation will order the wins in increasing order, since it maintains the proper meaning of
    /// a &lt; b. This implies that in order to have the "best" win come first, you need to reverse the sort
    /// order, so the wins are sorted in decreasing order.
    /// </remarks>
    public class WinOutcomeComparer : IComparer<WinOutcomeItem>
    {
        private readonly ProgressiveLevels progressiveLevels;

        /// <summary>
        /// Initializes a new instance of the <see cref="WinOutcomeComparer"/> class.
        /// </summary>
        /// <param name="progressiveLevels">The <see cref="ProgressiveLevels"/> from the paytable.</param>
        public WinOutcomeComparer(ProgressiveLevels progressiveLevels)
        {
            this.progressiveLevels = progressiveLevels;
        }

        /// <inheritdoc/>
        public int Compare(WinOutcomeItem x, WinOutcomeItem y)
        {
            return CompareWinOutcomeItems(x, y, progressiveLevels);
        }

        /// <summary> Compare two win outcome items to see which is better. </summary>
        /// <param name="item1"> The first item to compare (lhs.) </param>
        /// <param name="item2"> The second item to compare (rhs). </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <returns>
        /// An <see cref="int"/> that is negative if <paramref name="item1"/> is less than <paramref name="item2"/>,
        /// 0 if they are equal, and positive if item1 is greater than item2.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if item1 or item2 are null.</exception>
        /// <remarks>
        /// Progressive levels is not checked for null until it is used. Not all evaluations require progressive level
        /// information.
        /// </remarks>
        public static int CompareWinOutcomeItems(WinOutcomeItem item1, WinOutcomeItem item2,
                                                  ProgressiveLevels progressiveLevels)
        {
            if(item1 == null)
            {
                throw new ArgumentNullException("item1", "Parameter may not be null");
            }
            if(item2 == null)
            {
                throw new ArgumentNullException("item2", "Parameter may not be null");
            }

            int result;

            if(item2.Prize.ProgressiveLevels.Count != 0)
            {
                //If item 2 has progressives then item 1 must be checked for progressives.
                //If item 2 has a progressive and item 1 does not, then prize 2 is better
                if(item1.Prize.ProgressiveLevels.Count != 0)
                {
                    result = CompareProgressiveLevels(item1, item2, progressiveLevels);
                }
                else
                {
                    result = -1;
                }
            }
            //Neither prize 1 or 2 had a progressive, so the comparison is by trigger
            //and win amount only.
            else if(item1.Prize.ProgressiveLevels.Count == 0)
            {
                result = CompareWinOutcomeItemAmounts(item1, item2);
            }
            //Otherwise prize 1 had a progressive, but prize 2 did not, and therefore
            //prize 1 is better than prize 2.
            else
            {
                result = 1;
            }

            return result;
        }


        /// <summary> Compare the win amounts of 2 win outcome items to see which is better. </summary>
        /// <param name="item1"> The first item to compare (lhs.) </param>
        /// <param name="item2"> The second item to compare (rhs). </param>
        /// <exception cref="ArgumentNullException">Thrown if item1 or item2 are null.</exception>
        /// <returns>
        /// An <see cref="int"/> that is negative if <paramref name="item1"/> is less than <paramref name="item2"/>,
        /// 0 if they are equal, and positive if item1 is greater than item2.
        /// </returns>
        public static int CompareWinOutcomeItemAmounts(WinOutcomeItem item1, WinOutcomeItem item2)
        {
            if(item1 == null)
            {
                throw new ArgumentNullException("item1", "Parameter may not be null");
            }
            if(item2 == null)
            {
                throw new ArgumentNullException("item2", "Parameter may not be null");
            }

            var item1Value = item1.Prize.winAmount;
            var item2Value = item2.Prize.winAmount;

            checked
            {
                if(item1.Prize.averageBonusPaySpecified)
                {
                    // Scale the average bonus pay by the multiplier if there is a multiplier.
                    item1Value += item1.Prize.multiplier > 1
                        ? item1.Prize.averageBonusPay * item1.Prize.multiplier
                        : item1.Prize.averageBonusPay;
                }

                if(item2.Prize.averageBonusPaySpecified)
                {
                    // Scale the average bonus pay by the multiplier if there is a multiplier.
                    item2Value += item2.Prize.multiplier > 1
                        ? item2.Prize.averageBonusPay * item2.Prize.multiplier
                        : item2.Prize.averageBonusPay;
                }
            }

            var result = (int)(item1Value - item2Value);

            if(result == 0)
            {
                // Use the multiplier to break any ties.
                // A smaller multiplier implies a larger base pay, so the items are reversed below.
                result = (int)item2.Prize.multiplier - (int)item1.Prize.multiplier;

                if(result == 0)
                {
                    // Break any remaining ties by preferring the win with more matched populations (columns, typically).
                    result = (int)item1.Pattern.count - (int)item2.Pattern.count;

                    // Finally, for JumboPays, break ties by preferring the win with more matched symbols.
                    //Prefer the win with the most matched symbols.
                    if(result == 0)
                    {
                        result = item1.Pattern.SymbolList.Count - item2.Pattern.SymbolList.Count;
                    }
                }
            }

            return result;
        }

        /// <summary> Compare the progressive levels of 2 win outcome items to see which is better. </summary>
        /// <param name="item1"> The first item to compare (lhs.) </param>
        /// <param name="item2"> The second item to compare (rhs). </param>
        /// <param name="progressiveLevels">
        /// The progressive level information to use for this paytable. The progressive information is needed in order
        /// to prioritize prizes with multiple progressive wins, and multiple prizes with different progressive wins.
        /// </param>
        /// <returns>
        /// An <see cref="int"/> that is negative if <paramref name="item1"/> is less than <paramref name="item2"/>,
        /// 0 if they are equal, and positive if item1 is greater than item2.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static int CompareProgressiveLevels(WinOutcomeItem item1, WinOutcomeItem item2,
                                                    ProgressiveLevels progressiveLevels)
        {
            if(item1 == null)
            {
                throw new ArgumentNullException("item1", "Parameter may not be null");
            }
            if(item2 == null)
            {
                throw new ArgumentNullException("item2", "Parameter may not be null");
            }
            if(progressiveLevels == null)
            {
                throw new ArgumentNullException("progressiveLevels", "Parameter may not be null");
            }

            //The highest level of progressive will decide the pay. A win which awards level
            //1 will have priority over one which awards any combination of progressives
            //that do not include level 1. If two wins have the same progressive levels,
            //then the win will be determined by other attributes.
            IEnumerable<uint> item1Levels = from progressive in item1.Prize.ProgressiveLevels
                select
                    (from level in progressiveLevels.ProgressiveLevel
                        where level.ProgressiveName == progressive
                        select level.Level).First();

            item1Levels = from level in item1Levels orderby level select level;

            IEnumerable<uint> item2Levels = from progressive in item2.Prize.ProgressiveLevels
                select
                    (from level in progressiveLevels.ProgressiveLevel
                        where level.ProgressiveName == progressive
                        select level.Level).First();

            item2Levels = from level in item2Levels orderby level select level;

            for(int levelIndex = 0; levelIndex < item2Levels.Count(); levelIndex++)
            {
                //Being as the lists are sorted in ascending order, the first list to have a progressive level
                //which is less than the other list, is the better list. Lower levels are better.
                if(levelIndex < item1Levels.Count())
                {
                    uint item1Level = item1Levels.ElementAt(levelIndex);
                    uint item2Level = item2Levels.ElementAt(levelIndex);
                    if(item2Level < item1Level)
                    {
                        return -1;
                    }
                    if(item2Level > item1Level)
                    {
                        return 1;
                    }
                }
                else
                {
                    //All levels before this one matched, so item1 having fewer items means that item2 is better.
                    return -1;
                }
            }

            //If item2 has fewer levels and the best item was not determined, then
            //the best item is item1. Otherwise they need to be compared by value.
            return item2Levels.Count() < item1Levels.Count() ? 1 : CompareWinOutcomeItemAmounts(item1, item2);
        }
    }
}
