// -----------------------------------------------------------------------
// <copyright file = "IRegistryIndexer.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// This interface defines APIs to query the index of a registry (a theme, a payvar, or a configuration extension etc.).
    /// The indexes are used to retrieve data from the safe storage.
    /// </summary>
    /// <remarks>
    /// An example of usage is by standalone <see cref="GameConfigurationRead"/> to query the index of a specific
    /// extension registry which is required to retrieve a configuration value from the safe storage.
    /// </remarks>
    internal interface IRegistryIndexer
    {
        /// <summary>
        /// Gets the index of a specific theme registry.
        /// </summary>
        /// <param name="identifier">The identifier of the theme registry.</param>
        /// <returns>
        /// The index of the specified theme registry.
        /// Returns -1 if the specified registry does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="identifier"/> is null or empty;
        /// </exception>
        int GetThemeIndex(string identifier);

        /// <summary>
        /// Gets the index of a specific payvar registry.
        /// </summary>
        /// <param name="identifier">The identifier of the payvar registry.</param>
        /// <returns>
        /// The index of the specified payvar registry.
        /// Returns -1 if the specified registry does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="identifier"/> is null or empty;
        /// </exception>
        int GetPayvarIndex(string identifier);

        /// <summary>
        /// Gets the index of a specific configuration extension registry.
        /// </summary>
        /// <param name="identifier">The identifier of the configuration extension registry.</param>
        /// <returns>
        /// The index of the specified configuration extension registry.
        /// Returns -1 if the specified registry does not exist.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="identifier"/> is null or empty;
        /// </exception>
        int GetExtensionIndex(string identifier);
    }
}