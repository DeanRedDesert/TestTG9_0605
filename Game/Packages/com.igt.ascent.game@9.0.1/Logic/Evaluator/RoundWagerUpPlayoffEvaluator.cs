//-----------------------------------------------------------------------
// <copyright file = "RoundWagerUpPlayoffEvaluator.cs" company = "IGT">
//     Copyright (c) IGT 2015-2019.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// This class provides a helper function to do an evaluation if a round wager up playoff gets win or loss.
    /// </summary>
    public static class RoundWagerUpPlayoffEvaluator
    {
        /// <summary>
        /// Do an evaluation to determine if a round wager up playoff is a win or loss.
        /// </summary>
        /// <param name="residualAmount">
        /// Specify the residual amount for evaluating on round wager up playoff, in base units.
        /// </param>
        /// <param name="expectedBetAmount">
        /// Specify the expected bet amount for evaluating on round wager up playoff, in base units.
        /// </param>
        /// <param name="randomNumberGenerator">The random number generator.</param>
        /// <exception cref="ArgumentException">
        ///     Throw if <paramref name="residualAmount"/> or <paramref name="expectedBetAmount"/> is not positive,
        ///     or if <paramref name="expectedBetAmount"/> is not greater than <paramref name="residualAmount"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     Throw if <paramref name="randomNumberGenerator"/> is null.
        /// </exception>
        /// <returns>True if win, false otherwise.</returns>
        /// <remarks>
        /// The evaluation algorithm implemented in this helper function provides a win possibility which is
        /// the proportion of residual amount with respect to expectedBetAmount. 
        /// </remarks>
        public static bool Evaluate(long residualAmount, long expectedBetAmount,
                                        IRandomNumbers randomNumberGenerator)
        {
            if(residualAmount <= 0)
            {
                throw new ArgumentException("Parameter should be positive.", "residualAmount");
            }

            if(expectedBetAmount <= 0)
            {
                throw new ArgumentException("Parameter should be positive.", "expectedBetAmount");
            }

            if(expectedBetAmount <= residualAmount)
            {
                throw new ArgumentException(string.Format("{0} must be greater than {1}.",
                                            "expectedBetAmount", "residualAmount"));
            }

            if(randomNumberGenerator == null)
            {
                throw new ArgumentNullException("randomNumberGenerator", "Parameter may not be null.");
            }

            var request = new RandomValueRequest(1, 1, (int)expectedBetAmount);
            var numbers = randomNumberGenerator.GetRandomNumbers(request);

            return residualAmount >= numbers.ElementAt(0);
        }
    }
}
