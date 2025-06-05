// -----------------------------------------------------------------------
//  <copyright file = "CustomBuildPathsInspector.cs" company = "IGT">
//      Copyright (c) 2017 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SDKBuild.Editor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Custom inspector for the <see cref="CustomBuildPaths"/> class.
    /// </summary>
    [CustomEditor(typeof(CustomBuildPathsManager))]
    public class CustomBuildPathsInspector : Editor
    {
        private static CustomBuildPaths buildPaths;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            DisplayAndUpdateFile();
        }

        /// <summary>
        /// Displays and updates the custom build paths file.
        /// </summary>
        private static void DisplayAndUpdateFile()
        {
            EditorGUILayout.LabelField("Configure directories and files to copy to the build output.",
                                       new GUIStyle(GUI.skin.label) { wordWrap = true });
            EditorGUILayout.Space();

            if(DisplayFileManagement())
            {
                EditorGUI.BeginChangeCheck();
                DisplayCustomDirectories();
                DisplayCustomFiles();

                if(EditorGUI.EndChangeCheck())
                {
                    CustomBuildPathsManager.Save(buildPaths);
                }
            }
        }

        /// <summary>
        /// Displays  the custom build paths XML file path and buttons to reload or delete the file.
        /// </summary>
        /// <returns>Whether the inspector GUI should continue.</returns>
        private static bool DisplayFileManagement()
        {
            var continueInspector = true;
            EditorGUILayout.BeginVertical(GUI.skin.box);

            EditorGUILayout.LabelField(CustomBuildPathsManager.FileName, EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(Path.Combine(Path.GetFullPath(BuildUtilities.GetApplicationPath()),
                                                         CustomBuildPathsManager.FileName));

            EditorGUILayout.BeginHorizontal();

            if(GUILayout.Button("Reload File", GUILayout.ExpandWidth(false)) ||
               buildPaths == null)
            {
                buildPaths = CustomBuildPathsManager.Load();

                if(buildPaths == null)
                {
                    buildPaths = new CustomBuildPaths();
                    CustomBuildPathsManager.Save(buildPaths);
                }
            }

            if(GUILayout.Button("Delete File", GUILayout.ExpandWidth(false)))
            {
                if(EditorUtility.DisplayDialog(string.Format("Delete {0}", CustomBuildPathsManager.FileName),
                                               string.Format("'{0}' will be deleted permanently.",
                                                             CustomBuildPathsManager.FileName),
                                               "Delete",
                                               "Cancel"))
                {
                    continueInspector = false;
                    buildPaths = null;
                    CustomBuildPathsManager.Delete();
                    BuildEditorUtilities.DeleteAsset(BuildPaths.CustomBuildPathsAssetPath);
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return continueInspector;
        }

        /// <summary>
        /// Displays the custom directories and provides buttons to add or exclude directories.
        /// </summary>
        private static void DisplayCustomDirectories()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Custom Directories", EditorStyles.boldLabel);

            if(buildPaths.CustomDirectories == null)
            {
                buildPaths.CustomDirectories = new List<string>();
            }

            if(GUILayout.Button(new GUIContent("Add Directory...",
                                               "Browse for a directory to copy during Build"),
                                GUILayout.ExpandWidth(false)))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var newDirectory = EditorUtility.OpenFolderPanel("Custom Project Directory",
                                                                 currentDirectory,
                                                                 string.Empty);

                AddCustomPath(newDirectory, buildPaths.CustomDirectories, currentDirectory, "directory");
            }

            DisplayCustomPaths(buildPaths.CustomDirectories, "directory");

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Displays the custom files and provides buttons to add or exclude files.
        /// </summary>
        private static void DisplayCustomFiles()
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Custom Files", EditorStyles.boldLabel);

            if(buildPaths.CustomFiles == null)
            {
                buildPaths.CustomFiles = new List<string>();
            }

            if(GUILayout.Button(new GUIContent("Add File...",
                                               "Browse for a file to copy during Build"),
                                GUILayout.ExpandWidth(false)))
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var newFileName = EditorUtility.OpenFilePanel("Custom Project File",
                                                              currentDirectory,
                                                              string.Empty);

                AddCustomPath(newFileName, buildPaths.CustomFiles, currentDirectory, "file");
            }

            DisplayCustomPaths(buildPaths.CustomFiles, "file");

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Tries to add a relative version of <paramref name="customPath"/> to the specified collection 
        /// if the custom path is under the <paramref name="projectDirectory"/>.
        /// </summary>
        /// <param name="customPath">Full path to add after it is transformed to a relative path.</param>
        /// <param name="customPaths">Collection of custom paths to add to.</param>
        /// <param name="projectDirectory">Project directory that <paramref name="customPath"/> must be under
        /// and from which <paramref name="customPath"/> will be made relative.</param>
        /// <param name="pathTypeName">The name of the path type.</param>
        private static void AddCustomPath(string customPath, ICollection<string> customPaths, string projectDirectory,
                                          string pathTypeName)
        {
            if(!string.IsNullOrEmpty(customPath))
            {
                // Convert path separators.
                customPath = Path.GetFullPath(customPath);

                // Ensure customPath is under projectDirectory.
                if(customPath != projectDirectory &&
                   customPath.StartsWith(projectDirectory))
                {
                    // Make relative to projectDirectory.
                    customPath = customPath.Replace(projectDirectory, string.Empty)
                                           .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                    if(!customPaths.Contains(customPath))
                    {
                        customPaths.Add(customPath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(string.Format("Invalid {0}", pathTypeName),
                                                string.Format("The {0} must be under your project folder.", pathTypeName),
                                                "OK");
                }
            }
        }

        /// <summary>
        /// Displays all the paths and provides buttons to exclude any path.
        /// </summary>
        /// <param name="customPaths">Collection of custom paths to display and/or remove from.</param>
        /// <param name="pathTypeName">The name of the path type.</param>
        private static void DisplayCustomPaths(ICollection<string> customPaths, string pathTypeName)
        {
            foreach(var customPath in customPaths.ToList())
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent(customPath, customPath));
                if(GUILayout.Button(new GUIContent("Exclude",
                                                   string.Format("No longer copies this {0} during Build",
                                                                 pathTypeName))))
                {
                    if(EditorUtility.DisplayDialog(string.Format("Exclude {0}", pathTypeName),
                                                   string.Format("'{0}' will no longer be copied during Build.",
                                                                 customPath),
                                                   "Exclude",
                                                   "Cancel"))
                    {
                        customPaths.Remove(customPath);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}