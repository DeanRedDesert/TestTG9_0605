// -----------------------------------------------------------------------
//  <copyright file = "MonitorSettingsEditor.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.CsiConfig.Editor
{
    using System.Collections.Generic;
    using ConfigurationEditor.Editor;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Defines the inherited class for editing the configuration settings for Monitor Settings.
    /// </summary>
    internal class MonitorSettingsEditor : ConfigurationEditorBase<MonitorSettingsConfig>
    {
        /// <summary>
        /// The flag indicating if monitor settings folder is foldout or not.
        /// </summary>
        private bool isFoldout;

        /// <summary>
        /// List of flags indicating if the monitor configuration folder is foldout or not.
        /// </summary>
        private readonly List<bool> configurationFoldouts = new List<bool>();

        /// <summary>
        /// The flag indicating if the list of configurationFouldouts is initialized or not.
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// The size of button for the editor window.
        /// </summary>
        private const int ButtonSize = 50;

        /// <inheritdoc />
        public override void UpdateConfig(string configId)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, configId);
            if(!isFoldout) return;

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();
            Supported = EditorGUILayout.BeginToggleGroup("Supported", Supported);
            EditorGUI.indentLevel += 2;
            // Display the monitor configurations and get user input.
            if(!isInitialized)
            {
                configurationFoldouts.Clear();
                for(var index = 0; index < CachedConfig.Monitors.Count; index++)
                {
                    configurationFoldouts.Add(false);
                }
                isInitialized = true;
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            // Adds a new monitor configuration to the list.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Monitor Configurations");
            if(GUILayout.Button("+", GUILayout.MaxWidth(ButtonSize)))
            {
                var monitorType = new MonitorType
                {
                    Model = new MonitorModelType(),
                    DesktopCoordinates = new DesktopRectangleType()
                };
                CachedConfig.Monitors.Add(monitorType);
                configurationFoldouts.Add(false);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            for(var index = 0; index < CachedConfig.Monitors.Count; index++)
            {
                EditorGUILayout.Separator();
                if(DisplayMonitorConfiguration(index))
                {
                    CachedConfig.Monitors.RemoveAt(index);
                    configurationFoldouts.RemoveAt(index);
                    break;
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel -= 2;
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Displays individual monitor configuration.
        /// </summary>
        /// <param name="index">The index of the monitor configuration.</param>
        /// <returns>True if the monitor configuration is deleted.</returns>
        private bool DisplayMonitorConfiguration(int index)
        {
            var foldoutName = "Monitor Configuration " + (index + 1);
            var labelWith = 160;
            // The foldout name followed by a "-" button.
            EditorGUILayout.BeginHorizontal();
            configurationFoldouts[index] = EditorGUILayout.Foldout(configurationFoldouts[index], foldoutName);
            if(GUILayout.Button("-", GUILayout.Width(ButtonSize)))
            {
                return true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if(configurationFoldouts[index])
            {
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("DeviceId", "The device id of the monitor"), GUILayout.Width(labelWith));
                //Set the default device id as foldoutName if it is not set by user.
                CachedConfig.Monitors[index].DeviceId =
                EditorGUILayout.TextField(CachedConfig.Monitors[index].DeviceId);
                EditorGUILayout.EndHorizontal();
                // Check if Device Id is set or not by user.
                if(string.IsNullOrEmpty(CachedConfig.Monitors[index].DeviceId))
                {
                    var warning = "Device id may not be null or empty.";
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Role", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Role =
                    (MonitorRoleType)EditorGUILayout.EnumPopup( CachedConfig.Monitors[index].Role);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Style", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Style =
                    (MonitorStyleType)EditorGUILayout.EnumPopup( CachedConfig.Monitors[index].Style);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Aspect", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Aspect =
                    (MonitorAspectType)EditorGUILayout.EnumPopup( CachedConfig.Monitors[index].Aspect);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Model Type");
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                CachedConfig.Monitors[index].Model = CachedConfig.Monitors[index].Model ?? new MonitorModelType();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manufacturer",GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Model.Manufacturer =
                    EditorGUILayout.TextField(CachedConfig.Monitors[index].Model.Manufacturer);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Model", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Model.Model =
                    EditorGUILayout.TextField(CachedConfig.Monitors[index].Model.Model);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Version", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].Model.Version =
                    EditorGUILayout.TextField(CachedConfig.Monitors[index].Model.Version);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Desktop Rectangle");
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel++;
                CachedConfig.Monitors[index].DesktopCoordinates = CachedConfig.Monitors[index].DesktopCoordinates ??
                                                                  new DesktopRectangleType();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("x", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].DesktopCoordinates.x =
                    EditorGUILayout.IntField( CachedConfig.Monitors[index].DesktopCoordinates.x);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("y", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].DesktopCoordinates.y =
                    EditorGUILayout.IntField( CachedConfig.Monitors[index].DesktopCoordinates.y);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("w", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].DesktopCoordinates.w =
                    EditorGUILayout.IntField( CachedConfig.Monitors[index].DesktopCoordinates.w);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("h", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].DesktopCoordinates.h =
                    EditorGUILayout.IntField( CachedConfig.Monitors[index].DesktopCoordinates.h);
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Virtual X", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].VirtualX =
                    EditorGUILayout.FloatField( CachedConfig.Monitors[index].VirtualX);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Virtual Y", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].VirtualY =
                    EditorGUILayout.FloatField( CachedConfig.Monitors[index].VirtualY);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("ColorProfile Id", GUILayout.Width(labelWith));
                CachedConfig.Monitors[index].ColorProfileId =
                    EditorGUILayout.IntField( CachedConfig.Monitors[index].ColorProfileId);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            return false;
        }
    }
}