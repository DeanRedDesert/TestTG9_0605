// -----------------------------------------------------------------------
// <copyright file = "ISimulateGameModeControl.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides functionality to simulate the changing of game mode for games running in Standalone mode.
    /// </summary>
    public interface ISimulateGameModeControl
    {
        #region Game Mode Changing

        /// <summary>
        /// Enters a new game mode.
        /// </summary>
        /// <remarks>
        /// Nothing will be done if <paramref name="nextMode"/> is the same as the current mode.
        /// If <paramref name="nextMode"/> cannot be entered, the current mode will be exited,
        /// and the game context will be inactivated.
        /// </remarks>
        /// <param name="nextMode">The game mode to enter.</param>
        void EnterMode(GameMode nextMode);

        /// <summary>
        /// Exits the current game mode, and inactivate the game context.
        /// </summary>
        void ExitMode();

        /// <summary>
        /// Shuts down the game executable.
        /// </summary>
        void ShutDown();

        #endregion

        #region History Mode Support

        /// <summary>
        /// Gets the count of available history records.
        /// </summary>
        /// <returns>Count of available history records.</returns>
        int GetHistoryRecordCount();

        /// <summary>
        /// Checks if there are history records available after the current one.
        /// </summary>
        /// <returns>True if there are history records available after the current one.</returns>
        bool IsNextAvailable();

        /// <summary>
        /// Checks if there are history records available before the current one.
        /// </summary>
        /// <returns>True if there are history records available before the current one.</returns>
        bool IsPreviousAvailable();

        /// <summary>
        /// Moves forward to the next game cycle in history.
        /// </summary>
        void NextHistoryRecord();

        /// <summary>
        /// Moves backward to the previous game cycle in history.
        /// </summary>
        void PreviousHistoryRecord();

        #endregion

        #region Utility Mode Support

        /// <summary>
        /// Gets the flag indicating whether Utility Mode is enabled.
        /// If false, no need to display Utility operator menu at all.
        /// </summary>
        bool IsUtilityModeEnabled { get; }

        /// <summary>
        /// Gets the list of theme names as specified in the theme registries that support Utility mode.
        /// This is used by the Utility operator menu to present a sub-menu for selecting a theme.
        /// </summary>
        /// <returns>
        /// The list of theme names.
        /// </returns>
        IReadOnlyList<string> GetRegistrySupportedThemes();

        /// <summary>
        /// Gets the list of denominations supported by each payvar registry in a given theme,
        /// keyed by the pair of paytable name / paytable file name.
        /// This is used by the Utility operator menu to present a sub-menu
        /// for selecting a denomination for a given paytable in a given theme.
        /// </summary>
        /// <param name="theme">The theme name.</param>
        /// <returns>
        /// List of denominations supported by each payvar registry.
        /// Null if no game registries are being used.
        /// </returns>
        IReadOnlyDictionary<KeyValuePair<string, string>, IEnumerable<long>> GetRegistrySupportedDenominations(string theme);

        /// <summary>
        /// Sets the flag indicating whether a selection for Utility context has been complete.
        /// True if all of <see cref="UtilityTheme"/>, <see cref="UtilityPaytable"/> and <see cref="UtilityDenomination"/>
        /// have been set to valid values.
        /// </summary>
        bool UtilitySelectionComplete { set; }

        /// <summary>
        /// Sets the name of the theme to be used when Utility mode is next entered.
        /// </summary>
        string UtilityTheme { set; }

        /// <summary>
        /// Sets the paytable name / paytable file name to be used when Utility mode is next entered.
        /// </summary>
        KeyValuePair<string, string> UtilityPaytable { set; }

        /// <summary>
        /// Set the denomination to be used when Utility mode is next entered.
        /// </summary>
        long UtilityDenomination { set; }

        #endregion
    }
}