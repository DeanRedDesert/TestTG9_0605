// -----------------------------------------------------------------------
//  <copyright file = "GameReportBuildProcessor.cs" company = "IGT">
//      Copyright (c) 2020 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.SDK.GameReport.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    /// <summary>
    /// This class deploys the game report components during the game post build process.
    /// </summary>
    public class GameReportBuildProcessor : IPostprocessBuildWithReport
    {
        /// <summary>
        /// The minimum target version of mscorlib with IGT Unity 2018's dotnet 4.x runtime.
        /// </summary>
        private const string TargetCorlibVersion = "4.5";

        /// <summary>
        /// The 64 bit mono runtime relative paths for IGT Unity 2018 and 2020 installation, defined in
        /// order of most likely presence based on the installation tree structure. 
        /// </summary>
        private static readonly string[] CandidateMonoRuntimeInstallPaths = { @"Data\MonoBleedingEdge\bin-x64\mono-bdwgc.exe",
                                                                              @"Data\MonoBleedingEdge\bin\mono-bdwgc.exe" };

        /// <summary>
        /// The relative path of the mono runtime parent directory under the game build output folder.
        /// </summary>
        private const string MonoRuntimeTargetDirectory = @"MonoBleedingEdge\EmbedRuntime";

        /// <summary>
        /// The relative path of the mscorlib parent directory under the game build output folder.
        /// </summary>
        private const string CorlibTargetDirectory = @"MonoBleedingEdge\lib\mono\" + TargetCorlibVersion;
        
        #region IPostprocessBuildWithReport Implementation
 
        /// <inheritdoc/>
        // ReSharper disable once UnusedMember.Local
        public void OnPostprocessBuild(BuildReport report) 
        {
            if(report.summary.platform != BuildTarget.StandaloneWindows64)
            {
                return;
            }

            if(!string.IsNullOrEmpty(report.summary.outputPath))
            {
                PostProcessBuildInternal(report.summary.outputPath);
            }
        }

        /// <inheritdoc/>
        // ReSharper Disable All
        public int callbackOrder { get; }
        // ReSharper Restore All

        #endregion

        private static void PostProcessBuildInternal(string pathToBuiltProject)
        {
            try
            {
                var builtGamePath = Directory.GetParent(pathToBuiltProject).FullName;
                var applicationName = Path.GetFileNameWithoutExtension(pathToBuiltProject);

                // Copy GameReportActivator.exe to built game's Managed directory
                File.Copy(
                    GameReportActivatorBuilder.GetBuild(forceRebuild:true).ActivatorExecutablePath.NormalizePath(),
                    Path.Combine(builtGamePath,
                        $@"{applicationName}_Data\Managed\{GameReportActivatorBuilder.ActivatorExecutableName}").NormalizePath(),
                    true);
                // Copy Mono Runtime to built game's directory
                CopyMonoRuntime(builtGamePath, applicationName);
            }
            catch(Exception exception)
            {
                Debug.LogError("Game Report Assembly Build Error: " + exception);
            }
        }

        /// <summary>
        /// Copies the Mono Runtime files to "MonoBleedingEdge" under the built game's directory.
        /// </summary>
        /// <param name="gameMountPoint">Built game mount point to where Mono Runtime will be copied.</param>
        /// <param name="applicationName">The name of the output application.</param>
        /// <remarks>
        /// The Mono Runtime is required to run the GameReportActivator on an EGM. The 64 bit version is needed to
        /// run game reports that use MPT paytables.
        /// </remarks>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the source Mono Runtime directory does not exist.
        /// </exception>
        private static void CopyMonoRuntime(string gameMountPoint, string applicationName)
        {
            var unityLocation = AppDomain.CurrentDomain.BaseDirectory;
            var sourceMonoPath = "";
            foreach(var candidateRelPath in CandidateMonoRuntimeInstallPaths)
            {
                var candidateFullPath = Path.Combine(unityLocation, candidateRelPath);
                if(File.Exists(candidateFullPath.NormalizePath()))
                {
                    sourceMonoPath = candidateFullPath;
                    break;
                }
            }
 
            if(!File.Exists(sourceMonoPath.NormalizePath()))            
            {
                throw new FileNotFoundException("Mono Runtime was not found in the current Unity installation location.", 
                                        sourceMonoPath);
            }

            var destMonoDirectory = Path.Combine(gameMountPoint, MonoRuntimeTargetDirectory);
            // Copy the 64 bit runtime files but do not overwrite them if they exist. Mono runtime needs
            // mscorlib.dll in the predefined location "lib\mono\{version}\" where "{version}" is the runtime
            // framework version.
            CopyNewFile(sourceMonoPath, destMonoDirectory);

            var sourceCorlibPath = Path.Combine(gameMountPoint, $@"{applicationName}_Data\Managed\mscorlib.dll");
            var destCorlibDirectory = Path.Combine(gameMountPoint, CorlibTargetDirectory);
            CopyNewFile(sourceCorlibPath, destCorlibDirectory);
        }

        /// <summary>
        /// Copies the specified file from the source to the destination if the file doesn't already exist in the
        /// destination.
        /// If the destination directory structure doesn't exist, it will be created.
        /// </summary>
        /// <param name="sourceFilePath">Absolute path of the source file.</param>
        /// <param name="destDirectory">Absolute path to the file's destination directory.</param>
        /// <remarks>
        /// If the file already exists in the destination path, the file will not be overwritten.
        /// </remarks>
        private static void CopyNewFile(string sourceFilePath, string destDirectory)
        {
            // Create destination path if it doesn't exist
            if(!Directory.Exists(destDirectory.NormalizePath()))
            {
                Directory.CreateDirectory(destDirectory);
            }
            
            // ReSharper disable once AssignNullToNotNullAttribute
            var destFileName = Path.Combine(destDirectory, Path.GetFileName(sourceFilePath));
            // Copy file if it hasn't been copied
            if(!File.Exists(destFileName.NormalizePath()))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                File.Copy(sourceFilePath.NormalizePath(), destFileName.NormalizePath());
            }
        }
    }
}