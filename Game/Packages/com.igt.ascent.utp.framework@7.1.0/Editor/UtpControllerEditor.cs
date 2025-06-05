// -----------------------------------------------------------------------
// <copyright file = "UtpControllerEditor.cs" company = "IGT">
//     Copyright © 2014 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Utp.Framework.Editor
{
    using UnityEngine;
    using UnityEditor;
    using System.Linq;

    [InitializeOnLoad]
    [CustomEditor(typeof(UtpController))]
    public class UtpControllerEditor : Editor
    {
        private UtpController utpController;
        
        public static void RefreshModules()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var controller = (UtpController)FindObjectsOfType(typeof (UtpController)).FirstOrDefault();
            if(controller != null)
            {
                //  Force the refresh on save to ensure PreInitialize is run, required for certain components in the Editor (i.e. Current State Consumer)
                controller.RefreshModuleList(true);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            UtpController myScript = (UtpController)target;
            utpController = (UtpController)FindObjectsOfType(typeof(UtpController)).FirstOrDefault();


            if(utpController != null)
            {
                if(GUILayout.Button("Refresh Module List", GUILayout.Height(35)))
                {
                    myScript.RefreshModuleList(true);
                }

                if(GUILayout.Button("Initialize Modules", GUILayout.Height(35)))
                {
                    myScript.InitializeModules(null);
                }

                GUILayout.BeginVertical("Available Modules", GUI.skin.window, GUILayout.MaxHeight(350));
                GUILayout.BeginVertical("", GUI.skin.box, GUILayout.MaxHeight(100));
                
                if (myScript != null)
                {
                    //  Setting force=false means the refresh occurs only once
                    myScript.RefreshModuleList(false);

                    var sortedModules = myScript.TestModules.Where(mod => mod != null).OrderBy(GetModDisplayName);
                    foreach (var mod in sortedModules)
                    {
                        switch(mod.ModuleStatus)
                        {
                            case ModuleStatuses.Found:
                                GUI.contentColor = Color.white;
                                break;

                                case ModuleStatuses.InitializedEnabled:
                                GUI.contentColor = Color.green;
                                break;

                                case ModuleStatuses.InitializedDisabled:
                                GUI.contentColor = Color.yellow;
                                break;

                            case ModuleStatuses.Error:
                                GUI.contentColor = Color.red;
                                break;
                        }

                        GUILayout.Label(GetModDisplayName(mod));

                        GUI.contentColor = Color.white;
                    }
                }
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
            else
            {
                if(GUILayout.Button("Insert UTP", GUILayout.Height(35)))
                {
                    myScript.InsertUtp();
                }
            }
        }

        private static string GetModDisplayName(AutomationModule autoMod)
        {
            var modDesc = string.IsNullOrEmpty(autoMod.Description) ? "" : " - " + autoMod.Description;
            return " " + autoMod.Name + modDesc;
        }
    }
}