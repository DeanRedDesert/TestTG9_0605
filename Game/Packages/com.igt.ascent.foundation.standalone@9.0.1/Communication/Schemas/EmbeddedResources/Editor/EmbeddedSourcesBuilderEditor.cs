// -----------------------------------------------------------------------
//  <copyright file = "EmbeddedSourcesBuilder.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
// <auto-deployed>
//   This file is deployed by the UPM deployment tool.
// </auto-deployed>
// -----------------------------------------------------------------------

namespace EmbeddedResources.Editor
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using Assembly = System.Reflection.Assembly;

    /// <summary>
    /// This class builds embedded source assembly during Unity script compilation time and
    /// game build time for UPM packages.
    /// </summary>
    [InitializeOnLoad]
    internal class EmbeddedSourcesBuilderEditor : IPostprocessBuildWithReport
    {
        /// <summary>
        /// Executed after assemblies are loaded.
        /// </summary>
        static EmbeddedSourcesBuilderEditor()
        {
            if(!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                BuildEmbeddedResources();
            }
        }

        #region Interface IPostprocessBuildWithReport Implementation

        /// <summary>
        /// Default execution order.
        /// </summary>
        public int callbackOrder => 103;

        /// <summary>
        /// Build the embedded resources assembly into the game build folder.
        /// </summary>
        /// <param name="report"></param>
        public void OnPostprocessBuild(BuildReport report)
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var currentAssemblyName = Path.GetFileNameWithoutExtension(codeBase);
            var tempFolder = Path.GetFullPath(Path.GetDirectoryName(FileUtil.GetUniqueTempPathInProject()));
            var assemblyName = currentAssemblyName.Replace("EmbeddedResources.Editor", "Resources");
            var outputPath = Path.Combine(tempFolder, "EmbeddedResourcesAssemblies", assemblyName);
            new EmbeddedSourcesBuilder(outputPath).Build(forceRebuild:true);
            
            var buildDir = Path.GetDirectoryName(report.summary.outputPath);
            var gameName = Path.GetFileNameWithoutExtension(report.summary.outputPath);
            var destinationPath = Path.Combine(buildDir, gameName + "_Data", "Managed");
            var builtAssemblyFile = Path.Combine(outputPath, assemblyName+".dll");
            if(File.Exists(builtAssemblyFile.NormalizePath()))
            {
                var copyToFile = Path.Combine(destinationPath, assemblyName + ".dll");
                FileUtil.DeleteFileOrDirectory(copyToFile);
                FileUtil.CopyFileOrDirectory(builtAssemblyFile, Path.Combine(destinationPath, assemblyName + ".dll"));
            }
        }

        #endregion

        /// <summary>
        /// Build the embedded resources assembly into the ScriptAssemblies folder.
        /// </summary>
        private static void BuildEmbeddedResources()
        {
            var codeBase = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var scriptsAssembliesDir = Path.GetDirectoryName(codeBase);
            var currentAssemblyName = Path.GetFileNameWithoutExtension(codeBase);
            var tempFolder = Path.GetFullPath(Path.GetDirectoryName(FileUtil.GetUniqueTempPathInProject()));
            var assemblyName = currentAssemblyName.Replace("EmbeddedResources.Editor", "Resources");
            var outputPath = Path.Combine(tempFolder, "EmbeddedResourcesAssemblies", assemblyName);
            var destinationAssemblyFile = Path.Combine(outputPath, assemblyName+".dll");
            var embeddedResourcesDir = ScriptPathLocator.GetEmbeddedResourcesDir();
            if(IsDirectoryNewerThan(embeddedResourcesDir, destinationAssemblyFile))
            {
                new EmbeddedSourcesBuilder(outputPath).Build();
            }

            if(File.Exists(destinationAssemblyFile.NormalizePath()))
            {
                var copyToFile = Path.Combine(scriptsAssembliesDir, assemblyName + ".dll");
                FileUtil.DeleteFileOrDirectory(copyToFile);
                FileUtil.CopyFileOrDirectory(destinationAssemblyFile, copyToFile);
                new FileInfo(copyToFile.NormalizePath()).IsReadOnly = false;
            }
        }

        /// <summary>
        /// Check if the directory <paramref name="dir"/> is newer than file <paramref name="targetFile"/>.
        /// </summary>
        private static bool IsDirectoryNewerThan(string dir, string targetFile)
        {
            var targetLastWriteTime = File.GetLastWriteTimeUtc(targetFile.NormalizePath());
            var folderLastWriteTime = File.GetLastWriteTimeUtc(dir.NormalizePath());
            if(folderLastWriteTime > targetLastWriteTime)
            {
                return true;
            }

            foreach(var entry in Directory.EnumerateFileSystemEntries(dir.NormalizePath(), "*",
                        SearchOption.AllDirectories))
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(entry.NormalizePath());
                if(lastWriteTime > targetLastWriteTime)
                {
                    return true;
                }
            }

            return false;
        }
    }
}