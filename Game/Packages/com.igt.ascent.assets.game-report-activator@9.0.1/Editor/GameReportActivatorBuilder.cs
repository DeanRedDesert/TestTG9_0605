// -----------------------------------------------------------------------
//  <copyright file = "GameReportActivatorLocator.cs" company = "IGT">
//      Copyright (c) 2021 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.SDK.GameReport.Editor
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using Debug = UnityEngine.Debug;
    
    /// <summary>
    /// This class builds the activator application.
    /// </summary>
    public sealed class GameReportActivatorBuilder
    {
        /// <summary>
        /// Const of the activator executable file name.
        /// </summary>
        public const string ActivatorExecutableName = "GameReportActivator.exe";

        /// <summary>
        /// The singleton instance.
        /// </summary>
        private static GameReportActivatorBuilder builder;
        
        private const string ActivatorProjectDirName = ".ActivatorProject";
        private readonly string targetDir;
        private readonly string activatorProjectDirPath;
        
        /// <summary>
        /// Gets the activator executable path. 
        /// </summary>
        public string ActivatorExecutablePath => Path.Combine(targetDir, "Bin", ActivatorExecutableName);

        /// <summary>
        /// Gets the builder instance.
        /// </summary>
        public static GameReportActivatorBuilder GetBuild(bool forceRebuild=false)
        {
            if(builder == null || forceRebuild)
            {
                var activatorBuildPath = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(FileUtil.GetUniqueTempPathInProject())!,
                    "GameReportActivator"));
                builder = new GameReportActivatorBuilder(activatorBuildPath);
                builder.BuildProject(forceRebuild);
            }
            else if(!File.Exists(builder.ActivatorExecutablePath.NormalizePath()) ||
                    IsUpdatedAfter(builder.activatorProjectDirPath, 
                        File.GetLastWriteTime(builder.ActivatorExecutablePath.NormalizePath())))
            {
                // In case the activator target folder is cleared, or is out dated. 
                builder.BuildProject();
            }

            return builder;
        }

        /// <summary>
        /// Check if the files under the specified path has been updated after a given timestamp. 
        /// </summary>
        private static bool IsUpdatedAfter(string path, DateTime timestamp)
        {
            var normalizedPath = path.NormalizePath();
            if(File.GetLastWriteTime(normalizedPath) > timestamp)
            {
                return true;
            }

            var directoryInfo = new DirectoryInfo(normalizedPath);
            if(directoryInfo.Exists)
            {
                var fileInfos = directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories);
                return fileInfos.Any(file => file.LastWriteTime > timestamp);
            }

            return false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        private GameReportActivatorBuilder(string targetDir)
        {
            this.targetDir = targetDir;
            var assetIds = AssetDatabase.FindAssets($"t:Script {nameof(GameReportActivatorBuilder)}");
            if(!assetIds.Any())
            {
                throw new InvalidOperationException(
                    $"Failed to locate mono script asset {nameof(GameReportActivatorBuilder)}");
            }

            var scriptPath = AssetDatabase.GUIDToAssetPath(assetIds[0]);
            var scriptDir = Path.GetFullPath(Path.GetDirectoryName(scriptPath)!);
            activatorProjectDirPath = Path.Combine(scriptDir, ActivatorProjectDirName);
        }

        /// <summary>
        /// Build the activator project.
        /// </summary>
        private void BuildProject(bool forceRebuild=false)
        {
            var binPath = Path.GetFullPath(Path.Combine(targetDir, "Bin\\"));
            FileUtil.DeleteFileOrDirectory(binPath);
            if(!forceRebuild)
            {
                var prebuildExecutable = Path.Combine(Path.GetDirectoryName(activatorProjectDirPath!),
                    ".Exe",
                    ActivatorExecutableName);
                var normalizedExecutablePath = prebuildExecutable.NormalizePath();
                if(File.Exists(normalizedExecutablePath))
                {
                    Directory.CreateDirectory(binPath);
                    var targetFile = Path.Combine(binPath, ActivatorExecutableName).NormalizePath();
                    File.Copy(normalizedExecutablePath, targetFile, true);
                    new FileInfo(targetFile).IsReadOnly = false;
                    return;
                }
            }

            var activatorProjectPath = Path.Combine(activatorProjectDirPath, "BuildActivator.proj");
            // Since the project path is passed to the commandline as an argument,
            // we double quote the path if it contains space characters.
            if(activatorProjectPath.Contains(" "))
            {
                activatorProjectPath = $@"""{activatorProjectPath}""";
            }
            
            // For MSBuild property values, we use "%20" to escape the space characters.
            // See https://docs.microsoft.com/en-us/visualstudio/msbuild/how-to-escape-special-characters-in-msbuild
            binPath = binPath.Replace(" ", "%20");
            var objPath = Path.GetFullPath(Path.Combine(targetDir, "Obj\\")).Replace(" ", "%20");

            var msbuildArguments = $"{activatorProjectPath} /t:Rebuild  /p:OutputPath={binPath}" +
                                   $" /p:IntermediateOutputPath={objPath}";

            var msbuildPath = MsBuildPathLocator.GetPath();
            var (exitCode, output, errorOutput) = ProcessRunner.RunAndGetOutput(msbuildPath, msbuildArguments);
            PrintErrorMessages(exitCode, output, errorOutput);
            if(exitCode != 0)
            {
                throw new BuildFailedException("Failed to build game report activator.");
            }

            FileUtil.DeleteFileOrDirectory(objPath);
        }
        
        /// <summary />
        private static void PrintErrorMessages(int exitCode, string output, string errorOutput)
        {
            if(!string.IsNullOrWhiteSpace(errorOutput))
            {
                Debug.LogError(errorOutput);
            }

            if(exitCode != 0 && !string.IsNullOrWhiteSpace(output))
            {
                // Print the full log in error condition for troubleshooting purpose.
                Debug.LogError(output);
            }
        }
    }
}