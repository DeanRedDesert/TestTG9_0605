// -----------------------------------------------------------------------
//  <copyright file = "BuildPaths.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Static class containing constant paths of directories and files that are related to build processes.
    /// </summary>
    public static class BuildPaths
    {
        /// <summary>
        /// Path to the AscentBuildSettings asset referenced by the Unity build process and build settings.
        /// </summary>
        public const string BuildSettingsAssetPath = "Assets/AscentBuildSettings.asset";

        /// <summary>
        /// Location of the Custom Build Paths asset file.
        /// </summary>
        public const string CustomBuildPathsAssetPath = "Assets/CustomBuildPaths.asset";

        /// <summary>
        /// A collection of non-asset resource directories that will be copied from the project folder to the build folder.
        /// </summary>
        public static readonly ReadOnlyCollection<string> ResourceDirectories =
            new List<string>
            {
                "EGMResources"
            }.AsReadOnly();

        /// <summary>
        /// A collection of resource files that will be copied from the project folder to the build folder.
        /// </summary>
        public static readonly ReadOnlyCollection<string> ResourceFiles =
            new List<string>
            {
                "SystemConfig.xml",
                "CsiConfig.xml"
            }.AsReadOnly();

        /// <summary>
        /// The registries directory that will be copied from the project folder to the build folder.
        /// </summary>
        public const string RegistriesDirectory = "Registries";

        /// <summary>
        /// The paytable directory that will be copied from the project folder to the build folder.
        /// </summary>
        public const string PaytablesDirectory = "Paytables";

        /// <summary>
        /// The co-themes directory that will be copied from the project folder to the build folder.
        /// </summary>
        public const string CoThemesDirectory = "CoThemes";

        /// <summary>
        /// Root directory to build into when building on the command line.
        /// </summary>
        public const string BuildRoot = "Builds";
    }
}