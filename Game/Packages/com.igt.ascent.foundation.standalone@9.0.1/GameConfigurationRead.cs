// -----------------------------------------------------------------------
// <copyright file = "GameConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Registries;

    /// <summary>
    /// This class is used to acquire the custom configuration items from the game client.
    /// </summary>
    internal class GameConfigurationRead : IGameConfigurationRead
    {
        /// <summary>
        /// The disk store manager is used to read the configurations from the safe storage.
        /// </summary>
        private readonly IDiskStoreManager diskStoreManager;

        /// <summary>
        /// This object is used to find the section and index within a disk store where the configuration items are stored.
        /// </summary>
        private readonly DiskStoreSectionIndexer diskStoreSectionIndexer;

        /// <summary>
        /// This object is used to verify the available transaction.
        /// </summary>
        private readonly ITransactionVerification transactionVerifier;

        /// <summary>
        /// This object is used to verify the game context mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        /// <summary>
        /// Construct the instance.
        /// </summary>
        /// <param name="diskStoreManager">
        /// The disk store manager is used to read the configurations from the safe storage.
        /// </param>
        /// <param name="diskStoreSectionIndexer">
        /// This object is used to find where configuration items are located in the disk store.
        /// </param>
        /// <param name="transactionVerifier">
        /// This object is used to verify the available transaction.
        /// </param>
        /// <param name="gameModeQuery">
        /// This object is used to query game mode in order to validate the configuration access.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the parameters is null.
        /// </exception>
        public GameConfigurationRead(IDiskStoreManager diskStoreManager,
                                     DiskStoreSectionIndexer diskStoreSectionIndexer,
                                     ITransactionVerification transactionVerifier,
                                     IGameModeQuery gameModeQuery)
        {
            this.diskStoreManager = diskStoreManager ?? throw new ArgumentNullException(nameof(diskStoreManager));
            this.diskStoreSectionIndexer = diskStoreSectionIndexer ?? throw new ArgumentNullException(nameof(diskStoreSectionIndexer));
            this.transactionVerifier = transactionVerifier ?? throw new ArgumentNullException(nameof(transactionVerifier));
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));
        }

        #region Implementation of IConfigurationInquiry<GameConfigurationKey>

        /// <inheritdoc />
        public bool IsConfigurationDefined(GameConfigurationKey configKey)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            return IsConfigurationDefined(configKey, out _);
        }

        /// <inheritdoc />
        public bool IsConfigurationDefined(GameConfigurationKey configKey, out ConfigurationItemType configType)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            var (section, index) = diskStoreSectionIndexer.GetConfigurationProfileLocation(configKey.ConfigScope, configKey.ScopeIdentifier);
            var profile = diskStoreManager.Read<ConfigurationProfile>(section, index, configKey.ConfigName);

            configType = profile.DataType;
            return configType != ConfigurationItemType.Invalid;
        }

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(GameConfigurationKey configKey)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            IsConfigurationDefined(configKey, out var configType);
            return configType;
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, ConfigurationItemType> GetConfigurationTypes(
            IEnumerable<GameConfigurationKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException(nameof(configKeys));
            }

            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            // ReSharper disable once ConvertClosureToMethodGroup
            return configKeys.ToDictionary(configKey => configKey, configKey => GetConfigurationType(configKey));
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(GameConfigurationKey configKey)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);
            if(!IsConfigurationDefined(configKey))
            {
                throw new ConfigurationNotDefinedException(configKey.ConfigScope, configKey.ConfigName);
            }

            var (section, index) = diskStoreSectionIndexer.GetConfigurationLocation(configKey.ConfigScope, configKey.ScopeIdentifier);
            return diskStoreManager.Read<T>(section, index, configKey.ConfigName);
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(GameConfigurationKey configKey, ConfigurationItemType configType)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            return GetConfiguration<T>(configKey);
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, object> GetConfigurations(
            IDictionary<GameConfigurationKey, ConfigurationItemType> configKeysAndTypes)
        {
            if(configKeysAndTypes == null)
            {
                throw new ArgumentNullException(nameof(configKeysAndTypes));
            }

            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            return configKeysAndTypes.ToDictionary(keyTypePair => keyTypePair.Key,
                                                   keyTypePair => IsConfigurationDefined(keyTypePair.Key)
                                                                      ? GetConfiguration(keyTypePair.Key, keyTypePair.Value)
                                                                      : null);
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(GameConfigurationKey configKey)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            List<string> declarationList = null;

            var (section, index) = diskStoreSectionIndexer.GetConfigurationProfileLocation(configKey.ConfigScope, configKey.ScopeIdentifier);
            var profile = diskStoreManager.Read<ConfigurationProfile>(section, index, configKey.ConfigName);

            if(profile.DataType == ConfigurationItemType.Item)
            {
                var keyData = new ConfigurationKeyData(configKey.ConfigScope, configKey.ScopeIdentifier, profile.Reference);
                var referenceKey = GameConfigurationKey.From(keyData);

                declarationList = GetConfiguration<List<string>>(referenceKey, ConfigurationItemType.EnumerationList);
            }
            return declarationList;
        }

        /// <inheritdoc />
        public IDictionary<GameConfigurationKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IEnumerable<GameConfigurationKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException(nameof(configKeys));
            }

            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            return configKeys.ToDictionary(configKey => configKey,
                                           configKey => QueryReferencedEnumDeclaration(configKey));
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(GameConfigurationScopeKey scopeKey)
        {
            transactionVerifier.MustHaveOpenTransaction();
            Utility.ValidateConfigurationAccess(gameModeQuery.GameMode);

            var resultList = new Dictionary<string, ConfigurationItemType>();

            var (section, index) = diskStoreSectionIndexer.GetConfigurationLocation(scopeKey.ConfigScope, scopeKey.ScopeIdentifier);
            var configNameList = diskStoreManager.GetManifest(section, index);

            if(configNameList != null)
            {
                foreach(var configName in configNameList)
                {
                    var keyData = new ConfigurationKeyData(scopeKey.ConfigScope, scopeKey.ScopeIdentifier, configName);
                    IsConfigurationDefined(GameConfigurationKey.From(keyData), out var configType);
                    resultList.Add(configName, configType);
                }
            }

            return resultList;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the value of a configuration item with a specific type.
        /// </summary>
        /// <param name="configKey">The key to search the configuration item.</param>
        /// <param name="configType">The configuration type of the target configuration item.</param>
        /// <returns>The value of a configuration item.</returns>
        private object GetConfiguration(GameConfigurationKey configKey, ConfigurationItemType configType)
        {
            switch(configType)
            {
                case ConfigurationItemType.Amount:
                case ConfigurationItemType.Int64:
                    return GetConfiguration<long>(configKey, configType);
                case ConfigurationItemType.Boolean:
                    return GetConfiguration<bool>(configKey, configType);
                case ConfigurationItemType.EnumerationList:
                    return GetConfiguration<List<string>>(configKey, configType);
                case ConfigurationItemType.FlagList:
                    return GetConfiguration<List<KeyValuePair<string, bool>>>(configKey, configType);
                case ConfigurationItemType.Float:
                    return GetConfiguration<float>(configKey, configType);
                case ConfigurationItemType.Item:
                case ConfigurationItemType.String:
                    return GetConfiguration<string>(configKey, configType);
            }
            return null;
        }

        #endregion
    }
}