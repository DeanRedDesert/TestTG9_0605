//-----------------------------------------------------------------------
// <copyright file = "IPayvarRegistry.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface represents a theme registry object and is used to retrieve the information from
    /// the payvar registry file.
    /// </summary>
    public interface IPayvarRegistry
    {
        #region Properties

        /// <summary>
        /// Gets the custom configuration reader used to retrieve the custom configuration item values.
        /// </summary>
        ICustomConfigurationReader CustomConfiguration { get; }

        /// <summary>
        /// Gets a unique paytable identifier for this payvar.
        /// </summary>
        string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the paytable name of this payvar.
        /// </summary>
        string PaytableTagName { get; }

        /// <summary>
        /// Gets the paytable file name of this payvar.
        /// </summary>
        string PaytableTagFileName { get; }

        /// <summary>
        /// Gets the relative path to the theme registry which this payvar belongs to.
        /// </summary>
        string ThemeRegistryFileName { get; }

        /// <summary>
        /// Gets whether this payvar defines progressive levels.
        /// </summary>
        bool HasProgressive { get; }

        /// <summary>
        /// Gets whether the DoubleUp is supported by this payvar.
        /// </summary>
        bool DoubleUpSupported { get; }

        /// <summary>
        /// Gets the payvar registry type supported by this payvar.
        /// </summary>
        PayvarType PayvarRegistryType { get; }

        /// <summary>
        /// Gets the <see cref="IPayvarGroupRegistry"/> belonging to this payvar group if this payvar registry is a group
        /// template.
        /// </summary>
        IPayvarGroupRegistry PayvarGroupRegistry { get; }

        /// <summary>
        /// Gets the information on payback percentages.
        /// </summary>
        PaytablePaybackInfo PaybackInfo { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the list of denominations supported by this payvar.
        /// </summary>
        /// <returns>
        /// The list of denominations supported by this payvar.
        /// </returns>
        IEnumerable<long> GetSupportedDenominations();

        /// <summary>
        /// Gets all the configuration item values of this payvar.
        /// </summary>
        /// <returns>The configuration item values of this payvar, indexed by the configuration name.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when error occurs while retrieving values of the custom configuration items
        /// from the payvar registry.
        /// </exception>
        IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues();

        /// <summary>
        /// Gets the system progressives defined in this payvar registry.
        /// </summary>
        /// <returns>The system progressives defined in this payvar registry</returns>
        IEnumerable<ProgressiveLink> GetSystemProgressives();

        /// <summary>
        /// Gets the game controlled progressives defined in this payvar registry.
        /// </summary>
        /// <returns>The game controlled progressives defined in this payvar registry.</returns>
        IEnumerable<ProgressiveLink> GetGameControlledProgressives();

        /// <summary>
        /// Verify if the win level is valid in this payvar.
        /// </summary>
        /// <param name="winLevelIndex">The win level index.</param>
        /// <returns>True if the win level is valid; otherwise, false.</returns>
        bool IsValidWinLevel(int winLevelIndex);

        /// <summary>
        /// Gets whether a win level supports progressive.
        /// </summary>
        /// <param name="winLevelIndex">The index of the win level to verify.</param>
        /// <returns>True if the win level supports progressive; false, otherwise.</returns>
        bool IsWinLevelProgressiveSupport(int winLevelIndex);

        /// <summary>
        /// Gets the min bet for configuring the button panel for this payvar.
        /// </summary>
        /// <returns>
        /// The min bet for configuring the button panel for this payvar.
        /// Return 0 if the min bet for configuring the button panel is not defined in the payvar registry.
        /// </returns>
        long GetButtonPanelMinBet();

        /// <summary>
        /// Gets the min bet for configuring the button panel for the specified denomination in this payvar.
        /// </summary>
        /// <param name="denomination">The denomination.</param>
        /// <returns>
        /// The min bet for configuring the button panel for the specified denomination in this payvar.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the min bet for configuring the button panel for the specified denomination is not
        /// defined in the payvar registry.
        /// </exception>
        long GetButtonPanelMinBetPerDenomination(long denomination);

        /// <summary>
        /// Gets all the supported min bets for configuring the button panel for this payvar.
        /// </summary>
        /// <returns>
        /// The value pool of min bets supported for configuring the button panel for this payvar.
        /// Return null if the min bet for configuring the button panel is not defined in the payvar registry.
        /// </returns>
        IValuePool<long> GetSupportedButtonPanelMinBets();

        /// <summary>
        /// Gets the max bet for this payvar.
        /// </summary>
        /// <returns>The max bet for this payvar</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the max bet is not defined in the payvar registry.
        /// </exception>
        long GetMaxBet();

        /// <summary>
        /// Gets the max bet for the specified denomination in this payvar.
        /// </summary>
        /// <param name="denomination">The denomination.</param>
        /// <returns>The max bet for the specified denomination in this payvar</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the max bet for the specified denomination is not defined in the payvar registry.
        /// </exception>
        long GetMaxBetPerDenomination(long denomination);

        /// <summary>
        /// Gets all the supported max bets for this payvar.
        /// </summary>
        /// <returns>
        /// The value pool of max bets supported for this payvar.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the max bet is not defined in the payvar registry.
        /// </exception>
        IValuePool<long> GetSupportedMaxBets();

        #endregion
    }
}