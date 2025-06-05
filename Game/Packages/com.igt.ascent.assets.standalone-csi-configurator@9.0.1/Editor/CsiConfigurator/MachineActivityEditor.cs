//-----------------------------------------------------------------------
// <copyright file = "MachineActivityEditor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.CsiConfig.Editor
{
    using Core.Communication.Standalone.Schemas;
    using ConfigurationEditor.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Defines the inherited class for editing the configuration settings for Machine Activity.
    /// </summary>
    internal class MachineActivityEditor : ConfigurationEditorBase<MachineActivityConfig>
    {
        /// <summary>
        /// The flag indicating if this configuration is foldout or not.
        /// </summary>
        private bool isFoldout;

        /// <inheritdoc />
        public override void UpdateConfig(string configId)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, configId);
            if(!isFoldout)
                return;

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();
            Supported = EditorGUILayout.BeginToggleGroup("Supported", Supported);
            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;

            CachedConfig.NewGame = EditorGUILayout.Toggle(
                new GUIContent("New Game", "Flag indicating if the current game is a new game"),
                CachedConfig.NewGame);

            CachedConfig.AttractInterval = (uint)EditorGUILayout.IntField(
                new GUIContent("Attract Interval", "Number of minutes to wait while in attract mode between displaying attracts"),
                (int)CachedConfig.AttractInterval < 0 ? 0 : (int)CachedConfig.AttractInterval);

            CachedConfig.InActivityDelay = (uint)EditorGUILayout.IntField(
                new GUIContent("Inactivity Delay", "Number of minutes to wait before starting the attract"),
                (int)CachedConfig.InActivityDelay < 0 ? 0 : (int)CachedConfig.InActivityDelay);

            CachedConfig.AttractsEnabled = EditorGUILayout.Toggle(
                new GUIContent("Attracts Enabled", "Flag indicating if attracts are enabled"),
                CachedConfig.AttractsEnabled);

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
