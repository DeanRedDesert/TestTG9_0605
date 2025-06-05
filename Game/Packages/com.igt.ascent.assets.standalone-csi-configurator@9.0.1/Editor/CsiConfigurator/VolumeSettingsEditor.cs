// -----------------------------------------------------------------------
//  <copyright file = "VolumeSettingsEditor.cs" company = "IGT">
//      Copyright (c) 2016 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.CsiConfig.Editor
{
    using Core.Communication.Standalone.Schemas;
    using ConfigurationEditor.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Defines the inherited class for editing the configuration settings for Volume Settings.
    /// </summary>
    internal class VolumeSettingsEditor : ConfigurationEditorBase<VolumeSettingsConfig>
    {
        /// <summary>
        /// The flag indicating if this configuration is foldout or not.
        /// </summary>
        private bool isFoldout;

        /// <inheritdoc />
        public override void UpdateConfig(string configId)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, configId);
            if(!isFoldout) return;

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();

            Supported = EditorGUILayout.BeginToggleGroup("Supported", Supported);

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Sound Volume Player Selectable",
                "Allow Player to Select the Volume"));
            CachedConfig.VolumePlayerSelectable = EditorGUILayout.Toggle(CachedConfig.VolumePlayerSelectable);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!CachedConfig.VolumePlayerSelectable);
            EditorGUILayout.LabelField(new GUIContent("Volume Player Mute Selectable",
                "Allow Player to Mute the Volume"));
            CachedConfig.VolumePlayerMuteSelectable = EditorGUILayout.Toggle(CachedConfig.VolumePlayerMuteSelectable &&
                                                                             CachedConfig.VolumePlayerSelectable);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Mute All",
                "Mute All Sounds Except Alarms, Bells and Handpays"));
            CachedConfig.MuteAll = EditorGUILayout.Toggle(CachedConfig.MuteAll);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
