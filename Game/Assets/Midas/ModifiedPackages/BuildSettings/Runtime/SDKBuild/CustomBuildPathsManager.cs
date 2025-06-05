// -----------------------------------------------------------------------
//  <copyright file = "CustomBuildPathsManager.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild
{
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// Represents a <see cref="ScriptableObject"/> asset that loads and saves
    /// the XML file containing custom directories and files to copy when the project is built.
    /// </summary>
    public class CustomBuildPathsManager : ScriptableObject
    {
        /// <summary>
        /// The file name of the custom build paths XML file.
        /// </summary>
        public const string FileName = "CustomBuildPaths.xml";

        /// <summary>
        /// The full file path of the custom build paths XML file.
        /// </summary>
        public static string FullFilePath;

        /// <summary>
        /// Loads <see cref="CustomBuildPaths"/> from the default location.
        /// </summary>
        /// <returns>Custom build paths object, or null if no file exists.</returns>
        public static CustomBuildPaths Load()
        {
            return XmlUtilities.LoadFrom<CustomBuildPaths>(FullFilePath);
        }

        /// <summary>
        /// Saves <see cref="CustomBuildPaths"/> to the default location.
        /// </summary>
        /// <param name="customBuildPaths">Custom build paths to save.</param>
        public static void Save(CustomBuildPaths customBuildPaths)
        {
            XmlUtilities.SaveTo(FullFilePath, customBuildPaths);
        }

        /// <summary>
        /// Deletes <see cref="CustomBuildPaths"/> from the default location.
        /// </summary>
        public static void Delete()
        {
            if(File.Exists(FullFilePath))
            {
                File.Delete(FullFilePath);
            }
        }
        
        /// <summary>
        /// Builds <see cref="FullFilePath"/> from the root project directory.
        /// </summary>
        protected virtual void OnEnable()
        {
            FullFilePath = Path.Combine(BuildUtilities.GetApplicationPath(), FileName);
        }
    }
}