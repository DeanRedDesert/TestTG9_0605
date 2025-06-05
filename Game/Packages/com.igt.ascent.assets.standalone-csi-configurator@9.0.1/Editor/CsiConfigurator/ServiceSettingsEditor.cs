// -----------------------------------------------------------------------
// <copyright file = "ServiceSettingsEditor.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.CsiConfig.Editor
{
    using Core.Communication.Standalone.Schemas;
    using ConfigurationEditor.Editor;
    using UnityEditor;
    using UnityEngine;
    using IGT.Game.Core.Communication.Standalone.Schemas;

    /// <summary>
    /// Defines the inherited class for editing the configuration settings for Service Settings.
    /// </summary>
    internal class ServiceSettingsEditor : ConfigurationEditorBase<ServiceSettingsConfig>
    {
        /// <summary>
        /// The flag indicating if this configuration is foldout or not.
        /// </summary>
        private bool isFoldout;

        /// <summary>
        /// The size of button for the editor window.
        /// </summary>
        private const int ButtonSize = 50;
        private const int LabelSize = 160;

        public override void UpdateConfig(string configId)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, configId);
            if(!isFoldout) return;
            Supported = EditorGUILayout.BeginToggleGroup("Supported", Supported);

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Prompt Player On Cashout", "Prompt Player On Cashout"));
            CachedConfig.PromptPlayerOnCashout = EditorGUILayout.Toggle(CachedConfig.PromptPlayerOnCashout);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Emulatable Buttons");
            GUILayout.Space(50);
            if(GUILayout.Button("+", GUILayout.MaxWidth(ButtonSize)))
            {
                CachedConfig.EmulatableButtons.Add(ServiceSettingsConfigEmulatableButton.Cashout);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            for(int index = 0; index < CachedConfig.EmulatableButtons.Count; index++)
            {
                DisplayEmulatableButtonsConfiguration(index);
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndToggleGroup();
        }
        
        /// <summary>
        /// Display Each Emulatable Button with its index
        /// </summary>
        /// <param name="index">Index of the button</param>
        private void DisplayEmulatableButtonsConfiguration(int index)
        {
            var ButtonName = "Emulatable Button " + index;
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent(ButtonName), GUILayout.Width(LabelSize));
            CachedConfig.EmulatableButtons[index] = (ServiceSettingsConfigEmulatableButton)EditorGUILayout.EnumPopup(CachedConfig.EmulatableButtons[index]);
            if(GUILayout.Button("-", GUILayout.MaxWidth(ButtonSize)))
            {
                CachedConfig.EmulatableButtons.Remove(CachedConfig.EmulatableButtons[index]);
            }
            EditorGUILayout.EndHorizontal();

        }
    }
}
