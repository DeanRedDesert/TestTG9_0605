// -----------------------------------------------------------------------
//  <copyright file = "BuildEditorUtilities.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Contains utility methods related to the build process that are required
    /// by Editors, custom Menu Items, and Inspectors.
    /// </summary>
    public static class BuildEditorUtilities
    {
        /// <summary>
        /// Creates an asset for a specific <see cref="ScriptableObject"/>.
        /// </summary>
        /// <typeparam name="TAsset">Type to create asset for that must inherit <see cref="ScriptableObject"/>.</typeparam>
        /// <param name="path">Path of the asset file relative to the project folder.</param>
        public static void CreateAsset<TAsset>(string path) where TAsset : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<TAsset>();
            DeleteAsset(path);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
        }

        /// <summary>
        /// Deletes an asset.
        /// </summary>
        /// <param name="path">Full path of the asset file.</param>
        public static void DeleteAsset(string path)
        {
            AssetDatabase.DeleteAsset(path);
        }
    }
}