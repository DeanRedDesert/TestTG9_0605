//-----------------------------------------------------------------------
// <copyright file = "MaxWinEvaluator.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ReportLib.Interfaces;
    using Communication.Foundation;
    using Logic.Evaluator;
    using Logic.Evaluator.Schemas;

    /// <summary>
    /// Calculates the maximum credit award in a <see cref="SlotPaytableSection"/> 
    /// for a specific bet while excluding linked progressives. 
    /// </summary>
    /// <remarks> 
    /// Linked progressive prizes are ignored, but if an unlinked progressive has
    /// a consolation pay, the consolation pay will be evaluated when determining 
    /// the maximum award.
    /// 
    /// Any prizes with bet or pattern requirements different from the specified
    /// bet will be ignored.
    /// 
    /// Scatter prizes are ignored.  Maximum award only includes Lines or Ways.
    /// </remarks>
    public static class MaxWinEvaluator
    {
        private const string AllPatterns = "ALL";

        /// <summary>
        /// Calculates the maximum award for the specified bet out of all possible 
        /// Line and Multiway prizes within a <see cref="SlotPaytableSection"/>.
        /// </summary>
        /// <param name="paytableSection">The <see cref="SlotPaytableSection"/> for which
        /// the maximum award will be evaluated.</param>
        /// <param name="betDefinitionList">A <see cref="BetDefinitionList"/> used for any
        /// win amount modifications and for fulfilling any bet requirements.</param>
        /// <param name="progressiveLevels">The <see cref="ProgressiveLevels"/> describing
        /// all progressive levels that may be awarded by a prize.</param>
        /// <param name="linkedProgressiveLevels">A collection of linked progressive levels.</param>
        /// <param name="denomination">The current game denomination.</param>
        /// <returns>
        /// The maximum award in credits of the given <see cref="SlotPaytableSection"/> 
        /// for the specified bet.
        /// </returns>
        public static long GetMaxWin(SlotPaytableSection paytableSection,
                                     BetDefinitionList betDefinitionList,
                                     ProgressiveLevels progressiveLevels,
                                     IList<int> linkedProgressiveLevels,
                                     long denomination)
        {
            long maxWin = 0;

            maxWin = Math.Max(maxWin, GetMaxWin(paytableSection.LinePatternList,
                                                paytableSection.LinePrizeScale,
                                                betDefinitionList,
                                                progressiveLevels,
                                                linkedProgressiveLevels, 
                                                denomination));

            maxWin = Math.Max(maxWin, GetMaxWin(paytableSection.MultiwayPatternList,
                                                paytableSection.MultiwayPrizeScale,
                                                betDefinitionList,
                                                progressiveLevels,
                                                linkedProgressiveLevels, 
                                                denomination));

            return maxWin;
        }

        /// <summary>
        /// Calculates the maximum award for the specified bet out of all
        /// the given patterns and prizes.
        /// </summary>
        /// <param name="patternList">The <see cref="PatternList"/> containing all the patterns to check.</param>
        /// <param name="slotPrizeScale">The <see cref="SlotPrizeScale"/> of all prizes from which to calculate the 
        /// maximum win.</param>
        /// <param name="betDefinitionList">A <see cref="BetDefinitionList"/> used for any
        /// win amount modifications and for fulfilling any bet requirements.</param>
        /// <param name="progressiveLevels">The <see cref="ProgressiveLevels"/> describing
        /// all progressive levels that may be awarded by a prize.</param>
        /// <param name="linkedProgressiveLevels">A collection of linked progressive levels.</param>
        /// <param name="denomination">The current game denomination.</param>
        /// <returns>
        /// The maximum award in credits of the given patterns and prizes 
        /// for the specified bet.
        /// </returns>
        public static long GetMaxWin(PatternList patternList,
                                     SlotPrizeScale slotPrizeScale,
                                     BetDefinitionList betDefinitionList,
                                     ProgressiveLevels progressiveLevels,
                                     IList<int> linkedProgressiveLevels,
                                     long denomination)
        {
            long maxWin = 0;

            if(patternList != null && slotPrizeScale != null)
            {
                foreach(var slotPrize in slotPrizeScale.Prize)
                {
                    IEnumerable<string> patterns;

                    // If the SlotPrize specifies an eligible pattern, evaluate for only that pattern
                    if(!string.IsNullOrEmpty(slotPrize.EligiblePattern) &&
                       slotPrize.EligiblePattern != AllPatterns)
                    {
                        patterns = new List<string> { slotPrize.EligiblePattern };
                    }
                    else
                    {
                        // Get patterns with distinct bet amounts
                        patterns = DistinctBetAmountPatterns(betDefinitionList, patternList);
                    }

                    foreach(var patternName in patterns)
                    {
                        // TODO: Consider sorting winAmounts by progressives first, then by value descending.
                        // After the first winAmount without progressives that returns a valid win (given correct
                        // bet requirements), exit loop.
                        foreach(var winAmount in slotPrize.PrizePay.SelectMany(pay => pay.WinAmount))
                        {
                            // Calculate the total win value for this WinAmount
                            long totalWinValue = 0;

                            // Evaluate the winAmount's value
                            if(winAmount.valueSpecified && winAmount.value > 0)
                            {
                                var winValue = ProcessSingleWinAmount(betDefinitionList,
                                                                      patternName,
                                                                      slotPrize,
                                                                      winAmount);
                                totalWinValue += winValue;
                            }

                            // Add values from Progressive consolation pays for unlinked progressives
                            totalWinValue += CalculateProgressiveTotalFromWinAmount(winAmount,
                                                                                    progressiveLevels,
                                                                                    linkedProgressiveLevels,
                                                                                    denomination);

                            // Save the maximum win
                            maxWin = Math.Max(maxWin, totalWinValue);
                        }
                    }
                }
            }

            return maxWin;
        }

        /// <summary>
        /// Gets a collection of pattern names that represent distinct bet
        /// amounts in the given <see cref="BetDefinitionList"/>, usually reducing the number of patterns
        /// needed to evaluate.
        /// </summary>
        /// <param name="betDefinitionList">A <see cref="BetDefinitionList"/> describing the bets for
        /// all the patterns.</param>
        /// <param name="patternList">A <see cref="PatternList"/> containing all the patterns to check.</param>
        /// <returns>A collection of pattern names where each is the first in a group of patterns that all have the 
        /// same bet amount.</returns>
        /// <remarks>
        /// This method is used to reduce the number of patterns to evaluate for the max win because
        /// patterns with the same bet amount will evaluate to the same value.
        /// </remarks>
        public static IEnumerable<string> DistinctBetAmountPatterns(BetDefinitionList betDefinitionList,
                                                                    PatternList patternList)
        {
            // Group the patterns by their bet amounts 
            // and select the first pattern in each group.
            return from bet in betDefinitionList.BetDefinition
                   join pattern in patternList.Pattern
                       on bet.betableTypeReference equals pattern.name
                   group pattern by bet.betAmount
                   into patternGroup
                   select patternGroup.First().name;
        }

        /// <summary>
        /// Calculates the total credit value of all unlinked progressive consolation pays awarded
        /// by the specified <see cref="WinAmount"/>.
        /// </summary>
        /// <param name="winAmount">A <see cref="WinAmount"/> that may or may not award progressives.</param>
        /// <param name="progressiveLevels">The <see cref="ProgressiveLevels"/> describing
        /// all progressive levels that may be awarded by a prize.</param>
        /// <param name="linkedProgressiveLevels">A collection of linked progressive levels.</param>
        /// <param name="denomination">The current game denomination.</param>
        /// <returns>
        /// The total credit value of the unlinked progressives for the given <see cref="WinAmount"/>.
        /// </returns>
        public static long CalculateProgressiveTotalFromWinAmount(WinAmount winAmount,
                                                                  ProgressiveLevels progressiveLevels,
                                                                  IList<int> linkedProgressiveLevels,
                                                                  long denomination)
        {
            long totalProgressiveValue = 0;

            // Add each non-linked progressive level consolation pay value to the total win value
            foreach(var progressiveLevelName in winAmount.ProgressiveLevel)
            {
                // Find progressive level
                var progressiveLevel = progressiveLevels.ProgressiveLevel
                                                        .FirstOrDefault(level =>
                                                                        level.ProgressiveName == progressiveLevelName);
                if(progressiveLevel != null)
                {
                    // If a Progressive is not linked, look for a consolation pay.
                    if(!linkedProgressiveLevels.Contains((int)progressiveLevel.Level))
                    {
                        if(progressiveLevel.ConsolationPaySpecified)
                        {
                            var consolation = progressiveLevel.ConsolationPay;

                            // totalProgressiveValue is in credits but ConsolationPay is an Amount,
                            // unless MultiplyConsolationPayByDenom is true.
                            if(!progressiveLevel.MultiplyConsolationPayByDenomSpecified ||
                               !progressiveLevel.MultiplyConsolationPayByDenom)
                            {
                                // ConsolationPay is an Amount, convert to credits
                                consolation = Utility.ConvertToCredits(consolation, denomination);
                            }

                            totalProgressiveValue += consolation;
                        }
                    }
                }
            }

            return totalProgressiveValue;
        }

        /// <summary>
        /// Gets a list of linked progressive levels from the <see cref="IReportLib"/> and
        /// <see cref="ReportContext"/>.
        /// </summary>
        /// <param name="reportLib">The game report interface to the Foundation.</param>
        /// <param name="reportContext">Active <see cref="Core.GameReport.ReportContext"/>.</param>
        /// <returns>A list of linked progressive levels.</returns>
        public static IList<int> GetLinkedProgressiveLevels(IReportLib reportLib, ReportContext reportContext)
        {
            var settings = reportLib.GetLinkedProgressiveSettings(reportContext.PaytableTag.PaytableIdentifier,
                                                                  reportContext.Denomination);
            return settings.Keys.ToList();
        }

        /// <summary>
        /// Uses the <see cref="BetProcessor"/> to process a single <see cref="SlotPrize"/> and
        /// <see cref="WinAmount"/> pair based on the specified <see cref="BetDefinitionList"/>.
        /// </summary>
        /// <param name="betDefinitionList">A <see cref="BetDefinitionList"/> used for any
        /// win amount modifications and for fulfilling any bet requirements.</param>
        /// <param name="patternName">Name of the pattern for win evaluation.</param>
        /// <param name="slotPrize">Prize which is used for its amount modification strategy.</param>
        /// <param name="winAmount">The single <see cref="WinAmount"/> to evaluate.</param>
        /// <returns>
        /// A credit win amount modified by the amount modification strategy if
        /// the win is valid and the bet meets any requirements; otherwise, 0.
        /// </returns>
        private static long ProcessSingleWinAmount(BetDefinitionList betDefinitionList,
                                                   string patternName,
                                                   SlotPrize slotPrize,
                                                   WinAmount winAmount)
        {
            long result = 0;

            // Build a WinOutcomeItem with information required by BetProcessor
            var winOutcomeItem = new WinOutcomeItem
                                 {
                                     Pattern = new Pattern { name = patternName },
                                     Prize = new Prize
                                             {
                                                 // Only calculate for a single multiway; 
                                                 // multiplier is ignored for lines and scatters
                                                 multiplier = 1
                                             }
                                 };

            // Create new PrizePay with a single WinAmount
            var newPrizePay = new PrizePay
                              {
                                  WinAmount = new List<WinAmount> { winAmount }
                              };

            if(BetProcessor.ProcessWinAmounts(betDefinitionList, winOutcomeItem,
                                              slotPrize, newPrizePay))
            {
                result = winOutcomeItem.Prize.winAmount;
            }

            return result;
        }
    }
}
