// -----------------------------------------------------------------------
// <copyright file = "UtpAssetModificationProcessor.cs" company = "IGT">
//     Copyright © 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Framework;
    using UnityEngine;

    class UtpAssetModificationProcessor : UnityEditor.AssetModificationProcessor
    {
        /// <summary>
        /// List of namespaces of valid components that can be added to the UTP object in the hierarchy.
        /// </summary>
        private static readonly List<string> validComponents = new List<string>
        {
            "IGT.Game.Utp"
        };

        private static List<IUtpControllerAttachableDefinition> utpAttachableDefinitions;

        static string[] OnWillSaveAssets(string[] paths)
        {
            ValidateUtpComponents();

            UtpControllerEditor.RefreshModules();
            return paths;
        }

        /// <summary>
        /// Validate that all components attached to the UtpController are expected to be there.
        /// Logs an error message if a component namespace isn't in the list of valid components.
        /// </summary>
        private static void ValidateUtpComponents()
        {
            var utpControllers = UnityEngine.Object.FindObjectsOfType<UtpController>();

            //  If UTP isn't in the scene then there's nothing to validate
            if (!utpControllers.Any())
                return;

            //  If more than 1 UtpController exists, then let the user know (would probably most commonly occur when UTP is added to different scenes)
            if(utpControllers.Length != 1)
                Debug.LogError("UTP: Found " + utpControllers.Length + " UtpControllers in the game. Only 1 is needed.");

            if(utpAttachableDefinitions == null)
            {
                utpAttachableDefinitions = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => !assembly.IsDynamic && !assembly.ReflectionOnly)
                    .SelectMany(assembly => assembly.GetExportedTypes())
                    .Where(type => type.IsVisible
                                   && type.IsClass
                                   && !type.IsAbstract
                                   && !type.IsGenericType
                                   && typeof(IUtpControllerAttachableDefinition).IsAssignableFrom(type))
                    .Select(type => (IUtpControllerAttachableDefinition)Activator.CreateInstance(type))
                    .ToList();
                
                foreach(var attachableDefinition in utpAttachableDefinitions)
                {
                    validComponents.AddRange(attachableDefinition.GetAttachableNamespaces());
                }
            }

            foreach(var utpController in utpControllers)
            {
                //  Verify that the namespace of each component is in the safe list
                foreach(var obj in utpController.GetComponentsInChildren<MonoBehaviour>())
                {
                    var objFullName = obj.GetType().FullName;
                    if(string.IsNullOrEmpty(objFullName) || !validComponents.Any(e => objFullName.StartsWith(e, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        Debug.LogError("UTP: Invalid object found on UtpController: " + obj.name + " Component Type: " + objFullName);
                    }
                }

                //  Verify there are no objects without a MonoBehavior (potentially misplaced game content)
                ValidateChildren(utpController.transform);
            }
        }

        /// <summary>
        /// Recursively verify that the transform and its children each have at least 1 MonoBehaviour script.
        /// </summary>
        /// <param name="transform">The transform to validate.</param>
        private static void ValidateChildren(Transform transform)
        {
            var components = transform.GetComponents<MonoBehaviour>();
            if(!components.Any())
            {
                Debug.LogError("Invalid object found on UtpController (no MonoBehaviour attached): " + transform.name);
            }

            foreach (Transform child in transform)
            {
                ValidateChildren(child);
            }
        }
    }
}
