//-----------------------------------------------------------------------
// <copyright file = "ICustomConfigurationReader.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System.Collections.Generic;

    /// <summary>
    /// This interface is used to retrieve custom configuration values.
    /// </summary>
    public interface ICustomConfigurationReader
    {
        /// <summary>
        /// Get a custom configuration containing a Boolean value.
        /// </summary>
        /// <param name="configName">The configuration item name.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the value or the referenced enumeration list is invalid.
        /// </exception>
        bool GetBooleanData(string configName);

        /// <summary>
        /// Get a custom configuration containing an enumeration value.
        /// </summary>
        /// <param name="configName">The configuration item name.</param>
        /// <returns>The string value of the selected value.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value or the referenced enumeration list
        /// is invalid.</exception>
        string GetItemData(string configName);

        /// <summary>
        /// Get a custom configuration containing a string value.
        /// </summary>
        /// <param name="configName">The configuration item name.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        string GetStringData(string configName);

        /// <summary>
        /// Get a custom configuration containing a long value.
        /// </summary>
        /// <param name="configName">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        long GetInt64Data(string configName);

        /// <summary>
        /// Get a custom configuration containing a float value.
        /// </summary>
        /// <param name="configName">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        float GetFloatData(string configName);

        /// <summary>
        /// Get a custom configuration containing a flag list.
        /// </summary>
        /// <param name="configName">The configuration item.</param>
        /// <returns>A map list of flag names to boolean values.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when the referenced enumeration list is invalid.
        /// </exception>
        IEnumerable<KeyValuePair<string, bool>> GetFlagListData(string configName);

        /// <summary>
        /// Get a configuration item containing an enumeration.
        /// </summary>
        /// <param name="configName">The configuration item.</param>
        /// <returns>A list of the valid values of the enumeration.</returns>
        IEnumerable<string> GetEnumerationData(string configName);

        /// <summary>
        /// Get a custom configuration item containing an amount.
        /// </summary>
        /// <param name="configName">The configuration item.</param>
        /// <returns>The value of the configuration item.</returns>
        /// <exception cref="GameRegistryException">Thrown when the value is invalid.</exception>
        long GetAmountData(string configName);
    }
}