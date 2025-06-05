//-----------------------------------------------------------------------
// <copyright file = "ConfigurationItemDataCache.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class provides the functionality for the cache of configuration item data.
    /// </summary>
    internal class ConfigurationItemDataCache
    {
        #region Private Fields

        /// <summary>
        /// The configuration item data cache.
        /// </summary>
        private readonly Dictionary<ConfigurationItemKey, CachedConfigurationItem> configurationItemCache =
            new Dictionary<ConfigurationItemKey, CachedConfigurationItem>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Add the configuration data into cache.
        /// </summary>
        /// <remarks>This method does not save the foundation supported configuration item.</remarks>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The configuration item type.</param>
        /// <param name="cacheData">The configuration data we need add into cache.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="configType"/> is invalid.</exception>
        /// <exception cref="ConfigurationCacheConsistencyException">
        /// Thrown when the type of the configuration being cached does not match the already cached type.
        /// </exception>
        public void CacheValue(ConfigurationItemKey configKey, ConfigurationItemType configType, object cacheData)
        {
            if(configType == ConfigurationItemType.Invalid)
            {
                throw new ArgumentException("Thrown when the config item type is invalid.");
            }

            CachedConfigurationItem cachedItem;
            if(configurationItemCache.TryGetValue(configKey, out cachedItem))
            {
                if(cachedItem.ItemType != configType)
                {
                    throw new ConfigurationCacheConsistencyException(configType, cachedItem.ItemType);
                }

                cachedItem.UpdateCache(cacheData);
            }
            else
            {
                configurationItemCache[configKey] = new CachedConfigurationItem(configType, cacheData);
            }
        }

        /// <summary>
        /// Get the data from the configuration item data from the cache.
        /// </summary>
        /// <remarks> Before calling this function, please check if the configuration item is cached or not by calling
        /// <see cref="IsCached"/>.
        /// </remarks>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The custom configuration item type.</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>Return the value if there is configuration item data in the cache, else, return default(T).</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="configType"/> is invalid.</exception>
        public T GetValue<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            if(configType == ConfigurationItemType.Invalid)
            {
                throw new ArgumentException("Thrown when the config item type is invalid.");
            }

            var cacheData = default(T);

            CachedConfigurationItem cachedItem;
            if(configurationItemCache.TryGetValue(configKey, out cachedItem) && cachedItem.IsDataCached)
            {
                cacheData = (T)configurationItemCache[configKey].Data;
            }

            return cacheData;
        }

        /// <summary>
        /// Check to see whether the value of configuration item is in the cache.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The custom configuration item type.</param>
        /// <returns>Return true if there is configuration item data in the cache, else, return false.</returns>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="configType"/> is invalid.</exception>
        public bool IsCached(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            if(configType == ConfigurationItemType.Invalid)
            {
                throw new ArgumentException("Thrown when the config item type is invalid.");
            }

            CachedConfigurationItem cachedItem;
            return configurationItemCache.TryGetValue(configKey, out cachedItem) && cachedItem.IsDataCached;
        }

        /// <summary>
        /// Cache the configuration type of the given item and scope.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The type of the item.</param>
        /// <exception cref="ConfigurationCacheConsistencyException">
        /// Thrown when the type of the configuration being cached does not match the already cached type.
        /// </exception>
        public void CacheType(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            CachedConfigurationItem cachedItem;
            if(configurationItemCache.TryGetValue(configKey, out cachedItem))
            {
                if(cachedItem.ItemType != configType)
                {
                    throw new ConfigurationCacheConsistencyException(configType, cachedItem.ItemType);
                }
            }
            else
            {
                configurationItemCache[configKey] = new CachedConfigurationItem(configType);
            }
        }

        /// <summary>
        /// Check if the type of the given config item is cached.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>True if the config item type is cached false otherwise.</returns>
        public bool IsTypeCached(ConfigurationItemKey configKey)
        {
            return configurationItemCache.ContainsKey(configKey);
        }

        /// <summary>
        /// Get the type of the specified config item.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>The type of the item if it is cached. ConfigurationItemType.Invalid if it is not.</returns>
        /// <remarks>Please check if the type is cached, using IsTypeCached, before calling this function.</remarks>
        public ConfigurationItemType GetType(ConfigurationItemKey configKey)
        {
            var type = ConfigurationItemType.Invalid;

            if(configurationItemCache.ContainsKey(configKey))
            {
                type = configurationItemCache[configKey].ItemType;
            }

            return type;
        }

        /// <summary>
        /// Clear all the configuration item and type data in the cache.
        /// </summary>
        public void ClearCache()
        {
            configurationItemCache.Clear();
        }

        #endregion

        #region Internal Help Struct 

        /// <summary>
        /// Object for storing a cached configuration item and its type.
        /// </summary>
        private class CachedConfigurationItem
        {
            /// <summary>
            /// Get the type of the item.
            /// </summary>
            public ConfigurationItemType ItemType { get; private set; }

            /// <summary>
            /// The data for the configuration item. If the item is not defined, or it has not been read, then the value
            /// can be null.
            /// </summary>
            public object Data { get; private set; }

            /// <summary>
            /// Boolean indicating if data is cached for the item.
            /// </summary>
            public bool IsDataCached
            {
                get { return Data != null; }
            }

            /// <summary>
            /// Update the cached data.
            /// </summary>
            /// <param name="data">Data to replace existing data with.</param>
            public void UpdateCache(object data)
            {
                Data = data;
            }

            /// <summary>
            /// Create a config item cache with the given type.
            /// </summary>
            /// <param name="configType">The type of the configuration.</param>
            public CachedConfigurationItem(ConfigurationItemType configType)
            {
                ItemType = configType;
            }

            /// <summary>
            /// Create a cached configuration item with the given type and data.
            /// </summary>
            /// <param name="configType">The type of the configuration.</param>
            /// <param name="data">The data to cache.</param>
            public CachedConfigurationItem(ConfigurationItemType configType, object data)
            {
                ItemType = configType;
                Data = data;
            }
        }

        #endregion
    }
}
