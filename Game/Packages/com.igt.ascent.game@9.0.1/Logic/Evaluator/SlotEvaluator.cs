//-----------------------------------------------------------------------
// <copyright file = "SlotEvaluator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using Schemas;

    /// <summary>
    /// Class which contains methods for slot evaluation.
    /// </summary>
    public static class SlotEvaluator
    {
        /// <summary>
        /// Evaluate the given paytable section.
        /// </summary>
        /// <param name="paytableSection">Paytable section to evaluate.</param>
        /// <param name="randomNumbers">Random number source.</param>
        /// <param name="betDefinitions">Bet to use for the evaluation.</param>
        /// <param name="progressiveLevels">Progressive level information used when comparing progressives.</param>
        /// <param name="denomination">The denomination for the bets passed in.</param>
        /// <returns>A EvaluationResult containing the results of the evaluation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when passed paytable section is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when passed denomination is less than or equal to 0.</exception>
        public static EvaluationResult EvaluatePaytableSection(SlotPaytableSection paytableSection,
                                                               IRandomNumbers randomNumbers,
                                                               BetDefinitionList betDefinitions,
                                                               ProgressiveLevels progressiveLevels,
                                                               long denomination)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            var populatedWindow = StripBasedPopulator.CreateCellPopulationOutcome(paytableSection.StripList,
                                                                                  paytableSection.SymbolWindow,
                                                                                  randomNumbers);

            return EvaluatePaytableSection(paytableSection, populatedWindow, betDefinitions, progressiveLevels,
                                           denomination);
        }

        /// <summary>
        /// Evaluate the given paytable section.
        /// </summary>
        /// <param name="paytableSection">Paytable section to evaluate.</param>
        /// <param name="populatedWindow">Populated symbol window.</param>
        /// <param name="betDefinitions">Bet to use for the evaluation.</param>
        /// <param name="progressiveLevels">Progressive level information used when comparing progressives.</param>
        /// <param name="denomination">The denomination for the bets passed in.</param>
        /// <returns>A EvaluationResult containing the results of the evaluation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when passed paytable section is null.</exception>
        /// <exception cref="ArgumentNullException">Thrown when passed populatedWindow is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when passed denomination is less than or equal to 0.</exception>
        public static EvaluationResult EvaluatePaytableSection(SlotPaytableSection paytableSection,
                                                               CellPopulationOutcome populatedWindow,
                                                               BetDefinitionList betDefinitions,
                                                               ProgressiveLevels progressiveLevels,
                                                               long denomination)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection", "Argument may not be null");
            }

            if(populatedWindow == null)
            {
                throw new ArgumentNullException("populatedWindow", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            var winOutcome = new WinOutcome();

            if(paytableSection.LinePatternList != null)
            {
                var lineWins = EvaluateLineWins(paytableSection, populatedWindow, betDefinitions);
                winOutcome.WinOutcomeItems.AddRange(lineWins.WinOutcomeItems);
            }

            if(paytableSection.ScatterPatternList != null)
            {
                var scatterWins = EvaluateScatterWins(paytableSection, populatedWindow, betDefinitions);
                winOutcome.WinOutcomeItems.AddRange(scatterWins.WinOutcomeItems);
            }

            //The best static strategy is used for the lines and scatter wins, but multiway wins
            //utilize a different strategy.
            WinProcessor.ExecuteBestStaticMode(winOutcome, progressiveLevels);

            if(paytableSection.MultiwayPatternList != null)
            {
                var multiwayWins = EvaluateMultiwayWins(paytableSection, populatedWindow, betDefinitions);

                //In a multiway game which has wins that are PayBoth may not use this win processing method
                //if it is possible for the same prize to be won from the left and right during the same game.
                //If this is a requirement, then this processor can be used if the prizes are modified such
                //that there are different entries for both PayLeft and PayRight.
                WinProcessor.ExecuteBestWinPerPrize(multiwayWins, progressiveLevels);
                winOutcome.WinOutcomeItems.AddRange(multiwayWins.WinOutcomeItems);
            }

            //Convert the wins to base units
            DenominationProcessor.AdjustWinAmount(winOutcome, denomination);

            return new EvaluationResult(paytableSection.name, winOutcome, populatedWindow);
        }

        /// <summary>
        /// Evaluate line wins for the given section.
        /// </summary>
        /// <param name="paytableSection">Section to evaluate.</param>
        /// <param name="populatedWindow">Populated symbol window to evaluate.</param>
        /// <param name="betDefinitions">Bet definitions to use for the evaluation.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the passed paytable section is null.
        /// </exception>
        /// <returns>A win outcome containing the line wins for the section.</returns>
        public static WinOutcome EvaluateLineWins(SlotPaytableSection paytableSection,
                                                  CellPopulationOutcome populatedWindow,
                                                  BetDefinitionList betDefinitions)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection", "Argument may not be null");
            }

            var populatedLinePatterns = PatternPopulator.PopulatePatterns(populatedWindow,
                                                                          paytableSection.LinePatternList);

            var winOutcome = PrizeEvaluator.EvaluatePatterns(paytableSection.LinePrizeScale, populatedLinePatterns);

            BetProcessor.ProcessBetRequirements(winOutcome, paytableSection.LinePrizeScale, betDefinitions,
                                                paytableSection.LinePatternList);

            return winOutcome;
        }

        /// <summary>
        /// Evaluate scatter wins for the given section.
        /// </summary>
        /// <param name="paytableSection">Section to evaluate.</param>
        /// <param name="populatedWindow">Populated symbol window to evaluate.</param>
        /// <param name="betDefinitions">Bet definitions to use for the evaluation.</param>
        /// <returns>A win outcome containing the scatter wins for the section.</returns>
        /// <exception cref="ArgumentNullException">Thrown when paytableSection is null.</exception>
        public static WinOutcome EvaluateScatterWins(SlotPaytableSection paytableSection,
                                                     CellPopulationOutcome populatedWindow,
                                                     BetDefinitionList betDefinitions)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection", "Argument may not be null");
            }

            var populatedScatterPatterns = PatternPopulator.PopulatePatterns(populatedWindow,
                                                                             paytableSection.ScatterPatternList);

            var winOutcome = PrizeEvaluator.EvaluatePatterns(paytableSection.ScatterPrizeScale,
                                                             populatedScatterPatterns);

            BetProcessor.ProcessBetRequirements(winOutcome, paytableSection.ScatterPrizeScale, betDefinitions,
                                                paytableSection.ScatterPatternList);

            return winOutcome;
        }

        /// <summary>
        /// Evaluate multiway wins for the given section.
        /// </summary>
        /// <param name="paytableSection">Section to evaluate.</param>
        /// <param name="populatedWindow">Populated symbol window to evaluate.</param>
        /// <param name="betDefinitions">Bet definitions to use for the evaluation.</param>
        /// <returns>A win outcome containing the multiway wins for the section.</returns>
        /// <exception cref="ArgumentNullException">Thrown when passed paytable section is null.</exception>
        public static WinOutcome EvaluateMultiwayWins(SlotPaytableSection paytableSection,
                                                      CellPopulationOutcome populatedWindow,
                                                      BetDefinitionList betDefinitions)
        {
            if(paytableSection == null)
            {
                throw new ArgumentNullException("paytableSection", "Argument may not be null");
            }
            var populatedMultiwayPatterns = MultiwayPatternPopulator.PopulatePatterns(populatedWindow,
                                                                                      paytableSection.
                                                                                          MultiwayPatternList);

            var winOutcome = MultiwayPrizeEvaluator.EvaluatePatterns(paytableSection.MultiwayPrizeScale,
                                                                     populatedMultiwayPatterns,
                                                                     paytableSection.MultiwayPatternList.name);

            BetProcessor.ProcessBetRequirements(winOutcome, paytableSection.MultiwayPrizeScale, betDefinitions,
                                                paytableSection.MultiwayPatternList);

            return winOutcome;
        }
    }
}
