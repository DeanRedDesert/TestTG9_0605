// -----------------------------------------------------------------------
// <copyright file = "IGenericConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// This interface defines the methods to request information about
    /// custom configuration items declared by the game or extension in registry files.
    /// </summary>
    /// <typeparam name="TConfigurationKey">The type of the configuration item key.</typeparam>
    /// <typeparam name="TConfigurationScopeKey">The type of the configuration scope key.</typeparam>
    public interface IGenericConfigurationRead<TConfigurationKey, in TConfigurationScopeKey>
    {
        /// <summary>
        /// Check if a configuration item exists.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>True if the configuration item exists.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        bool IsConfigurationDefined(TConfigurationKey configKey);

        /// <summary>
        /// Check if a configuration item exists.  If yes, returns its config type.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The config type of the configuration item.</param>
        /// <returns>True if the configuration item exists.  False otherwise.</returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        bool IsConfigurationDefined(TConfigurationKey configKey, out ConfigurationItemType configType);

        /// <summary>
        /// Get the type of a configuration item.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>
        /// The type of the configuration item. Returns <see cref="ConfigurationItemType.Invalid"/>
        /// if the configuration item is not defined.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        ConfigurationItemType GetConfigurationType(TConfigurationKey configKey);

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
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        IDictionary<TConfigurationKey, ConfigurationItemType> GetConfigurationTypes(
            IEnumerable<TConfigurationKey> configKeys);

        /// <summary>
        /// Get the value of a configuration item without specifying its config type.
        /// This function will firstly query the type of the configuration item, then
        /// retrieve the value, which makes it slower than the version called with
        /// the config type specified.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>
        /// The value of the configuration item, or default value of the element type of the configuration item
        /// if the <paramref name="configKey"/> is not defined.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        /// <exception cref="ConfigurationNotDefinedException">
        /// Thrown when the configuration item being accessed is not defined.
        /// </exception>
        T GetConfiguration<T>(TConfigurationKey configKey);

        /// <summary>
        /// Get the value of a configuration item with a specific type.
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <param name="configType">The <see cref="ConfigurationItemType"/> of the configuration item.</param>
        /// <typeparam name="T">The element type of the configuration item.</typeparam>
        /// <returns>
        /// The value of the configuration item, or default value of the element type of the configuration item
        /// if the <paramref name="configKey"/> is not defined.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        /// <exception cref="ConfigurationNotDefinedException">
        /// Thrown when the configuration item being accessed is not defined.
        /// </exception>
        T GetConfiguration<T>(TConfigurationKey configKey, ConfigurationItemType configType);

        /// <summary>
        /// Get the values of a list of configuration items identified by a key and of a specified type.
        /// </summary>
        /// <param name="configKeysAndTypes">The list of pairs of a key and a configuration type.</param>
        /// <returns>
        /// The list of configuration item values, each is of object type. The object is null if the configuration
        /// item is not defined.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="configKeysAndTypes"/> is null.
        /// </exception>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there is no configuration scope identifier defined in any of the configuration item key.
        /// </exception>
        IDictionary<TConfigurationKey, object> GetConfigurations(
            IDictionary<TConfigurationKey, ConfigurationItemType> configKeysAndTypes);

        /// <summary>
        /// Retrieve the enumeration declaration list referenced by an <see cref="ConfigurationItemType.Item"/>
        /// type of configuration item. 
        /// </summary>
        /// <param name="configKey">The key identifying the configuration item.</param>
        /// <returns>
        /// The enumeration declaration list referenced by the specified configuration item.
        /// Null if the specified configuration item does not exist, or does not reference an enumeration list.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        ICollection<string> QueryReferencedEnumDeclaration(TConfigurationKey configKey);

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
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        IDictionary<TConfigurationKey, ICollection<string>> QueryReferencedEnumDeclarations(
            IEnumerable<TConfigurationKey> configKeys);

        /// <summary>
        /// Get the list of configuration items at a given scope.
        /// </summary>
        /// <param name="scopeKey">The data to identify a unique configuration scope.</param>
        /// <returns>
        /// List of configuration items and their types in the given scope.
        /// Empty if there is no configuration items available in the given scope.
        /// </returns>
        /// <exception cref="InvalidTransactionException">
        /// Thrown when the function is called while there is no open transaction available.
        /// </exception>
        /// <exception cref="ConfigurationAccessDeniedException">
        /// Thrown when the access to the configuration data is denied.
        /// </exception>
        IDictionary<string, ConfigurationItemType> QueryConfigurations(TConfigurationScopeKey scopeKey);
    }
}