//-----------------------------------------------------------------------
// <copyright file = "IGTEGMSettings.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using IgtUnityEditor;
    using SDKBuild;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// This class sets all the properties needed to be set for making an EGM specific build.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class IGTEGMSettings : IPreprocessBuildWithReport
    {
        #region Private Fields

        /// <summary>
        /// Stored the active build settings.
        /// </summary>
        private static IGTEGMBuild igtEgmBuildSettings;

        #endregion

		public static event Action OnPreBuild;

        #region Public

        /// <summary>
        /// The current build settings property.
        /// </summary>
        public static IGTEGMBuild IgtEgmBuildSettings
        {
            get
            {
                // if build settings haven't been loaded, load them now
                if(igtEgmBuildSettings == null)
                {
                    igtEgmBuildSettings = AssetDatabase.LoadAssetAtPath(BuildPaths.BuildSettingsAssetPath,
                        typeof(IGTEGMBuild)) as IGTEGMBuild;
                }

                return igtEgmBuildSettings;
            }

            set => igtEgmBuildSettings = value;
        }

        /// <summary>
        /// Builds the IGT EGM player to the specified location.
        /// </summary>
        /// <param name="location">Location where the executable will be stored.</param>
        /// <param name="buildOptions">The options to build with.</param>
        /// <remarks>
        /// The EditorUserBuildSettings for the IGT EGM player must be set before calling this function.
        /// </remarks>
        public static void BuildPlayer(string location, BuildOptions buildOptions)
        {
            var enabledScenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            var buildResults = BuildPlayer(enabledScenes, location + PlayerSettings.productName + ".exe",
                BuildTarget.StandaloneWindows64, buildOptions);

            Debug.Log(buildResults);
        }

        /// <summary>
        /// Serializes the build settings to an .asset file.
        /// </summary>
        /// <param name="igtEgmBuild">a <see cref="IGTEGMBuild"/> object.</param>
        /// <param name="path">The new asset file location.</param>
        public static void SerializeIgtEgmBuildSettings(IGTEGMBuild igtEgmBuild, string path)
        {
            // Create a new asset
            AssetDatabase.CreateAsset(igtEgmBuild, path);
            AssetDatabase.ImportAsset(path);
        }
        
        #endregion

        /// <summary>
        /// Build the IGT EGM player.
        /// </summary>
        /// <param name="scenes">List of the scenes to include.</param>
        /// <param name="newLocation">Location to build to.</param>
        /// <param name="buildTarget">Build target information.</param>
        /// <param name="options">Build option information.</param>
        /// <returns>The result of the build. Either an error message or the information from the BuildReport.</returns>
        /// <remarks>
        /// The EditorUserBuildSettings for the IGT EGM player must be set before calling this function.
        /// </remarks>
        private static string BuildPlayer(string[] scenes, string newLocation, BuildTarget buildTarget,
            BuildOptions options)
        {
            if(igtEgmBuildSettings == null)
            {
                throw new BuildFailedException(
                    "Build settings must be set before attempting to build the Ascent platform.");
            }

			OnPreBuild?.Invoke();

            var report = BuildPipeline.BuildPlayer(scenes, newLocation, buildTarget, options);
            var buildSummary = ProcessBuildReport(report);
            
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(newLocation) ?? 
                                           string.Empty, "BuildReport.log"),buildSummary);
            
            PackageVersionsReport.BuildReport(report.summary);
            
            if(report.summary.result != BuildResult.Succeeded)
            {
                var stringBuilder = new StringBuilder();
                var errorLines = report.steps.SelectMany(step => step.messages)
                    .Where(message => message.type == LogType.Exception || message.type == LogType.Error)
                    .Select(message => message.content);
                foreach(var line in errorLines)
                {
                    stringBuilder.AppendLine(line);
                }

                // Throw an exception when the build report indicates a failure of the build process, so that the Unity
                // process will exit with a non zero code and propagate the build failure to the caller process.
                throw new BuildFailedException($"Build error with error code '{report.summary.result}'.\n" +
                                               stringBuilder);
            }

            return buildSummary;
        }
        
        #region IPreprocessBuildWithReport implementation
 
        /// <inheritdoc/>
        // ReSharper disable once UnusedMember.Global
        public void OnPreprocessBuild(BuildReport report) 
        {
            if(report.summary.platform != BuildTarget.StandaloneWindows64)
            {
                return;
            }

            if(!string.IsNullOrEmpty(report.summary.outputPath))
            {
                PreprocessBuildInternal(report.summary.outputPath);
            }
        }

        /// <inheritdoc/>
        // ReSharper Disable All
        public int callbackOrder { get; }
        // ReSharper Restore All
        
        #endregion
        
        #region Private Methods

        /// <summary>
        /// Internal helper to copy any custom directories or files to the build directory before the build process is started.
        /// </summary>
        /// <param name="buildOutputPath">The output path of the build tree.</param>  
        private static void PreprocessBuildInternal(string buildOutputPath)
        {
            if(IgtEgmBuildSettings != null)
            {
                // Apply Ascent settings before building
                ApplyIgtEgmBuildSettings(igtEgmBuildSettings);

                // Copy all files outside the Assets folder that belong to the build.
                CopyNonAssetFiles(buildOutputPath, igtEgmBuildSettings.BuildType);

                // Save the game parameters
                SaveParameters(buildOutputPath, igtEgmBuildSettings);
            }
            else
            {
                Debug.LogError("Build settings must be set before attempting to build the IGT EGM platform.");
            }
        }
 
        /// <summary>
        /// Copies the Paytable and Registry directories from the project location to the build location.
        /// </summary>
        /// <param name="buildPath">Destination where all the files are to be copied to.</param>
        /// <param name="buildType">The build type to copy files for. Not all builds require the same files.</param>
        // ReSharper disable once UnusedMember.Local  
        private static void CopyNonAssetFiles(string buildPath, BuildType buildType)
        {
            if(string.IsNullOrEmpty(buildPath))
            {
                Debug.LogWarning(
                    "Could not copy registries, paytables and EGM resources etc. because the build location was not specified. " +
                    "If using the \"Install in builds folder option.\" the files will need to be copied manually.");
 
                return;
            }
 
            var buildLocation = Path.GetDirectoryName(buildPath);
            var projectDirectory = Directory.GetCurrentDirectory();
 
            var directoriesToCopy = GetDirectoriesToCopy(buildType);
            foreach(var directory in directoriesToCopy)
            {
                BuildUtilities.CopyBuildDirectory(projectDirectory, buildLocation, directory);
            }
 
            // Game specific files should only be copied for Game projects.
            if(IgtEgmBuildSettings.BuildType == BuildType.Game 
               && (IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.StandaloneNoSafeStorage 
                   || IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.StandaloneFileBackedSafeStorage 
                   || IgtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage
                   || igtEgmBuildSettings.GameParameters.Type == IgtGameParameters.GameType.Standard))
            {
                var filesToCopy = GetGameSpecificFilesToCopy();
                foreach(var file in filesToCopy)
                {
                    BuildUtilities.CopyBuildFile(projectDirectory, buildLocation, file);
                }
            }
        }
 
        /// <summary>
        /// Gets the directories to copy to the build location.
        /// </summary>
        /// <param name="buildType">The build type to copy files for. Not all builds require the same files.</param>
        private static IEnumerable<string> GetDirectoriesToCopy(BuildType buildType)
        {
            // All build types need the resource directories.
            var result = new List<string>(BuildPaths.ResourceDirectories);
 
            switch(buildType)
            {
                case BuildType.Game:
                {
                    // Whether a shell or a single game package, all Game builds need the Registries folder.
                    result.Add(BuildPaths.RegistriesDirectory);
 
                    // If this is a shell package, there is no Paytables folder for the Shell.
                    // The paytables, registries and EGM resources for each theme locate under
                    // the CoThemes folder.
                    result.Add(Directory.Exists(BuildPaths.CoThemesDirectory)
                        ? BuildPaths.CoThemesDirectory
                        : BuildPaths.PaytablesDirectory);

                    break;
                }
                case BuildType.ThemeMenu:
                {
                    result.Add(BuildPaths.RegistriesDirectory);
                    break;
                }
            }
 
            return result;
        }
 
        /// <summary>
        /// Gets the game specific files (that are not under a directory) to copy to the build location.
        /// </summary>
        /// <devdoc>
        /// Currently all build types require the same set of files,
        /// therefore, there is no need for the BuildTyp argument.
        /// </devdoc>
        private static IEnumerable<string> GetGameSpecificFilesToCopy()
        {
            return BuildPaths.ResourceFiles;
        }
 
        /// <summary>
        /// Applies the IGT EGM build settings to the player settings.
        /// </summary>
        /// <param name="buildSettings">Settings for the build.</param>
        // ReSharper disable once UnusedMember.Local 
        private static void ApplyIgtEgmBuildSettings(IGTEGMBuild buildSettings)
        {
            //Reset this flag.
            IgtPlayerSettings.MachineTargetBuild = false;
 
            SetScriptingDefineSymbols(buildSettings);
 
            // Ensure the player settings are correct for release.
            if(buildSettings.Release || 
               (buildSettings.BuildType == BuildType.Game && buildSettings.GameParameters.Type == IgtGameParameters.GameType.Standard))
            {
                // If more settings are modified, then they should be added to the PlayerSettingsBackup class.
                IgtPlayerSettings.MachineTargetBuild = true;
                PlayerSettings.resizableWindow = false;
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerSettings.runInBackground = true;
            }
            else if(buildSettings.BuildType == BuildType.ThemeMenu ||
                    buildSettings.BuildType == BuildType.Other)
            {
                IgtPlayerSettings.MachineTargetBuild = buildSettings.Release;
                PlayerSettings.runInBackground = true;
            }
        }
 
        /// <summary>
        /// Set scripting define symbols for the build.
        /// </summary>
        /// <param name="buildSettings">Settings for the build.</param>
        private static void SetScriptingDefineSymbols(IGTEGMBuild buildSettings)
        {
            // Only add symbols for Game and ThemeMenu.
            if(buildSettings.BuildType != BuildType.Game && buildSettings.BuildType != BuildType.ThemeMenu)
                return;
 
            #region These are readonly settings needed by this method.
 
            // IGT Unity applications always use build target group of Standalone.
            const BuildTargetGroup targetGroup = BuildTargetGroup.Standalone;
 
            var debugSymbols = new List<string> { "DEBUG_TRACING" };
 
            // Currently there is no symbol to add for Release build.
            var releaseSymbols = new List<string>();
 
            #endregion
 
            // PlayerSettings is serialized and saved in ProjectSettings.asset.
            // So symbols added in the previous build will stay and take effect in following builds.
            // Therefore, any symbol that could be auto-added by a different build should be removed.
            // A caveat is that a user won't be able to add "DEBUG" manually in Editor and hope it
            // works with a Release build. To do that, one has to modify this file.
            var including = buildSettings.Release ? releaseSymbols : debugSymbols;
            var excluding = buildSettings.Release ? debugSymbols : releaseSymbols;
 
            var currentDefineList = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
 
            var allDefines = currentDefineList.Split(';').Except(excluding).Union(including);
 
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup,
                                                             string.Join(";", allDefines.ToArray()));
        }
 
        /// <summary>
        /// Saves the build parameters.
        /// </summary>
        /// <param name="location">Location to save the parameters.</param>
        /// <param name="buildSettings">Settings for the build.</param>
        /// <remarks>
        /// Currently saves the game parameters. It should be extended to include theme parameters if needed.
        /// </remarks>
        // ReSharper disable once UnusedMember.Local  
        private static void SaveParameters(string location, IGTEGMBuild buildSettings)
        {
            if(!string.IsNullOrEmpty(location))
            {
                var directoryLocation = Path.GetDirectoryName(location);
 
                if(!string.IsNullOrEmpty(directoryLocation))
                {
                    if(buildSettings.BuildType == BuildType.Game)
                    {
                        if(!Directory.Exists(directoryLocation))
                        {
                            Directory.CreateDirectory(directoryLocation);
                        }
 
                        // Save the game parameters to the build location.
                        buildSettings.GameParameters.Save(directoryLocation +
                                                          "/" + IgtGameParameters.GameParametersFileName);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Could not save the game parameters because the build location was not specified. " +
                                 "If using the \"Install in builds folder option.\" the file will need to be copied manually.");
            }
        }
 
        /// <summary>
        /// Extracts information from the BuildReport into a string.
        /// </summary>
        /// <param name="buildReport">information about the files output, the build steps taken, and other platform-specific information such as native code stripping.</param>
        /// <returns>The information from the BuildReport.</returns>
        private static string ProcessBuildReport(BuildReport buildReport)
        {
            var report = "----------- Summary:-----------\n";
            report += "\nThe time the build was started: " + buildReport.summary.buildStartedAt;
            report += "\nThe time the build ended: " + buildReport.summary.buildEndedAt;
            report += "\nThe Application.buildGUID of the build: " + buildReport.summary.guid;
            report += "\nThe output path for the build: " + buildReport.summary.outputPath;
            report += "\nThe outcome of the build: " + buildReport.summary.result;
            report += "\nThe total number of errors and exceptions recorded during the build process.: " + buildReport.summary.totalErrors;
            report += "\nThe total number of warnings recorded during the build process.: " + buildReport.summary.totalWarnings;
            report += "\n---------- Files:---------- \n";
            report = buildReport.files.Aggregate(report, (current, file) => current + (file + "\n"));
            report += "\n---------- Steps:---------- \n";
            report = buildReport.steps.Aggregate(report, (current, step) =>
            {
                var result = current + (step + "\n");
                result = step.messages.Where((m) => m.type != LogType.Log).Aggregate(result, (s, message) =>
                {
                    string typeStr;
                    switch(message.type)
                    {
                        case LogType.Warning:
                            typeStr = "Warning";
                            break;
                        case LogType.Error:
                            typeStr = "Error";
                            break;
                        case LogType.Exception:
                            typeStr = "Exception";
                            break;
                        case LogType.Assert:
                            typeStr = "Assertion";
                            break;
                        default:
                            typeStr = "Log";
                            break;
                    }
                    return "\t" + typeStr + ": " + message.content + "\n";
                });
                return result;
            });
 
            return report;
        }
 
        #endregion
    }
}
