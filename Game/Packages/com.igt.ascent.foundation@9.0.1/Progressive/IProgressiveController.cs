//-----------------------------------------------------------------------
// <copyright file = "IProgressiveController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.OutcomeList;

    /// <summary>
    /// This interface is used for game progressive controllers
    /// that manage the progressive levels and amounts.
    /// </summary>
    /// <remarks>
    /// This Interface API is designed to be called from a game's
    /// state machine, that is, the game gets the direct control
    /// over how the progressive controller works, in terms
    /// of when to contribute, when to evaluate, and when to
    /// reset.
    /// </remarks>
    public interface IProgressiveController
    {
        #region Events

        /// <summary>
        /// Event for broadcasting progressive amounts.
        /// </summary>
        event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Get the name of the controller.  It must conform to the
        /// requirement of a critical data path, since the name
        /// is used as the critical data path for this controller.
        /// </summary>
        /// <seealso cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>
        string ControllerName { get; }

        /// <summary>
        /// Get the number of progressive levels supported by the controller.
        /// </summary>
        int ControllerLevelCount { get; }

        /// <summary>
        /// Get the number of progressive levels in the game that are
        /// mapped to one of the controller levels.
        /// </summary>
        int GameLevelCount { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Check whether a specified game level is linked
        /// to this progressive controller or not.
        /// </summary>
        /// <param name="gameLevel">The game level in query.</param>
        /// <returns>True if the game level is linked.  False otherwise.</returns>
        bool IsGameLevelLinked(int gameLevel);

        /// <summary>
        /// Get the settings of a specific progressive game level.
        /// </summary>
        /// <param name="gameLevel">The game level in query.</param>
        /// <returns>The configuration of the progressive game level.</returns>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        ProgressiveConfiguration GetProgressiveConfiguration(int gameLevel);

        /// <summary>
        /// Get the settings of all linked game levels.
        /// </summary>
        /// <returns>A list of game levels and their settings, keyed by the game level.</returns>
        IDictionary<int, ProgressiveConfiguration> GetAllProgressiveConfigurations();

        /// <summary>
        /// Get the current amount of a specific progressive game level.
        /// </summary>
        /// <param name="gameLevel">The game level in query.</param>
        /// <returns>The current amount of the progressive game level.</returns>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        long GetProgressiveAmount(int gameLevel);

        /// <summary>
        /// Get the progressive amounts of all linked game levels.
        /// </summary>
        /// <returns>A list of game levels and their progressive amounts, keyed by the game level.</returns>
        IDictionary<int, long> GetAllProgressiveAmounts();

        /// <summary>
        /// Calculate the contribution of the player bet and add it to the progressive amount
        /// for the specified non event-based progressive game level.
        /// </summary>
        /// <param name="gameLevel">The game level that the contribution destined.</param>
        /// <param name="bet">The player bet, in units of denomination passed in.</param>
        /// <param name="denomination">The denomination of the player bet.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bet"/> is less than 0, or <paramref name="denomination"/>
        /// is less than 1.
        /// </exception>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        void ContributeToProgressive(int gameLevel, long bet, long denomination);

        /// <summary>
        /// Calculate the contribution of the player bet and add it to the progressive amount
        /// for all available non event-based progressive game levels.
        /// </summary>
        /// <param name="bet">The player bet, in units of denomination passed in.</param>
        /// <param name="denomination">The denomination of the player bet.</param>
        /// <param name="saveToCriticalData">A flag indicating if this contribution should be persisted to safe storage.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bet"/> is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        void ContributeToAllProgressives(long bet, long denomination, bool saveToCriticalData);

        /// <summary>
        /// Calculate the contribution of the player bet and add it to the progressive amount
        /// for all available event-based progressive game levels.
        /// </summary>
        /// <param name="bet">The player bet, in units of denomination passed in.</param>
        /// <param name="denomination">The denomination of the player bet.</param>
        /// <param name="saveToCriticalData">A flag indicating if this contribution should be persisted to safe storage.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="bet"/> is less than 0, or <paramref name="denomination"/> is less than 1.
        /// </exception>
        void ContributeToAllEventBasedProgressives(long bet, long denomination, bool saveToCriticalData);

        /// <summary>
        /// Validate a progressive hit specified by a progressive award.
        /// Fill in the hit state, amount and prize string fields of the
        /// progressive award accordingly.
        /// </summary>
        /// <param name="progressiveAward">The progressive award to validate.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="progressiveAward"/> passed in is invalid.
        /// </exception>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the game level specified in the progressive award is not linked.
        /// </exception>
        void ValidateProgressiveHit(ProgressiveAward progressiveAward);

        /// <summary>
        /// Reset the progressive levels that have been hit.
        /// The displayable amount will be reset to the starting amount
        /// plus amount escrowed in the overflow meter, up to the
        /// maximum amount.
        /// </summary>
        void ResetProgressiveHits();

        /// <summary>
        /// Get the hit record of a specific progressive game level.
        /// </summary>
        /// <param name="gameLevel">The game level in query.</param>
        /// <returns>The hit record of the progressive game level.</returns>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        ProgressiveHitRecord GetProgressiveHitRecord(int gameLevel);

        /// <summary>
        /// Get the progressive hit records of all linked game levels.
        /// </summary>
        /// <returns>A list of game levels and their progressive hit records, keyed by the game level.</returns>
        IDictionary<int, ProgressiveHitRecord> GetAllProgressiveHitRecords();

        /// <summary>
        /// Get the broadcast data of a specific progressive game level.
        /// </summary>
        /// <param name="gameLevel">The game level in query.</param>
        /// <returns>The broadcast data of the progressive game level.</returns>
        /// <exception cref="GameLevelNotLinkedException">
        /// Thrown when the specified game level is not linked.
        /// </exception>
        ProgressiveBroadcastData GetProgressiveBroadcastData(int gameLevel);

        /// <summary>
        /// Get the progressive broadcast data of all linked game levels.
        /// </summary>
        /// <returns>A list of game levels and their progressive broadcast data, keyed by the game level.</returns>
        IDictionary<int, ProgressiveBroadcastData> GetAllProgressiveBroadcastData();

        #endregion
    }

}
