// -----------------------------------------------------------------------
// <copyright file = "ExtensionConfigurationReader.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Core.Registries.Internal.F2X.F2XConfigurationExtensionRegistryVer1;
    using Core.Registries.Internal.F2X.F2XRegistryTypesVer1;

    /// <summary>
    /// This class is used to read configuration item values from the configuration extension registry.
    /// </summary>
    internal static class ExtensionConfigurationReader
    {
        /// <summary>
        /// Read the items values from the list of configuration sections.
        /// </summary>
        /// <param name="versionedSections">The configuration section list with different versions.</param>
        /// <param name="jurisdiction">
        /// The jurisdiction string is used to retrieve the jurisdiction override values if any from the
        /// configuration provider. An empty string means no jurisdiction will be applied.
        /// </param>
        /// <returns>The configuration item values indexed by the configuration name from the registry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameter is null.</exception>
        public static IDictionary<string, KeyValuePair<ConfigurationProfile, object>> ReadItemValues(
            IList<VersionedConfigSection> versionedSections, string jurisdiction)
        {
            if(versionedSections == null)
            {
                throw new ArgumentNullException(nameof(versionedSections));
            }

            if(jurisdiction == null)
            {
                throw new ArgumentNullException(nameof(jurisdiction));
            }

            var combinedItemList = versionedSections.SelectMany(section => section.ConfigItems);
            var combinedOverrides = versionedSections.
                SelectMany(overrideSection => overrideSection.Overrides).
                Where(overrideSection => overrideSection.Jurisdictions.Contains(jurisdiction)).
                SelectMany(overrideItem => overrideItem.ConfigItemValues);
            return GetItemValues(combinedItemList.ToList(), combinedOverrides.ToList());
        }

        /// <summary>
        /// Gets all the configuration items from a specific section in the registry.
        /// </summary>
        /// <param name="items">The combined items crossing versions from the registry.</param>
        /// <param name="overrideItems">The combined override items crossing versions from the registry.</param>
        /// <returns>The configuration items from the specified section.</returns>
        private static IDictionary<string, KeyValuePair<ConfigurationProfile, object>>GetItemValues(
            IList<CustomConfigItemsTypeConfigItem> items,
            IList<ConfigItemValue> overrideItems)
        {
            var result = new Dictionary<string, KeyValuePair<ConfigurationProfile, object>>();
            var referencedItemsList = items.ToList();
            // ReSharper disable once PossibleMultipleEnumeration
            foreach(var item in items)
            {
                ConfigurationProfile profile = default;
                object value = null;
                object overrideValue = null;
                if(overrideItems.Any())
                {
                    var jurisdictionOverride =
                        overrideItems.FirstOrDefault(overrideItem => overrideItem.Name == item.Name);
                    if(jurisdictionOverride != null)
                    {
                        overrideValue = jurisdictionOverride.Item;
                    }
                }

                switch(item.Data.Item)
                {
                    // ReSharper disable once CanBeReplacedWithTryCastAndCheckForNull
                    case AmountType amountData:
                        value = GetConfigItemAmountData(amountData, overrideValue);
                        profile = new ConfigurationProfile(ConfigurationItemType.Amount);
                        break;
                    case bool _:
                        value = (bool)(overrideValue ?? item.Data.Item);
                        profile = new ConfigurationProfile(ConfigurationItemType.Boolean);
                        break;
                    case EnumerationType enumData:
                        value = GetConfigItemEnumerationData(enumData);
                        profile = new ConfigurationProfile(ConfigurationItemType.EnumerationList);
                        break;
                    case FlagListType flagListData:
                        value = GetConfigItemFlagListData(flagListData,
                            referencedItemsList, overrideValue);
                        profile = new ConfigurationProfile(ConfigurationItemType.FlagList);
                        break;
                    case FloatType floatData:
                        value = GetConfigItemFloatData(floatData, overrideValue);
                        profile = new ConfigurationProfile(ConfigurationItemType.Float);
                        break;
                    case Int64Type int64Data:
                        value = GetConfigItemInt64Data(int64Data, overrideValue);
                        profile = new ConfigurationProfile(ConfigurationItemType.Int64);
                        break;
                    case ItemType itemData:
                        value = GetConfigItemItemData(itemData, referencedItemsList, overrideValue);
                        profile = new ConfigurationProfile(
                            ConfigurationItemType.Item,
                            itemData.EnumReferenceList);
                        break;
                    case StringType stringData:
                        value = GetConfigItemStringData(stringData, overrideValue);
                        profile = new ConfigurationProfile(ConfigurationItemType.String);
                        break;
                }
                if(value != null)
                {
                    result.Add(
                        item.Name,
                        new KeyValuePair<ConfigurationProfile, object>(profile, value));
                }
            }
            return result;
        }

        /// <summary>
        /// Get a custom configuration containing an enumeration value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <param name="configItems">The referenced configuration item list.</param>
        /// <param name="overrideValue">This value will override the item value if not null.</param>
        /// <returns>The string value of the selected value; a null string if no custom configuration
        /// items are present.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the value or the referenced enumeration list is invalid.
        /// </exception>
        private static string GetConfigItemItemData(ItemType item, List<CustomConfigItemsTypeConfigItem> configItems,
                                                    object overrideValue)
        {
            var value = overrideValue == null ? item.Value : (string)overrideValue;
            var reference = item.EnumReferenceList;
            var referencedConfig = configItems.FirstOrDefault(itemValue => itemValue.Name == reference);

            if(referencedConfig?.Data.Item is EnumerationType enumData)
            {
                var referencedEnumeration = enumData
                    .Enumeration.FirstOrDefault(e => e.Value == value);

                if(referencedEnumeration == null)
                {
                    throw new GameRegistryException(
                        $"The value {value} is not a valid enumeration of {reference}.");
                }
            }
            else
            {
                throw new GameRegistryException(
                    $"The referenced enumeration list {reference} is invalid.");
            }

            return value;
        }

        /// <summary>
        /// Get a custom configuration containing an string value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <param name="overrideValue">This value will override the item value if not null.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private static string GetConfigItemStringData(StringType item, object overrideValue)
        {
            var value = (overrideValue == null ? item.Value : (string)overrideValue) ?? string.Empty;

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
        /// <param name="overrideValue">This value will override the item value if not null.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private static long GetConfigItemInt64Data(Int64Type item, object overrideValue)
        {
            var value = (long?)overrideValue ?? item.Value;

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
        /// Get a custom configuration containing a float value.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <param name="overrideValue">This value will override the item value if not null.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private static float GetConfigItemFloatData(FloatType item, object overrideValue)
        {
            var value = (float?)overrideValue ?? item.Value;

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
        /// <param name="configItems">The referenced item list to look up for the enumeration list.</param>
        /// <param name="overrideValue">This value will override the item value if not null.</param>
        /// <returns>
        /// A <![CDATA[List<KeyValuePair<string, bool>>]]> collection of flag names to boolean values.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the referenced enumeration list is invalid.
        /// </exception>
        private static List<KeyValuePair<string, bool>> GetConfigItemFlagListData(
            FlagListType item,
            List<CustomConfigItemsTypeConfigItem> configItems,
            object overrideValue)
        {
            var value = new List<KeyValuePair<string, bool>>();
            var trueList = (overrideValue == null ? item : (FlagListType)overrideValue)
                .Element.Select(element => element.Value).ToList();
            var reference = item.EnumReferenceList;
            var referencedConfig = configItems.FirstOrDefault(itemValue => itemValue.Name == reference);

            if(referencedConfig?.Data.Item is EnumerationType enumData)
            {
                var fullList =
                    enumData.Enumeration.Select(
                        element => element.Value);

                value.AddRange(
                    fullList.Select(element => new KeyValuePair<string, bool>(element, trueList.Contains(element))));
            }
            else
            {
                throw new GameRegistryException(
                    $"The referenced enumeration list {reference} is invalid.");
            }

            return value;
        }

        /// <summary>
        /// Get a configuration item containing an enumeration.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <returns>A list of the valid values of the enumeration.</returns>
        private static List<string> GetConfigItemEnumerationData(EnumerationType item)
        {
            return item.Enumeration.Select(value => value.Value).ToList();
        }

        /// <summary>
        /// Get a custom configuration item containing an amount.
        /// </summary>
        /// <param name="item">The configuration item.</param>
        /// <param name="valueOverride">This value will override the item value if not null.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        private static long GetConfigItemAmountData(AmountType item, object valueOverride)
        {
            var value = (long?)valueOverride ?? item.Value;

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
    }
}