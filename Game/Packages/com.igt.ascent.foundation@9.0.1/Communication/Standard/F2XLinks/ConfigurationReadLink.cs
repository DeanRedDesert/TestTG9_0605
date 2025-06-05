// -----------------------------------------------------------------------
// <copyright file = "ConfigurationReadLink.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.CustomConfigurationRead;
    using F2XCallbacks;

    using ConfigurationItemType = Ascent.Communication.Platform.Interfaces.ConfigurationItemType;

    /// <summary>
    /// This class represents a link to the custom configuration items via the category
    /// <see cref="ICustomConfigurationReadCategory"/>.
    /// </summary>
    internal class ConfigurationReadLink : IConfigurationReadLink
    {
        #region Private Fields

        /// <summary>
        /// The object used to retrieve custom configuration data from the Foundation.
        /// </summary>
        private readonly ICustomConfigurationReadCategory configurationReadCategory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="ConfigurationReadLink"/> with the communication category.
        /// </summary>
        /// <param name="configurationReadCategory">
        /// The category instance used to query configurations from the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configurationReadCategory"/> is null;
        /// </exception>
        public ConfigurationReadLink(ICustomConfigurationReadCategory configurationReadCategory)
        {
            this.configurationReadCategory = configurationReadCategory ?? throw new ArgumentNullException(nameof(configurationReadCategory));
        }

        #endregion

        #region IConfigurationReadLink Members

        /// <inheritdoc />
        public ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey)
        {
            // Ask the Foundation for the information.
            var replyEntries = configurationReadCategory.GetCustomConfigItemTypes(
                new List<CustomConfigurationItemSelector> {configKey.ToConfigurationItemSelector()}).ToList();

            // TODO: Do we need to check for TypeSpecified?
            return replyEntries.First().Type.ToPublic();
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(
            IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            var result = new Dictionary<ConfigurationItemKey, ConfigurationItemType>(queryKeys.Count);
            if(queryKeys.Any())
            {
                var replyEntries = configurationReadCategory.GetCustomConfigItemTypes(
                    queryKeys.Select(key => key.ToConfigurationItemSelector()));

                foreach(var replyEntry in replyEntries)
                {
                    var configKey = replyEntry.CustomConfigurationItemSelector.ToConfigurationItemKey();
                    result[configKey] = replyEntry.Type.ToPublic();
                }
            }
            return result;
        }

        /// <inheritdoc />
        public IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey)
        {
            IEnumerable<CustomConfigItemNameAndType> replyEntries = null;

            switch(scopeKey.ConfigScope)
            {
                case ConfigurationScope.Payvar:
                {
                    var payvarIdentifier = scopeKey.ScopeIdentifier.ToPayvarIdentifier();
                    replyEntries = configurationReadCategory.GetScopedCustomConfigItemNames(
                        payvarIdentifier.ToConfigurationItemScopeSelector());
                    break;
                }

                case ConfigurationScope.Theme:
                {
                    var themeIdentifier = scopeKey.ScopeIdentifier.ToThemeIdentifier();
                    replyEntries = configurationReadCategory.GetScopedCustomConfigItemNames(
                        themeIdentifier.ToConfigurationItemScopeSelector());
                    break;
                }

                case ConfigurationScope.Extension:
                {
                    var extensionIdentifier = scopeKey.ScopeIdentifier.ToExtensionIdentifier();
                    replyEntries = configurationReadCategory.GetScopedCustomConfigItemNames(
                        extensionIdentifier.ToConfigurationItemScopeSelector());
                    break;
                }
            }

            return replyEntries != null
                ? replyEntries.ToDictionary(entry => entry.Name, entry => entry.Type.ToPublic())
                : new Dictionary<string, ConfigurationItemType>();
        }

        /// <inheritdoc />
        public T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType)
        {
            // Ask the Foundation for information.
            var replyEntries = configurationReadCategory.GetCustomConfigItemValueData(
                new List<GetCustomConfigItemValueDataSendSelector>
                {
                    new GetCustomConfigItemValueDataSendSelector
                    {
                        CustomConfigurationItemSelector = configKey.ToConfigurationItemSelector(),
                        Type = configType.ToInternal()
                    }
                });

            var valueData = replyEntries.First().ConfigItemValueData;

            if(valueData == null)
            {
                throw new ConfigurationNotDefinedException(configKey);
            }

            return ConvertConfigurationValue<T>(configType, valueData.Item);
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> queryEntries)
        {
            if(queryEntries == null)
            {
                throw new ArgumentNullException(nameof(queryEntries));
            }

            var result = new Dictionary<ConfigurationItemKey, object>(queryEntries.Count);
            if(queryEntries.Any())
            {
                // TODO: Change after ICustomConfigurationReadCategory is corrected.
                var replyEntries = configurationReadCategory.GetCustomConfigItemValueData(
                    queryEntries.Select(
                        pair => new GetCustomConfigItemValueDataSendSelector
                                {
                                    CustomConfigurationItemSelector = pair.Key.ToConfigurationItemSelector(),
                                    Type = pair.Value.ToInternal()
                                }));

                foreach(var replyEntry in replyEntries)
                {
                    var configKey = replyEntry.CustomConfigurationItemSelector.ToConfigurationItemKey();

                    var valueData = replyEntry.ConfigItemValueData;
                    if(valueData != null)
                    {
                        var configType = queryEntries[configKey];
                        var value = ConvertConfigurationValue<object>(configType, valueData.Item);
                        result[configKey] = value;
                    }
                    else
                    {
                        result[configKey] = null;
                    }
                }
            }

            return result;
        }

        /// <inheritdoc />
        public ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey)
        {
            var replyEntries = configurationReadCategory.GetCustomConfigItemReferencedEnumerations(
                new List<CustomConfigurationItemSelector>
                {
                    configKey.ToConfigurationItemSelector()
                });

            var enumerationList = replyEntries.First().ReferencedEnumerationList;
            return enumerationList?.Item;
        }

        /// <inheritdoc />
        public IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IList<ConfigurationItemKey> queryKeys)
        {
            if(queryKeys == null)
            {
                throw new ArgumentNullException(nameof(queryKeys));
            }

            var result = new Dictionary<ConfigurationItemKey, ICollection<string>>(queryKeys.Count);
            if(queryKeys.Any())
            {
                var replyEntries = configurationReadCategory.GetCustomConfigItemReferencedEnumerations(
                    queryKeys.Select(key => key.ToConfigurationItemSelector()));
                foreach(var replyEntry in replyEntries)
                {
                    var configKey = replyEntry.CustomConfigItem.ToConfigurationItemKey();

                    var valueData = replyEntry.ReferencedEnumerationList;

                    if(valueData != null)
                    {
                        result[configKey] = valueData.Item.ToList();
                    }
                    else
                    {
                        result[configKey] = null;
                    }
                }
            }
            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts an F2X configuration item value data to a value of specified implementation type.
        /// </summary>
        /// <param name="configType">
        /// The <see cref="ConfigurationItemType"/> of the configuration item.
        /// </param>
        /// <param name="dataItem">The item object of a F2X configuration item value data.</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>The value of the configuration item of type T.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="configType"/> is not supported.
        /// </exception>
        private static T ConvertConfigurationValue<T>(ConfigurationItemType configType, object dataItem)
        {
            T result = default;

            switch(configType)
            {
                case ConfigurationItemType.Amount:
                {
                    if(dataItem is ConfigurationItemValueDataAmountData data)
                    {
                        var value = data.Value.Value;
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.Boolean:
                {
                    if(dataItem is bool)
                    {
                        result = (T)dataItem;
                    }
                    break;
                }
                case ConfigurationItemType.EnumerationList:
                {
                    if(dataItem is EnumerationList data)
                    {
                        var value = data.Item.ToList();
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.FlagList:
                {
                    if(dataItem is ConfigurationItemValueDataFlagListData data)
                    {
                        var value = data.FlagData.Select(flagData => new KeyValuePair<string, bool>(flagData.Item,
                            flagData.Value))
                            .ToList();
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.Float:
                {
                    if(dataItem is ConfigurationItemValueDataFloatData data)
                    {
                        var value = data.Value;
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.Int64:
                {
                    if(dataItem is ConfigurationItemValueDataInt64Data data)
                    {
                        var value = data.Value;
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.Item:
                {
                    if(dataItem is ConfigurationItemValueDataItemData data)
                    {
                        var value = data.Item;
                        result = (T)(object)value;
                    }
                    break;
                }
                case ConfigurationItemType.String:
                {
                    if(dataItem is ConfigurationItemValueDataStringData data)
                    {
                        var value = data.Value;
                        result = (T)(object)value;
                    }
                    break;
                }
                default:
                {
                    throw new ArgumentException(
                        $"Configuration item type {configType} is not supported.", nameof(configType));
                }
            }

            return result;
        }

        #endregion
    }
}