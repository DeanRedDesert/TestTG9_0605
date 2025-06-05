//-----------------------------------------------------------------------
// <copyright file = "IgtEgmBuildSettingsAutoRun.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor
{
    using System.IO;
    using SDKBuild;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class is used to execute a method when Unity loads a project,
    /// or when changes occur in the project window.
    /// </summary>
    [InitializeOnLoad]
    public class IgtEgmBuildSettingsAutoRun
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        static IgtEgmBuildSettingsAutoRun()
        {
            LoadAscentBuildSettings();

            // This makes sure a new build settings asset gets created if the existing one gets deleted.
            EditorApplication.projectChanged += LoadAscentBuildSettings;
        }

        /// <summary>
        /// Creates a new build settings asset if none exists.
        /// </summary>
        [MenuItem("Assets/Create/Ascent Build Settings")]
        private static void LoadAscentBuildSettings()
        {
            var asset = AssetDatabase.LoadAssetAtPath(BuildPaths.BuildSettingsAssetPath, typeof(IGTEGMBuild)) as IGTEGMBuild;
            if(asset == null)
            {
                // We could be checking before library is created, so try to look for the ascent build settings
                // asset using file system.
                var found = File.Exists(Path.Combine(Directory.GetCurrentDirectory(), BuildPaths.BuildSettingsAssetPath));

                if(!found)
                {
                    asset = ScriptableObject.CreateInstance<IGTEGMBuild>();

                    // create a new asset
                    IGTEGMSettings.SerializeIgtEgmBuildSettings(asset, BuildPaths.BuildSettingsAssetPath);
                }
            }
        }
    }
}
