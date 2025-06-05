// -----------------------------------------------------------------------
//  <copyright file = "CustomBuildPathsTools.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
 
namespace IGT.Game.SDKAssets.SDKBuild.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;

    /// <summary>
    /// Editor functionality for the <see cref="CustomBuildPaths"/> class
    /// for creating the asset and copying the custom paths during a build.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class CustomBuildPathTools : IPreprocessBuildWithReport
    {
        #region Menu
 
        /// <summary>
        /// (Re)creates a new Custom Build Paths asset.
        /// </summary>
        [MenuItem("Assets/Create/Custom Build Paths")]
        public static void CreateCustomBuildPaths()
        {
            // This asset enables CustomBuildPathsInspector to edit the XML file.
            BuildEditorUtilities.CreateAsset<CustomBuildPathsManager>(BuildPaths.CustomBuildPathsAssetPath);
        }
 
        #endregion
        
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
        public int callbackOrder { get; } = 100;
        // ReSharper Restore All

        #endregion

        /// <summary>
        /// Internal helper to copy any custom directories or files to the build directory before the build
        /// process is started.
        /// </summary>
        /// <param name="buildOutputPath">The build output root.</param>  
        private static void PreprocessBuildInternal(string buildOutputPath)
        {
            if(!string.IsNullOrEmpty(buildOutputPath))
            {
                var projectDirectory = Directory.GetCurrentDirectory();
                var buildDirectory = Path.GetDirectoryName(buildOutputPath);
 
                var customBuildPaths = CustomBuildPathsManager.Load();
 
                if(customBuildPaths != null)
                {
                    if(customBuildPaths.CustomDirectories != null)
                    {
                        foreach(var directory in customBuildPaths.CustomDirectories)
                        {
                            BuildUtilities.CopyBuildDirectory(projectDirectory, buildDirectory, directory);
                        }
                    }
 
                    if(customBuildPaths.CustomFiles != null)
                    {
                        foreach(var file in customBuildPaths.CustomFiles)
                        {
                            BuildUtilities.CopyBuildFile(projectDirectory, buildDirectory, file);
                        }
                    }
                }
            }
        }
    }
}
