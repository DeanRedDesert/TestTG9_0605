//-----------------------------------------------------------------------
// <copyright file = "IConfigurationExtensionManager.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;

    internal interface IConfigurationExtensionManager
    {
        /// <summary>
        /// Gets the identifiers of all linked configuration extension interface definitions.
        /// </summary>
        IList<Guid> LinkedIdentifiers { get; }

        /// <summary>
        /// Gets the provider's index by its corresponding interface definition identifier.
        /// </summary>
        /// <param name="identifier">The identifier of the configuration interface definition.</param>
        /// <returns>
        /// The index of the provider for the specified interface definition.
        /// </returns>
        int GetProviderIndex(Guid identifier);

        /// <summary>
        /// Gets all the configuration item values by an extension provider for the given interface identifier .
        /// </summary>
        /// <param name="identifier">
        /// The identifier of the configuration extension interface definition.
        /// </param>
        /// <param name="jurisdiction">
        /// The jurisdiction string which is used to retrieve the jurisdiction overrides from the
        /// configuration extension provider registry.
        /// </param>
        /// <returns>
        /// The configuration item values of this configuration extension, indexed by the configuration name.
        /// </returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when error occurs while retrieving values of the custom configuration items from the theme registry.
        /// </exception>
        IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues(Guid identifier,
                                                                                                   string jurisdiction);

        /// <summary>
        /// Verify a Theme Registry's extension import list.
        /// </summary>
        /// <param name="themeRegistry">
        /// The corresponding themeRegistry that will have its extension import list examined.
        /// <list type="number">
        /// <item>
        /// A theme registry's extension import list cannot directly link to a configuration provider.
        /// It should link to an interface definition instead.
        /// </item>
        /// </list>
        /// </param>
        /// <exception cref="GameRegistryException">
        /// Thrown when a theme tries to directly link to an extension provider.
        /// </exception>
        void VerifyConfigurationExtensionsInExtensionImportList(IThemeRegistry themeRegistry);
    }
}
