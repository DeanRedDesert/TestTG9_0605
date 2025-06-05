//-----------------------------------------------------------------------
// <copyright file = "SystemConfigEditor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Main class for the editor.
    /// </summary>
    public class SystemConfigEditor : EditorWindow
    {
        private SetFoundationOwnedSettings foundationOwnedSettings;
        private SetPaytableList paytableList;
        private SetSystemControlledProgressives systemControlledProgressives;
        private SetProgressiveSimulatorSetup progressiveSimulator;
        private SetGameSubMode setGameSubMode;
        private SetTournamentSessionConfiguration setTournamentSessionConfiguration;
        private SetGameLinkConfiguration setGameLinkConfiguration;
        private SystemConfigFileHelper systemConfigFileHelper;
        private Vector2 scrollPos;
        public static int BoxSize = 250;
        public static int ButtonSize = 50;
        public static int WindowSize = 700;
        public static int FoldoutSize = 400;

        #region Initialize

        /// <summary>
        /// Get the window when the menu item for it is selected.
        /// Init function sets the default values for the members in the system configurator file.
        /// </summary>
        [MenuItem("Tools/Standalone Configurator/Foundation Configurator")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var systemEditor =
                GetWindow(typeof(SystemConfigEditor), false, "Foundation Configurator", true) as SystemConfigEditor;
            if(systemEditor == null)
            {
                return;
            }
            systemEditor.maxSize = new Vector2(WindowSize, WindowSize);
            systemEditor.paytableList = new SetPaytableList();
            systemEditor.foundationOwnedSettings = new SetFoundationOwnedSettings();
            systemEditor.systemControlledProgressives = new SetSystemControlledProgressives(systemEditor.paytableList);
            systemEditor.progressiveSimulator = new SetProgressiveSimulatorSetup();
            systemEditor.setGameSubMode = new SetGameSubMode();
            systemEditor.setTournamentSessionConfiguration = new SetTournamentSessionConfiguration();
            systemEditor.setGameLinkConfiguration = new SetGameLinkConfiguration();
            systemEditor.systemConfigFileHelper = new SystemConfigFileHelper(systemEditor.paytableList,
                systemEditor.foundationOwnedSettings,
                systemEditor.systemControlledProgressives,
                systemEditor.progressiveSimulator,
                systemEditor.setGameSubMode,
                systemEditor.setTournamentSessionConfiguration,
                systemEditor.setGameLinkConfiguration);
            systemEditor.systemConfigFileHelper.LoadSetsFromFile();
        }

        #endregion

        /// <summary>
        /// Called each time the GUI is refreshed.
        /// </summary>
        private void OnGUI()
        {
            if(paytableList == null || foundationOwnedSettings == null ||
               systemControlledProgressives == null || progressiveSimulator == null)
            {
                return;
            }
            DisplaySystemConfiguration();
        }

        #region Foundation Configuration Foldout

        /// <summary>
        /// The main foldout to display the Foundation Configurations.
        /// </summary>
        public void DisplaySystemConfiguration()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Foundation Config Editor");
            EditorGUILayout.Space();
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - 8),
                GUILayout.Height(position.height - 32));
            paytableList.DisplayPaytable();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            foundationOwnedSettings.DisplayFoundation();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            progressiveSimulator.DisplayProgressiveSimulatorSetup();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            systemControlledProgressives.DisplaySystemControlledProgressives();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            setGameSubMode.UpdateGameSubMode(paytableList.GetFirstPaytableType());
            setGameSubMode.DisplayGameSubMode();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            if(setGameSubMode.Data.Type == GameSubModeString.Tournament)
            {
                setTournamentSessionConfiguration.DisplayTournamentSessionConfiguration();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            setGameLinkConfiguration.DisplayGameLinkConfiguration();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            systemConfigFileHelper.Display();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        /// <summary>
        /// Checks if the value is negative. if yes returns 0 else the same number.
        /// </summary>
        /// <param name="value">The number passed.</param>
        /// <returns>The value returned is the number passed if its value greater or equal to 0 else 0.</returns>
        public static int CheckNegative(int value)
        {
            return value < 0 ? 0 : value;
        }
    }
}
