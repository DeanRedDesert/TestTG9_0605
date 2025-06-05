//-----------------------------------------------------------------------
// <copyright file = "CustomConfigurationReader.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2LRegTypesVerTip = Core.Registries.Internal.F2L.F2LRegistryTypeVer2;

    /// <summary>
    /// This class holds the custom configuration from the theme or payvar registry.
    /// </summary>
    internal class CustomConfigurationReader : ICustomConfigurationReader
    {
        #region Fields

        /// <summary>
        /// The raw custom configuration item collection from the registry file.
        /// </summary>
        private readonly IEnumerable<F2LRegTypesVerTip.CustomConfigItemsTypeConfigItem> customConfigItems;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the instance with the raw custom configuration item collection from the registry file.
        /// </summary>
        /// <param name="customConfigItems">The raw custom configuration item collection from the registry file</param>
        public CustomConfigurationReader(IEnumerable<F2LRegTypesVerTip.CustomConfigItemsTypeConfigItem> customConfigItems = null)
        {
            this.customConfigItems = customConfigItems ?? new List<F2LRegTypesVerTip.CustomConfigItemsTypeConfigItem>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the KVP collection of custom configuration item values keyed by the configuration name.
        /// </summary>
        /// <returns>A collection of <![CDATA[IDictionary<string, KeyValuePair<ConfigurationProfile, object>>]]>/>.</returns>
        public IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItems()
        {
            var thisCustomConfigItems = new Dictionary<string, KeyValuePair<ConfigurationProfile, object>>();

            foreach(var configItem in customConfigItems)
            {
                if(configItem?.Data == null)
                {
                    continue;
                }

                try
                {
                    var configName = configItem.Name;
                    var data = configItem.Data?.Item;
                    var configData = new KeyValuePair<ConfigurationProfile, object>();

                    switch(data)
                    {
                        case F2LRegTypesVerTip.AmountType amountData:
                        {
                            var value = GetConfigItemAmountData(amountData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.Amount), value);
                            break;
                        }
                        case bool value:
                        {
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.Boolean), value);
                            break;
                        }
                        case F2LRegTypesVerTip.EnumerationType enumData:
                        {
                            var value = GetConfigItemEnumerationData(enumData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.EnumerationList), value);
                            break;
                        }
                        case F2LRegTypesVerTip.FlagListType flagListData:
                        {
                            var value = GetConfigItemFlagListData(flagListData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.FlagList, flagListData.EnumReferenceList),
                                value);
                            break;
                        }
                        case F2LRegTypesVerTip.FloatType floatData:
                        {
                            var value = GetConfigItemFloatData(floatData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.Float), value);
                            break;
                        }
                        case F2LRegTypesVerTip.Int64Type int64Data:
                        {
                            var value = GetConfigItemInt64Data(int64Data);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.Int64), value);
                            break;
                        }
                        case F2LRegTypesVerTip.ItemType itemData:
                        {
                            var value = GetConfigItemItemData(itemData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.Item, itemData.EnumReferenceList), value);
                            break;
                        }
                        case F2LRegTypesVerTip.StringType stringData:
                        {
                            var value = GetConfigItemStringData(stringData);
                            configData = new KeyValuePair<ConfigurationProfile, object>(new ConfigurationProfile(ConfigurationItemType.String), value);
                            break;
                        }
                    }

                    if(configData.Value != null)
                    {
                        thisCustomConfigItems.Add(configName, configData);
                    }
                }
                catch(GameRegistryException exception)
                {
                    throw new GameRegistryException($"Failed to read the custom configuration item \"{configItem.Name}\": ",
                        exception);
                }
            }

            return thisCustomConfigItems;
        }

        #endregion

        #region ICustomConfigurationReader Members

        /// <inheritDoc />
        public bool GetBooleanData(string configName)
        {
            return GetConfigurationItem<bool>(configName);
        }

        /// <inheritDoc />
        public string GetItemData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.ItemType>(configName);
            return GetConfigItemItemData(data);
        }

        /// <inheritDoc />
        public string GetStringData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.StringType>(configName);
            return GetConfigItemStringData(data);
        }

        /// <inheritDoc />
        public long GetInt64Data(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.Int64Type>(configName);
            return GetConfigItemInt64Data(data);
        }

        /// <inheritDoc />
        public float GetFloatData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.FloatType>(configName);
            return GetConfigItemFloatData(data);
        }

        /// <inheritDoc />
        public IEnumerable<KeyValuePair<string, bool>> GetFlagListData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.FlagListType>(configName);
            return GetConfigItemFlagListData(data);
        }

        /// <inheritDoc />
        public IEnumerable<string> GetEnumerationData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.EnumerationType>(configName);
            return GetConfigItemEnumerationData(data);
        }

        /// <inheritDoc />
        public long GetAmountData(string configName)
        {
            var data = GetConfigurationItem<F2LRegTypesVerTip.AmountType>(configName);
            return GetConfigItemAmountData(data);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get a custom configuration containing an enumeration value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>The string value of the selected value; a null string if no custom configuration
        /// items are present.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the value or the referenced enumeration list is invalid.
        /// </exception>
        private string GetConfigItemItemData(F2LRegTypesVerTip.ItemType item)
        {
            var value = item?.Value;

            var reference = item?.EnumReferenceList;

            if(value == null || string.IsNullOrEmpty(reference))
            {
                return default;
            }

            var referencedConfig = customConfigItems.FirstOrDefault(thisItem => thisItem?.Name == reference);

            if(referencedConfig?.Data.Item is F2LRegTypesVerTip.EnumerationType enumData)
            {
                var referencedEnumeration = enumData.Enumeration.FirstOrDefault(thisEnum => thisEnum.Value == value);

                if(referencedEnumeration == null)
                {
                    throw new GameRegistryException($"The value {value} is not a valid enumeration of {reference}.");
                }
            }
            else
            {
                throw new GameRegistryException($"The referenced enumeration list {reference} is invalid.");
            }

            return value;
        }

        /// <summary>
        /// Get a custom configuration containing a string value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>The Value field of the configuration item; string.Empty otherwise.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private string GetConfigItemStringData(F2LRegTypesVerTip.StringType item)
        {
            var value = item.Value ?? string.Empty;

            // Verify the value is valid.
            if(item.MinLenSpecified && value.Length < item.MinLen)
            {
                throw new GameRegistryException(
                    $"Length of {value} is less than Min limit {item.MinLen}.");
            }
            if(item.MaxLenSpecified && value.Length > item.MaxLen)
            {
                throw new GameRegistryException(
                    $"Length of {value} is greater than Max limit {item.MaxLen}.");
            }

            return value;
        }

        /// <summary>
        /// Get a custom configuration containing a long value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private long GetConfigItemInt64Data(F2LRegTypesVerTip.Int64Type item)
        {
            var value = item.Value;

            // Verify the value is valid.
            if(item.MinSpecified && value < item.Min)
            {
                throw new GameRegistryException($"Invalid value: {value} is less than Min limit {item.Min}.");
            }
            if(item.MaxSpecified && value > item.Max)
            {
                throw new GameRegistryException($"Invalid value: {value} is greater than Max limit {item.Max}.");
            }

            return value;
        }

        /// <summary>
        /// Get a custom configuration containing a float value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private float GetConfigItemFloatData(F2LRegTypesVerTip.FloatType item)
        {
            var value = item.Value;

            // Verify the value is valid.
            if(item.MinSpecified && value < item.Min)
            {
                throw new GameRegistryException(
                    $"Invalid value: {value} is less than Min limit {item.Min}.");
            }
            if(item.MaxSpecified && value > item.Max)
            {
                throw new GameRegistryException(
                    $"Invalid value: {value} is greater than Max limit {item.Max}.");
            }

            return value;
        }

        /// <summary>
        /// Get a custom configuration containing a flag list.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>A <![CDATA[List<KeyValuePair<string, bool>>]]> collection of flag names to boolean values.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the referenced enumeration list is invalid.
        /// </exception>
        private List<KeyValuePair<string, bool>> GetConfigItemFlagListData(F2LRegTypesVerTip.FlagListType item)
        {
            var value = new List<KeyValuePair<string, bool>>();
            var trueList = item.Element.Select(thisItem => thisItem.Value).ToList();
            var reference = item.EnumReferenceList;
            var referencedConfig = customConfigItems.FirstOrDefault(thisItem => thisItem.Name == reference);

            if(referencedConfig?.Data.Item is F2LRegTypesVerTip.EnumerationType enumData)
            {
                var fullList = enumData.Enumeration.Select(thisEnum => thisEnum.Value);

                value.AddRange(fullList.Select(thisEnum => new KeyValuePair<string, bool>(thisEnum, trueList.Contains(thisEnum))));
            }
            else
            {
                throw new GameRegistryException($"The referenced enumeration list {reference} is invalid.");
            }

            return value;
        }

        /// <summary>
        /// Get a configuration item containing an enumeration.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>A list of the valid values of the enumeration.</returns>
        private List<string> GetConfigItemEnumerationData(F2LRegTypesVerTip.EnumerationType item)
        {
            return item.Enumeration.Select(enumeration => enumeration.Value).ToList();
        }

        /// <summary>
        /// Get a custom configuration item containing an amount.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private long GetConfigItemAmountData(F2LRegTypesVerTip.AmountType item)
        {
            var value = item.Value;

            // Verify the value is valid.
            if(item.MinSpecified && value < item.Min)
            {
                throw new GameRegistryException(
                    $"Invalid value: {value} is less than Min limit {item.Min}.");
            }
            if(item.MaxSpecified && value > item.Max)
            {
                throw new GameRegistryException(
                    $"Invalid value: {value} is greater than Max limit {item.Max}.");
            }

            return value;
        }

        /// <summary>
        /// Get the custom configuration item object by the configuration name.
        /// </summary>
        /// <param name="configName">The configuration name.</param>
        /// <returns>The custom configuration item object.</returns>
        /// <exception cref="GameRegistryException">Thrown when the configuration item is not found,
        /// the type specified is not equal to the type found by the config name, or no items are present.</exception>
        private T GetConfigurationItem<T>(string configName)
        {
            var configItem = customConfigItems?.FirstOrDefault(item => item.Name == configName);
            if(configItem == null)
            {
                throw new GameRegistryException($"Custom configuration item \"{configName}\" is not found");
            }

            if(!(configItem.Data.Item is T dataItem))
            {
                throw new GameRegistryException($"Invalid type {GetTypeString<T>()} of the custom configuration item \"{configName}\".");
            }

            return dataItem;
        }

        /// <summary>
        /// Get the type string of the configuration item value.
        /// </summary>
        /// <typeparam name="T">The type of the configuration item value.</typeparam>
        /// <returns>The type string of the configuration item value.</returns>
        private string GetTypeString<T>()
        {
            if(typeof(T) == typeof(F2LRegTypesVerTip.AmountType))
            {
                return "AmountData";
            }
            if(typeof(T) == typeof(bool))
            {
                return "BooleanData";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.StringType))
            {
                return "StringData";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.EnumerationType))
            {
                return "EnumerationData";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.FlagListType))
            {
                return "FlagListData";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.Int64Type))
            {
                return "Int64Data";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.ItemType))
            {
                return "ItemData";
            }
            if(typeof(T) == typeof(F2LRegTypesVerTip.FloatType))
            {
                return "FloatData";
            }
            return "Unknown data type";
        }

        #endregion
    }
}
