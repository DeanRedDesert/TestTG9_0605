//-----------------------------------------------------------------------
// <copyright file = "IGameRegistryInfoProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System.Collections.Generic;
    using Registries;

    /// <summary>
    /// This interface is used for providing the information retrieved from the
    /// game registries so that other internal components in the standalone
    /// Game Lib can use the information to validate their own configurations
    /// and settings.
    /// </summary>
    /// <remarks>
    /// Note that APIs used by GameLib only should not be placed in this interface.
    /// GameLib has full access to GameRegistryManger, and does not use this interface.
    /// Limiting the number of APIs of this interface is helpful for maintaining
    /// good encapsulations of the internal components.
    /// The internal components should not be able to retrieve more information
    /// from GameRegistryManager than what they must have to know.
    /// </remarks>
    internal interface IGameRegistryInfoProvider
    {
        /// <summary>
        /// Get the list of theme names specified in the theme registries.
        /// </summary>
        /// <returns>
        /// The list of theme names.
        /// </returns>
        IList<string> GetThemeList();

        /// <summary>
        /// Get the bet resolution defined in theme registry.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>The bet resolution defined in the theme registry.</returns>
        MaxBetResolution GetBetResolution(string themeIdentifier);

        /// <summary>
        /// Get the maximum number of denominations that can be enabled for a given theme.
        /// It is defined in the theme registry.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// The maximum number of denominations that can be enabled for the given theme.
        /// <see cref="GameRegistryManager.NoRegistryInUse"/> if no game registries are
        /// being used, or the corresponding element is not present in the theme registry.
        /// </returns>
        int GetMaxNumberOfEnabledDenominations(string themeIdentifier);

        /// <summary>
        /// Get the list of denominations supported by each payvar in a given theme.
        /// They are defined in the payvar registries.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// List of denominations supported by each payvar in the given theme.
        /// Null if no game registries are being used.
        /// </returns>
        IDictionary<PaytableVariant, IEnumerable<long>> GetSupportedDenominations(string themeIdentifier);

        /// <summary>
        /// Get the value pool of max bets supported by the game registry corresponding to the
        /// specific paytable variant. Depending on bet resolution defined in theme registry,
        /// the value pool of max bets are may be defined in theme registry or payvar registry.
        /// </summary>
        /// <param name="paytableVariant">
        /// Specify the paytable variant to get the value pool of max bets.
        /// </param>
        /// <returns>
        /// Value pool of max bets supported by corresponding game registry.
        /// Null if no game registries are being used.
        /// </returns>
        IValuePool<long> GetSupportedMaxBets(PaytableVariant paytableVariant);

        /// <summary>
        /// Get the value pool of button panel min bets supported by the game registry corresponding to the
        /// specific paytable variant. Depending on bet resolution defined in theme registry,
        /// the value pool of button panel min bets are may be defined in theme registry or payvar registry.
        /// </summary>
        /// <param name="paytableVariant">
        /// Specify the paytable variant to get the value pool of button panel min bets.
        /// </param>
        /// <returns>
        /// Value pool of button panel min bets supported by corresponding game registry.
        /// Null if no game registries are being used.
        /// </returns>
        IValuePool<long> GetSupportedButtonPanelMinBets(PaytableVariant paytableVariant);

        /// <summary>
        /// Get the max bet for the current theme context from the theme or
        /// payvar registries depending on the bet limit resolution.
        /// </summary>
        /// <param name="paytableVariant">
        /// Specify the paytable variant to get the value of max bet.
        /// </param>
        /// <param name="denom">
        /// Specify the denom associated to the paytable variant.
        /// </param>
        /// <returns>
        /// The max bet for the specific game registry.
        /// Return <see cref="GameRegistryManager.NoRegistryInUse"/> if no game registries are being used.
        /// </returns>
        long GetMaxBet(PaytableVariant paytableVariant, long denom);

        /// <summary>
        /// Get the button panel min bet for configuring the button panel for
        /// the current theme context from the theme or
        /// payvar registries depending on the bet limit resolution.
        /// </summary>
        /// <param name="paytableVariant">
        /// Specify the paytable variant to get the value of button panel min bet.
        /// </param>
        /// <param name="denom">
        /// Specify the denom associated to the paytable variant.
        /// </param>
        /// <returns>
        /// The button panel min bet for the specific game registry.
        /// Return <see cref="GameRegistryManager.NoRegistryInUse"/> if no game registries are being used.
        /// </returns>
        long GetButtonPanelMinBet(PaytableVariant paytableVariant, long denom);

        /// <summary>
        /// Gets the list of hard coded system and game controlled progressive links
        /// for each payvar. They are defined in the payvar registries.
        /// </summary>
        /// <param name="systemProgressives">
        /// The list of hard coded system controlled progressive links for each payvar.
        /// Null if no game registries are being used.
        /// </param>
        /// <param name="gameControlledProgressives">
        /// The list of game controlled progressive links for each payvar.
        /// Null if no game registries are being used.
        /// </param>
        void GetProgressiveSetups(out IDictionary<PaytableVariant, IList<ProgressiveLink>> systemProgressives, out IDictionary<PaytableVariant, IList<ProgressiveLink>> gameControlledProgressives);

        // Do not add APIs that are used only by GameLib.
        // See remarks above for more information.
    }
}
