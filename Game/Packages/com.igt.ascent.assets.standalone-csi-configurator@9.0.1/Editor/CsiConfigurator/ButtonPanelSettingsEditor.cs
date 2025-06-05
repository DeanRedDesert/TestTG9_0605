// -----------------------------------------------------------------------
//  <copyright file = "ButtonPanelSettingsEditor.cs" company = "IGT">
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
    /// Defines the inherited class for editing the configuration settings for Button Panel.
    /// </summary>
    internal class ButtonPanelSettingsEditor : ConfigurationEditorBase<ButtonPanelSettingsConfig>
    {
        /// <summary>
        /// The flag indicating if button panel settings folder is foldout or not.
        /// </summary>
        private bool isFoldout;

        /// <summary>
        /// List of flags indicating if the button panel configuration folders are foldout or not.
        /// </summary>
        private readonly List<bool> configurationFoldouts = new List<bool>();

        /// <summary>
        /// Dictionary of the Button configurations.
        /// </summary>
        private readonly Dictionary<int, List<bool>> currentButtonConfigurations = new Dictionary<int, List<bool>>();

        /// <summary>
        /// The flag indicating if the configurationFouldouts and currentButtonConfigurations is initialized or not.
        /// </summary>
        private bool isInitialized;

        /// <summary>
        /// The size of button for the editor window.
        /// </summary>
        private const int ButtonSize = 50;
        private const int LabelSize = 160;

        /// <inheritdoc />
        public override void UpdateConfig(string configId)
        {
            isFoldout = EditorGUILayout.Foldout(isFoldout, configId);
            if(!isFoldout) return;

            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();
            Supported = EditorGUILayout.BeginToggleGroup("Supported", Supported);
            EditorGUI.indentLevel++;
            if(!isInitialized)
            {
                configurationFoldouts.Clear();
                currentButtonConfigurations.Clear();
                for(var index = 0; index < CachedConfig.ButtonPanels.Count; index++)
                {
                    configurationFoldouts.Add(false);
                    currentButtonConfigurations.Add(index, new List<bool>());
                    for(var indicator = 0; indicator < CachedConfig.ButtonPanels[index].Buttons.Count; indicator++)
                    {
                        currentButtonConfigurations[index].Add(false);
                    }
                }
                isInitialized = true;
            }
            // Display the button panel configurations and get user input.
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(CsiConfigEditor.FoldoutSize));
            EditorGUILayout.BeginVertical();
            // Adds a new button panel configuration.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Button Panel Configurations");
            GUILayout.Space(50);
            if(GUILayout.Button("+", GUILayout.MaxWidth(ButtonSize)))
            {
                var buttonPanel = new ButtonPanel
                {
                    PanelLocation = new PanelLocation(),
                    PanelType = new PanelType(),
                    Buttons = new List<Button>()
                };
                CachedConfig.ButtonPanels.Add(buttonPanel);
                configurationFoldouts.Add(false);
                currentButtonConfigurations.Add(CachedConfig.ButtonPanels.Count-1, new List<bool>());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            for(var index = 0; index < CachedConfig.ButtonPanels.Count; index++)
            {
                EditorGUILayout.Separator();
                if(DisplayButtonPanelConfiguration(index))
                {
                    CachedConfig.ButtonPanels.RemoveAt(index);
                    configurationFoldouts.RemoveAt(index);
                    currentButtonConfigurations.Remove(index);
                    break;
                }
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Displays individual button panel configuration.
        /// </summary>
        /// <param name="index">The index of the button panel configuration.</param>
        /// <returns>True if the button panel configuration is deleted.</returns>
        private bool DisplayButtonPanelConfiguration(int index)
        {
            var foldoutName = "Button Panel Configuration " + (index + 1);
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
                EditorGUILayout.LabelField(new GUIContent("Panel ID"), GUILayout.Width(LabelSize));
                CachedConfig.ButtonPanels[index].PanelID =
                    (uint)EditorGUILayout.IntField((int)CachedConfig.ButtonPanels[index].PanelID < 0
                            ? 0
                            : (int)CachedConfig.ButtonPanels[index].PanelID);
                CachedConfig.ButtonPanels[index].PanelIDSpecified = true;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Panel Location", GUILayout.Width(LabelSize));
                CachedConfig.ButtonPanels[index].PanelLocation =
                    (PanelLocation)
                        EditorGUILayout.EnumPopup( CachedConfig.ButtonPanels[index].PanelLocation);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Panel Type", GUILayout.Width(LabelSize));
                CachedConfig.ButtonPanels[index].PanelType =
                    (PanelType)EditorGUILayout.EnumPopup( CachedConfig.ButtonPanels[index].PanelType);
                EditorGUILayout.EndHorizontal();
                //Display button configurations.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Buttons:");
                if(GUILayout.Button("+", GUILayout.MaxWidth(ButtonSize)))
                {
                    CachedConfig.ButtonPanels[index].Buttons.Add(new Button
                    {
                        ButtonId = new ButtonId(),
                        ButtonFunctions = new List<ButtonFunction>()
                    });
                    currentButtonConfigurations[index].Add(false);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                if(CachedConfig.ButtonPanels[index].Buttons.Count > 0)
                {
                    for(var indicator = 0; indicator < CachedConfig.ButtonPanels[index].Buttons.Count; indicator++)
                    {
                        if(DisplayButtonConfiguration(index, indicator))
                        {
                            CachedConfig.ButtonPanels[index].Buttons.RemoveAt(indicator);
                            currentButtonConfigurations[index].RemoveAt(indicator);
                            break;
                        }
                        EditorGUILayout.Separator();
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUI.indentLevel--;
            return false;
        }

        /// <summary>
        /// Displays individual button configuration.
        /// </summary>
        /// <param name="panelIndex">The index of the button panel.</param>
        /// <param name="buttonIndex">The index of the button in the specified button panel.</param>
        /// <returns>True if the button panel is deleted.</returns>
        private bool DisplayButtonConfiguration(int panelIndex, int buttonIndex)
        {
            var foldoutName = "Button " + (buttonIndex + 1);
            // The foldout name followed by a "-" button.
            EditorGUILayout.BeginHorizontal();
            currentButtonConfigurations[panelIndex][buttonIndex] =
                EditorGUILayout.Foldout(currentButtonConfigurations[panelIndex][buttonIndex], foldoutName);
            if(GUILayout.Button("-", GUILayout.Width(ButtonSize)))
            {
                return true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if(currentButtonConfigurations[panelIndex][buttonIndex])
            {
                CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonId =
                    CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonId ?? new ButtonId();
                EditorGUILayout.Separator();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Button Id", GUILayout.Width(LabelSize));
                CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonId.Value =
                    (byte)EditorGUILayout.IntField(CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonId.Value);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Button Type", GUILayout.Width(LabelSize));
                CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonType =
                    (ButtonType)
                        EditorGUILayout.EnumPopup(CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonType);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Has Dynamic Display");
                CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].HasDynamicDisplay =
                    EditorGUILayout.ToggleLeft(
                        "",
                        CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].HasDynamicDisplay);
                EditorGUILayout.EndHorizontal();
                // Display button functions.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Button Functions");
                if(GUILayout.Button("+", GUILayout.MaxWidth(ButtonSize)))
                {
                    CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonFunctions.Add(new ButtonFunction());
                }
                EditorGUILayout.EndHorizontal();
                var funcCount = CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonFunctions.Count;
                for(var index = 0; index < funcCount; index++)
                {
                    if(DisplayButtonFunction(panelIndex, buttonIndex, index))
                    {
                        CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonFunctions.RemoveAt(index);
                        break;
                    }
                }
            }
            EditorGUI.indentLevel--;
            return false;
        }

        /// <summary>
        /// Displays individual button function.
        /// </summary>
        /// <param name="panelIndex">The index of the button panel.</param>
        /// <param name="buttonIndex">The index of the button.</param>
        /// <param name="functionIndex">The index of the button function.</param>
        /// <returns>True if the button function is deleted.</returns>
        private bool DisplayButtonFunction(int panelIndex, int buttonIndex, int functionIndex)
        {
            var functionName = "Function" + (functionIndex + 1);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(functionName, GUILayout.Width(LabelSize));
            CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonFunctions[functionIndex] =
                (ButtonFunction)
                    EditorGUILayout.EnumPopup(CachedConfig.ButtonPanels[panelIndex].Buttons[buttonIndex].ButtonFunctions[functionIndex]);
            if(GUILayout.Button("-", GUILayout.Width(ButtonSize)))
            {
                return true;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            return false;
        }
    }
}