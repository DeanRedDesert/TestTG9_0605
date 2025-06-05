//-----------------------------------------------------------------------
// <copyright file = "SetProgressiveSimulatorSetup.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores progressive simulator settings.
    /// </summary>
    internal class SetProgressiveSimulatorSetup
    {
        /// <summary>
        /// Foundation Owned Settings, also used in <see cref="SystemConfigFileHelper"/>.
        /// <returns> null if the <see cref="SetProgressiveSimulatorSetup"/> is not supported
        /// else returns the data in <see cref="SetProgressiveSimulatorSetup"/>. </returns>
        /// </summary>
        public SystemProgressiveSimulator Data => simulationEnabled ? EditorData : null;

        /// <summary>
        /// Foundation owned settings used to write to the system config editor file
        /// in <see cref="SystemConfigFileHelper"/>.
        /// <returns>Data in <see cref="SetProgressiveSimulatorSetup"/>.</returns>
        /// </summary>
        public SystemProgressiveSimulator EditorData { get; private set; }

        private bool showSimulator;
        private bool simulationEnabled;
        private float creditContrib;
        private int updateFrequency;

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public SetProgressiveSimulatorSetup()
        {
            EditorData = new SystemProgressiveSimulator
            {
                Enabled = false,
                EnabledSpecified = true,
                Credits = 1,
                CreditsSpecified = true,
                ContributionFrequency = 1,
                ContributionFrequencySpecified = true,
            };
        }
        /// <summary>
        /// Sets the supported flag to true if <see cref="SetProgressiveSimulatorSetup"/> exists in SystemConfig.xml
        /// </summary>
        public void SetSupportedFlag()
        {
            showSimulator = true;
        }

        /// <summary>
        /// Update and refresh simulator settings.
        /// </summary>
        /// <param name="progressiveSimulator">The <see cref="SystemProgressiveSimulator"/> instance.</param>
        public void UpdateProgressiveSimulatorSettings(SystemProgressiveSimulator progressiveSimulator)
        {
            if(progressiveSimulator != null)
            {
                EditorData = new SystemProgressiveSimulator
                {
                    Enabled = progressiveSimulator.Enabled,
                    EnabledSpecified = progressiveSimulator.EnabledSpecified,
                    Credits = progressiveSimulator.Credits,
                    CreditsSpecified = progressiveSimulator.CreditsSpecified, 
                    ContributionFrequency = progressiveSimulator.ContributionFrequency,
                    ContributionFrequencySpecified = progressiveSimulator.ContributionFrequencySpecified,
                };
            }
        }

        /// <summary>
        /// Displays the Progressive Simulator values.
        /// </summary>
        public void DisplayProgressiveSimulatorSetup()
        {
            showSimulator = EditorGUILayout.Foldout(showSimulator, "Progressive Simulator Settings");
            if(showSimulator)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                EditorGUILayout.Separator();
                EditorData.Enabled = EditorGUILayout.BeginToggleGroup("Progressive Simulator Enabled", EditorData.Enabled);
                simulationEnabled = EditorData.Enabled;
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.EndVertical();
                EditorGUILayout.Separator();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Credits", "Credit amount to contribute");
                EditorData.Credits = EditorGUILayout.IntField(SystemConfigEditor.CheckNegative(EditorData.Credits));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Contribution Frequency In Seconds", "How frequently the simulator contributes to the progressive in seconds");
                EditorData.ContributionFrequency = EditorGUILayout.IntSlider(SystemConfigEditor.CheckNegative(EditorData.ContributionFrequency), 1, 120);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
        }

        #region Private Helper Fields and Functions

        private static readonly int LabelWidth = 160;
        private static readonly int IndentWidth = 40;

        /// <summary>
        /// Display a label.
        /// </summary>
        /// <param name="text">
        /// The text of the label.
        /// </param>
        /// <param name="toolTip">
        /// The text tool tip for the label.
        /// </param>
        /// <param name="indentLevel">
        /// The indent level used to calculate the label width. This is in order to align the controls better.
        /// If a control is inside a "EditorGUI.indentLevel++" section, then use 1 for the indent level.
        /// This way the label width will be decreased so that the following controls will be aligned with
        /// controls that do not have an indent.
        /// </param>
        private static void DisplayLabel(string text, string toolTip = null, int indentLevel = 0)
        {
            EditorGUILayout.LabelField(new GUIContent(text, toolTip),
                new GUIStyle(GUI.skin.label)
                {
                    wordWrap = true,
                    alignment = TextAnchor.UpperLeft
                },
                GUILayout.Width(LabelWidth - indentLevel * IndentWidth));
        }

        #endregion
    }
}
