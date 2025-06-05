//-----------------------------------------------------------------------
// <copyright file = "ThemeRegistryProxy.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using F2LThemeVerTip = Core.Registries.Internal.F2L.F2LThemeRegistryVer5;

    /// <summary>
    /// A proxy for the <see cref="F2LThemeVerTip.ThemeRegistry"/> object that is used to
    /// retrieve information from the tip version of the theme registry.
    /// </summary>
    internal class ThemeRegistryProxy : IThemeRegistry
    {
        #region Fields

        /// <summary>
        /// Gets the theme registry that this proxy represents.
        /// </summary>
        private readonly F2LThemeVerTip.ThemeRegistry themeRegistry;

        /// <summary>
        /// The custom configuration reader used to retrieve the values of the configuration items.
        /// </summary>
        private readonly CustomConfigurationReader customConfigReader;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the proxy instance with the theme name, theme ID and the theme registry object.
        /// </summary>
        /// <param name="themeRegistry">The theme registry object.</param>
        /// <param name="themeName">The theme name of this theme registry.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="themeRegistry"/> or <paramref name="themeName"/> is null.
        /// </exception>
        public ThemeRegistryProxy(F2LThemeVerTip.ThemeRegistry themeRegistry, string themeName)
        {
            this.themeRegistry = themeRegistry ?? throw new ArgumentNullException(nameof(themeRegistry), "Parameters may not be null.");
            ThemeName = themeName ?? throw new ArgumentNullException(nameof(themeName), "Parameters may not be null.");

            customConfigReader = new CustomConfigurationReader(themeRegistry.CustomConfigItems?.ConfigItem);
        }

        #endregion

        #region IThemeRegisry Members

        /// <inheritDoc />
        public string ThemeName { get; }

        /// <inheritDoc />
        public string G2SThemeId => themeRegistry.G2SThemeId;

        /// <inheritDoc />
        public string Tag => themeRegistry.TagDataFile.Tag;

        /// <inheritDoc />
        public string TagDataFile => themeRegistry.TagDataFile.Value;

        /// <inheritDoc />
        public ICustomConfigurationReader CustomConfiguration => customConfigReader;

        /// <inheritDoc />
        public MaxBetResolution MaxBetResolution
        {
            get
            {
                MaxBetResolution result;
                switch(themeRegistry.MaxBetResolution)
                {
                    case F2LThemeVerTip.ThemeRegistryMaxBetResolution.PerTheme:
                        result = MaxBetResolution.PerTheme;
                        break;
                    case F2LThemeVerTip.ThemeRegistryMaxBetResolution.PerPayvar:
                        result = MaxBetResolution.PerPayvar;
                        break;
                    case F2LThemeVerTip.ThemeRegistryMaxBetResolution.PerPayvarDenomination:
                        result = MaxBetResolution.PerPayvarDenomination;
                        break;
                    default:
                        throw new GameRegistryException("Unsupported max bet resolution.");
                }
                return result;
            }
        }

        /// <inheritDoc />
        public int? MaxNumberOfEnabledDenominations
        {
            get
            {
                if(themeRegistry.MaxNumberOfEnabledDenominationsSpecified)
                {
                    return themeRegistry.MaxNumberOfEnabledDenominations;
                }

                return null;
            }
        }

        /// <inheritDoc />
        public bool AutoPlaySupported =>
            themeRegistry.AutoPlaySupportedSpecified &&
            themeRegistry.AutoPlaySupported;

        /// <inheritDoc />
        public bool AutoPlayConfirmationSupported =>
            themeRegistry.AutoPlayConfirmationSupportedSpecified &&
            themeRegistry.AutoPlayConfirmationSupported;

        /// <inheritDoc />
        public bool TopMonitorPortraitRequired =>
            themeRegistry.HardwareRequirements?.TopMonitorPortraitRequiredSpecified == true &&
            themeRegistry.HardwareRequirements.TopMonitorPortraitRequired;

        /// <inheritDoc />
        public bool RoundWagerUpPlayoffSupported =>
            themeRegistry.RoundWagerUpPlayoffSupportedSpecified &&
            themeRegistry.RoundWagerUpPlayoffSupported;

        /// <inheritdoc />
        /// <devdoc>Default value is true if element not present.</devdoc>
        public bool UtilityModeSupported =>
            !themeRegistry.UtilityModeSupportedSpecified ||
            (themeRegistry.UtilityModeSupportedSpecified && themeRegistry.UtilityModeSupported);

        /// <inheritDoc />
        public IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues()
        {
            Dictionary<string, KeyValuePair<ConfigurationProfile, object>> results;
            try
            {
                results = (Dictionary<string, KeyValuePair<ConfigurationProfile, object>>)customConfigReader.GetConfigurationItems();
            }
            catch(GameRegistryException exception)
            {
                throw new GameRegistryException(
                    $"Error occurred while reading the theme registry --> \"{ThemeName}\"",
                    exception);
            }

            return results;
        }

        /// <inheritDoc />
        public long GetButtonPanelMinBet()
        {
            long result = 0;
            if(themeRegistry.ButtonPanelMinBet != null)
            {
                RegistryUtility.ValidateBetLimitValue(themeRegistry.ButtonPanelMinBet);
                checked
                {
                    result = (long)themeRegistry.ButtonPanelMinBet.Value[0].Value;
                }
            }
            return result;
        }

        /// <inheritDoc />
        public IValuePool<long> GetSupportedButtonPanelMinBets()
        {
            IValuePool<long> result = null;
            if(themeRegistry.ButtonPanelMinBet != null)
            {
                result = RegistryUtility.GetSupportedBetValuePool(themeRegistry.ButtonPanelMinBet.ValuePool.Item);
            }
            return result;
        }

        /// <inheritDoc />
        public long GetMaxBet()
        {
            if(themeRegistry.MaxBet == null)
            {
                throw new GameRegistryException(
                    "No theme level Max Bet is defined while the max bet resolution is Per Theme.");
            }
            RegistryUtility.ValidateBetLimitValue(themeRegistry.MaxBet);
            checked
            {
                return (long)themeRegistry.MaxBet.Value[0].Value;
            }
        }

        /// <inheritDoc />
        public IValuePool<long> GetSupportedMaxBets()
        {
            if(themeRegistry.MaxBet == null)
            {
                throw new GameRegistryException(
                    "No theme level Max Bet is defined while the max bet resolution is Per Theme.");
            }
            return RegistryUtility.GetSupportedBetValuePool(themeRegistry.MaxBet.ValuePool.Item);
        }

        /// <inheritDoc />
        public MaxBetButtonBehavior GetMaxBetButtonBehavior()
        {
            // Default to BetMaxCreditsOnly if it is not specified in the registry.
            var result = MaxBetButtonBehavior.BetMaxCreditsOnly;

            if(themeRegistry.MaxBetButtonBehaviorSupportedSpecified)
            {
                var behavior = themeRegistry.MaxBetButtonBehaviorSupported;
                switch(behavior)
                {
                    case F2LThemeVerTip.ThemeRegistryMaxBetButtonBehaviorSupported.BetMaxCreditsOnly:
                        result = MaxBetButtonBehavior.BetMaxCreditsOnly;
                        break;

                    case F2LThemeVerTip.ThemeRegistryMaxBetButtonBehaviorSupported.BetAvailableCredits:
                        result = MaxBetButtonBehavior.BetAvailableCredits;
                        break;

                    default:
                        throw new GameRegistryException("Unsupported max bet button behavior.");
                }
            }

            return result;
        }

        /// <inheritDoc />
        public LineSelectionMode GetDefaultLineSelectionMode()
        {
            var result = LineSelectionMode.Undefined;

            if(themeRegistry.LineSelectionConfiguration != null)
            {
                switch(themeRegistry.LineSelectionConfiguration.DefaultLineSelection)
                {
                    case F2LThemeVerTip.LineSelection.Forced:
                        result = LineSelectionMode.Forced;
                        break;
                    case F2LThemeVerTip.LineSelection.PlayerSelectable:
                        result = LineSelectionMode.PlayerSelectable;
                        break;
                }
            }
            return result;
        }

        /// <inheritDoc />
        public ICollection<string> GetSupportedCultures()
        {
            return themeRegistry.SupportedCultures;
        }

        /// <inheritDoc />
        public bool IsExtensionImportRequired(Guid extensionImportId)
        {
            return themeRegistry.ExtensionImportList.Any(extensionImport =>
                new Guid(extensionImport.ExtensionId) == extensionImportId &&
                extensionImport.RequiredExtension);
        }

        /// <inheritDoc />
        public IList<Guid> GetRequiredExtensionImportIdentifiers()
        {
            return themeRegistry.ExtensionImportList
                .Where(extensionImport => IsExtensionImportRequired(new Guid(extensionImport.ExtensionId)))
                .Select(extensionImport => new Guid(extensionImport.ExtensionId)).ToList();
        }

        /// <inheritDoc />
        public IEnumerable<IExtensionImport> GetExtensionImports()
        {
            var extensionImportList = new List<IExtensionImport>();

            foreach(var extensionImport in themeRegistry.ExtensionImportList)
            {
                extensionImportList.AddRange(extensionImport.ExtensionVersionList.Select(
                    extensionVersion => new ExtensionImport(extensionImport.ExtensionId,
                        new Version((int)extensionVersion.MajorVersion, (int)extensionVersion.MinorVersion,
                            (int)extensionVersion.PatchVersion), string.Empty)));
            }

            return extensionImportList;
        }

        #endregion
    }
}
