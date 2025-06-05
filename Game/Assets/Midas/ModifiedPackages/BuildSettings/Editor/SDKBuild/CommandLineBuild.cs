//-----------------------------------------------------------------------
// <copyright file = "CommandLineBuild.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using IGT.Game.SDKAssets.SDKBuild.Editor;

namespace IGT.Game.SDKAssets.DevelopmentFiles.Editor
{
    using AscentBuildSettings;
    using AscentBuildSettings.Editor;
    using SDKBuild;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which performs command line builds.
    /// </summary>
    internal static class CommandLineBuild
    {
        /// <summary>
        /// Path for storing release command line builds.
        /// </summary>
        public const string ReleaseBuildPath = BuildPaths.BuildRoot + "/Release/";

        /// <summary>
        /// Build a game for release.
        /// </summary>
        public static void ReleaseGame()
        {
			Build.BuildPlatform(true, true, BuildOptions.StrictMode);
        }

        /// <summary>
        /// Build a game for release.
        /// </summary>
        public static void ReleaseGameProduceTextAssets()
        {
#if !IGT_WEB_MOBILE
            var foundationTarget = IGTEGMSettings.IgtEgmBuildSettings.GameParameters.TargetedFoundation;
            var buildSettings = ScriptableObject.CreateInstance<IGTEGMBuild>();
            buildSettings.BuildType = BuildType.Game;
            buildSettings.GameParameters = IgtGameParameters.CreateReleaseParameters(foundationTarget);
            buildSettings.Release = true;
            IGTEGMSettings.IgtEgmBuildSettings = buildSettings;
            IGTEGMSettings.BuildPlayer(ReleaseBuildPath, BuildOptions.StrictMode);
#endif
        }

        /// <summary>
        /// Build a game for development.
        /// </summary>
        public static void DevelopmentGame()
        {
			Build.BuildPlatform(true, false, BuildOptions.StrictMode);
        }

        /// <summary>
        /// Build a game for the Universal Controller platform.
        /// </summary>
        public static void UniversalControllerGame()
        {
#if !IGT_WEB_MOBILE
            var targetedFoundation = IGTEGMSettings.IgtEgmBuildSettings.GameParameters.TargetedFoundation;
            var parameters = IgtGameParameters.CreateUcParameters(targetedFoundation);
            var buildSettings = ScriptableObject.CreateInstance<IGTEGMBuild>();
            buildSettings.BuildType = BuildType.Game;
            buildSettings.GameParameters = parameters;
            // Based on IGTEGMBuildCustomInspector.cs release is always false for UC game
            buildSettings.Release = false;
            buildSettings.MonoAoTCompile = false;
            IGTEGMSettings.IgtEgmBuildSettings = buildSettings;
            IGTEGMSettings.BuildPlayer(ReleaseBuildPath, BuildOptions.StrictMode);
#endif
        }

        /// <summary>
        /// Build a standalone game instance.
        /// </summary>
        public static void StandaloneGame()
		{
			Build.BuildStandalone(true, false, BuildOptions.StrictMode);
		}

        /// <summary>
        /// Build a theme menu for release.
        /// </summary>
        public static void ReleaseMenu()
        {
#if !IGT_WEB_MOBILE
            var buildSettings = ScriptableObject.CreateInstance<IGTEGMBuild>();
            buildSettings.BuildType = BuildType.ThemeMenu;
            buildSettings.GameParameters = null;
            buildSettings.Release = true;
            IGTEGMSettings.IgtEgmBuildSettings = buildSettings;
            IGTEGMSettings.BuildPlayer(ReleaseBuildPath, BuildOptions.StrictMode);
#endif
        }
    }
}