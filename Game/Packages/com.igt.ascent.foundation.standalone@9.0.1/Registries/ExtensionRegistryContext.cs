// -----------------------------------------------------------------------
// <copyright file = "ExtensionRegistryContext.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using Core.Registries.Internal.F2X.F2XConfigurationExtensionInterfaceDefinitionRegistryVer1;
    using Core.Registries.Internal.F2X.F2XConfigurationExtensionRegistryVer1;
    using Core.Registries.Internal.F2X.F2XRegistryVer1;
    using Core.Registries.Internal.F2X.F2XResourceExtensionRegistryVer1;

    /// <summary>
    /// This class is used to hold the data for different extension registry types.
    /// </summary>
    internal class ExtensionRegistryContext
    {
        #region Private Fields

        /// <summary>
        /// The target extension registry object.
        /// </summary>
        private readonly object registryObject;

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the context with the target extension registry data.
        /// </summary>
        /// <param name="registry">The extension registry schema object.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the reference parameter is null.
        /// </exception>
        /// <remarks>
        /// Currently only the following registries are loaded as needed.
        /// <list type="bullet">
        ///   <item>ResourceExtensionRegistry</item>
        ///   <item>ConfigurationExtensionRegistry</item>
        ///   <item>ConfigurationExtensionInterfaceDefinitionRegistry</item>
        /// </list>
        /// </remarks>
        public ExtensionRegistryContext(Registry registry)
        {
            if(registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }

            switch(registry.RegistryType)
            {
                case RegistryType.ResourceExtension:
                    var resourceExtensionRegistry = Registry.Load<ResourceExtensionRegistry>(registry);
                    resourceExtensionRegistry.ResourceDirectoryBase =
                        Utility.UniformSlashes(resourceExtensionRegistry.ResourceDirectoryBase);
                    Name = resourceExtensionRegistry.Name;
                    RegistryType = RegistryType.ResourceExtension;
                    Identifier = ValidateIdentifier(resourceExtensionRegistry.Identifier);
                    Version = resourceExtensionRegistry.Version.ToVersion();
                    InterfaceVersion = resourceExtensionRegistry.InterfaceDefinition.InterfaceVersion.ToVersion();
                    registryObject = resourceExtensionRegistry;
                    break;
                case RegistryType.ConfigurationExtension:
                    var configurationRegistry = Registry.Load<ConfigurationExtensionRegistry>(registry);
                    Name = configurationRegistry.Name;
                    RegistryType = RegistryType.ConfigurationExtension;
                    Identifier = ValidateIdentifier(configurationRegistry.Identifier);
                    Version = configurationRegistry.Version.ToVersion();
                    InterfaceIdentifier = ValidateInterfaceIdentifier(configurationRegistry.InterfaceDefinition.InterfaceIdentifier);
                    InterfaceVersion = configurationRegistry.InterfaceDefinition.InterfaceVersion.ToVersion();
                    registryObject = configurationRegistry;
                    break;
                case RegistryType.ConfigurationExtensionInterfaceDefinition:
                    var interfaceRegistry = Registry.Load<ConfigurationExtensionInterfaceDefinitionRegistry>(registry);
                    Name = interfaceRegistry.Name;
                    Identifier = ValidateIdentifier(interfaceRegistry.Identifier);
                    RegistryType = RegistryType.ConfigurationExtensionInterfaceDefinition;
                    Version = interfaceRegistry.Version.ToVersion();
                    registryObject = interfaceRegistry;
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the target extension registry object.
        /// </summary>
        /// <returns>The extension registry object.</returns>
        public object GetRegistry()
        {
            return registryObject;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validates and returns a requested extension identifier as a Guid 
        /// </summary>
        /// <param name="identifier">The identifier of the extension</param>
        private Guid ValidateIdentifier(string identifier)
        {
            Guid validIdentifier;

            if(string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The extension identifier cannot be null or empty.", identifier);
            }

            try
            {
                validIdentifier = new Guid(identifier);
            }
            catch(FormatException)
            {
                throw new FormatException(
                    $"Extension Identifier: {identifier} is not a valid GUID. Please check the format and ensure that the extension is using a valid GUID for its identifier.");
            }

            return validIdentifier;
        }

        /// <summary>
        /// Validates and returns a requested extension identifier's interface identifier. 
        /// </summary>
         /// <param name="identifier">The identifier of the extension</param>
        /// <remarks>
        /// Will return Guid.Empty if the extension does not request an interface.
        /// </remarks>
        private Guid ValidateInterfaceIdentifier(string identifier)
        {
            var validIdentifier = Guid.Empty;

            if(identifier == null)
            {
                return validIdentifier;
            }

            try
            {
                validIdentifier = new Guid(identifier);
            }
            catch(FormatException)
            {
                throw new FormatException(
                    $"Extension Identifier: {identifier} is not a valid GUID. Please check the format and ensure that the extension is using a valid GUID for its identifier.");
            }

            return validIdentifier;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the target extension registry type.
        /// </summary>
        public RegistryType RegistryType { get; }

        /// <summary>
        /// Gets the name of the extension registry.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the identifier of the target extension registry.
        /// </summary>
        public Guid Identifier { get; }

        /// <summary>
        /// Gets the version of the target extension registry.
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Gets the related interface for a an extension registry.
        /// </summary>
        /// <remarks>
        /// Returns Guid.Empty if interface is not applicable.
        /// </remarks>
        public Guid InterfaceIdentifier { get; }

        /// <summary>
        /// Gets the related interface for a an extension registry.
        /// </summary>
        /// <remarks>
        /// Returns null if interface version is not applicable.
        /// </remarks>
        public Version InterfaceVersion { get; }

        #endregion
    }
}