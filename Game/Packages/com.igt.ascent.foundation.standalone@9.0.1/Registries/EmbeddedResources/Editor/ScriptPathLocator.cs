// -----------------------------------------------------------------------
//  <copyright file = "ScriptPathLocator.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
// <auto-deployed>
//   This file is deployed by the UPM deployment tool.
// </auto-deployed>
// -----------------------------------------------------------------------

namespace EmbeddedResources.Editor
{
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEngine;

    /// <summary>
    /// This is an assistant class used to locate the script path.
    /// </summary>
    /// <remarks>
    /// The only reason that this class inherits from ScriptableObject is to be able to invoke the API
    /// MonoScript.FromScriptableObject(ScriptableObject) to locate the current script path.
    /// We couldn't use API AssetDatabase.FindAssets($"t:Script ScriptName") to achieve this, because there
    /// could be multiple "ScriptPathLocator" scripts deployed and the later API would always return the
    /// first script file with the same name.
    /// </remarks>
    internal class ScriptPathLocator : ScriptableObject
    {
        private static string currentScriptPath;
        
        /// <summary>
        /// Gets the embedded resources directory's full path.
        /// </summary>
        /// <returns></returns>
        public static string GetEmbeddedResourcesDir()
        {
            return Path.Combine(GetCurrentDir(), @"..\.Res");
        }

        /// <summary>
        /// Gets the embedded resources directory's full path.
        /// </summary>
        /// <returns></returns>
        public static string GetEmbeddedResourcesAssemblyDir()
        {
            return Path.Combine(GetCurrentDir(), @"..\.Dll");
        }
        
        /// <summary>
        /// Gets the current script directory's full path.
        /// </summary>
        /// <returns>The current script directory's full path.</returns>
        private static string GetCurrentDir()
        {
            var dirPath = Path.GetDirectoryName(GetCurrentScriptPath());
            return dirPath;
        }

        /// <summary>
        /// Gets the current script's full path.
        /// </summary>
        /// <returns>The current script's full path.</returns>
        private static string GetCurrentScriptPath()
        {
            if(currentScriptPath != null)
            {
                return currentScriptPath;
            }
            
            var instance = CreateInstance<ScriptPathLocator>();
            try
            {
                var asset = MonoScript.FromScriptableObject(instance);
                if(asset == null)
                {
                    throw new BuildFailedException($"Failed to locate script {nameof(ScriptPathLocator)}.");
                }

                var scriptPath = AssetDatabase.GetAssetPath(asset);
                currentScriptPath = Path.GetFullPath(scriptPath);
            }
            finally
            {
                DestroyImmediate(instance);
            }

            return currentScriptPath;
        }
    }
}