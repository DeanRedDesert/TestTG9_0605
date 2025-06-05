//-----------------------------------------------------------------------
// <copyright file = "ExtensionRegistryProxy.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Core.Registries.Internal.F2X.F2XBaseExtensionRegistryVer1;
    using ResourceExtensionRegistry =
        Core.Registries.Internal.F2X.F2XResourceExtensionRegistryVer1.ResourceExtensionRegistry;

    /// <summary>
    /// A proxy for the <see cref="BaseExtensionRegistry"/> object that is used to retrieve
    /// registry information from an extension registry.
    /// </summary>
    internal class ExtensionRegistryProxy : IExtensionRegistry
    {
        #region Private

        #endregion Private

        #region Constructor

        /// <summary>
        /// Constructs a <see cref="ExtensionRegistryProxy"/> with the given <paramref name="registryContext"/>.
        /// </summary>
        /// <param name="registryContext">
        /// The context that defines information on the extension being imported.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="registryContext" /> is null.
        /// </exception>
        public ExtensionRegistryProxy(ExtensionRegistryContext registryContext)
        {
            RegistryContext = registryContext ?? throw new ArgumentNullException(nameof(registryContext));
        }

        #endregion Constructor

        /// <summary>
        /// Gets the context of the target registry object.
        /// </summary>
        internal ExtensionRegistryContext RegistryContext { get; }

        #region IExtensionRegistry

        /// <inheritDoc />
        public IExtensionImport GetExtensionImport(IThemeRegistry themeRegistry)
        {
            if(themeRegistry == null)
            {
                throw new ArgumentNullException(nameof(themeRegistry));
            }

            // Get a list of extension imports requested by theme and group them by their extension identifier.
            var extensionImports = themeRegistry.GetExtensionImports()
                .Where(import => import.ExtensionIdentifier == RegistryContext.Identifier);

            IExtensionImport compatibleImport = null;
            var providerVersion = RegistryContext.Version;

            // Get the largest extension versions specified by the theme registry that is compatible with the 
            // versions specified in this imported extension registry. The imported extension version is compatible
            // with the theme if it is greater than or equals to the extension versions specified by the theme
            // registry.
            foreach(var extensionImport in
                extensionImports.Where(import => import.ExtensionVersion <= providerVersion))
            {
                if(extensionImport.ExtensionVersion == providerVersion)
                {
                    compatibleImport = extensionImport;
                    break;
                }
                if(compatibleImport == null || compatibleImport.ExtensionVersion < extensionImport.ExtensionVersion)
                {
                    compatibleImport = extensionImport;
                }
            }

            if(compatibleImport != null)
            {
                var resourceExtensionDirectoryBase = string.Empty;
                if(RegistryContext.GetRegistry() is ResourceExtensionRegistry resourceExtensionRegistry)
                {
                    resourceExtensionDirectoryBase = resourceExtensionRegistry.ResourceDirectoryBase;
                }

                return new ExtensionImport(compatibleImport.ExtensionIdentifier.ToString(), compatibleImport.ExtensionVersion,
                    resourceExtensionDirectoryBase);
            }

            if(themeRegistry.IsExtensionImportRequired(RegistryContext.Identifier))
            {
                throw new InvalidOperationException(
                    $"The required extension {RegistryContext.Identifier} cannot be linked.");
            }

            return null;
        }

        #endregion IExtensionRegistry
    }
}
