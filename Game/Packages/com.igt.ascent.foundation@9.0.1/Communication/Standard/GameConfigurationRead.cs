// -----------------------------------------------------------------------
// <copyright file = "GameConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using F2XLinks;

    /// <summary>
    /// This class is used to query the custom configuration items from the game client.
    /// </summary>
    internal class GameConfigurationRead : IGameConfigurationRead
    {
        #region Fields

        /// <summary>
        /// This object is used to query configuration items from theme or payvar.
        /// </summary>
        private readonly ConfigurationRead configurationRead;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the instance with a given game control interface.
        /// </summary>
        /// <param name="transactionVerification">This object is used to confirm the open transaction.</param>
        /// <param name="configurationAccessContext">
        /// This object is used to validate the configuration access context.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if either of the parameters is null.</exception>
        public GameConfigurationRead(ITransactionVerification transactionVerification,
                                     IConfigurationAccessContext configurationAccessContext)
        {
            configurationRead = new ConfigurationRead(transactionVerification, configurationAccessContext, true);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize with the required categories.
        /// </summary>
        /// <param name="gameConfigurationReadLink">
        /// The object used to query configuration items from theme or payvar.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameConfigurationReadLink"/> is null.
        /// </exception>
        public void Initialize(IConfigurationReadLink gameConfigurationReadLink)
        {
            if(gameConfigurationReadLink == null)
            {
                throw new ArgumentNullException(nameof(gameConfigurationReadLink));
            }

            configurationRead.Initialize(gameConfigurationReadLink);
        }

        /// <summary>
        /// Clear the configuration items cache.
        /// </summary>
        public void ClearCache()
        {
            configurationRead.ClearCache();
        }

        #endregion

        #region Implementation of IGameConfigurationRead

        /// <inheritdoc />
        public bool IsConfigurationDefined(GameConfigurationKey configKey)
        {
            return configurationRead.IsConfigurationDefined(ConfigurationItemKey.From(configKey.KeyData));
        }

        /// <inheritdoc />
        public bool IsConfigurationDefined(GameConfigurationKey configKey, out ConfigurationItemType configType)
        {
            return configurationRead.IsConfigurationDefined(ConfigurationItemKey.From(configKey.KeyData), out configType);
        }

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(GameConfigurationKey configKey)
        {
            return configurationRead.GetConfigurationType(ConfigurationItemKey.From(configKey.KeyData));
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, ConfigurationItemType> GetConfigurationTypes(
            IEnumerable<GameConfigurationKey> configKeys)
        {
            var result = configurationRead.GetConfigurationTypes(configKeys.Select(key => ConfigurationItemKey.From(key.KeyData)));
            return result.ToDictionary(
                pair => GameConfigurationKey.From(pair.Key.KeyData),
                pair => pair.Value);
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(GameConfigurationKey configKey)
        {
            return configurationRead.GetConfiguration<T>(ConfigurationItemKey.From(configKey.KeyData));
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(GameConfigurationKey configKey, ConfigurationItemType configType)
        {
            return configurationRead.GetConfiguration<T>(ConfigurationItemKey.From(configKey.KeyData), configType);
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, object> GetConfigurations(
            IDictionary<GameConfigurationKey, ConfigurationItemType> configKeysAndTypes)
        {
            if(configKeysAndTypes == null)
            {
                throw new ArgumentNullException(nameof(configKeysAndTypes));
            }

            var queryKeys = configKeysAndTypes.ToDictionary(
                keyPair => ConfigurationItemKey.From(keyPair.Key.KeyData) ,
                keyPair => keyPair.Value);
            var result = configurationRead.GetConfigurations(queryKeys);
            return result.ToDictionary(
                resultPair => GameConfigurationKey.From(resultPair.Key.KeyData),
                resultPair => resultPair.Value);
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(GameConfigurationKey configKey)
        {
            return configurationRead.QueryReferencedEnumDeclaration(ConfigurationItemKey.From(configKey.KeyData));
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IEnumerable<GameConfigurationKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException(nameof(configKeys));
            }

            var result =
                configurationRead.QueryReferencedEnumDeclarations(configKeys.Select(key =>
                    ConfigurationItemKey.From(key.KeyData)));
            return result.ToDictionary(
                pair => GameConfigurationKey.From(pair.Key.KeyData),
                pair => pair.Value);
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(GameConfigurationScopeKey scopeKey)
        {
            return configurationRead.QueryConfigurations(ConfigurationScopeKey.From(scopeKey.KeyData));
        }

        #endregion
    }
}