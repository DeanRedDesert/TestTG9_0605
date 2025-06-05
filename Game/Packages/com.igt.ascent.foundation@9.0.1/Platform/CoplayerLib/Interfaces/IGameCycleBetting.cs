// -----------------------------------------------------------------------
// <copyright file = "IGameCycleBetting.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using Platform.Interfaces;

    /// <summary>
    /// This class defines the interface for the coplayers to talk to the Foundation in terms of game cycle betting.
    /// </summary>
    public interface IGameCycleBetting
    {
        /// <summary>
        /// Gets the config data related to game cycle betting.
        /// </summary>
        GameCycleBettingConfigData ConfigData { get; }

        /// <summary>
        /// Requests an amount be committed to bet.
        /// </summary>
        /// <remarks>
        /// <see cref="CommitBet"/> can be called more than once before enrolling.  It is the last call that matters.
        /// The bet amount in latter calls can be more or less than the bet amount in the previous calls.
        /// For example, it is okay to call CommitBet(5, 1) followed by CommitBet(20, 1), then CommitBet(10, 1).
        /// The final committed bet will be 10.
        /// 
        /// Can only be called in the <see cref="GameCycleState.Committed"/> state.
        /// </remarks>
        /// <param name="betInCredits">The bet amount in units of the denomination passed in.</param>
        /// <param name="denomination">The denomination of the bet.</param>
        /// <returns>
        /// The flag that indicates whether or not the bet was committed.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="betInCredits"/> is less than 0, or <paramref name="denomination"/>
        /// is less than 1.
        /// </exception>
        bool CommitBet(long betInCredits, long denomination);

        /// <summary>
        /// Gets the amount, in base units, that is committed as a bet at the moment.
        /// This value does not include any mid-game bet information.
        /// </summary>
        /// <returns>
        /// The bet amount committed at the moment.  Null if no <see cref="CommitBet"/> has been called.
        /// </returns>
        /// <devdoc>
        /// Keeping this as a method so that in the future, if needed, we can modify it to GetCommittedBet(denom)
        /// to return a value in credits.
        /// </devdoc>
        long? GetCommittedBet();

        /// <summary>
        /// Requests the currently committed bet be uncommitted.
        /// </summary>
        /// <remarks>
        /// Can only be called in the <see cref="GameCycleState.Committed"/> or <see cref="GameCycleState.EnrollComplete"/> state.
        /// It is an error to call <see cref="UncommitBet"/> without calling <see cref="CommitBet"/> first.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when called without calling <see cref="CommitBet"/> first.
        /// </exception>
        void UncommitBet();

        /// <summary>
        /// Requests the currently committed amount be placed as a bet.
        /// </summary>
        /// <remarks>
        /// Can only be called in the <see cref="GameCycleState.EnrollComplete"/> state.
        /// It is an error to call <see cref="PlaceStartingBet"/> without calling <see cref="CommitBet"/> first.
        /// </remarks>
        /// <param name="isMaxBet">
        /// The flag that indicates whether this bet is the max bet.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when called without calling <see cref="CommitBet"/> first.
        /// </exception>
        void PlaceStartingBet(bool isMaxBet);

        /// <summary>
        /// Gets the amount, in base units, that was committed and placed as a bet when starting the game cycle.
        /// This returns a valid value when called after the <see cref="GameCycleState.EnrollComplete"/> state.
        /// This value does not include any mid-game bet information.
        /// </summary>
        /// <returns>
        /// The amount placed as the bet when starting the game cycle.
        /// </returns>
        long GetStartingBet();

        /// <summary>
        /// Requests the amount passed in be placed as a bet. This can occur mid game.
        /// </summary>
        /// <remarks>
        /// Can only be called in the <see cref="GameCycleState.Playing"/> state.
        /// </remarks>
        /// <param name="betInCredits">Number of credits to bet.</param>
        /// <param name="denomination">Denomination of the placed bet.</param>
        /// <returns>
        /// The flag that indicates whether or not the mid game bet was placed successfully.
        /// </returns>
        bool PlaceMidGameBet(long betInCredits, long denomination);

        /// <summary>
        /// Gets the accumulation of all mid game bets placed, in base units.
        /// </summary>
        /// <returns>
        /// The accumulated amount of mid game bets.
        /// </returns>
        long GetAccumulatedMidGameBet();
    }
}