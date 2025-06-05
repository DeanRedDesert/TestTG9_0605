//-----------------------------------------------------------------------
// <copyright file = "RegistryLoader.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using F2XRegistryVerTip = Core.Registries.Internal.F2X.F2XRegistryVer1;
    using F2XPayvarGroupRegistryVerTip = Core.Registries.Internal.F2X.F2XPayvarGroupRegistryVer1;
    using F2LPayvarRegistryVerTip = Core.Registries.Internal.F2L.F2LPayvarRegistryVer3;
    using F2LThemeRegistryVerTip = Core.Registries.Internal.F2L.F2LThemeRegistryVer5;
    using F2XBetStepsRegistryTip = Core.Registries.Internal.F2X.F2XBetStepsRegistryVer1;

    /// <summary>
    /// This class provides static methods to load game registry files.
    /// </summary>
    public static class RegistryLoader
    {
        #region Constants

        /// <summary>
        /// The directory where all registry files are located.
        /// </summary>
        private const string RegistryDirectory = "Registries";

        /// <summary>
        /// The directory where all cotheme files are located in a concurrent game.
        /// </summary>
        private const string CoThemeRegistryDirectory = "CoThemes";

        /// <summary>
        /// File name pattern used to search for theme registries.
        /// </summary>
        private const string ThemeRegistryPattern = "*.xthemereg";

        /// <summary>
        /// File name pattern used to search for the shell registries.
        /// </summary>
        private const string ShellRegistryPattern = "*.xshellreg";

        /// <summary>
        /// File name pattern used to search for payvar registries.
        /// </summary>
        private const string PayvarRegistryPattern = "*.xpayvarreg";

        /// <summary>
        /// File name pattern used to search for payvar group registries.
        /// </summary>
        private const string PayvarGroupRegistryPattern = "*.xpayvargroupreg";

        /// <summary>
        /// File name pattern used to search for extension registries.
        /// </summary>
        private const string ExtensionRegistryPattern = "*.xextensionreg";

        /// <summary>
        /// File name pattern used to search for extension interface definition registries.
        /// </summary>
        private const string ExtensionInterfaceDefRegistryPattern = "*.xextinterfacedefreg";

        /// <summary>
        /// File name pattern used to search for bet steps registries.
        /// </summary>
        private const string BetStepsRegistryPattern = "*.xbetstepreg";

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the payvar registry from the specified payvar registry file.
        /// </summary>
        /// <param name="registryFile">
        /// The payvar registry file name.
        /// </param>
        /// <param name="payvarGroupRegistries">
        /// A collection of <see cref="IPayvarGroupRegistry"/> initialized using the payvar group template.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <returns>
        /// The payvar registry instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="registryFile"/> is null.
        /// </exception>
        public static IPayvarRegistry LoadPayvarRegistry(string registryFile,
                                                         IEnumerable<IPayvarGroupRegistry> payvarGroupRegistries = null)
        {
            if(registryFile == null)
            {
                throw new ArgumentNullException(nameof(registryFile));
            }

            var payvarRegistry = F2LPayvarRegistryVerTip.PayvarRegistry.Load(registryFile);

            if(payvarRegistry != null)
            {
                UniformSlashesInRegistry(payvarRegistry);
            }

            return new PayvarRegistryProxy(payvarRegistry, Path.GetFileNameWithoutExtension(registryFile), payvarGroupRegistries);
        }

        /// <summary>
        /// Load the theme registry from the specified theme registry file.
        /// </summary>
        /// <param name="registryFile">The theme registry file name.</param>
        /// <returns>The theme registry instance.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="registryFile"/> is null.
        /// </exception>
        public static IThemeRegistry LoadThemeRegistry(string registryFile)
        {
            if(registryFile == null)
            {
                throw new ArgumentNullException(nameof(registryFile));
            }

            var themeRegistry = F2LThemeRegistryVerTip.ThemeRegistry.Load(registryFile);

            if(themeRegistry != null)
            {
                UniformSlashesInRegistry(themeRegistry);
            }

            return new ThemeRegistryProxy(themeRegistry, Path.GetFileNameWithoutExtension(registryFile));
        }

        /// <summary>
        /// Load the payvar group registry from the specified payvar group registry file.
        /// </summary>
        /// <param name="registryFile">
        /// The name of the payvar group registry file.
        /// </param>
        /// <returns>
        /// The payvar group registry object.
        /// </returns>
        public static IPayvarGroupRegistry LoadPayvarGroupRegistry(string registryFile)
        {
            var payvarGroupRegistry = F2XRegistryVerTip.Registry.Load<F2XPayvarGroupRegistryVerTip.PayvarGroupRegistry>(
                registryFile);

            if(payvarGroupRegistry != null)
            {
                UniformSlashesInRegistry(payvarGroupRegistry);
            }

            return new PayvarGroupRegistryProxy(payvarGroupRegistry);
        }

        /// <summary>
        /// Load the imported extension registry from the specified extension registry file.
        /// </summary>
        /// <remarks>
        /// Imported extension registries are registries for extensions that are imported by an application (i.e.
        /// resource and configuration extensions). This does not include the executable extension because it provides
        /// services to an application.
        /// </remarks>
        /// <param name="registryFile">
        /// The name of the extension registry file.
        /// </param>
        /// <returns>
        /// A <see cref="IExtensionRegistry"/> that is used to retrieve information from the imported extension
        /// registry if it is an imported extension registry and can be loaded, otherwise null.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="registryFile"/> is either null or empty.
        /// </exception>
        public static IExtensionRegistry LoadImportedExtensionRegistryFromFile(string registryFile)
        {
            if(string.IsNullOrEmpty(registryFile))
            {
                throw new ArgumentException("The extension registry file to load is null or empty.", registryFile);
            }

            if(!File.Exists(registryFile))
            {
                return null;
            }

            var registry = F2XRegistryVerTip.Registry.GetRegistry(registryFile);
            var context = new ExtensionRegistryContext(registry);
            return context.GetRegistry() == null ? null : new ExtensionRegistryProxy(context);
        }

        /// <summary>
        /// Loads a bet steps registry file from the specified bet steps registry file.
        /// </summary>
        /// <param name="registryFile">The name of the bet steps registry.</param>
        /// <returns>
        /// A <see cref="IBetStepsRegistry"/> that is used to relay information about its configured bet steps.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="registryFile"/> is either null or empty.
        /// </exception>
        public static IBetStepsRegistry LoadBetStepsRegistry(string registryFile)
        {
            if(string.IsNullOrEmpty(registryFile))
            {
                throw new ArgumentException("The bet steps registry file to load is null or empty.", registryFile);
            }

            if(!File.Exists(registryFile))
            {
                return null;
            }

            var registry = F2XRegistryVerTip.Registry.Load<F2XBetStepsRegistryTip.BetStepsRegistry>(registryFile);

            if(registry != null)
            {
                UniformSlashesInRegistry(registry);
            }

            return new BetStepsRegistryProxy(registry, registryFile);
        }

        /// <summary>
        /// Load game registries from the specified game mount point.
        /// </summary>
        /// <param name="gameMountPoint">
        /// The mount point of the application.
        /// </param>
        /// <returns>
        /// A dictionary of <see cref="IThemeRegistry"/> and its list of <see cref="IPayvarRegistry"/>.
        /// </returns>
        public static IDictionary<IThemeRegistry, IList<IPayvarRegistry>> Load(string gameMountPoint)
        {
            var path = GetRegistryPath(gameMountPoint);

            // Discover theme registries.  The result is a list keyed by the absolute file paths.
            var themeDiscoveryList = DiscoverThemeRegistries(path);

            var themeRegistries = new Dictionary<string, IThemeRegistry>();
            foreach(var themeRegistry in themeDiscoveryList)
            {
                var themeFileName = Path.GetFileNameWithoutExtension(themeRegistry.Key);
                if(themeFileName != null)
                {
                    themeRegistries.Add(themeFileName, themeRegistry.Value);
                }
            }

            // Discover payvar registries.  The result is a flat list.
            var payvarDiscoveryList = DiscoverPayvarRegistries(path);

            var payvarRegistries = new Dictionary<string, IList<IPayvarRegistry>>();

            // Group the payvar registries by the theme names retrieved from the theme registries
            // that are referenced by the payvar registries.
            foreach(var payvarRegistry in payvarDiscoveryList)
            {
                var themeRegistryPath = Path.Combine(gameMountPoint, payvarRegistry.ThemeRegistryFileName);
                if(!themeDiscoveryList.ContainsKey(themeRegistryPath))
                {
                    throw new GameRegistryException(
                        $"The theme registry {themeRegistryPath} referenced by a payvar registry is not found.");
                }

                var themeFileName = Path.GetFileNameWithoutExtension(themeRegistryPath);
                if(!payvarRegistries.ContainsKey(themeFileName))
                {
                    payvarRegistries[themeFileName] = new List<IPayvarRegistry>();
                }

                payvarRegistries[themeFileName].Add(payvarRegistry);
            }

            var dictionary = new Dictionary<IThemeRegistry, IList<IPayvarRegistry>>();
            foreach(var registry in themeRegistries)
            {
                var thisKey = registry.Value;
                var thisValue = payvarRegistries.TryGetValue(registry.Key, out var tempValue)
                    ? tempValue
                    : new List<IPayvarRegistry>();
                dictionary.Add(thisKey, thisValue);
            }
			
            return dictionary;
        }

        /// <summary>
        /// Load imported extension registries stored under the specified game mount point.
        /// </summary>
        /// <remarks>
        /// Imported extension registries are registries for extensions that are imported by an application (i.e.
        /// resource and configuration extensions). This does not include the executable extension because it provides
        /// services to an application.
        /// </remarks>
        /// <param name="mountPoint">
        /// The mount point of the application.
        /// </param>
        /// <returns>
        /// A collection of <see cref="IExtensionRegistry"/> loaded from the <paramref name="mountPoint"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="mountPoint"/> is null or empty.
        /// </exception>
        public static IEnumerable<IExtensionRegistry> LoadImportedExtensionRegistriesFromDirectory(string mountPoint)
        {
            if(string.IsNullOrEmpty(mountPoint))
            {
                throw new ArgumentException("The mount point is null or empty.", mountPoint);
            }

            // Registries must locate at a designated directory and its subdirectories.
            return DiscoverImportedExtensionRegistries(GetRegistryPath(mountPoint));
        }

        /// <summary>
        /// Load imported bet steps registries stored under the specified game mount point.
        /// </summary>
        /// <remarks>
        /// Bet steps registries are used
        /// </remarks>
        /// <param name="mountPoint">
        /// The mount point of the application.
        /// </param>
        /// <returns>
        /// A collection of <see cref="IExtensionRegistry"/> loaded from the <paramref name="mountPoint"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Throw if the <paramref name="mountPoint"/> is null or empty.
        /// </exception>
        public static IEnumerable<IBetStepsRegistry> LoadBetStepsRegistriesFromDirectory(string mountPoint)
        {
            if(string.IsNullOrEmpty(mountPoint))
            {
                throw new ArgumentException("The mount point is null or empty.", mountPoint);
            }

            var path = GetRegistryPath(mountPoint);
            var discoveredBetStepsRegistries =  DiscoverBetStepsRegistries(path).ToList();

            // Discover all payvar registries and validate that the bet steps file is requesting an appropriate payvar registry.
            var payvarFiles = Directory.GetFiles(path, PayvarRegistryPattern, SearchOption.AllDirectories).Select(f => Path.GetFileName(f));

            foreach(var betStepsRegistry in discoveredBetStepsRegistries)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                if(!payvarFiles.Contains(betStepsRegistry.PayvarRegistryField))
                {
                    throw new GameRegistryException(
                        $"Could not find payvar registry {betStepsRegistry.PayvarRegistryField} specified in bet steps registry {betStepsRegistry.BetStepsRegistryFileName}");
                }
            }
            return discoveredBetStepsRegistries;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Search and load the theme registries in the specified path and its subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <returns>The theme registries loaded.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when no theme registry file is found.
        /// </exception>
        private static IDictionary<string, IThemeRegistry> DiscoverThemeRegistries(string path)
        {
            var themeFiles = Directory.GetFiles(path, ThemeRegistryPattern, SearchOption.AllDirectories);

            if(themeFiles.Length == 0)
            {
                throw new GameRegistryException(
                    $"No theme registry is found in {path} and its subdirectories.");
            }

            // The mono compiler does not recognize the method group correctly, so we have to use the lambda instead.
            // ReSharper disable ConvertClosureToMethodGroup
            return themeFiles.ToDictionary(themeFile => themeFile, themeFile => LoadThemeRegistry(themeFile));
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Search and load the payvar registries in the specified path and its subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <returns>The list of payvar registries loaded.</returns>
        /// <exception cref="GameRegistryException">
        /// Thrown when no payvar registry file is found.
        /// </exception>
        private static IEnumerable<IPayvarRegistry> DiscoverPayvarRegistries(string path)
        {
            var payvarFiles = Directory.GetFiles(path, PayvarRegistryPattern, SearchOption.AllDirectories);

            if(payvarFiles.Length == 0)
            {
                throw new GameRegistryException(
                    $"No payvar registry is found in {path} and its subdirectories.");
            }

            var payvarGroupRegistries = DiscoverPayvarGroupRegistries(path);

            // The mono compiler does not recognize the method group correctly, so we have to use the lambda instead.
            // ReSharper disable ConvertClosureToMethodGroup
            return payvarFiles.Select(payvarFile => LoadPayvarRegistry(payvarFile, payvarGroupRegistries));
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Search and load the payvar group registries in the specified path and its subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <returns>The list of payvar group registries loaded.</returns>
        private static IEnumerable<IPayvarGroupRegistry> DiscoverPayvarGroupRegistries(string path)
        {
            var payvarGroupRegistries = Directory.GetFiles(path, PayvarGroupRegistryPattern, SearchOption.AllDirectories);

            // ReSharper disable ConvertClosureToMethodGroup
            return payvarGroupRegistries.Length == 0 ? null :
                payvarGroupRegistries.Select(payvarGroupRegistry => LoadPayvarGroupRegistry(payvarGroupRegistry));
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Search and load the bet steps registries in the specified path and its subdirectories.
        /// </summary>
        /// <param name="path">The directory to search.</param>
        /// <returns>The list of verified bet steps registries loaded.</returns>
        private static IEnumerable<IBetStepsRegistry> DiscoverBetStepsRegistries(string path)
        {
            var betStepsRegistries = Directory.GetFiles(path, BetStepsRegistryPattern, SearchOption.AllDirectories);

            // ReSharper disable ConvertClosureToMethodGroup
            return betStepsRegistries.Length == 0 ? null :
                betStepsRegistries.Select(betStepRegistry => LoadBetStepsRegistry(betStepRegistry));
            // ReSharper restore ConvertClosureToMethodGroup
        }

        /// <summary>
        /// Search and load the imported extension registries in the specified path and its subdirectories.
        /// </summary>
        /// <remarks>
        /// Imported extension registries are registries for extensions that are imported by an application (i.e.
        /// resource and configuration extensions). This does not include the executable extension because it provides
        /// services to an application.
        /// </remarks>
        /// <param name="path">
        /// The directory to search.
        /// </param>
        /// <returns>
        /// A collection of imported extension registries loaded.
        /// </returns>
        private static IEnumerable<IExtensionRegistry> DiscoverImportedExtensionRegistries(string path)
        {
            var extensionRegistries = new List<IExtensionRegistry>();
            var extensionRegistryFiles = Directory.GetFiles(
                path,
                $"{ExtensionRegistryPattern}",
                SearchOption.AllDirectories);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var registryFile in extensionRegistryFiles)
            {
                var registryProxy = (ExtensionRegistryProxy)LoadImportedExtensionRegistryFromFile(registryFile);
                if(registryProxy == null)
                {
                    continue;
                }

                if(registryProxy.RegistryContext.RegistryType ==
                   F2XRegistryVerTip.RegistryType.ResourceExtension ||
                   registryProxy.RegistryContext.RegistryType ==
                   F2XRegistryVerTip.RegistryType.ConfigurationExtension)
                {
                    extensionRegistries.Add(registryProxy);
                }
            }

            var extensionInterfaceRegistries = Directory.GetFiles(
                path,
                $"{ExtensionInterfaceDefRegistryPattern}",
                SearchOption.AllDirectories);

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var registryFile in extensionInterfaceRegistries)
            {
                var registryProxy = (ExtensionRegistryProxy)LoadImportedExtensionRegistryFromFile(registryFile);
                if(registryProxy == null ||
                   registryProxy.RegistryContext.RegistryType !=
                   F2XRegistryVerTip.RegistryType.ConfigurationExtensionInterfaceDefinition)
                {
                    continue;
                }

                extensionRegistries.Add(registryProxy);
            }

            return extensionRegistries;
        }

        /// <summary>
        /// Convert the slashes (backward or forward) in the paths appearing
        /// in a theme registry to a uniform one.
        /// </summary>
        /// <param name="themeRegistry">The theme registry to check and convert.</param>
        private static void UniformSlashesInRegistry(F2LThemeRegistryVerTip.ThemeRegistry themeRegistry)
        {
            if(themeRegistry.TagDataFile?.Value != null)
            {
                themeRegistry.TagDataFile.Value = Utility.UniformSlashes(themeRegistry.TagDataFile.Value);
            }
        }

        /// <summary>
        /// Convert the slashes (backward or forward) in the paths appearing
        /// in a payvar registry to a uniform one.
        /// </summary>
        /// <param name="payvarRegistry">The payvar registry to check and convert.</param>
        private static void UniformSlashesInRegistry(F2LPayvarRegistryVerTip.PayvarRegistry payvarRegistry)
        {
            if(payvarRegistry.ThemeRegistry != null)
            {
                payvarRegistry.ThemeRegistry = Utility.UniformSlashes(payvarRegistry.ThemeRegistry);
            }

            if(payvarRegistry.TagDataFile?.Value != null)
            {
                payvarRegistry.TagDataFile.Value = Utility.UniformSlashes(payvarRegistry.TagDataFile.Value);
            }
        }

        /// <summary>
        /// Convert the slashes (backward or forward) in the paths appearing in a payvar group registry to a uniform one.
        /// </summary>
        /// <param name="payvarGroupRegistry">
        /// The payvar group registry to check and convert.
        /// </param>
        private static void UniformSlashesInRegistry(F2XPayvarGroupRegistryVerTip.PayvarGroupRegistry payvarGroupRegistry)
        {
            foreach(var payvar in payvarGroupRegistry.PayvarGroup)
            {
                // The GroupTagDataFile is a required field.
                payvar.GroupTagDataFile.Value = Utility.UniformSlashes(payvar.GroupTagDataFile.Value);
            }
        }

        /// <summary>
        /// Convert the slashes (backward or forward) in the paths appearing in a bet steps registry to a uniform one.
        /// </summary>
        /// <param name="betStepsRegistry">
        /// The bet steps registry to check and convert.
        /// </param>
        private static void UniformSlashesInRegistry(F2XBetStepsRegistryTip.BetStepsRegistry betStepsRegistry)
        {
            if(betStepsRegistry.PayvarRegistry != null)
            {
                betStepsRegistry.PayvarRegistry = Utility.UniformSlashes(betStepsRegistry.PayvarRegistry);
            }
        }

        /// <summary>
        /// Determines the location of theme registry files based on the mount point
        /// and whether any shell registry files exist.
        /// </summary>
        /// <param name="gameMountPoint">The mount point of the application.</param>
        /// <returns>The parent directory for where registry files are located.</returns>
        private static string GetRegistryPath(string gameMountPoint)
        {
            if(string.IsNullOrEmpty(gameMountPoint))
            {
                throw new ArgumentException("The mount point is null or empty.", gameMountPoint);
            }

            // Registries must exist at a designated directory and its subdirectories.
            var registryPath = Path.Combine(gameMountPoint, RegistryDirectory);

            // Check to see if this is a concurrent game setup, in which case we need to update the path.
            var shellRegistryFiles = Directory.GetFiles(registryPath, ShellRegistryPattern);
            if(shellRegistryFiles.Length > 1)
            {
                throw new GameRegistryException("Only a single .xshellreg file is supported in a concurrent game!");
            }

            if(shellRegistryFiles.Length > 0)
            {
                registryPath = Path.Combine(gameMountPoint, CoThemeRegistryDirectory);
            }

            return registryPath;
        }

        #endregion
    }
}
