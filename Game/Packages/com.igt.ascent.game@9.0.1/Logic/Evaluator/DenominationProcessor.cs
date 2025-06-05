//-----------------------------------------------------------------------
// <copyright file = "DenominationProcessor.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using Communication.Foundation;
    using Schemas;

    /// <summary>
    /// The purpose of the denomination processor is to convert the win amount in the win outcome
    /// to units of specified denomination.
    /// </summary>
    public static class DenominationProcessor
    {
        /// <summary>
        /// Convert the win amount in <paramref name="win"/> to base units.
        /// </summary>
        /// <param name="win">The win outcome.</param>
        /// <param name="denomination">The denomination for the win amount passed in.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="win"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the passed <paramref name="denomination"/> is less than or equal to 0.</exception>
        public static void AdjustWinAmount (WinOutcome win, long denomination)
        {
            if(win == null)
            {
                throw new ArgumentNullException("win", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            foreach(var winItem in win.WinOutcomeItems)
            {
                AdjustWinAmount(winItem, denomination);
            }

            checked
            {
                win.totalWinAmount = Utility.ConvertToCents(win.totalWinAmount, denomination);
            }
        }

        /// <summary>
        /// Convert the win amount in <paramref name="win"/> to base units.
        /// </summary>
        /// <param name="win">The win outcome item.</param>
        /// <param name="denomination">The denomination for the win amount passed in.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="win"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the passed <paramref name="denomination"/> is less than or equal to 0.</exception>
        public static void AdjustWinAmount(WinOutcomeItem win, long denomination)
        {
            if(win == null)
            {
                throw new ArgumentNullException("win", "Argument may not be null");
            }

            if(denomination <= 0)
            {
                throw new ArgumentOutOfRangeException("denomination", "Denomination must be greater than 0");
            }

            win.Prize.winAmount = Utility.ConvertToCents(win.Prize.winAmount, denomination);

            if(win.Prize.averageBonusPaySpecified)
            {
                win.Prize.averageBonusPay = Utility.ConvertToCents(win.Prize.averageBonusPay, denomination);
            }
        }
    }
}
