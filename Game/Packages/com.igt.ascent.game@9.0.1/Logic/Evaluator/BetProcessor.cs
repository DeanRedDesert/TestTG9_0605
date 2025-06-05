//-----------------------------------------------------------------------
// <copyright file = "BetProcessor.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Logging;
    using Schemas;

    /// <summary>
    /// The purpose of the bet processor is to filter a set of potential
    /// wins from a prize evaluator. The bet processor checks the eligibility
    /// of each win based on the required bet and required patterns. If a requirement
    /// is not met, then that win is removed.
    /// </summary>
    public static class BetProcessor
    {
        /// <summary>
        /// Value which indicates that a prize is valid on any pattern, but that pattern must have the specified pattern
        /// bet.
        /// </summary>
        public const string AnyPattern = "ANY";

        /// <summary>
        /// Process the wins in the win outcome to determine if they are eligible
        /// based on their bet requirements. If a win is not eligible then it is removed
        /// if it is eligible, then its win information is populated.
        /// </summary>
        /// <param name="winOutcome"> The win outcome to process. </param>
        /// <param name="slotPrizeScale">
        /// The prize scale to use when processing the requirements. Only wins from the
        /// specified prize scale will be processed. Other wins will be ignored.
        /// </param>
        /// <param name="betDefinitions">
        /// The bet definition for the current evaluation. It is used when
        /// determining if pattern bet requirements are met, or for multiplying
        /// by a pattern bet.
        /// </param>
        /// <param name="patternList">
        /// The pattern list used for bet validation.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        /// <exception cref="EvaluatorConfigurationException">
        /// Exception which is thrown if a win specifies a prize which does not exist in the given prize scale, or
        ///  specifies a pay count which does not exist in the prize.
        /// </exception>
        public static void ProcessBetRequirements(WinOutcome winOutcome, SlotPrizeScale slotPrizeScale,
                                                  BetDefinitionList betDefinitions, PatternList patternList)
        {
            if(winOutcome == null)
            {
                throw new ArgumentNullException("winOutcome", "Parameter may not be null");
            }
            if(slotPrizeScale == null)
            {
                throw new ArgumentNullException("slotPrizeScale", "Parameter may not be null");
            }
            if(betDefinitions == null)
            {
                throw new ArgumentNullException("betDefinitions", "Parameter may not be null");
            }
            if(patternList == null)
            {
                throw new ArgumentNullException("patternList", "Parameter may not be null");
            }

            ValidateBetDefinitions(betDefinitions, patternList);

            //If a win doesn't meet any of the bet requirements it will be placed in this list. After examining all wins,
            //the wins in this list will be removed from the win outcome.
            var winsToRemove = new List<WinOutcomeItem>();

            foreach(var win in winOutcome.WinOutcomeItems)
            {
                var prizeFromScale =
                    (from prize in slotPrizeScale.Prize where prize.name == win.Prize.prizeName select prize).
                        FirstOrDefault();

                if(prizeFromScale == null)
                {
                    throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                            "Could not find prize: {0} in prize scale: {1}",
                                                                            win.Prize.prizeName, slotPrizeScale.name));
                }

                var scalePay =
                    (from pay in prizeFromScale.PrizePay
                        where
                            pay.count == win.Pattern.count &&
                            (!pay.totalSymbolCountSpecified || pay.totalSymbolCount == win.Pattern.Cluster.Cells.Count)
                        select pay).FirstOrDefault();

                if(scalePay == null)
                {
                    throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                            "Could not find pay for count: {0} in prize: {1}",
                                                                            win.Pattern.count, prizeFromScale.name));
                }

                var validWinAmountFound = ProcessWinAmounts(betDefinitions, win, prizeFromScale, scalePay);

                if(!validWinAmountFound)
                {
                    winsToRemove.Add(win);
                }
            }

            foreach(var win in winsToRemove)
            {
                winOutcome.WinOutcomeItems.Remove(win);
            }
        }


        /// <summary>
        /// Validates the pattern names in the bet definition list
        /// against the pattern names in the pattern list of the paytable.
        /// </summary>
        /// <param name="betDefinitions">
        /// The bet definition for the current evaluation.
        /// </param>
        /// <param name="patternList">
        /// The pattern list from the paytable that is being used.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static void ValidateBetDefinitions(BetDefinitionList betDefinitions, PatternList patternList)
        {
            if(betDefinitions == null)
            {
                throw new ArgumentNullException("betDefinitions", "Parameter may not be null");
            }
            if(patternList == null)
            {
                throw new ArgumentNullException("patternList", "Parameter may not be null");
            }

            var patternListNames = patternList.Pattern.Select(value => value.name);
            var betDefinitionsList = betDefinitions.BetDefinition.Select
                (value => value.betableTypeReference);

            if(!patternListNames.Intersect(betDefinitionsList).Any())
            {
                Log.WriteWarning(string.Format(
                    "The {0} in paytable does not contain any bet defined in BetDefinitions.", patternList.name));
            }
        }

        /// <summary>
        /// Add the first eligible win amount for the specified prize pay to the given win.
        /// </summary>
        /// <param name="betDefinitions">
        /// Bet information used to determine if a win amounts bet requirements are met.
        /// </param>
        /// <param name="win">Any eligible win amount will be added to this win.</param>
        /// <param name="prizeFromScale">Prize which is used for its amount modification strategy.</param>
        /// <param name="scalePay">Prize pay which contains win amounts to check.</param>
        /// <returns>True if a valid win amount was found and added.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        /// <exception cref="EvaluationException">
        /// Thrown when the passed win outcome item does not have a pattern or prize.
        /// </exception>
        public static bool ProcessWinAmounts(BetDefinitionList betDefinitions, WinOutcomeItem win,
                                             SlotPrize prizeFromScale, PrizePay scalePay)
        {
            if(betDefinitions == null)
            {
                throw new ArgumentNullException("betDefinitions", "Parameter may not be null");
            }
            if(win == null)
            {
                throw new ArgumentNullException("win", "Parameter may not be null");
            }
            if(prizeFromScale == null)
            {
                throw new ArgumentNullException("prizeFromScale", "Parameter may not be null");
            }
            if(scalePay == null)
            {
                throw new ArgumentNullException("scalePay", "Parameter may not be null");
            }
            if(win.Pattern == null)
            {
                throw new EvaluationException("Win outcome item must have a pattern.");
            }
            if(win.Prize == null)
            {
                throw new EvaluationException("Win outcome must have a prize.");
            }


            bool validWinAmountFound = false;

            var patternBet =
                (from bet in betDefinitions.BetDefinition
                 where bet.betableTypeReference == win.Pattern.name
                 select bet).FirstOrDefault();

            //Only process the win if there was a bet on the pattern on which is appeared.
            if(patternBet != null)
            {
                var betOnPattern = patternBet.betAmount;

                // Determine if any of the win amounts has a progressive.
                var winAmountHasProgressive = scalePay.WinAmount.Any(winAmount => winAmount.ProgressiveLevel.Count > 0);

                foreach(var winAmount in scalePay.WinAmount)
                {
                    //If there is not a total bet requirement, then the requirement is met.
                    //Otherwise check that the required total bet is met. In addition to the
                    //total required bet, each pattern entry must also satisfy its bet
                    //requirements.
                    if(winAmount.RequiredTotalBetRange.RequiredBetMet(betDefinitions.totalBet, betDefinitions.isMax) &&
                       CheckRequiredPatterns(winAmount, betDefinitions))
                    {
                        if(winAmount.RequiredBetOnPatternRange.RequiredBetMet(betOnPattern, patternBet.isMaxBet))
                        {
                            //Add the amount to the prize.
                            var modifiedAmount = GetAmountWithModification(prizeFromScale, winAmount, betOnPattern,
                                                                           betDefinitions.totalBet, win.Prize.multiplier);

                            win.Prize.winAmount = modifiedAmount;

                            if(winAmount.averageBonusPaySpecified)
                            {
                                win.Prize.averageBonusPay = winAmount.averageBonusPay;
                                win.Prize.averageBonusPaySpecified = true;
                            }

                            win.Prize.Trigger.AddRangeCopy(winAmount.Trigger);

                            win.Prize.ProgressiveLevels.AddRange(winAmount.ProgressiveLevel);
                            win.winLevelIndex = winAmount.winLevelIndex;

                            // Check to see if any win amount has a progressive and this win does not.
                            win.Prize.nearHitProgressive = winAmountHasProgressive && win.Prize.ProgressiveLevels.Count == 0;

                            validWinAmountFound = true;

                            //Win amounts need to be in order from best to worst. The first one to meet its requirements
                            //is awarded.
                            break;
                        }
                    }
                }
            }
            return validWinAmountFound;
        }

        /// <summary>
        /// Get the win amount for a prize with the amount modification strategy applied.
        /// </summary>
        /// <param name="prize"> [in] The prize which the win amount is from. </param>
        /// <param name="winAmount"> [in] The win amount for which to get the modified value. </param>
        /// <param name="betOnPattern"> [in] The bet on the pattern which the prize was on. </param>
        /// <param name="totalBet"> [in] The total bet for the current game. </param>
        /// <param name="multiplier">Multiplier used for TimesMultiplierTimesBetOnPattern strategy.</param>
        /// <returns> The modified amount. </returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static long GetAmountWithModification(SlotPrize prize, WinAmount winAmount, uint betOnPattern,
                                                     uint totalBet, uint multiplier)
        {
            if(prize == null)
            {
                throw new ArgumentNullException("prize", "Parameter may not be null");
            }
            if(winAmount == null)
            {
                throw new ArgumentNullException("winAmount", "Parameter may not be null");
            }

            long modifiedAmount = 0;

            if(winAmount.valueSpecified)
            {
                switch(prize.AmountModificationStrategy)
                {
                    case SlotPrizeAmountModificationStrategy.None:
                        {
                            modifiedAmount = winAmount.value;
                        }
                        break;
                    case SlotPrizeAmountModificationStrategy.TimesBetOnPattern:
                        {
                            checked
                            {
                                modifiedAmount = winAmount.value * betOnPattern;
                            }
                        }
                        break;
                    case SlotPrizeAmountModificationStrategy.TimesTotalBet:
                        {
                            checked
                            {
                                modifiedAmount = winAmount.value * totalBet;
                            }
                        }
                        break;
                    case SlotPrizeAmountModificationStrategy.TimesMultiplierTimesBetOnPattern:
                        {
                            checked
                            {
                                modifiedAmount = winAmount.value * betOnPattern * multiplier;
                            }
                        }
                        break;
                    default:
                        {
                            throw new EvaluatorConfigurationException(string.Format(CultureInfo.InvariantCulture,
                                                                                    "Invalid amount modification strategy: {0}",
                                                                                    prize.AmountModificationStrategy));
                        }
                }
            }

            return modifiedAmount;
        }

        /// <summary>
        /// Check that all of the patterns and bets required by a bet are satisfied.
        /// </summary>
        /// <param name="winAmount"> Prize scale win amount to verify. </param>
        /// <param name="betDefinitions"> The bets for this game. </param>
        /// <returns> True if the requirements are met. </returns>
        /// <exception cref="EvaluatorConfigurationException">
        /// Thrown when a required pattern of any is not coupled with a required bet of max.
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown if any parameters are null.</exception>
        public static bool CheckRequiredPatterns(WinAmount winAmount, BetDefinitionList betDefinitions)
        {
            if(winAmount == null)
            {
                throw new ArgumentNullException("winAmount", "Parameter may not be null");
            }
            if(betDefinitions == null)
            {
                throw new ArgumentNullException("betDefinitions", "Parameter may not be null");
            }

            var requiredPatternsMet = winAmount.RequiredPatterns.Count == 0;

            if(!requiredPatternsMet)
            {
                foreach(var pattern in winAmount.RequiredPatterns)
                {
                    if(pattern.RequiredActivePattern == AnyPattern)
                    {
                        //Any pattern is eligible, so the bet amount needs to be verified. If the pattern is for any, then the
                        //bet requirement must be max in reference to the total bet, or all indicating any bet is valid.
                        //This is because an individual pattern cannot be referenced in this scenario.
                        if(string.Equals(pattern.BetAmountRequired, RequiredBetRange.BetMax, StringComparison.OrdinalIgnoreCase))
                        {
                            //The overall bet must be max if the requirement is max
                            requiredPatternsMet = betDefinitions.isMax;
                        }
                        else if(string.Equals(pattern.BetAmountRequired, RequiredBetRange.BetAll, StringComparison.OrdinalIgnoreCase))
                        {
                            requiredPatternsMet = true;
                        }
                        else
                        {
                            throw new EvaluatorConfigurationException(string.Format("The bet requirement- \"{0}\"," +
                                                                            " for win amount is not a valid value." +
                                                                            "It should be \"{1}\" or \"{2}\"",
                                                                            pattern.BetAmountRequired,
                                                                            RequiredBetRange.BetAll,
                                                                            RequiredBetRange.BetMax));
                        }
                    }
                    else
                    {
                        var patternBet =
                            (from bet in betDefinitions.BetDefinition
                             where bet.betableTypeReference == pattern.RequiredActivePattern
                             select bet).FirstOrDefault();

                        //If the pattern is not present in the bet definition list then the required patterns are not met.
                        //Conversely, patterns without bets must not appear in the bet definition.
                        requiredPatternsMet = patternBet != null && pattern.BetAmountRequiredRange.RequiredBetMet(patternBet.betAmount, patternBet.isMaxBet);
                    }

                    //All required patterns must be met, so if any pattern was not met, then stop checking and break
                    //indicating a failure.
                    if(!requiredPatternsMet)
                    {
                        break;
                    }
                }
            }

            return requiredPatternsMet;
        }
    }
}
