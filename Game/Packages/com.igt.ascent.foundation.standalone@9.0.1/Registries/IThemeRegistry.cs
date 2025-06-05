//-----------------------------------------------------------------------
// <copyright file = "IThemeRegistry.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface represents a theme registry object and is used to retrieve the information from
    /// the theme registry file.
    /// </summary>
    public interface IThemeRegistry
    {
        #region Properties

        /// <summary>
        /// Gets the theme name.
        /// </summary>
        string ThemeName { get; }

        /// <summary>
        /// Gets the G2S theme ID.
        /// </summary>
        string G2SThemeId { get; }

        /// <summary>
        /// Gets the tag value.
        /// </summary>
        string Tag { get; }

        /// <summary>
        /// Gets a string indicating the tag data file.
        /// </summary>
        string TagDataFile { get; }

        /// <summary>
        /// Gets the custom configuration reader used to retrieve the custom configuration item values.
        /// </summary>
        ICustomConfigurationReader CustomConfiguration { get; }

        /// <summary>
        /// Gets the max bet resolution for this theme.
        /// </summary>
        MaxBetResolution MaxBetResolution { get; }

        /// <summary>
        /// Gets the maximum number of the enabled denominations. 
        /// </summary>
        int? MaxNumberOfEnabledDenominations { get; }

        /// <summary>
        /// Gets whether this theme supports autoplay.
        /// </summary>
        bool AutoPlaySupported { get; }

        /// <summary>
        /// Gets whether this theme supports auto play confirmation.
        /// </summary>
        bool AutoPlayConfirmationSupported { get; }

        /// <summary>
        /// Gets whether a portrait top monitor is required for this theme.
        /// </summary>
        bool TopMonitorPortraitRequired { get; }

        /// <summary>
        /// Get whether this theme supports round wager up playoff.
        /// </summary>
        bool RoundWagerUpPlayoffSupported { get; }

        /// <summary>
        /// Get whether this theme supports utility mode.
        /// </summary>
        bool UtilityModeSupported { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all the configuration item values of this theme.
        /// </summary>
        /// <returns>The configuration item values of this theme, indexed by the configuration name.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when error occurs while retrieving values of the custom configuration items from the theme registry.
        /// </exception>
        IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues();

        /// <summary>
        /// Gets the min bet for configuring the button panel for this theme.
        /// </summary>
        /// <returns>
        /// The min bet for configuring the button panel for this theme.
        /// Return 0 if the min bet for configuring the button panel is not defined in the theme registry.
        /// </returns>
        long GetButtonPanelMinBet();

        /// <summary>
        /// Gets all the supported min bet for configuring the button panel for this theme.
        /// </summary>
        /// <returns>
        /// The value pool of min bet supported for configuring the button panel for this theme.
        /// Return null if the min bet for configuring the button panel is not defined in the theme registry.
        /// </returns>
        IValuePool<long> GetSupportedButtonPanelMinBets();

        /// <summary>
        /// Gets the max bet for this theme.
        /// </summary>
        /// <returns>The max bet for this theme.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the max bet is not defined in the theme registry.
        /// </exception>
        long GetMaxBet();

        /// <summary>
        /// Gets all the supported max bet for this theme.
        /// </summary>
        /// <returns>
        /// The value pool of max bet supported for this theme.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the max bet is not defined in the theme registry.
        /// </exception>
        IValuePool<long> GetSupportedMaxBets();

        /// <summary>
        /// Gets the max bet button behavior for the this theme.
        /// </summary>
        /// <returns>
        /// The max bet button behavior for the current theme context.
        /// BetMaxCreditsOnly if it is not specified in the registry.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the specified behavior is not supported.
        /// </exception>
        MaxBetButtonBehavior GetMaxBetButtonBehavior();

        /// <summary>
        /// Gets the default line selection mode for this theme.
        /// </summary>
        /// <returns>The default line selection mode for the given registry.</returns>
        LineSelectionMode GetDefaultLineSelectionMode();

        /// <summary>
        /// Gets the list of cultures supported by this theme.
        /// </summary>
        /// <returns></returns>
        ICollection<string> GetSupportedCultures();

        /// <summary>
        /// Checks if an imported extension requested by this theme is required given its <paramref name="extensionId"/>.
        /// </summary>
        /// <param name="extensionId">
        /// The extension id that identifies the extension.
        /// </param>
        /// <returns>
        /// True if the extension requested by this theme is required, and false otherwise.
        /// </returns>
        bool IsExtensionImportRequired(Guid extensionId);

        /// <summary>
        /// Gets a list of imported extension identifiers requested by this theme.
        /// </summary>
        /// <returns>
        /// A list of imported extension identifiers requested by this theme.
        /// An empty list is returned if no extension is requested.
        /// </returns>
        IList<Guid> GetRequiredExtensionImportIdentifiers();

        /// <summary>
        /// Gets a collection of <see cref="ExtensionImport"/> requested by this theme.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="ExtensionImport"/> requested by this theme.
        /// An empty collection is returned if no extension is requested.
        /// </returns>
        IEnumerable<IExtensionImport> GetExtensionImports();

        #endregion
    }
}