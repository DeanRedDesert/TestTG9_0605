// -----------------------------------------------------------------------
// <copyright file = "RegistryIndexer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Registries;

    /// <summary>
    /// This class is used to convert registry name to index.
    /// </summary>
    internal class RegistryIndexer : IRegistryIndexer
    {
        /// <summary>
        /// The theme registries list.
        /// </summary>
        private readonly IList<IThemeRegistry> themeRegistries = new List<IThemeRegistry>();

        /// <summary>
        /// The payvar registries list.
        /// </summary>
        private readonly IList<IList<IPayvarRegistry>> payvarLists = new List<IList<IPayvarRegistry>>();

        /// <summary>
        /// The configuration extension manager.  Null means the indexer does not support configuration extensions.
        /// </summary>
        private readonly IConfigurationExtensionManager configurationExtensionManager;

        /// <summary>
        /// Initializes a new instance of <see cref="RegistryIndexer"/>.
        /// </summary>
        /// <param name="registries">
        /// The theme and payvar registries used.
        /// </param>
        /// <param name="configurationExtensionManager">
        /// The configuration extension manager.  Could be null if the indexer does not
        /// support querying configuration extensions.
        /// </param>
        public RegistryIndexer(IDictionary<IThemeRegistry, IList<IPayvarRegistry>> registries,
                               IConfigurationExtensionManager configurationExtensionManager = null)
        {
            if(registries == null)
            {
                throw new ArgumentNullException(nameof(registries));
            }

            foreach(var pair in registries)
            {
                themeRegistries.Add(pair.Key);
                payvarLists.Add(new List<IPayvarRegistry>(pair.Value));
            }

            this.configurationExtensionManager = configurationExtensionManager;
        }

        /// <inheritdoc />
        public int GetThemeIndex(string identifier)
        {
            if(string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var currentTheme = themeRegistries.FirstOrDefault(registry => registry.G2SThemeId == identifier);

            return currentTheme != null ? themeRegistries.IndexOf(currentTheme) : -1;
        }

        /// <inheritdoc />
        public int GetPayvarIndex(string identifier)
        {
            if(string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            var result = -1;

            var query = from payvarList in payvarLists
                        from payvarRegistry in payvarList
                        where payvarRegistry.PaytableIdentifier == identifier
                        select payvarRegistry;

            var currentPayvar = query.ToList().FirstOrDefault();
            if(currentPayvar != null)
            {
                var themeRegistryName = Path.GetFileNameWithoutExtension(currentPayvar.ThemeRegistryFileName);

                var currentTheme = themeRegistries.FirstOrDefault(registry => registry.ThemeName == themeRegistryName);
                if(currentTheme != null)
                {
                    var themeIndex = themeRegistries.IndexOf(currentTheme);
                    var payvarIndex = payvarLists[themeIndex].IndexOf(currentPayvar);

                    // Use the higher two bytes for theme index, and the lower two for payvar.
                    result = themeIndex << 16 | payvarIndex;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public int GetExtensionIndex(string identifier)
        {
            if(string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            if(configurationExtensionManager == null)
            {
                throw new NotSupportedException("This indexer instance does not support querying configuration extensions.");
            }

            return configurationExtensionManager.GetProviderIndex(new Guid(identifier));
        }
    }
}