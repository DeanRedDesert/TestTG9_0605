// -----------------------------------------------------------------------
// <copyright file = "DiskStoreSectionIndexer.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class maps a safe storage selector (scope + identifier) used in writing/reading methods
    /// to a designated disk store location (section + index).
    /// </summary>
    internal sealed class DiskStoreSectionIndexer
    {
        #region Private Fields

        /// <summary>
        /// Section mappings for client critical data.
        /// </summary>
        private static readonly Dictionary<CriticalDataScope, DiskStoreSection> CriticalDataSections =
            new Dictionary<CriticalDataScope, DiskStoreSection>
                {
                    { CriticalDataScope.Feature, DiskStoreSection.CriticalData },
                    { CriticalDataScope.GameCycle, DiskStoreSection.CriticalData },
                    { CriticalDataScope.History, DiskStoreSection.CriticalData },
                    { CriticalDataScope.Payvar, DiskStoreSection.PayvarCriticalData },
                    { CriticalDataScope.PayvarPersistent, DiskStoreSection.PayvarPersistentCriticalData },
                    { CriticalDataScope.PayvarAnalytics, DiskStoreSection.PayvarAnalyticsCriticalData },
                    { CriticalDataScope.Theme, DiskStoreSection.ThemeCriticalData },
                    { CriticalDataScope.ThemePersistent, DiskStoreSection.ThemePersistentCriticalData },
                    { CriticalDataScope.ThemeAnalytics, DiskStoreSection.ThemeAnalyticsCriticalData },
                };

        /// <summary>
        /// Section mappings for simulate foundation critical data.
        /// </summary>
        private static readonly Dictionary<FoundationDataScope, DiskStoreSection> FoundationDataSections =
            new Dictionary<FoundationDataScope, DiskStoreSection>
                {
                    { FoundationDataScope.GameCycle, DiskStoreSection.FoundationData },
                    { FoundationDataScope.PayVar, DiskStoreSection.FoundationData },
                    { FoundationDataScope.Theme, DiskStoreSection.FoundationData },
                };

        /// <summary>
        /// Section mappings for configuration items.
        /// </summary>
        private static readonly Dictionary<ConfigurationScope, DiskStoreSection> ConfigurationSections =
            new Dictionary<ConfigurationScope, DiskStoreSection>
                {
                    { ConfigurationScope.Payvar, DiskStoreSection.PayvarConfigurations },
                    { ConfigurationScope.Theme, DiskStoreSection.ThemeConfigurations },
                    { ConfigurationScope.Extension, DiskStoreSection.ExtensionConfigurations },
                };


        /// <summary>
        /// Section mappings for configuration profiles.
        /// </summary>
        private static readonly Dictionary<ConfigurationScope, DiskStoreSection> ConfigurationProfileSections =
            new Dictionary<ConfigurationScope, DiskStoreSection>
                {
                    { ConfigurationScope.Payvar, DiskStoreSection.PayvarConfigurationProfiles },
                    { ConfigurationScope.Theme, DiskStoreSection.ThemeConfigurationProfiles },
                    { ConfigurationScope.Extension, DiskStoreSection.ExtensionConfigurationProfiles },
                };

        /// <summary>
        /// Object used to query a registry's index.
        /// </summary>
        private readonly IRegistryIndexer registryIndexer;

        private string currentThemeIdentifier = string.Empty;
        private string currentPaytableIdentifier = string.Empty;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DiskStoreSectionIndexer"/>.
        /// </summary>
        public DiskStoreSectionIndexer(IRegistryIndexer registryIndexer)
        {
            this.registryIndexer = registryIndexer ?? throw new ArgumentNullException(nameof(registryIndexer));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the indexer with the theme identifier and paytable identifier that are currently in play.
        /// </summary>
        /// <param name="themeIdentifier">The current theme identifier.</param>
        /// <param name="paytableIdentifier">The current paytable identifier.</param>
        public void UpdateThemeContext(string themeIdentifier, string paytableIdentifier)
        {
            currentThemeIdentifier = themeIdentifier;
            currentPaytableIdentifier = paytableIdentifier;
        }

        /// <summary>
        /// Gets the disk store section and the index in the section that maps to
        /// a scope of client critical data.
        /// </summary>
        /// <param name="scope">The critical data scope.</param>
        /// <param name="identifier">The scope identifier.</param>
        /// <returns>
        /// The disk store section and the index in the section mapped.
        /// </returns>
        public (DiskStoreSection section, int index) GetCriticalDataLocation(CriticalDataScope scope, string identifier = null)
        {
            if(!CriticalDataSections.TryGetValue(scope, out var section))
            {
                // No standalone extension lib is implemented.  Standalone extension bin lib does not support critical data yet.
                throw new NotSupportedException($"Standalone critical data for scope {scope} is not supported.");
            }

            int index;
            if(section == DiskStoreSection.CriticalData)
            {
                index = (int)scope;
            }
            else
            {
                index = GetRegistryIndex(section, identifier);
            }

            return (section, index);
        }

        /// <summary>
        /// Gets the disk store section and the index in the section that maps to
        /// a scope of foundation critical data.
        /// </summary>
        /// <param name="scope">The foundation data scope.</param>
        /// <returns>
        /// The disk store section and the index in the section mapped.
        /// </returns>
        public (DiskStoreSection section, int index) GetCriticalDataLocation(FoundationDataScope scope)
        {
            var section = FoundationDataSections[scope];
            var index = (int)scope;

            return (section, index);
        }

        /// <summary>
        /// Gets the disk store section and the index in the section that maps to
        /// a scope of configuration items.
        /// </summary>
        /// <param name="scope">The configuration scope.</param>
        /// <param name="identifier">The scope identifier.</param>
        /// <returns>
        /// The disk store section and the index in the section mapped.
        /// </returns>
        public (DiskStoreSection section, int index) GetConfigurationLocation(ConfigurationScope scope, string identifier = null)
        {
            var section = ConfigurationSections[scope];
            var index = GetRegistryIndex(section, identifier);

            return (section, index);
        }

        /// <summary>
        /// Gets the disk store section and the index in the section that maps to
        /// a scope of configuration profiles.
        /// </summary>
        /// <param name="scope">The configuration scope.</param>
        /// <param name="identifier">The scope identifier.</param>
        /// <returns>
        /// The disk store section and the index in the section mapped.
        /// </returns>
        public (DiskStoreSection section, int index) GetConfigurationProfileLocation(ConfigurationScope scope, string identifier = null)
        {
            var section = ConfigurationProfileSections[scope];
            var index = GetRegistryIndex(section, identifier);

            return (section, index);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the registry index for a scope <paramref name="identifier"/>,
        /// whose scope type is indicated by <paramref name="section"/>.
        /// </summary>
        /// <param name="section">The section where the index locates.</param>
        /// <param name="identifier">The scope identifier whose index to return.</param>
        /// <returns></returns>
        private int GetRegistryIndex(DiskStoreSection section, string identifier)
        {
            int result;

            switch(section)
            {
                case DiskStoreSection.PayvarCriticalData:
                case DiskStoreSection.PayvarAnalyticsCriticalData:
                case DiskStoreSection.PayvarPersistentCriticalData:
                case DiskStoreSection.PayvarConfigurations:
                case DiskStoreSection.PayvarConfigurationProfiles:
                {
                    if(string.IsNullOrEmpty(identifier))
                    {
                        identifier = currentPaytableIdentifier;
                    }

                    result = registryIndexer.GetPayvarIndex(identifier);
                    break;
                }
                case DiskStoreSection.ThemeCriticalData:
                case DiskStoreSection.ThemeAnalyticsCriticalData:
                case DiskStoreSection.ThemePersistentCriticalData:
                case DiskStoreSection.ThemeConfigurations:
                case DiskStoreSection.ThemeConfigurationProfiles:
                {
                    if(string.IsNullOrEmpty(identifier))
                    {
                        identifier = currentThemeIdentifier;
                    }

                    result = registryIndexer.GetThemeIndex(identifier);
                    break;
                }
                case DiskStoreSection.ExtensionConfigurations:
                case DiskStoreSection.ExtensionConfigurationProfiles:
                {
                    // Extension scope identifier is never null or empty.
                    result = registryIndexer.GetExtensionIndex(identifier);
                    break;
                }
                default:
                {
                    throw new NotSupportedException($"Indexing within Disk Store Section of {section} is not supported.");
                }
            }

            return result;
        }

        #endregion
    }
}