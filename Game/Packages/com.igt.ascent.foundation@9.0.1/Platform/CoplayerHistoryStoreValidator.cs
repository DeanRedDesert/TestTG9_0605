// -----------------------------------------------------------------------
// <copyright file = "CoplayerHistoryStoreValidator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using Interfaces;

    /// <summary>
    /// The class defines a critical data validator utility class for the CoplayerHistoryStore.
    /// </summary>
    internal class CoplayerHistoryStoreValidator : ICriticalDataStoreAccessValidator
    {
        /// <summary>
        /// Cached interface for querying the game cycle state on the coplayer.
        /// </summary>
        private readonly IGameCycleStateQuery gameCycleStateQuery;

        /// <summary>
        /// Cached interface for querying the game mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameCycleStateQuery">
        /// The interface for querying the game cycle state on the coplayer.
        /// </param>
        /// <param name="gameModeQuery">
        /// The interface for querying the game mode.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameCycleStateQuery"/> or <paramref name="gameModeQuery"/> is null.
        /// </exception>
        public CoplayerHistoryStoreValidator(IGameCycleStateQuery gameCycleStateQuery, IGameModeQuery gameModeQuery)
        {
            this.gameCycleStateQuery = gameCycleStateQuery ?? throw new ArgumentNullException(nameof(gameCycleStateQuery));
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));
        }

        #region ICriticalDataStoreAccessValidator Implementation

        /// <inheritdoc/>
        public void Validate(DataAccess dataAccess, string storeName)
        {
            // PlayMode: Access is allowed in all game-cycle states except Idle (See Foundation Game Manager code,
            //           GamePlay::isCriticalDataAccessPermitted(), case CriticalDataScope::COPLAYER_HISTORY).
            // HistoryMode: Readonly
            // UtilityMode: No access
            switch(gameModeQuery.GameMode)
            {
                case GameMode.Play:
                    {
                        var gameCycleState = gameCycleStateQuery.GameCycleState;
                        if(gameCycleState == GameCycleState.Idle)
                        {
                            throw new CriticalDataAccessDeniedException(
                                $"The critical data store [{storeName}] is not accessible in current game cycle state [{gameCycleState}].");
                        }
                        break;
                    }
                case GameMode.History:
                    {
                        if(dataAccess != DataAccess.Read)
                        {
                            throw new CriticalDataAccessDeniedException(
                                $"The critical data store [{storeName}] is readonly in the History mode.");
                        }
                        break;
                    }
                default:
                    {
                        throw new CriticalDataAccessDeniedException(
                            $"The critical data store [{storeName}] is not accessible in the Utility mode.");
                    }
            }
        }

        #endregion
    }
}
