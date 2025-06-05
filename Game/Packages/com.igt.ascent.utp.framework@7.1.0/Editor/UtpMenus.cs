// -----------------------------------------------------------------------
// <copyright file = "UtpMenus.cs" company = "IGT">
//     Copyright © 2014 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    // ReSharper disable UnusedMember.Local
    internal static class UtpMenus
    {
        /// <summary>
        /// Creates and adds a UnityTestPortal prefab to \UnityTestPortal and adds it to the scene
        /// </summary>
        /// <returns></returns>
        [MenuItem("Tools/Unity Test Portal/Create UTP Prefab")]
        private static bool CreatePrefab()
        {
            var result = false;
            Debug.Log("UTPBuild.CreatePrefab() started...");

            // Find any existing UtpController objects
            var utpController = Object.FindObjectOfType(typeof(UtpController)) as UtpController;

            if(utpController == null)
            {
                var tempObject = new GameObject("UnityTestPortal");
                tempObject.AddComponent(typeof(UtpController));
            }

            var utpGameObject = GameObject.Find("UnityTestPortal");
            if(utpGameObject != null)
            {
                Debug.Log("Creating prefab with UTP GameObject");
                PrefabUtility.SaveAsPrefabAssetAndConnect(utpGameObject, "Assets/UnityTestPortal/UnityTestPortal.prefab", InteractionMode.AutomatedAction);

                AssetDatabase.SaveAssets();

                // Remove game object
                Object.DestroyImmediate(utpGameObject);

                result = true;
            }
            else
            {
                Debug.LogError("Couldn't find 'UnityTestPortal' GameObject");
            }

            Debug.Log("UTPBuild.CreatePrefab() ended!");
            return result;
        }

        /// <summary>
        /// Packages UTP
        /// </summary>
        [MenuItem("Tools/Unity Test Portal/Create Package")]
        private static void CreatePackage()
        {
            Debug.Log("UTPBuild.CreatePackage() started...");

            string packageName = "UnityTestPortal.unitypackage";
            string[] assetPathsNames = { "Assets/UnityTestPortal" };

            // Process any provided arguments
            var args = GetCustomArguments();

            if(args.ContainsKey("packagename"))
            {
                packageName = args["packagename"];
            }
            if(args.ContainsKey("assetpathnames"))
            {
                assetPathsNames = args["assetpathnames"].Split(',');
            }

            Debug.Log("Exporting package...");
            AssetDatabase.ExportPackage(assetPathsNames, packageName, ExportPackageOptions.Recurse);
            Debug.Log("Export complete!");

            Debug.Log("UTPBuild.CreatePackage() ended!");
        }

        /// <summary>
        /// Inserts a Unity Test Portal object into the scene if one doesn't already exist
        /// </summary>
        /// <returns>Success of insertion</returns>
        [MenuItem("Tools/Unity Test Portal/Insert UTP")]
        [MenuItem("GameObject/Create Other/Unity Test Portal")]
        private static bool InsertUtp()
        {
            bool result = false;
            Debug.Log("UTPBuild.InsertUtp() started...");

            // Find any existing UtpController objects
            var utpController = Object.FindObjectOfType(typeof(UtpController)) as UtpController;

            if(utpController == null)
            {
                var tempObject = new GameObject("UnityTestPortal");
                tempObject.AddComponent(typeof(UtpController));
                utpController = tempObject.GetComponent(typeof(UtpController)) as UtpController;
                if(utpController != null)
                {
                    Debug.Log("Inserting UTP...");
                    utpController.InsertUtp();
                    result = true;
                }
                else
                {
                    Debug.LogError("Couldn't find UtpController, UTP not inserted!");
                }
            }
            else
            {
                Debug.LogWarning("UtpController already available in the scene, skipped inserting another");
                result = true;
            }


            Debug.Log("UTPBuild.InsertUtp() ended!");
            return result;
        }

        private static Dictionary<string, string> GetCustomArguments()
        {
            const string prefix = "-Args:";
            var customArgDict = new Dictionary<string, string>();
            var commandLineArgs = Environment.GetCommandLineArgs();
            var customString = commandLineArgs.FirstOrDefault(a => a.StartsWith(prefix));

            if(customString != null)
            {
                customString = customString.Replace(prefix, "");
                foreach(var customArg in customString.Split(';'))
                {
                    var customArgParts = customArg.Split('=');
                    if(customArgParts.Length > 0)
                    {
                        var key = customArgParts[0].ToLower();
                        var val = customArgParts.Length == 1 ? "" : customArgParts[1];
                        if(!customArgDict.ContainsKey(key))
                        {
                            customArgDict.Add(key, val);
                        }
                    }
                }
            }
            return customArgDict;
        }

    }
}