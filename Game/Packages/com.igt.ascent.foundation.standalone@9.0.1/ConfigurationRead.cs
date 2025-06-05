//-----------------------------------------------------------------------
// <copyright file = "ConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Registries;

    /// <summary>
    /// Defines the methods to request information about
    /// custom configuration items declared by the game in registry files.
    /// </summary>
    /// <remarks>
    /// The constructor takes a dictionary that maps paytable identifiers to payvar names.
    /// A paytable identifier is used for all the API calls and the payvar name is
    /// used for indexing into the loaded registries.
    /// </remarks>
    internal class ConfigurationRead : IConfigurationRead
    {
        /// <summary>
        /// The collection of registries loaded upon construction.
        /// </summary>
        private readonly IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries;

        /// <summary>
        /// A list of paytable identifiers.
        /// </summary>
        private readonly IList<string> paytableIdentifierList;

        /// <summary>
        /// Instantiates a new <see cref="ConfigurationRead"/> with the game's registries.
        /// </summary>
        /// <param name="registries">
        /// The game's loaded registries.
        /// </param>
        /// <param name="paytableIdentifierList">
        /// A list of paytable identifiers, including those belonging to a payvar group.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if either 
        /// <paramref name="registries"/> or <paramref name="paytableIdentifierList"/> is null.</exception>
        public ConfigurationRead(IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries,
                                 IList<string> paytableIdentifierList)
        {
            this.registries = registries ?? throw new ArgumentNullException(nameof(registries));
            this.paytableIdentifierList = paytableIdentifierList ?? throw new ArgumentNullException(nameof(paytableIdentifierList));
        }

        #region IConfigurationRead Members

        /// <inheritdoc/>
        public bool IsConfigurationDefined(ConfigurationItemKey configKey)
        {
            return GetConfigurationType(configKey) != ConfigurationItemType.Invalid;
        }

        /// <inheritdoc/>
        public bool IsConfigurationDefined(ConfigurationItemKey configKey, out ConfigurationItemType configType)
        {
            configType = GetConfigurationType(configKey);

            return configType != ConfigurationItemType.Invalid;
        }

        /// <inheritdoc/>
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            var configType = ConfigurationItemType.Invalid;

            var configurations = GetConfigurationsForScope(configKey.ConfigScope, configKey.ScopeIdentifier);

            if(configurations?.ContainsKey(configKey.ConfigName) == true)
            {
                configType = configurations[configKey.ConfigName].Key.DataType;
            }

            return configType;
        }

        /// <inheritdoc/>
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(IEnumerable<ConfigurationItemKey> configKeys)
        {
            var dictionary = new Dictionary<ConfigurationItemKey, ConfigurationItemType>();

            foreach(var configKey in configKeys)
            {
                dictionary[configKey] = GetConfigurationType(configKey);
            }

            return dictionary;
        }

        /// <inheritdoc/>
        public T GetConfiguration<T>(ConfigurationItemKey configKey)
        {
            var result = default(T);
            var valueObject = GetConfiguration(configKey);

            if(valueObject != null)
            {
                result = (T)valueObject;
            }

            return result;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">
        /// Thrown when <param name="configType"/> does not match the configuration item type
        /// represented by <param name="configKey"/>.
        /// </exception>
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            var actualConfigurationType = GetConfigurationType(configKey);

            if(actualConfigurationType == ConfigurationItemType.Invalid)
            {
                throw new ConfigurationNotDefinedException(configKey);
            }

            if(actualConfigurationType != configType)
            {
                throw new ArgumentException(
                    $"Configuration item type {configType} does not match configuration type" +
                    $"represented by {configKey}.",
                    nameof(configType));
            }

            // GetConfiguration will cast the configuration value to type T
            return GetConfiguration<T>(configKey);
        }

        /// <inheritdoc/>
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> configKeysAndTypes)
        {
            var configurations = new Dictionary<ConfigurationItemKey, object>();

            foreach(var configKey in configKeysAndTypes.Keys)
            {
                object value;

                try
                {
                    value = GetConfiguration(configKey);
                }
                catch(ConfigurationNotDefinedException)
                {
                    value = null;
                }

                configurations.Add(configKey, value);
            }

            return configurations;
        }

        /// <inheritdoc/>
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            List<string> enumList = null;

            var configurations = GetConfigurationsForScope(configKey.ConfigScope, configKey.ScopeIdentifier);

            if(configurations?.ContainsKey(configKey.ConfigName) == true)
            {
                var config = configurations[configKey.ConfigName];
                if(config.Key.DataType == ConfigurationItemType.Item)
                {
                    var enumKey = new ConfigurationItemKey(configKey.ConfigScope,
                        configKey.ScopeIdentifier,
                        config.Key.Reference);

                    enumList = GetConfiguration<List<string>>(enumKey);
                }
            }

            return enumList;
        }

        /// <inheritdoc/>
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IEnumerable<ConfigurationItemKey> configKeys)
        {
            // ReSharper disable once ConvertClosureToMethodGroup
            // Mono can't determine the correct version of ToDictionary to call with a method group
            return configKeys.ToDictionary(configKey => configKey,
                                           configKey => QueryReferencedEnumDeclaration(configKey));
        }

        /// <inheritdoc/>
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            var result = new Dictionary<string, ConfigurationItemType>();

            var configurations = GetConfigurationsForScope(scopeKey.ConfigScope, scopeKey.ScopeIdentifier);

            if(configurations?.Count > 0)
            {
                result = configurations.ToDictionary(config => config.Key, config => config.Value.Key.DataType);
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the correct configuration set based on the given configuration scope.
        /// </summary>
        /// <param name="configScope">
        /// <see cref="ConfigurationScope"/> for which to get the configurations.
        /// </param>
        /// <param name="scopeIdentifier">
        /// Scope identifier for which to get the configurations.
        /// </param>
        /// <returns>
        /// The loaded registry configurations for the specified configuration scope.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown if either the registries specified by <paramref name="configScope"/> and <paramref name="scopeIdentifier"/>
        /// cannot be loaded.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the <paramref name="scopeIdentifier" /> is null or empty.
        /// </exception>
        private IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationsForScope(ConfigurationScope configScope,
                                                                                                          string scopeIdentifier)
        {
            if(string.IsNullOrEmpty(scopeIdentifier))
            {
                throw new InvalidOperationException(
                    "There is no configuration scope identifier defined in the configuration item key. " +
                    "Scope identifier is required.");
            }

            IDictionary<string, KeyValuePair<ConfigurationProfile, object>> configurations;

            switch(configScope)
            {
                case ConfigurationScope.Theme:
                    // Use the specified theme registry
                    configurations = FindThemeRegistry(scopeIdentifier).GetConfigurationItemValues();
                    break;

                // ReSharper disable RedundantCaseLabel
                case ConfigurationScope.Payvar:
                default:
                    // ReSharper restore RedundantCaseLabel
                    // Use the specified payvar registry
                    configurations = FindPayvarRegistry(scopeIdentifier).GetConfigurationItemValues();
                    break;
            }

            return configurations;
        }

        /// <summary>
        /// Attempts to locate a <see cref="IThemeRegistry"/> in the loaded registries.
        /// </summary>
        /// <param name="themeIdentifier">
        /// Name of the theme for which to find the registries.
        /// </param>
        /// <returns>
        /// The theme registry.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown if the theme registry cannot be found.
        /// </exception>
        private IThemeRegistry FindThemeRegistry(string themeIdentifier)
        {
            var themeRegistry = registries.Keys.FirstOrDefault(theme => theme.G2SThemeId == themeIdentifier);

            if(themeRegistry == null)
            {
                throw new GameRegistryException("Failed to load theme registry for: " + themeIdentifier);
            }

            return themeRegistry;
        }

        /// <summary>
        /// Attempts to locate a <see cref="IPayvarRegistry"/> in the loaded registries.
        /// </summary>
        /// <param name="paytableIdentifier">
        /// The paytable identifier for which to find the registries.
        /// </param>
        /// <returns>
        /// The payvar registry.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown if the payvar registry cannot be found.
        /// </exception>
        private IPayvarRegistry FindPayvarRegistry(string paytableIdentifier)
        {
            if(!paytableIdentifierList.Contains(paytableIdentifier))
            {
                throw new GameRegistryException($"Cannot find payvar registry for paytable identifier: {paytableIdentifier}");
            }

            var payvarRegistry = registries.SelectMany(set => set.Value)
                                           .FirstOrDefault(payvar =>
                                                               payvar.PaytableIdentifier == paytableIdentifier ||
                                                               payvar.PayvarGroupRegistry?.Payvars
                                                                   .Select(groupPayvar => groupPayvar.PaytableIdentifier)
                                                                   .Contains(paytableIdentifier) == true);

            if(payvarRegistry == null)
            {
                throw new GameRegistryException("Failed to load payvar registry for: " + paytableIdentifier);
            }

            return payvarRegistry;
        }

        /// <summary>
        /// Gets a weakly-typed configuration value for the specified <see cref="ConfigurationItemKey"/>.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="ConfigurationNotDefinedException">
        /// Thrown if the configuration item specified by <paramref name="configKey"/> does not exist.
        /// </exception>
        private object GetConfiguration(ConfigurationItemKey configKey)
        {
            object configuration = null;

            var configurations = GetConfigurationsForScope(configKey.ConfigScope, configKey.ScopeIdentifier);

            if(configurations != null)
            {
                if(!configurations.ContainsKey(configKey.ConfigName))
                {
                    throw new ConfigurationNotDefinedException(configKey);
                }

                configuration = configurations[configKey.ConfigName].Value;
            }

            return configuration;
        }

        #endregion
    }
}
