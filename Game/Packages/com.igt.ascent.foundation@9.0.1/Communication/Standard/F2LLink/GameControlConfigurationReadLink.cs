// -----------------------------------------------------------------------
// <copyright file = "GameControlConfigurationReadLink.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2L;
    using F2L.Schemas.Internal;

    /// <summary>
    /// This class represents a link to the custom configuration items via the game control category.
    /// </summary>
    internal class GameControlConfigurationReadLink : IConfigurationReadLink
    {
        #region Fields

        /// <summary>
        /// This object is used to query configurations from the Foundation.
        /// </summary>
        private readonly IGameControlCategory gameControl;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="GameConfigurationReadLink"/> with the communication category.
        /// </summary>
        /// <param name="gameControlCategory">
        /// The game control category instance used to query configurations from the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="gameControlCategory"/> is null.
        /// </exception>
        public GameControlConfigurationReadLink(IGameControlCategory gameControlCategory)
        {
            gameControl = gameControlCategory ?? throw new ArgumentNullException(nameof(gameControlCategory));
        }

        #endregion

        #region Private Methods

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            var customConfigItemScope = configKey.ConfigScope.ToCustomConfigItemScope();
            gameControl.GetCustomConfigItemType(configKey.ConfigName, customConfigItemScope,
                                                out var customConfigType);
            return customConfigType.ToConfigurationItemType();
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            var result = new Dictionary<ConfigurationItemKey, ConfigurationItemType>();
            foreach(var key in queryKeys)
            {
                result[key] = GetConfigurationType(key);
            }

            return result;
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            var result = gameControl.GetCustomConfigItemTypes(scopeKey.ConfigScope.ToCustomConfigItemScope());
            return result.ToDictionary(pair => pair.Key,
                                       pair => pair.Value.ToConfigurationItemType());
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            if(!GetConfiguration(configKey, configType, out T value))
            {
                throw new ConfigurationNotDefinedException(configKey.ConfigScope, configKey.ConfigName);
            }

            return value;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> queryEntries)
        {
            if(queryEntries == null)
            {
                throw new ArgumentNullException(nameof(queryEntries));
            }

            var result = new Dictionary<ConfigurationItemKey, object>();
            foreach(var configKeyType in queryEntries)
            {
                var isDefined = GetConfiguration(configKeyType.Key, configKeyType.Value, out object value);
                result.Add(configKeyType.Key, isDefined ? value : null);
            }

            return result;
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            var enumeration = new List<string>();
            if(!gameControl.GetCustomConfigItemReferencedEnumeration(configKey.ConfigName,
                                                                     configKey.ConfigScope.ToCustomConfigItemScope(),
                                                                     enumeration))
            {
                enumeration = null;
            }

            return enumeration;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            var result = new Dictionary<ConfigurationItemKey, ICollection<string>>();
            foreach(var configKey in queryKeys)
            {
                var value = QueryReferencedEnumDeclaration(configKey);
                result.Add(configKey, value);
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the value of a configuration item with a specific type from the foundation.
        /// </summary>
        /// <param name="configKey">The key of the configuration item.</param>
        /// <param name="configType">The <see cref="CustomConfigItemType"/> of the configuration item.</param>
        /// <param name="value">The value of the configuration item</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>True if the configuration item is defined; otherwise, false.</returns>
        private bool GetConfiguration<T>(ConfigurationItemKey configKey,
                                         ConfigurationItemType configType,
                                         out T value)
        {
            var isDefined = false;
            var configScope = configKey.ConfigScope.ToCustomConfigItemScope();

            switch(configType)
            {
                case ConfigurationItemType.Amount:
                {
                    isDefined = gameControl.GetCustomConfigItemAmount(
                        configKey.ConfigName,
                        configScope,
                        out var amountValue,
                        out _,
                        out _);
                    value = (T)(object)amountValue;
                    break;
                }
                case ConfigurationItemType.Boolean:
                {
                    isDefined = gameControl.GetCustomConfigItemBoolean(configKey.ConfigName, configScope,
                                                                       out var boolValue);
                    value = (T)(object)boolValue;
                    break;
                }

                case ConfigurationItemType.EnumerationList:
                {
                    var values = new List<string>();
                    isDefined = gameControl.GetCustomConfigItemEnumeration(configKey.ConfigName, configScope,
                                                                           values);
                    value = (T)(object)values;
                    break;
                }
                case ConfigurationItemType.Item:
                {
                    isDefined = gameControl.GetCustomConfigItem(configKey.ConfigName, configScope,
                                                                out var stringValue);
                    value = (T)(object)stringValue;
                    break;
                }
                case ConfigurationItemType.FlagList:
                {
                    var values = new Dictionary<string, bool>();
                    isDefined = gameControl.GetCustomConfigItemFlagList(configKey.ConfigName, configScope, values);
                    value = (T)(object)values.ToList();
                    break;
                }
                case ConfigurationItemType.Float:
                {
                    isDefined = gameControl.GetCustomConfigItemFloat(configKey.ConfigName, configScope,
                                                                     out var floatValue);
                    value = (T)(object)floatValue;
                    break;
                }
                case ConfigurationItemType.Int64:
                {
                    isDefined = gameControl.GetCustomConfigItemInt64(configKey.ConfigName, configScope,
                                                                     out var longValue);
                    value = (T)(object)longValue;
                    break;
                }
                case ConfigurationItemType.String:
                {
                    isDefined = gameControl.GetCustomConfigItemString(configKey.ConfigName, configScope,
                                                                      out var stringValue);
                    value = (T)(object)stringValue;
                    break;
                }
                default:
                    value = default;
                    break;
            }

            return isDefined;
        }

        #endregion
    }
}