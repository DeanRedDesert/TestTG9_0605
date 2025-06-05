//-----------------------------------------------------------------------
// <copyright file = "CsiConfigEditor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.CsiConfig.Editor
{
    using System;
    using System.Reflection;
    using ConfigurationEditor.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Main class for the configuration editor.
    /// </summary>
    [ConfigurationFile("CsiConfig.xml")]
    [ConfigurationRoot("CsiConfigurations")]
    [SupportedConfiguration("Volume Settings",
                            Name = "VolumeSettings",
                            ConfigTypeAssembly = "IGT.Game.Core.Communication.Standalone.Schemas",
                            ConfigTypeName = "VolumeSettingsConfig",
                            EditorTypeName = "VolumeSettingsEditor")]
    [SupportedConfiguration("Machine Activity",
                            Name = "MachineActivity",
                            ConfigTypeAssembly = "IGT.Game.Core.Communication.Standalone.Schemas",
                            ConfigTypeName = "MachineActivityConfig",
                            EditorTypeName = "MachineActivityEditor")]
    [SupportedConfiguration("Monitor Settings",
                            Name = "MonitorSettings",
                            ConfigTypeAssembly = "IGT.Game.Core.Communication.Standalone.Schemas",
                            ConfigTypeName = "MonitorSettingsConfig",
                            EditorTypeName = "MonitorSettingsEditor")]
    [SupportedConfiguration("ButtonPanel Settings",
                            Name = "ButtonPanelSettings",
                            ConfigTypeAssembly = "IGT.Game.Core.Communication.Standalone.Schemas",
                            ConfigTypeName = "ButtonPanelSettingsConfig",
                            EditorTypeName = "ButtonPanelSettingsEditor")]
    [SupportedConfiguration("Service Settings",
                            Name = "ServiceSettings",
                            ConfigTypeAssembly = "IGT.Game.Core.Communication.Standalone.Schemas",
                            ConfigTypeName = "ServiceSettingsConfig",
                            EditorTypeName = "ServiceSettingsEditor")]
    public class CsiConfigEditor : EditorWindow
    {
        private static ConfigurationMaster master;

        // Settings for the editor window.
        private Vector2 scrollPos;
        private const int BoxSize = 250;
        private static readonly Vector2 WindowSize = new Vector2(700, 700);
        private static readonly Vector2 WindowMinSize = new Vector2(500, 500);
        public const int FoldoutSize = 400;

        // Local error message and type.
        private static string errorMessage;
        private static MessageType messageType;

        #region Initialize

        /// <summary>
        /// Get the window when the menu item for it is selected.
        /// Init function sets the default values for the members in the system configurator file.
        /// </summary>
        [MenuItem("Tools/Standalone Configurator/CSI Configurator")]
        private static void Init()
        {
            var thisType = MethodBase.GetCurrentMethod().DeclaringType;

            // Get existing open window or if none, make a new one:
            var csiConfigurationEditor = GetWindow(thisType, false, "CSI Configurator", true);
            if(csiConfigurationEditor == null) return;

            csiConfigurationEditor.maxSize = WindowSize;
            csiConfigurationEditor.minSize = WindowMinSize;

            errorMessage = string.Empty;
            try
            {
                master = new ConfigurationMaster(thisType);
                master.OpenConfigurations();
            }
            catch(Exception exception)
            {
                errorMessage = exception.Message;
                messageType = MessageType.Error;
                throw;
            }
        }

        #endregion

        /// <summary>
        /// Called each time the GUI is refreshed.
        /// </summary>
        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("CSI Config Editor");
            EditorGUILayout.Space();
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width - 8),
                GUILayout.Height(position.height - 32));

            if(master != null)
            {
                master.EditConfigurations();
                if(GUILayout.Button(new GUIContent("Save Configuration", "Save contents to XML file"),
                    GUILayout.MaxWidth(BoxSize)))
                {
                    errorMessage = string.Empty;
                    try
                    {
                        master.SaveConfigurations();
                        errorMessage = "Write Complete without errors";
                        messageType = MessageType.Info;
                    }
                    catch(Exception exception)
                    {
                        errorMessage = string.Format(
                            "There was an unhandled exception of type {1} when trying to save the CSI configurations. Error: {0}",
                            exception.Message, exception.GetType().Name);
                        messageType = MessageType.Error;
                        throw;
                    }
                }

                if(GUILayout.Button(new GUIContent("Clear Cache", "Clears editor system config cache and reloads from the root system config file."), 
                       GUILayout.MaxWidth(BoxSize)))
                {
                    try
                    {
                        master = new ConfigurationMaster(MethodBase.GetCurrentMethod().DeclaringType);
                        master.OpenConfigurations();
                    }
                    catch (Exception exception)
                    {
                        errorMessage = exception.Message;
                        messageType = MessageType.Error;
                        throw;
                    }
                }

                if(!string.IsNullOrEmpty(errorMessage))
                {
                    EditorGUILayout.HelpBox(errorMessage, messageType);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
