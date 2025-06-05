//-----------------------------------------------------------------------
// <copyright file = "Utility.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Static class holding a collection of utility functions and constants.
    /// </summary>
    public static class Utility
    {
        #region Private Members

        /// <summary>
        /// Regular expression used to validate the critical data name.
        /// </summary>
        private static readonly Regex RgxCriticalDataName = new Regex(@"^[a-zA-Z0-9\._-]([a-zA-Z0-9/\._-]{0,1}[a-zA-Z0-9\._-])+$");

        #endregion

        #region Public Methods

        /// <summary>
        /// Check if the given name is a valid name for critical data.
        /// <list type="number">
        /// <item>
        /// The character set for the name is limited to a subset of
        /// ASCII characters that include numeric, alphabetic and the
        /// characters '/', '.', '_', and '-'.
        /// </item>
        /// <item>
        /// The name cannot start with '/'.
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="name">The name to check.</param>
        /// <returns>True if the given name is valid.  False otherwise.</returns>
        public static bool ValidateCriticalDataName(string name)
        {
            return RgxCriticalDataName.IsMatch(name);
        }

        /// <summary>
        /// Convert the passed value from in base units to units of the game denomination.
        /// The credits will be rounded down if it is fractional.
        /// </summary>
        /// <param name="valueInBaseDenom">The value to be converted, in base units.</param>
        /// <param name="gameDenomination">The game denomination to be used for conversion.</param>
        /// <returns>The converted value in units of game denomination.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="valueInBaseDenom"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="gameDenomination"/> is less than or equal to 0.
        /// </exception>
        public static long ConvertToCredits(long valueInBaseDenom, long gameDenomination)
        {
            if(valueInBaseDenom < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valueInBaseDenom), "valueInBaseDenom may not be less than 0.");
            }

            if(gameDenomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gameDenomination),
                    "gameDenomination may not be less than or equal to 0.");
            }

            return ConvertToCredits(valueInBaseDenom, gameDenomination, false);
        }

        /// <summary>
        /// Convert the passed value from in base units to units of the game denomination.
        /// </summary>
        /// <param name="valueInBaseDenom">The value to be converted, in base units.</param>
        /// <param name="gameDenomination">The game denomination to be used for conversion.</param>
        /// <param name="roundedUp">True if rounding the fractional credits up; otherwise, false.</param>
        /// <returns>The converted value in units of game denomination.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="valueInBaseDenom"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="gameDenomination"/> is less than or equal to 0.
        /// </exception>
        public static long ConvertToCredits(long valueInBaseDenom, long gameDenomination, bool roundedUp)
        {
            if(valueInBaseDenom < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(valueInBaseDenom), "valueInBaseDenom may not be less than 0.");
            }

            if(gameDenomination <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gameDenomination),
                    "gameDenomination may not be less than or equal to 0.");
            }

            var quotient = Math.DivRem(valueInBaseDenom, gameDenomination, out var remainder);
            checked
            {
                return roundedUp && remainder != 0 ? quotient + 1 : quotient;
            }
        }

        /// <summary>
        /// Convert the passed value from units of the game denomination to in base units.
        /// </summary>
        /// <param name="valueInCredits">The value to be converted, in units of game denomination.</param>
        /// <param name="gameDenomination">The game denomination to be used for conversion.</param>
        /// <returns>The converted value in base units.</returns>
        public static long ConvertToCents(long valueInCredits, long gameDenomination)
        {
            return checked (valueInCredits * gameDenomination);
        }

        /// <summary>
        /// Make the slashes in a path string uniform.
        /// Covert all backward slashes '/' to forward ones '\'.
        /// </summary>
        /// <remarks>
        /// The Foundation doesn't recognize '\' in registries, so '/' has to be used.
        /// However, since the .NET classes, such as File, Directory and Path, return '\'
        /// in their methods, it is decided that '\' should be used in all game side
        /// implementations, including Standalone Game Lib and Progressive Controller.
        /// Therefore, the '/' appearing in registries (as well as the system config file)
        /// must be converted to '\'.
        /// If the Foundation changes to accept '\' in the future, this function will
        /// not be needed then.
        /// </remarks>
        /// <param name="path">The string to convert.</param>
        /// <returns>New string with unified slashes.</returns>
        public static string UniformSlashes(string path)
        {
            return path?.Replace('/', '\\');
        }

        /// <summary>
        /// Constructs a history step path string with the given <paramref name="historyStep"/> step number.
        /// </summary>
        /// <param name="historyStep">The history step number.</param>
        /// <returns>A history step path string with the given step number.</returns>
        /// <remarks>If the history step number is used as a critical data path, it must meet the critical data name requirement
        /// defined in this class. Refer to <see cref="RgxCriticalDataName"/>.
        /// </remarks>
        public static string GetHistoryStepPath(int historyStep)
        {
            string historyStepPath = 'S' + historyStep.ToString(CultureInfo.InvariantCulture);
            return historyStepPath;
        }

        /// <summary>
        /// Checks if the game is allowed to read or write history scope critical data during the Play
        /// <see cref="GameMode"/> and a given <see cref="GameCycleState"/>.
        /// </summary>
        /// <param name="state">The game cycle state to check.</param>
        /// <returns>Returns true if the game is allowed to read or write history scope critical data.</returns>
        public static bool CanAccessHistoryDataInPlayMode(GameCycleState state)
        {
            if(state == GameCycleState.Idle || state == GameCycleState.Committed || 
               state == GameCycleState.EnrollPending || state == GameCycleState.EnrollComplete)
            {
                return false;
            }

            return true;
        }
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Check if the access to a critical data scope is allowed.
        /// </summary>
        /// <remarks>
        /// In certain game modes, the accessibility of a critical data
        /// scope varies in different game cycle states.  Use Invalid
        /// state for <paramref name="gameState"/> if the state checking
        /// is to be skipped.
        /// </remarks>
        /// <param name="gameContextMode">The game mode in which the critical data is accessed.</param>
        /// <param name="gameState">The game cycle state in which the critical data is accessed.</param>
        /// <param name="attemptedAccess">The description of the attempted access.</param>
        /// <param name="scope">The critical data scope accessed.</param>
        /// <exception cref="CriticalDataAccessDeniedException">
        /// Thrown when the access to the critical data scope is denied in the specified game mode.
        /// </exception>
        internal static void ValidateCriticalDataAccess(GameMode gameContextMode,
                                                        GameCycleState gameState,
                                                        DataAccessing attemptedAccess,
                                                        CriticalDataScope scope)
        {
            if(!SupportedCriticalDataScopeTable.IsScopeAllowed(CriticalDataScopeClientType.Game, scope))
            {
                throw new ArgumentException(scope + " is invalid for accessing critical data for game.");
            }

            var allowed = true;

            string message = null;

            switch(gameContextMode)
            {
                case GameMode.Play:
                    // GameCycle scope is not accessible in Idle State in Play mode.
                    if(scope == CriticalDataScope.GameCycle && gameState == GameCycleState.Idle)
                    {
                        allowed = false;
                        message = "GameCycle scope is not accessible in Idle state in Play mode.";
                    }
                    // History scope is not accessible in some states in Play mode.
                    else if(scope == CriticalDataScope.History && !CanAccessHistoryDataInPlayMode(gameState))
                    {
                        allowed = false;
                        message = $"History scope is not accessible in {gameState} state in Play mode.";
                    }
                    else if(scope == CriticalDataScope.Feature)
                    {
                        allowed = false;
                        message = "Feature scope is not supported by Ascent.";
                    }

                    break;

                case GameMode.Utility:
                    // History scope is not accessible in Utility mode.
                    if(scope == CriticalDataScope.History)
                    {
                        allowed = false;
                        message = "History scope is not accessible in Utility mode.";
                    }
                    break;

                case GameMode.History:
                    // Only History scope is accessible in History mode,
                    // and it is read only.
                    if(scope != CriticalDataScope.History)
                    {
                        allowed = false;
                        message = "Only History scope is accessible in History mode.";
                    }
                    else if(attemptedAccess != DataAccessing.Read)
                    {
                        allowed = false;
                        message = "History scope is read only in History mode.";
                    }
                    break;

                case GameMode.Invalid:
                    allowed = false;
                    message = "No critical data access is allowed in Invalid mode.";
                    break;
            }

            if(!allowed)
            {
                throw new CriticalDataAccessDeniedException(message);
            }
        }

        /// <summary>
        /// Check if the access to the configuration data is allowed.
        /// </summary>
        /// <param name="gameContextMode">The game mode in which the data is accessed.</param>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied in the current game mode.
        /// </exception>
        internal static void ValidateConfigurationAccess(GameMode gameContextMode)
        {
            if(gameContextMode == GameMode.History || gameContextMode == GameMode.Invalid)
            {
                throw new ConfigurationAccessDeniedException(
                    $"Configuration data is not accessible in {gameContextMode} mode.");
            }
        }

        #endregion
    }
}
