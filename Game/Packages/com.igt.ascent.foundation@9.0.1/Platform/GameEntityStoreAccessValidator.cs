// -----------------------------------------------------------------------
// <copyright file = "GameEntityStoreAccessValidator.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using Interfaces;

    /// <summary>
    /// The class defines a critical data validator utility class for the ShellStore, ThemeStore and PayvarStore.
    /// </summary>
    internal class GameEntityStoreAccessValidator : ICriticalDataStoreAccessValidator
    {
        /// <summary>
        /// Cached interface for querying game mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameModeQuery">
        /// The interface for querying the game mode.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameModeQuery"/> is null.
        /// </exception>
        public GameEntityStoreAccessValidator(IGameModeQuery gameModeQuery)
        {
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));
        }

        #region ICriticalDataStoreAccessValidator Implementation

        /// <inheritdoc/>
        public void Validate(DataAccess dataAccess, string storeName)
        {
            // Play/Utility Mode: R/W, History Mode: No access
            if(gameModeQuery.GameMode == GameMode.Play || gameModeQuery.GameMode == GameMode.Utility)
            {
                return;
            }

            throw new CriticalDataAccessDeniedException(
                $"The critical data store [{storeName}] is not accessible in {gameModeQuery.GameMode} mode.");
        }

        #endregion
    }
}
