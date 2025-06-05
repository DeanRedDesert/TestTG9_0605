//-----------------------------------------------------------------------
// <copyright file = "GameRegistryManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using InterfaceExtensions.Interfaces;
    using Registries;

    /// <summary>
    /// This class manages the game registries. 
    /// </summary>
    internal sealed class GameRegistryManager : IGameRegistryInfoProvider, 
                                                IStandaloneRegistryInformationDependency,
                                                IRegistryIndexer
    {
        #region Constants

        /// <summary>
        /// Constant indicating that no game registries are being used.
        /// Common usage is to be returned as an invalid payvar registry id.
        /// </summary>
        public const int NoRegistryInUse = -1;

        #endregion

        #region Fields

        /// <summary>
        /// The list of theme registries for the game.
        /// </summary>
        private readonly IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries;

        /// <summary>
        /// A flag indicating whether the game registries are being used.
        /// </summary>
        private readonly bool useGameRegistries;

        /// <summary>
        /// The theme registry for the current theme context.
        /// </summary>
        private IThemeRegistry currentTheme;

        /// <summary>
        /// The payvar registry for the current theme context.
        /// </summary>
        private IPayvarRegistry currentPayvar;

        /// <summary>
        /// A collection of extension registries the game supports.
        /// </summary>
        private readonly IList<IExtensionRegistry> allExtensionRegistries;

        /// <summary>
        /// The configuration extension manager is used to load configuration extension registries,
        /// validate that clients are not directly linking to a configuration provider, and indicate the safe storage section where
        /// the configuration items are stored.
        /// </summary>
        private readonly IConfigurationExtensionManager configurationExtensionManager;

        /// <summary>
        /// The object in charge of finding out the index of a registry.
        /// </summary>
        private readonly IRegistryIndexer registryIndexer;

        #endregion

        #region Properties

        /// <summary>
        /// Get the mount point of the game package.
        /// </summary>
        /// <remarks>
        /// When running with Standalone Game Lib, assuming
        /// that the game is always launched from its root
        /// directory.
        /// </remarks>
        /// <DevDoc>
        /// AppDomain.CurrentDomain.BaseDirectory does not
        /// work with Unity.
        /// </DevDoc>
        public string GameMountPoint { get; }

        /// <summary>
        /// Gets the information on imported extensions linked to the application.
        /// </summary>
        /// <remarks>
        /// If the imported extension registry is not available, then returns an empty collection.
        /// </remarks>
        public IExtensionImportCollection ExtensionImportCollection
        {
            get
            {
                List<IExtensionImport> extensionImports;

                if(useGameRegistries)
                {
                    extensionImports = allExtensionRegistries.
                        Select(extensionRegistry => extensionRegistry.GetExtensionImport(currentTheme)).
                        Where(extensionImport => extensionImport != null).
                        ToList();
                    var importIdentifiers = extensionImports.Select(import => import.ExtensionIdentifier);
                    var missingImport =
                        currentTheme.GetRequiredExtensionImportIdentifiers().
                            Except(importIdentifiers).
                            FirstOrDefault();
                    if(missingImport != Guid.Empty)
                    {
                        throw new InvalidOperationException(
                            $"The extension {missingImport} is required.");
                    }
                }
                else
                {
                    extensionImports = new List<IExtensionImport>();
                }

                return new ExtensionImportCollection(extensionImports);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new instance of GameRegistryManager using a disk store
        /// manager to write configurations, and a flag indicating whether to
        /// discover game registries for the game.
        /// </summary>
        /// <param name="useGameRegistries">
        /// A flag indicating whether to discover game registries for the game.
        /// </param>
        /// <param name="gameMountPoint">The mount point for the game.</param>
        public GameRegistryManager(bool useGameRegistries, string gameMountPoint)
        {
            GameMountPoint = gameMountPoint;
            this.useGameRegistries = useGameRegistries;

            if(useGameRegistries)
            {
                registries = RegistryLoader.Load(gameMountPoint);

                allExtensionRegistries = RegistryLoader.LoadImportedExtensionRegistriesFromDirectory(gameMountPoint).ToList();

                configurationExtensionManager = new ConfigurationExtensionManager(allExtensionRegistries);

                foreach(var themeRegistry in registries.Keys)
                {
                    configurationExtensionManager.VerifyConfigurationExtensionsInExtensionImportList(themeRegistry);
                }

                registryIndexer = new RegistryIndexer(registries, configurationExtensionManager);
            }
        }

        #endregion

        #region IGameRegistryInfoProvider Members

        /// <inheritdoc />
        public IList<string> GetThemeList()
        {
            return useGameRegistries
                ? registries.Keys.Select(registry => registry.G2SThemeId).ToList()
                : null;
        }

        /// <inheritdoc />
        public MaxBetResolution GetBetResolution(string themeIdentifier)
        {
            var result = MaxBetResolution.Unknown;

            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);

                result = themeRegistry.MaxBetResolution;
            }
            return result;
        }

        /// <inheritdoc />
        public int GetMaxNumberOfEnabledDenominations(string themeIdentifier)
        {
            var result = NoRegistryInUse;

            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                if(themeRegistry.MaxNumberOfEnabledDenominations.HasValue)
                {
                    result = themeRegistry.MaxNumberOfEnabledDenominations.Value;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IDictionary<PaytableVariant, IEnumerable<long>> GetSupportedDenominations(string themeIdentifier)
        {
            Dictionary<PaytableVariant, IEnumerable<long>> result = null;
            if(useGameRegistries)
            {
                result = new Dictionary<PaytableVariant, IEnumerable<long>>();
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                foreach(var payvar in registries[themeRegistry])
                {
                    var paytableVariant = new PaytableVariant(themeIdentifier, payvar.PaytableTagName, payvar.PaytableTagFileName);
                    result.Add(paytableVariant, payvar.GetSupportedDenominations());
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IValuePool<long> GetSupportedMaxBets(PaytableVariant paytableVariant)
        {
            IValuePool<long> result = null;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(paytableVariant.ThemeIdentifier);
                switch(themeRegistry.MaxBetResolution)
                {
                    case MaxBetResolution.PerTheme:
                        result = themeRegistry.GetSupportedMaxBets();
                        break;
                    case MaxBetResolution.PerPayvar:
                    case MaxBetResolution.PerPayvarDenomination:
                        var payvarRegistry = registries[themeRegistry].FirstOrDefault(pv =>
                            pv.PaytableTagName == paytableVariant.PaytableName &&
                            Path.GetFileName(pv.PaytableTagFileName) == Path.GetFileName(paytableVariant.PaytableFileName));
                        if(payvarRegistry != null)
                        {
                            result = payvarRegistry.GetSupportedMaxBets();
                        }
                        break;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IValuePool<long> GetSupportedButtonPanelMinBets(PaytableVariant paytableVariant)
        {
            IValuePool<long> result = null;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(paytableVariant.ThemeIdentifier);
                switch(themeRegistry.MaxBetResolution)
                {
                    case MaxBetResolution.PerTheme:
                        result = themeRegistry.GetSupportedButtonPanelMinBets();
                        break;
                    case MaxBetResolution.PerPayvar:
                    case MaxBetResolution.PerPayvarDenomination:
                        var payvarRegistry = registries[themeRegistry].FirstOrDefault(pv =>
                            pv.PaytableTagName == paytableVariant.PaytableName &&
                            Path.GetFileName(pv.PaytableTagFileName) == Path.GetFileName(paytableVariant.PaytableFileName));
                        if(payvarRegistry != null)
                        {
                            result = payvarRegistry.GetSupportedButtonPanelMinBets();
                        }
                        break;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public long GetMaxBet(PaytableVariant paytableVariant, long denom)
        {
            long result = NoRegistryInUse;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(paytableVariant.ThemeIdentifier);
                switch(themeRegistry.MaxBetResolution)
                {
                    case MaxBetResolution.PerTheme:
                        result = themeRegistry.GetMaxBet();
                        break;
                    case MaxBetResolution.PerPayvar:
                        var payvarRegistry = GetPayvarRegistry(paytableVariant);
                        result = payvarRegistry.GetMaxBet();
                        break;
                    case MaxBetResolution.PerPayvarDenomination:
                        payvarRegistry = GetPayvarRegistry(paytableVariant);
                        result = payvarRegistry.GetMaxBetPerDenomination(denom);
                        break;
                }
            }
            return result;
        }

        /// <inheritdoc />
        public long GetButtonPanelMinBet(PaytableVariant paytableVariant, long denom)
        {
            long result = NoRegistryInUse;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(paytableVariant.ThemeIdentifier);
                switch(themeRegistry.MaxBetResolution)
                {
                    case MaxBetResolution.PerTheme:
                        result = themeRegistry.GetButtonPanelMinBet();
                        break;
                    case MaxBetResolution.PerPayvar:
                        var payvarRegistry = GetPayvarRegistry(paytableVariant);
                        result = payvarRegistry.GetButtonPanelMinBet();
                        break;
                    case MaxBetResolution.PerPayvarDenomination:
                        payvarRegistry = GetPayvarRegistry(paytableVariant);
                        result = payvarRegistry.GetButtonPanelMinBetPerDenomination(denom);
                        break;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public void GetProgressiveSetups(out IDictionary<PaytableVariant, IList<ProgressiveLink>> systemProgressives,
            out IDictionary<PaytableVariant, IList<ProgressiveLink>> gameControlledProgressives)
        {
            systemProgressives = null;
            gameControlledProgressives = null;

            if(useGameRegistries)
            {
                systemProgressives = new Dictionary<PaytableVariant, IList<ProgressiveLink>>();
                gameControlledProgressives = new Dictionary<PaytableVariant, IList<ProgressiveLink>>();

                foreach(var themeRegistry in registries.Keys)
                {
                    foreach(var payvarRegistry in registries[themeRegistry].Where(payvar => payvar.HasProgressive))
                    {
                        var paytableVariant = new PaytableVariant(themeRegistry.G2SThemeId,
                            payvarRegistry.PaytableTagName,
                            payvarRegistry.PaytableTagFileName);
                        var systemMappingList = payvarRegistry.GetSystemProgressives().ToList();
                        var gameMappingList = payvarRegistry.GetGameControlledProgressives().ToList();
                        if(systemMappingList.Count > 0)
                        {
                            systemProgressives.Add(paytableVariant, systemMappingList);
                        }
                        if(gameMappingList.Count > 0)
                        {
                            gameControlledProgressives.Add(paytableVariant, gameMappingList);
                        }
                    }
                }
            }
        }

        #endregion

        #region IStandaloneRegistryInformationDependency

        /// <inheritdoc/>
        ReadOnlyCollection<IPayvarInfo> IStandaloneRegistryInformationDependency.GetPayvarsInCurrentGroup()
        {
            IList<IPayvarInfo> result;

            if(useGameRegistries &&
               currentPayvar != null &&
               (currentPayvar.PayvarRegistryType == PayvarType.PayvarGroupTemplate ||
                currentPayvar.PayvarRegistryType == PayvarType.SingleMultiTemplate))
            {
                result = currentPayvar.PayvarGroupRegistry.Payvars.Cast<IPayvarInfo>().ToList();
            }
            else
            {
                result = new List<IPayvarInfo>();
            }

            return new ReadOnlyCollection<IPayvarInfo>(result);
        }

        #endregion

        #region IRegistryIndexer Implementation

        /// <inheritdoc />
        public int GetThemeIndex(string identifier)
        {
            return useGameRegistries
                       ? registryIndexer.GetThemeIndex(identifier)
                       : -1;
        }

        /// <inheritdoc />
        public int GetPayvarIndex(string identifier)
        {
            return useGameRegistries
                       ? registryIndexer.GetPayvarIndex(identifier)
                       : -1;
        }

        /// <inheritdoc />
        public int GetExtensionIndex(string identifier)
        {
            return useGameRegistries
                       ? registryIndexer.GetExtensionIndex(identifier)
                       : -1;
        }

        #endregion

        #region Registry Management

        /// <summary>
        /// Write the configuration items defined in the game registries into the safe storage.
        /// </summary>
        public void WriteConfigurationItems(IDiskStoreManager diskStoreManager, string jurisdiction)
        {
            if(diskStoreManager == null)
            {
                throw new ArgumentNullException(nameof(diskStoreManager));

            }

            if(useGameRegistries)
            {
                var diskStoreSectionIndexer = new DiskStoreSectionIndexer(registryIndexer);

                foreach(var entry in registries)
                {
                    var themeRegistry = entry.Key;
                    WriteConfigurationItems(diskStoreManager,
                                            diskStoreSectionIndexer,
                                            ConfigurationScope.Theme,
                                            themeRegistry.G2SThemeId,
                                            themeRegistry.GetConfigurationItemValues());

                    var payvarRegistries = entry.Value;
                    foreach(var payvarRegistry in payvarRegistries)
                    {
                        WriteConfigurationItems(diskStoreManager,
                                                diskStoreSectionIndexer,
                                                ConfigurationScope.Payvar,
                                                payvarRegistry.PaytableIdentifier,
                                                payvarRegistry.GetConfigurationItemValues());
                    }
                }

                foreach(var extensionGuid in configurationExtensionManager.LinkedIdentifiers)
                {
                    WriteConfigurationItems(diskStoreManager,
                                            diskStoreSectionIndexer,
                                            ConfigurationScope.Extension,
                                            extensionGuid.ToString(),
                                            configurationExtensionManager.GetConfigurationItemValues(extensionGuid, jurisdiction));
                }
            }
        }

        /// <summary>
        /// Update the reference to the theme registry according to the new theme selection.
        /// </summary>
        public void UpdateThemeSelection(string themeIdentifier)
        {
            if(useGameRegistries)
            {
                currentTheme = GetThemeRegistry(themeIdentifier);
            }
        }

        public string UpdatePayvarSelection(PaytableVariant paytableVariant)
        {
            var result = string.Empty;

            if(useGameRegistries)
            {
                currentPayvar = GetPayvarRegistry(paytableVariant);
                result = currentPayvar.PaytableIdentifier;
            }

            return result;
        }

        #endregion

        #region Registry Information Services

        /// <summary>
        /// Get the resource for a given theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>The resource for the theme. Return null if no game registries are being used.</returns>
        public ThemeResource GetThemeResource(string themeIdentifier)
        {
            ThemeResource result = null;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                result = new ThemeResource
                {
                    GameMountPoint = GameMountPoint,
                    Tag = themeRegistry.Tag,
                    TagDataFile = themeRegistry.TagDataFile
                };
            }
            return result;
        }

        /// <summary>
        /// Validate that the specified win level exists.
        /// </summary>
        /// <param name="winLevelIndex">The win level to check for.</param>
        /// <param name="mustSupportProgressives">
        /// If true, validates that the win level supports progressive awards.
        /// </param>
        /// <exception cref="InvalidWinLevelException">
        /// Thrown if the win level doesn't exist.
        /// Thrown if <paramref name="mustSupportProgressives"/> is true and the
        /// win level does not support progressives.
        /// </exception>
        public void ValidateWinLevel(int winLevelIndex, bool mustSupportProgressives)
        {
            if(currentPayvar != null)
            {
                if(!currentPayvar.IsValidWinLevel(winLevelIndex))
                {
                    throw new InvalidWinLevelException($"Win level {winLevelIndex} does not exist in the paytable.");
                }

                if(mustSupportProgressives && !currentPayvar.IsWinLevelProgressiveSupport(winLevelIndex))
                {
                    throw new InvalidWinLevelException($"Win level {winLevelIndex} does not support progressives.");
                }
            }
        }

        /// <summary>
        /// Get the max bet button behavior for the current theme context.
        /// </summary>
        /// <returns>
        /// The max bet button behavior for the current theme context.
        /// Return BetMaxCreditsOnly if it is not specified in the registry.
        /// </returns>
        public MaxBetButtonBehavior GetMaxBetButtonBehavior()
        {
            // Default to BetMaxCreditsOnly if it is not specified in the registry.
            var result = MaxBetButtonBehavior.BetMaxCreditsOnly;
            if(useGameRegistries)
            {
                result = currentTheme.GetMaxBetButtonBehavior();
            }

            return result;
        }

        /// <summary>
        /// Get whether a portrait top monitor is required for this theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// True if the specified theme requires a portrait top monitor, false otherwise.
        /// Return false if the game registries are not being used.
        /// </returns>
        public bool GetTopMonitorPortraitRequired(string themeIdentifier)
        {
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                return themeRegistry.TopMonitorPortraitRequired;
            }
            return false;
        }

        /// <summary>
        /// Get whether a payvar in a given theme supports double up.
        /// </summary>
        /// <param name="paytableVariant">The <see cref="PaytableVariant"/> instance.
        /// </param>
        /// <returns>
        /// True if double up is supported; false, otherwise.
        /// Return false if no game registries are being used.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when no registry is found by the specified paytable variant.
        /// </exception>
        public bool GetDoubleUpSupported(PaytableVariant paytableVariant)
        {
            var result = false;
            if (useGameRegistries && paytableVariant != default)
            {
                var themeRegistry = GetThemeRegistry(paytableVariant.ThemeIdentifier);
                var payvarRegistry = registries[themeRegistry].FirstOrDefault(payvar =>
                    payvar.PaytableTagName == paytableVariant.PaytableName &&
                    Path.GetFileName(payvar.PaytableTagFileName) == Path.GetFileName(paytableVariant.PaytableFileName));
                if (payvarRegistry == null)
                {
                    throw new GameRegistryException(
                        $"No registry is found for paytable '{paytableVariant.PaytableName}' and paytable file '{paytableVariant.PaytableFileName}' for theme {paytableVariant.ThemeIdentifier}.");
                }
                result = payvarRegistry.DoubleUpSupported;
            }

            return result;
        }

        /// <summary>
        /// Get whether the theme supports autoplay.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// True if the autoplay is supported; false, otherwise.
        /// Return false if no game registries are being used.
        /// </returns>
        public bool GetAutoPlaySupported(string themeIdentifier)
        {
            var result = false;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                result = themeRegistry.AutoPlaySupported;
            }
            return result;
        }

        /// <summary>
        /// Get whether the theme supports auto play confirmation.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// True if the theme supports auto play confirmation; false otherwise.
        /// Return false if the game registries are not being used.
        /// </returns>
        public bool GetAutoPlayConfirmationSupported(string themeIdentifier)
        {
            var result = false;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                result = themeRegistry.AutoPlayConfirmationSupported;
            }
            return result;
        }

        /// <summary>
        /// Get the list of cultures supported by the specified theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// A list of cultures supported by the specified theme. If registries are not in use, then null
        /// will be returned.
        /// </returns>
        public ICollection<string> GetSupportedCultures(string themeIdentifier)
        {
            ICollection<string> cultures = null;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                cultures = themeRegistry.GetSupportedCultures();
            }
            return cultures;
        }

        /// <summary>
        /// Get the G2S theme identifier for the specified theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme name.</param>
        /// <returns>The G2S theme identifier for the specified theme.</returns>
        public string GetG2SThemeId(string themeIdentifier)
        {
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                return themeRegistry.G2SThemeId;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get whether the specific theme supports round wager up playoff.
        /// </summary>
        /// <param name="themeIdentifier">Specify the theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// True if round wager up playoff is supported; false, otherwise.
        /// Return false if no game registries are being used.
        /// </returns>
        public bool GetRoundWagerUpPlayoffSupported(string themeIdentifier)
        {
            var result = false;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                result = themeRegistry.RoundWagerUpPlayoffSupported;
            }
            return result;
        }

        /// <summary>
        /// Get whether the specific theme supports utility mode.
        /// </summary>
        /// <param name="themeIdentifier">Specify the theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// True if utility mode is supported; false, otherwise.
        /// Return true if no game registries are being used.
        /// </returns>
        public bool GetUtilityModeSupported(string themeIdentifier)
        {
            var result = true;
            if(useGameRegistries)
            {
                var themeRegistry = GetThemeRegistry(themeIdentifier);
                result = themeRegistry.UtilityModeSupported;
            }
            return result;
        }

        /// <summary>
        /// Get the payvar type from the <paramref name="paytableVariant" />.
        /// </summary>
        /// <param name="paytableVariant">The <see cref="PaytableVariant"/> instance.</param>
        /// <returns>
        /// The payvar type from the <paramref name="paytableVariant" /> if registries are in use, otherwise
        /// the <see cref="PayvarType.Standard"/> is returned.
        /// </returns>
        public PayvarType GetPayvarType(PaytableVariant paytableVariant)
        {
            var result = default(PayvarType);

            if(useGameRegistries && paytableVariant != default)
            {
                var payvarRegistry = GetPayvarRegistry(paytableVariant);
                result = payvarRegistry.PayvarRegistryType;
            }
            return result;
        }

        /// <summary>
        /// Gets the line selection mode specified by the theme.
        /// </summary>
        /// <returns>The specified line selection mode.</returns>
        /// <remarks> Will return <see cref="LineSelectionMode.Undefined"/> if not specified in the registry.</remarks>
        public LineSelectionMode GetDefaultLineSelectionMode()
        {
            // Default to Undefined if it is not specified in the registry.
            var result = LineSelectionMode.Undefined;
            if(useGameRegistries)
            {
                result = currentTheme.GetDefaultLineSelectionMode();
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the theme registry by the theme identifier.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier.</param>
        /// <returns>The theme registry.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the theme registry is not found by the specified theme name
        /// </exception>
        private IThemeRegistry GetThemeRegistry(string themeIdentifier)
        {
            var result = registries.Keys.FirstOrDefault(r => r.G2SThemeId == themeIdentifier);

            if(result == null)
            {
                throw new GameRegistryException(
                    $"No theme registry is found by the identifier \"{themeIdentifier}\".");
            }

            return result;
        }

        /// <summary>
        /// Get the payvar registry by the info specified by <paramref name="paytableVariant"/>.
        /// </summary>
        /// <param name="paytableVariant">The paytable variant.</param>
        /// <returns>The payvar registry found.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the payvar registry is not found by the specified paytable variant.
        /// </exception>
        private IPayvarRegistry GetPayvarRegistry(PaytableVariant paytableVariant)
        {
            // Empty theme identifier in paytable variant means to use current theme.
            var themeRegistry = string.IsNullOrEmpty(paytableVariant.ThemeIdentifier)
                                    ? currentTheme
                                    : GetThemeRegistry(paytableVariant.ThemeIdentifier);

            var payvarRegistry = registries[themeRegistry]
                .FirstOrDefault(
                    payvar => payvar.PaytableTagName == paytableVariant.PaytableName &&
                              Path.GetFileName(payvar.PaytableTagFileName) == Path.GetFileName(paytableVariant.PaytableFileName));

            if(payvarRegistry == null)
            {
                throw new GameRegistryException($"No payvar registry is found for {paytableVariant}");
            }

            return payvarRegistry;
        }

        /// <summary>
        /// Write the list of configuration items into the safe storage.
        /// </summary>
        private static void WriteConfigurationItems(IDiskStoreManager diskStoreManager,
                                                    DiskStoreSectionIndexer diskStoreSectionIndexer,
                                                    ConfigurationScope configurationScope,
                                                    string identifier,
                                                    IEnumerable<KeyValuePair<string, KeyValuePair<ConfigurationProfile, object>>> configItems)
        {
            if(configItems == null)
            {
                return;
            }

            foreach(var configItem in configItems)
            {
                var (configSection, configIndex) = diskStoreSectionIndexer.GetConfigurationLocation(configurationScope, identifier);
                var (profileSection, profileIndex) = diskStoreSectionIndexer.GetConfigurationProfileLocation(configurationScope, identifier);

                var configName = configItem.Key;
                var data = configItem.Value.Value;
                var profile = configItem.Value.Key;

                diskStoreManager.Write(configSection, configIndex, configName, data);
                diskStoreManager.Write(profileSection, profileIndex, configName, profile);
            }
        }

        #endregion
    }
}
