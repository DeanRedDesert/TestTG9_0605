// -----------------------------------------------------------------------
// <copyright file = "IConfigurationReadLink.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This interface represents a link to the custom configuration items defined in game or extension registries.
    /// </summary>
    internal interface IConfigurationReadLink
    {
        /// <summary>
        /// Get the type of a configuration item from the Foundation.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>
        /// The type of the configuration item. Returns <see cref="ConfigurationItemType.Invalid"/>
        /// if the configuration item is not defined.
        /// </returns>
        ConfigurationItemType GetConfigurationType(ConfigurationItemKey configKey);

        /// <summary>
        /// Get the types of a list of configuration items.
        /// </summary>
        /// <param name="configKeys">The list of keys identifying the configuration items.</param>
        /// <returns>
        /// The types of the configuration items, each with its corresponding key.
        /// The <see cref="ConfigurationItemType.Invalid"/> is returned for an <paramref name="configKeys"/> item
        /// is not defined.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configKeys"/> is null.
        /// </exception>
        IDictionary<ConfigurationItemKey, ConfigurationItemType> GetConfigurationTypes(
            IList<ConfigurationItemKey> configKeys);

        /// <summary>
        /// Get the list of configuration items at a given scope.
        /// </summary>
        /// <param name="scopeKey">The data to identify a unique configuration scope.</param>
        /// <returns>
        /// List of configuration items and their types in the given scope.
        /// Empty if there is no configuration items available in the given scope.
        /// </returns>
        IDictionary<string, ConfigurationItemType> QueryConfigurations(ConfigurationScopeKey scopeKey);

        /// <summary>
        /// Get the value of a configuration item with a specific type.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The <see cref="ConfigurationItemType"/> of the configuration item.</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>
        /// The value of the configuration item if defined.
        /// </returns>
        /// <exception cref="ConfigurationNotDefinedException">
        /// Thrown when the acquired configuration item is not defined in the registry.
        /// </exception>
        T GetConfiguration<T>(ConfigurationItemKey configKey, ConfigurationItemType configType);

        /// <summary>
        /// Get the values of a list of configuration items identified by a key and of a specified type.
        /// </summary>
        /// <param name="configKeysAndTypes">The list of pairs of a key and a configuration type.</param>
        /// <returns>
        /// The list of configuration item values, each is of object type. The value is null if the configuration
        /// item is not defined.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configKeysAndTypes"/> is null.
        /// </exception>
        IDictionary<ConfigurationItemKey, object> GetConfigurations(
            IDictionary<ConfigurationItemKey, ConfigurationItemType> configKeysAndTypes);

        /// <summary>
        /// Retrieve the enumeration declaration list referenced by an <see cref="ConfigurationItemType.Item"/>
        /// type of configuration item. 
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>
        /// The enumeration declaration list referenced by the specified configuration item.
        /// Null if the specified configuration item does not exist, or does not reference an enumeration list.
        /// </returns>
        ICollection<string> QueryReferencedEnumDeclaration(ConfigurationItemKey configKey);

        /// <summary>
        /// Retrieve the enumeration declaration lists for a list of <see cref="ConfigurationItemType.Item"/>
        /// type of configuration items.
        /// </summary>
        /// <param name="configKeys">The keys identifying the configuration items.</param>
        /// <returns>
        /// The list of enumeration declaration lists.  Each enumeration declaration list
        /// corresponds to a configuration key.  The declaration list will be null if the
        /// corresponding configuration item does not exist, or does not reference an
        /// enumeration list.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configKeys"/> is null.
        /// </exception>
        IDictionary<ConfigurationItemKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IList<ConfigurationItemKey> configKeys);
    }
}