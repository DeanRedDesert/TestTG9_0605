// -----------------------------------------------------------------------
// <copyright file = "CanBetPerLimits.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.FoundationBettingPermit
{
    using System;
    using Game.Core.Money;
    using Interfaces;

    /// <summary>
    /// A can-bet operand that checks the bet amount against max and min bet limits.
    /// </summary>
    public class CanBetPerLimits : ICanBetOperand
    {
        #region Private Fields

        private readonly long maxBetCredits;
        private readonly long minBetCredits;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="CanBetPerLimits"/>.
        /// </summary>
        /// <param name="maxBetCredits">
        /// The max bet amount allowed, in units of game denomination.
        /// </param>
        /// <param name="minBetCredits">
        /// The min bet amount allowed, in units of game denomination.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="maxBetCredits"/> is less than <paramref name="minBetCredits"/>.
        /// </exception>
        public CanBetPerLimits(long maxBetCredits, long minBetCredits)
        {
            if(maxBetCredits < minBetCredits)
            {
                throw new ArgumentException($"Max credits ({maxBetCredits}) cannot be less than Min credits ({minBetCredits})");
            }

            this.maxBetCredits = maxBetCredits;
            this.minBetCredits = minBetCredits;
        }

        #endregion

        #region ILogicalOperand<Amount> Implementation

        /// <inheritdoc />
        public bool Execute(Amount data)
        {
            var betCredits = data.GameCreditValue;

            return betCredits <= maxBetCredits && betCredits >= minBetCredits;
        }

        #endregion
    }
}