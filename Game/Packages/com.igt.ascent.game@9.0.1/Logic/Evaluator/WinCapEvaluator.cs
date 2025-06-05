//-----------------------------------------------------------------------
// <copyright file = "WinCapEvaluator.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System.Collections.Generic;
    using System.Linq;
    using Schemas;

    /// <summary>
    /// Class which contains methods for win cap evaluation.
    /// </summary>
    public static class WinCapEvaluator
    {
        /// <summary>
        /// Apply the current game win cap by removing or decreasing wins that exceed the win cap.
        /// </summary>
        /// <param name="winOutcome">The current game win outcome without the win cap being applied.</param>
        /// <param name="winCapLimit">The current game win cap limit.</param>
        /// <param name="totalWin">The current win amount of the active game or bonus stage.</param>
        /// <returns>True if the win cap is applied, or false if the win cap is not applied.</returns>
        public static bool ApplyWinCap(WinOutcome winOutcome, long winCapLimit, long totalWin)
        {
            var winCapApplied = false;
            // Only continue if the game has a valid win cap
            if(winCapLimit > 0)
            {
                long totalWinAmount = 0;
                // The availale win amount that can be awarded (win cap - current win amount)
                var availableWinAmount = winCapLimit - totalWin;
                // A list of wins that need removing as they exceed the win cap limit
                var winsToRemove = new List<WinOutcomeItem>();

                // loop through each win
                foreach(var winOutcomeitem in winOutcome.WinOutcomeItems)
                {
                    // calculate the new win amount
                    var newWinAmount = totalWinAmount + winOutcomeitem.Prize.winAmount;

                    // is the new win amount too much? Does it exceed the win cap?
                    if(newWinAmount > availableWinAmount)
                    {
                        // How much has the win cap been exceeded by?`
                        var winDeduction = newWinAmount - availableWinAmount;

                        // A part deduction i.e. the award needs reducing to meet the win cap
                        if(winOutcomeitem.Prize.winAmount > winDeduction)
                        {
                            winOutcomeitem.Prize.winAmount -= winDeduction; // adjust the win amount
                        }
                        else // The entire award breached the win cap limit then it can be removed
                        {
                            winsToRemove.Add(winOutcomeitem);
                            winOutcomeitem.Prize.winAmount = 0;
                        }
                    }
                    // only increment win amount once prize amount has been adjusted
                    totalWinAmount += winOutcomeitem.Prize.winAmount; 
                }

                // Remove awards that exceed the win cap
                foreach(var winOutcomeitem in winsToRemove)
                {
                    winOutcome.WinOutcomeItems.Remove(winOutcomeitem);
                }

                // Win amount > win amount available then win cap exceeded and error required
                if(totalWinAmount > availableWinAmount)
                {
                    throw new WinCapEvaluatorException("The win cap has been exceeded attempting to pay " + totalWinAmount +
                        ". The win cap is currently set to " + winCapLimit +
                        " and the current total win is " + totalWin);
                }
                // If the win cap has been reached then no further triggers are allowed
                if(totalWinAmount == availableWinAmount)
                {
                    var winsWithTriggers = winOutcome.WinOutcomeItems.Where(winWithTrigger => winWithTrigger.Prize.Trigger != null);

                    foreach (var winOutcomeitem in winsWithTriggers)
                    {
                        winOutcomeitem.Prize.Trigger.Clear();  // Remove the trigger
                    }

                    // win cap has been applied
                    winOutcome.totalWinAmount = totalWinAmount;
                    winCapApplied = true;
                }
            }
            // return whether the win cap was applied
            return winCapApplied;
        }
    }
}
