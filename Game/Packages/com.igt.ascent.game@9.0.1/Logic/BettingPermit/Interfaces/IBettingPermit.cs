// -----------------------------------------------------------------------
// <copyright file = "IBettingPermit.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the functionality of a game side can-bet logic module.
    /// </summary>
    public interface IBettingPermit
    {
        /// <summary>
        /// Gets the game denomination used for permission check.
        /// </summary>
        long GameDenomination { get; }

        /// <summary>
        /// Check to see if the given bet amount may be committed.
        /// </summary>
        /// <param name="betInCredits">
        /// Total requested bet amount, in units of game denomination.
        /// </param>
        /// <returns>
        /// True if requested bet amount may be committed.  False otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="betInCredits"/> is less than 0.
        /// </exception>
        bool CanBet(long betInCredits);

        /// <summary>
        /// Checks which of the given list of bets can be committed.
        /// </summary>
        /// <param name="betsInCredits">
        /// The list of bet amounts, in units of game denomination.</param>
        /// <returns>
        /// A list of Boolean flags in which each Boolean value correlates to a bet passed in.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="betsInCredits"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="betsInCredits"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="betsInCredits"/> has element which is less than 0.
        /// </exception>
        IEnumerable<bool> CanBet(IEnumerable<long> betsInCredits);
    }
}