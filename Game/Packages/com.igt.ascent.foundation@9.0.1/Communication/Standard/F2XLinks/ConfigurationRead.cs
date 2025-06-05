//-----------------------------------------------------------------------
// <copyright file = "ConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using ConfigurationItemType = Ascent.Communication.Platform.Interfaces.ConfigurationItemType;

    /// <summary>
    /// Implementation of <see cref="IConfigurationRead"/> interface that requests
    /// information about custom configuration items from the Foundation.
    /// </summary>
    internal class ConfigurationRead : IConfigurationRead
    {
        #region Private Fields
        /// <summary>
        /// Object used to verify the available transaction.
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        /// <summary>
        /// Object used to validate the context before accessing to the configurations.
        /// </summary>
        private readonly IConfigurationAccessContext configurationAccessContext;

        /// <summary>
        /// The object used to cache the configuration items locally.
        /// </summary>
        private readonly ConfigurationItemDataCache configurationItemDataCache;

        /// <summary>
        /// The object used to retrieve custom configuration data from the Foundation.
        /// </summary>
        private IConfigurationReadLink configurationReadLink;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="ConfigurationRead"/>,
        /// with caching enabled or disabled as specified.
        /// </summary>
        /// <remarks>
        /// If <paramref name="enableCaching"/> is true, the caller is responsible for
        /// calling <see cref="ClearCache"/> to clear the cache at appropriate times.
        /// </remarks>
        /// <param name="transactionVerification">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <param name="configurationAccessContext">
        /// The interface that provides information on and validation for a <see cref="ConfigurationScope"/>.
        /// </param>
        /// <param name="enableCaching">
        /// The flag indicating whether to cache the configuration item data for improved reading performance.
        /// If true, the caller must call <see cref="ClearCache"/> to clear the cache at appropriate times.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either of the arguments is null.
        /// </exception>
        public ConfigurationRead(ITransactionVerification transactionVerification,
                                 IConfigurationAccessContext configurationAccessContext,
                                 bool enableCaching)
        {
            if(transactionVerification == null)
            {
                throw new ArgumentNullException("transactionVerification");
            }

            if(configurationAccessContext == null)
            {
                throw new ArgumentNullException("configurationAccessContext");
            }

            this.transactionVerification = transactionVerification;
            this.configurationAccessContext = configurationAccessContext;

            if(enableCaching)
            {
                configurationItemDataCache = new ConfigurationItemDataCache();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="configurationReadHandler">
        /// The interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configurationReadHandler"/> is null.
        /// </exception>
        public void Initialize(IConfigurationReadLink configurationReadHandler)
        {
            if(configurationReadHandler == null)
            {
                throw new ArgumentNullException("configurationReadHandler");
            }

            configurationReadLink = configurationReadHandler;
        }

        /// <summary>
        /// Clear the cached data.
        /// </summary>
        public void ClearCache()
        {
            if(configurationItemDataCache != null)
            {
                configurationItemDataCache.ClearCache();
            }
        }

        #endregion

        #region IConfigurationRead Members

        /// <inheritdoc />
        public bool IsConfigurationDefined(ConfigurationItemKey configKey)
        {
            return GetConfigurationType(configKey) != ConfigurationItemType.Invalid;
        }

        /// <inheritdoc />
        public bool IsConfigurationDefined(ConfigurationItemKey configKey, out ConfigurationItemType configType)
        {
            configType = GetConfigurationType(configKey);

            return configType != ConfigurationItemType.Invalid;
        }

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            configurationAccessContext.ValidateConfigurationAccess(configKey.ConfigScope);

            ConfigurationItemType result;

            if(configKey.ScopeIdentifierMissing && configurationAccessContext.IsConfigurationScopeIdentifierRequired)
            {
                throw new InvalidOperationException(
                    "There is no configuration scope identifier defined in the configuration item key. " +
                    "Scope identifier is required.");
            }

            // Try to get the result from the cache first.
            if(configurationItemDataCache != null && configurationItemDataCache.IsTypeCached(configKey))
            {
                result = configurationItemDataCache.GetType(configKey);
            }
            else
            {
                result = configurationReadLink.GetConfigurationType(configKey);
                CacheTypeAsNeeded(configKey, result);
            }

            return result;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(
            IEnumerable<ConfigurationItemKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException("configKeys");
            }

            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            // Validate keys
            var configurationKeys = configKeys.ToList();
            ValidateConfigurationItemKeys(configurationKeys);

            var result = new Dictionary<ConfigurationItemKey, ConfigurationItemType>(configurationKeys.Count);
            if(configurationItemDataCache != null)
            {
                // Try to get the result from the cache first.
                var cachedKeys = configurationKeys.Where(key => configurationItemDataCache.IsTypeCached(key)).ToList();

                foreach(var cachedKey in cachedKeys)
                {
                    result[cachedKey] = configurationItemDataCache.GetType(cachedKey);
                    configurationKeys.Remove(cachedKey);
                }
            }

            // Ask the Foundation for information about the rest of the items.
            if(configurationKeys.Any())
            {
                var foundationResult = configurationReadLink.GetConfigurationTypes(configurationKeys);
                foreach(var resultPair in foundationResult)
                {
                    result[resultPair.Key] = resultPair.Value;
                    CacheTypeAsNeeded(resultPair.Key, resultPair.Value);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            configurationAccessContext.ValidateConfigurationAccess(scopeKey.ConfigScope);

            return configurationReadLink.QueryConfigurations(scopeKey);
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            configurationAccessContext.ValidateConfigurationAccess(configKey.ConfigScope);

            ConfigurationItemType configType;

            var isDefined = true;

            var dataType = typeof(T);

            // Try to induce the config type based on the return type.
            if(dataType == typeof(bool))
            {
                configType = ConfigurationItemType.Boolean;
            }
            else if(dataType == typeof(List<string>))
            {
                configType = ConfigurationItemType.EnumerationList;
            }
            else if(dataType == typeof(List<KeyValuePair<string, bool>>))
            {
                configType = ConfigurationItemType.FlagList;
            }
            else if(dataType == typeof(float))
            {
                configType = ConfigurationItemType.Float;
            }
            else
            {
                // If the config type cannot be induced from the data type, get the config type first.
                isDefined = IsConfigurationDefined(configKey, out configType);
            }

            if(!isDefined)
            {
                throw new ConfigurationNotDefinedException(configKey.ConfigScope, configKey.ConfigName);
            }

            return GetConfiguration<T>(configKey, configType);
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            configurationAccessContext.ValidateConfigurationAccess(configKey.ConfigScope);

            T result;

            if(configKey.ScopeIdentifierMissing && configurationAccessContext.IsConfigurationScopeIdentifierRequired)
            {
                throw new InvalidOperationException(
                    "There is no configuration scope identifier defined in the configuration item key. " +
                    "Scope identifier is required.");
            }

            // Try to get the result from the cache first.
            if(configurationItemDataCache != null && configurationItemDataCache.IsCached(configKey, configType))
            {
                result = configurationItemDataCache.GetValue<T>(configKey, configType);
            }
            else
            {
                result = configurationReadLink.GetConfiguration<T>(configKey, configType);
                // Add the configuration item data into the cache.
                CacheValueAsNeeded(configKey, configType, result);
            }

            return result;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> configKeysAndTypes)
        {
            if(configKeysAndTypes == null)
            {
                throw new ArgumentNullException("configKeysAndTypes");
            }

            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            // Validate keys
            ValidateConfigurationItemKeys(configKeysAndTypes.Keys);

            var result = new Dictionary<ConfigurationItemKey, object>(configKeysAndTypes.Count);

            if(configurationItemDataCache != null)
            {
                // Try to get the result from the cache first.
                var cachedKeys =
                    configKeysAndTypes.Where(pair => configurationItemDataCache.IsCached(pair.Key, pair.Value))
                        .Select(pair => pair.Key)
                        .ToList();

                foreach(var cachedKey in cachedKeys)
                {
                    result[cachedKey] = configurationItemDataCache.GetValue<object>(cachedKey,
                        configKeysAndTypes[cachedKey]);

                    configKeysAndTypes.Remove(cachedKey);
                }
            }

            // Ask the Foundation for information about the rest of the items.
            if(configKeysAndTypes.Any())
            {
                var foundationResult = configurationReadLink.GetConfigurations(configKeysAndTypes);

                foreach(var replyPair in foundationResult)
                {
                    result[replyPair.Key] = replyPair.Value;
                    // Add the configuration item data into the cache.
                    CacheValueAsNeeded(replyPair.Key, configKeysAndTypes[replyPair.Key], replyPair.Value);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            configurationAccessContext.ValidateConfigurationAccess(configKey.ConfigScope);

            if(configKey.ScopeIdentifierMissing && configurationAccessContext.IsConfigurationScopeIdentifierRequired)
            {
                throw new InvalidOperationException(
                    "There is no configuration scope identifier defined in the configuration item key. " +
                    "Scope identifier is required.");
            }

            return configurationReadLink.QueryReferencedEnumDeclaration(configKey);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IEnumerable<ConfigurationItemKey> configKeys)
        {
            if(configKeys == null)
            {
                throw new ArgumentNullException("configKeys");
            }

            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();

            // Validate keys
            var configurationKeys = configKeys.ToList();
            ValidateConfigurationItemKeys(configurationKeys);

            var result = new Dictionary<ConfigurationItemKey, ICollection<string>>(configurationKeys.Count);
            if(configurationKeys.Any())
            {
                var foundationResult = configurationReadLink.QueryReferencedEnumDeclarations(configurationKeys);
                foreach(var resultPair in foundationResult)
                {
                    result[resultPair.Key] = resultPair.Value;
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(configurationReadLink == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "ConfigurationRead cannot be used without calling its Initialize method first.");
            }
        }

        /// <summary>
        /// Validates a collection of <see cref="ConfigurationItemKey"/> objects.
        /// </summary>
        /// <param name="configKeys">Collection of configuration keys to validate.</param>
        private void ValidateConfigurationItemKeys(IEnumerable<ConfigurationItemKey> configKeys)
        {
            foreach(var key in configKeys)
            {
                configurationAccessContext.ValidateConfigurationAccess(key.ConfigScope);
                if(configurationAccessContext.IsConfigurationScopeIdentifierRequired &&
                   key.ScopeIdentifierMissing)
                {
                    throw new InvalidOperationException(
                        "There is no configuration scope identifier defined in any of the configuration item keys. " +
                        "Scope identifier is required.");
                }
            }
        }

        /// <summary>
        /// Cache the configuration type if caching is enabled.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The <see cref="ConfigurationItemType"/> of the configuration item.</param>
        private void CacheTypeAsNeeded(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            if(configurationItemDataCache != null)
            {
                configurationItemDataCache.CacheType(configKey, configType);
            }
        }

        /// <summary>
        /// Cache the configuration value if caching is enabled.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The <see cref="ConfigurationItemType"/> of the configuration item.</param>
        /// <param name="value">The value of the configuration item.</param>
        private void CacheValueAsNeeded(ConfigurationItemKey configKey, ConfigurationItemType configType, object value)
        {
            if(configurationItemDataCache != null)
            {
                configurationItemDataCache.CacheValue(configKey, configType, value);
            }
        }

        #endregion
    }
}