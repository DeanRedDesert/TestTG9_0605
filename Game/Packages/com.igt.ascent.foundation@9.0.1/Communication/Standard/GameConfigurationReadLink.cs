// -----------------------------------------------------------------------
// <copyright file = "GameConfigurationReadLink.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2L;
    using F2LLink;
    using F2X;
    using F2XLinks;

    /// <summary>
    /// This class represents a link to the custom configuration items for the game client inquiry.
    /// </summary>
    internal class GameConfigurationReadLink : IConfigurationReadLink
    {
        #region Fields

        /// <summary>
        /// This object is used to query configurations from the Foundation.
        /// </summary>
        private readonly IConfigurationReadLink gameControlLink;

        /// <summary>
        /// The object used to retrieve custom configuration data from the Foundation.
        /// </summary>
        private readonly IConfigurationReadLink extensionLink;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="GameConfigurationReadLink"/> with the communication category.
        /// </summary>
        /// <param name="gameControlCategory">
        /// The game control category instance used to query configurations from the Foundation.
        /// </param>
        /// <param name="configurationReadCategory">
        /// The object used to retrieve custom configuration data from the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameControlCategory"/> is null.
        /// </exception>
        public GameConfigurationReadLink(IGameControlCategory gameControlCategory,
                                         ICustomConfigurationReadCategory configurationReadCategory)
        {
            if(gameControlCategory == null)
            {
                throw new ArgumentNullException(nameof(gameControlCategory));
            }

            gameControlLink = new GameControlConfigurationReadLink(gameControlCategory);
            extensionLink = configurationReadCategory == null
                ? (IConfigurationReadLink)new EmptyConfigurationReadLink()
                : new ConfigurationReadLink(configurationReadCategory);
        }

        #endregion

        #region Private Methods

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            return GetConfigurationReadLink(configKey.ConfigScope).GetConfigurationType(configKey);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(
            IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            CategorizeKeys(queryKeys, out var extensionKeys, out var gameControlKeys);

            var result = gameControlKeys.Any()
                ? gameControlLink.GetConfigurationTypes(gameControlKeys)
                : new Dictionary<ConfigurationItemKey, ConfigurationItemType>();

            if(extensionKeys.Any())
            {
                var extensionResult = extensionLink.GetConfigurationTypes(extensionKeys);
                foreach(var configKey in extensionKeys)
                {
                    result.Add(configKey, extensionResult[configKey]);
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            return GetConfigurationReadLink(scopeKey.ConfigScope).QueryConfigurations(scopeKey);
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            return GetConfigurationReadLink(configKey.ConfigScope).GetConfiguration<T>(configKey, configType);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> queryEntries)
        {
            if(queryEntries == null)
            {
                throw new ArgumentNullException(nameof(queryEntries));
            }

            var gameControlKeys = new Dictionary<ConfigurationItemKey, ConfigurationItemType>();
            var extensionKeys = new Dictionary<ConfigurationItemKey, ConfigurationItemType>();
            foreach(var pair in queryEntries)
            {
                switch(pair.Key.ConfigScope)
                {
                    case ConfigurationScope.Extension:
                        extensionKeys.Add(pair.Key, pair.Value);
                        break;
                    default:
                        gameControlKeys.Add(pair.Key, pair.Value);
                        break;
                }
            }

            var result = gameControlKeys.Any()
                ? gameControlLink.GetConfigurations(gameControlKeys)
                : new Dictionary<ConfigurationItemKey, object>();

            if(extensionKeys.Any())
            {
                var extensionResult = extensionLink.GetConfigurations(extensionKeys);
                foreach(var pair in extensionResult)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            return GetConfigurationReadLink(configKey.ConfigScope).QueryReferencedEnumDeclaration(configKey);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            CategorizeKeys(queryKeys, out var extensionKeys, out var gameControlKeys);

            var result = gameControlKeys.Any()
                ? gameControlLink.QueryReferencedEnumDeclarations(gameControlKeys)
                : new Dictionary<ConfigurationItemKey, ICollection<string>>();

            if(extensionKeys.Any())
            {
                var extensionResult = extensionLink.QueryReferencedEnumDeclarations(extensionKeys);
                foreach(var configKey in extensionKeys)
                {
                    result.Add(configKey, extensionResult[configKey]);
                }
            }
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the proper configuration read link object according to the configuration scope.
        /// </summary>
        /// <param name="configScope">The target configuration scope where the configuration items are queried.</param>
        /// <returns>The proper configuration read link object.</returns>
        private IConfigurationReadLink GetConfigurationReadLink(ConfigurationScope configScope)
        {
            switch(configScope)
            {
                case ConfigurationScope.Extension:
                    return extensionLink;
                default:
                    return gameControlLink;
            }
        }

        /// <summary>
        /// Categorize the input keys based on their target scopes.
        /// </summary>
        /// <param name="inputKeys">The input key collection.</param>
        /// <param name="extensionKeys">The keys that acquires configurations from the extensions.</param>
        /// <param name="gameControlKeys">The keys that acquires configurations from the theme or payvar.</param>
        private void CategorizeKeys(IEnumerable<ConfigurationItemKey> inputKeys,
                                    out List<ConfigurationItemKey> extensionKeys,
                                    out List<ConfigurationItemKey> gameControlKeys)
        {
            gameControlKeys = new List<ConfigurationItemKey>();
            extensionKeys = new List<ConfigurationItemKey>();
            foreach(var key in inputKeys)
            {
                switch(key.ConfigScope)
                {
                    case ConfigurationScope.Extension:
                        extensionKeys.Add(key);
                        break;
                    default:
                        gameControlKeys.Add(key);
                        break;
                }
            }
        }

        #endregion
    }
}