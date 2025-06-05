//-----------------------------------------------------------------------
// <copyright file = "ConfigurationExtensionManager.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ascent.Build.EmbeddedResources;
    using Core.Registries.Internal.F2X.F2XConfigurationExtensionRegistryVer1;
    using Core.Registries.Internal.F2X.F2XRegistryVer1;

    /// <summary>
    /// This class manages configuration extensions and provides the necessary information 
    /// for saving configuration items to safe storage.
    /// </summary>
    internal sealed class ConfigurationExtensionManager : IConfigurationExtensionManager
    {
        #region Private Fields

        /// <summary>
        /// List of discovered extension registries.
        /// </summary>
        private readonly List<ExtensionRegistryProxy> discoveredExtensionRegistries;

        /// <summary>
        /// The list of the configuration providers discovered within the game package 
        /// and SDK extension directory.
        /// </summary>
        private readonly List<ExtensionRegistryProxy> discoveredConfigurationProviders =
            new List<ExtensionRegistryProxy>();

        /// <summary>
        /// The list of configuration providers that have been verified through a theme's extension import list.
        /// </summary>
        private readonly List<ExtensionRegistryProxy> linkedConfigurationProviders =
            new List<ExtensionRegistryProxy>();

        /// <summary>
        /// The registry map which is indexed the interface definition registry identifier.
        /// </summary>
        private readonly Dictionary<Guid, int> providerIndices = new Dictionary<Guid, int>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationExtensionManager"/> for standalone support.
        /// This class is responsible for writing configuration items to disk based on what is 
        /// requested in an extension import list and verifying that games are not directly 
        /// linking to a configuration provider when an interface exists. 
        /// </summary>
        /// <param name="extensionRegistries">
        /// The entire collection of discovered extension registries. Configuration Providers will be 
        /// separated out from the collection and used by ConfigurationExtensionManager to 
        /// link configuration interfaces with providers.
        /// </param>
        public ConfigurationExtensionManager(IList<IExtensionRegistry> extensionRegistries)
        {
            discoveredExtensionRegistries = extensionRegistries != null
                                                ? extensionRegistries.Cast<ExtensionRegistryProxy>().ToList()
                                                : new List<ExtensionRegistryProxy>();

            // Load signed configuration providers supported by the Standalone SDK.
            discoveredExtensionRegistries.AddRange(LoadSignedConfigurationExtensions());

            // Load all available configuration providers.
            foreach(var registryProxy in discoveredExtensionRegistries)
            {
                if(registryProxy.RegistryContext.RegistryType == RegistryType.ConfigurationExtension)
                {
                    // Check if duplicate provider was found.
                    var duplicate =
                        discoveredConfigurationProviders.SingleOrDefault(
                            provider => provider.RegistryContext.Identifier == registryProxy.RegistryContext.Identifier);

                    if(duplicate == null)
                    {
                        discoveredConfigurationProviders.Add(registryProxy);
                    }
                    else
                    {
                        // If the current stored provider has a lesser version than the recently discovered,
                        // replace the provider with the more recent version.
                        if(duplicate.RegistryContext.Version < registryProxy.RegistryContext.Version)
                        {
                            discoveredConfigurationProviders.Remove(duplicate);
                            discoveredConfigurationProviders.Add(registryProxy);
                        }
                    }
                }
            }
        }

        #endregion

        #region IConfigurationExtensionManager

        /// <inheritdoc />
        public void VerifyConfigurationExtensionsInExtensionImportList(IThemeRegistry themeRegistry)
        {
            foreach(var extensionImport in themeRegistry.GetExtensionImports())
            {
                var extensionRegistry = discoveredExtensionRegistries.FirstOrDefault(
                        e => e.RegistryContext.Identifier == extensionImport.ExtensionIdentifier);

                if(extensionRegistry != null)
                {
                    switch(extensionRegistry.RegistryContext.RegistryType)
                    {
                        // Check if interface exists
                        case RegistryType.ConfigurationExtension when extensionRegistry.RegistryContext.InterfaceIdentifier != Guid.Empty:
                            throw new GameRegistryException(
                                $"Configuration Extension Error: Theme({themeRegistry.ThemeName}) " +
                                $"is trying to directly link to Extension {extensionRegistry.RegistryContext.Name} (ID: {extensionRegistry.RegistryContext.Identifier})" +
                                $"Please use its respective interface(ID: {extensionRegistry.RegistryContext.InterfaceIdentifier})"
                                );
                        case RegistryType.ConfigurationExtensionInterfaceDefinition:
                            LinkConfigurationExtensionInterface(extensionRegistry);
                            break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public IList<Guid> LinkedIdentifiers => providerIndices.Keys.ToList();

        /// <inheritdoc />
        public int GetProviderIndex(Guid identifier)
        {
            if(identifier == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if(!providerIndices.TryGetValue(identifier, out var result))
            {
                result = -1;
            }

            return result;
        }

        /// <inheritdoc />
        public IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues(Guid identifier,
                                                                                                          string jurisdiction)
        {
            if(jurisdiction == null)
            {
                throw new ArgumentNullException(nameof(jurisdiction));
            }

            IDictionary<string, KeyValuePair<ConfigurationProfile, object>> result;

            var providerIndex = GetProviderIndex(identifier);

            if(providerIndex != -1)
            {
                var configurationExtension = (ConfigurationExtensionRegistry)linkedConfigurationProviders[providerIndex].RegistryContext.GetRegistry();
                result = ExtensionConfigurationReader.ReadItemValues(configurationExtension.VersionedConfigSections,
                                                                     jurisdiction);
            }
            else
            {
                result = new Dictionary<string, KeyValuePair<ConfigurationProfile, object>>();
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Links the configuration interface to its respective configuration provider.
        /// </summary>
        /// <param name="interfaceRegistry">
        /// The interface registry requested by the game to be linked to a provider.
        /// </param>
        /// <remarks>This method is only capable of linking to a single provider within a game package.</remarks>
        private void LinkConfigurationExtensionInterface(ExtensionRegistryProxy interfaceRegistry)
        {
            foreach(var provider in discoveredConfigurationProviders)
            {
                // Match the interface with its appropriate provider and
                // verify that the provider supports the interface included
                // in the game package.
                if(provider.RegistryContext.InterfaceIdentifier == interfaceRegistry.RegistryContext.Identifier &&
                   provider.RegistryContext.InterfaceVersion <= interfaceRegistry.RegistryContext.Version)
                {
                    // TODO look into supporting multiple configuration providers.
                    // Check if a provider has already been linked by a previous theme.
                    if(!providerIndices.ContainsKey(interfaceRegistry.RegistryContext.Identifier))
                    {
                        providerIndices[interfaceRegistry.RegistryContext.Identifier] = linkedConfigurationProviders.Count;

                        linkedConfigurationProviders.Add(provider);
                    }
                }
            }
        }

        /// <summary>
        /// Loads already signed and verified configuration provider registries put 
        /// into an outside assembly as embedded resources.
        /// </summary>
        private IList<ExtensionRegistryProxy> LoadSignedConfigurationExtensions()
        {
            var signedConfigurationExtensions = new List<ExtensionRegistryProxy>();

            // NOTE: this implementation differs from the non-UPM implementation due to dependency to ResourcesAssembly.
            var assembly = ResourcesAssembly.GetCurrent();

            var internalConfigurationProviders = assembly.GetManifestResourceNames();
            foreach(var configurationProvider in internalConfigurationProviders)
            {
                if(Path.GetExtension(configurationProvider) == ".xextensionreg")
                {
                    using(var stream = assembly.GetManifestResourceStream(configurationProvider))
                    {
                        var registry = Registry.GetRegistryFromStream(stream);

                        if(registry.RegistryType == RegistryType.ConfigurationExtension)
                        {
                            var context = new ExtensionRegistryContext(registry);
                            var registryProxy = context.GetRegistry() == null
                                                    ? null
                                                    : new ExtensionRegistryProxy(context);

                            if(registryProxy != null)
                            {
                                signedConfigurationExtensions.Add(registryProxy);
                            }
                        }
                    }
                }
            }

            return signedConfigurationExtensions;
        }

        #endregion
    }
}