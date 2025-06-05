// -----------------------------------------------------------------------
// <copyright file = "BetLimits.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// Contains the amount limits related to max bet and min bet.
    /// </summary>
    public class BetLimits
    {
        #region Properties

        /// <summary>
        /// Gets the min bet limit, in units of base units.
        /// A zero amount indicates there is no limit.
        /// </summary>
        public long MinBet { get; }

        /// <summary>
        /// Gets the max bet limit, in units of base units.
        /// A zero amount indicates there is no limit.
        /// </summary>
        public long MaxBet { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="BetLimits"/>.
        /// </summary>
        /// <param name="minBet">The min bet limit, in units of base units.</param>
        /// <param name="maxBet">The max bet limit, in units of base units.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="minBet"/> or <paramref name="maxBet"/> is negative.
        /// </exception>
        public BetLimits(long minBet, long maxBet)
        {
            if(minBet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minBet), "Min bet limit cannot be negative.");
            }
            if(maxBet < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxBet), "Max bet limit cannot be negative.");
            }

            MinBet = minBet;
            MaxBet = maxBet;
        }

        #endregion

        #region Public Method

        /// <summary>
        /// Checks if <paramref name="betValue"/> is allowed by the limits held by this object.
        /// </summary>
        /// <param name="betValue">
        /// The bet value to check, in units of base units.
        /// </param>
        /// <returns>
        /// True if <paramref name="betValue"/> is allowed by both MinBet and MaxBet limits; False otherwise.
        /// </returns>
        public bool Allows(long betValue)
        {
            return MinAllows(betValue) && MaxAllows(betValue);
        }

        /// <summary>
        /// Checks if <paramref name="betValue"/> is allowed by the MinBet limit held by this object.
        /// </summary>
        /// <param name="betValue">
        /// The bet value to check, in units of base units.
        /// </param>
        /// <returns>
        /// True if <paramref name="betValue"/> is allowed by the MinBet limit; False otherwise.
        /// </returns>
        public bool MinAllows(long betValue)
        {
            return MinBet == 0 || betValue >= MinBet;
        }

        /// <summary>
        /// Checks if <paramref name="betValue"/> is allowed by the MaxBet limit held by this object.
        /// </summary>
        /// <param name="betValue">
        /// The bet value to check, in units of base units.
        /// </param>
        /// <returns>
        /// True if <paramref name="betValue"/> is allowed by the MaxBet limit; False otherwise.
        /// </returns>
        public bool MaxAllows(long betValue)
        {
            return MaxBet == 0 || betValue <= MaxBet;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"BetLimits: max({MaxBet}) / min({MinBet})";
        }

        #endregion
    }
}
