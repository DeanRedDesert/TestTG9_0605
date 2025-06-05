//-----------------------------------------------------------------------
// <copyright file = "SetProgressiveController.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores individual Progressive Controller.
    /// </summary>
    internal class SetProgressiveController
    {
        /// <summary>
        /// Individual Progressive Controller.
        /// </summary>
        public SystemControlledProgressivesProgressiveController Data { get; private set; }

        private bool progressiveFoldout;
        private readonly List<bool> controllerLevelFoldouts = new List<bool>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public SetProgressiveController()
        {
            Data = new SystemControlledProgressivesProgressiveController
            {
                Name = "",
                ControllerLevel =
                    new List<SystemControlledProgressivesProgressiveControllerControllerLevel>()
            };
        }

        /// <summary>
        /// Parameterized constructor used to update the ProgressiveController from an existing SystemConfig file.
        /// </summary>
        /// <param name="progressiveController">Existing Progressive controller in the XML file.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="progressiveController"/> is null.
        /// </exception>
        public SetProgressiveController(SystemControlledProgressivesProgressiveController progressiveController)
        {
            Data = progressiveController ?? throw new ArgumentNullException("progressiveController");

            controllerLevelFoldouts.Clear();
            for(var index = 0; index < progressiveController.ControllerLevel.Count; index++)
            {
                controllerLevelFoldouts.Add(false);
            }
        }

        /// <summary>
        /// Displays the values of the Progressive Controller.
        /// </summary>
        /// <param name="index">The index of the Progressive Controller.</param>
        /// <returns>True if the progressive controller is deleted.</returns>
        public bool DisplayController(int index)
        {
            EditorGUILayout.BeginHorizontal();
            var foldOutName = "Progressive Controller: " + (index + 1);
            progressiveFoldout = EditorGUILayout.Foldout(progressiveFoldout,
                new GUIContent(foldOutName, "Name of the Progressive Controller"));

            if(Data != null)
            {
                Data.Name = EditorGUILayout.TextField(Data.Name, GUILayout.MaxWidth(SystemConfigEditor.ButtonSize + 80));
            }

            if(GUILayout.Button("-", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
            {
                return true;
            }
            EditorGUILayout.EndHorizontal();
            if(progressiveFoldout && Data != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller Levels");
                if(GUILayout.Button("+", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                {
                    Data.ControllerLevel.Add(new SystemControlledProgressivesProgressiveControllerControllerLevel
                                             {
                                                 Id = 0,
                                                 PrizeString = "",
                                                 ContributionPercentage = 0
                                             });

                    controllerLevelFoldouts.Add(false);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for(var level = 0; level < Data.ControllerLevel.Count; level++)
                {
                    if(DisplayControllerLevel(level))
                    {
                        //If the function returns true, we need to remove that controller level from the list.
                        Data.ControllerLevel.RemoveAt(index);
                        controllerLevelFoldouts.RemoveAt(index);
                        break;
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            return false;
        }

        /// <summary>
        /// Displays the Controller level.
        /// </summary>
        /// <param name="index">The index of the controller level.</param>
        /// <returns>True if the controller level is deleted.</returns>
        private bool DisplayControllerLevel(int index)
        {
            const int labelWidth = 100;

            var helpStyle = new GUIStyle(GUI.skin.label)
                            {
                                wordWrap = true,
                                alignment = TextAnchor.UpperLeft
                            };

            var amountStyle = new GUIStyle(GUI.skin.textArea)
                              {
                                  alignment = TextAnchor.MiddleRight
                              };

            if(Data.ControllerLevel != null)
            {
                if(index >= Data.ControllerLevel.Count)
                {
                    return false;
                }

                var currentControllerLevel = Data.ControllerLevel[index];
                currentControllerLevel.Id = index;

                var levelFoldoutName = "Controller Level:\t" + currentControllerLevel.Id;

                EditorGUILayout.BeginHorizontal();
                controllerLevelFoldouts[index] = EditorGUILayout.Foldout(controllerLevelFoldouts[index], levelFoldoutName);

                if(GUILayout.Button("-", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                {
                    return true;
                }

                EditorGUILayout.EndHorizontal();

                if(controllerLevelFoldouts[index])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        new GUIContent("Starting Amount",
                                       "The starting amount for this progressive level when it is reset, in base units"),
                        helpStyle,
                        GUILayout.Width(labelWidth));

                    currentControllerLevel.StartingAmount = (uint)EditorGUILayout.IntField(
                        SystemConfigEditor.CheckNegative((int)currentControllerLevel.StartingAmount),
                        amountStyle);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        new GUIContent("Maximum Amount",
                                       "The inclusive maximum amount for the configured progressive level, in base units"),
                        helpStyle,
                        GUILayout.Width(labelWidth));

                    currentControllerLevel.MaximumAmount = (uint)EditorGUILayout.IntField(
                        SystemConfigEditor.CheckNegative((int)currentControllerLevel.MaximumAmount),
                        amountStyle);

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        new GUIContent("Contribution Rate (1 = 100%)",
                                       "The percentage of the bet amount or event-based amount that is contributed to the progressive amount per game cycle." +
                                       "  1 means 100%"),
                        helpStyle,
                        GUILayout.Width(labelWidth));

                    currentControllerLevel.ContributionPercentage =
                        EditorGUILayout.Slider(currentControllerLevel.ContributionPercentage,
                                               0,
                                               1,
                                               GUILayout.MaxWidth(SystemConfigEditor.WindowSize - SystemConfigEditor.BoxSize));

                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        new GUIContent("Prize String",
                                       "The description of a prize to be awarded that is not part of the progressive amount"),
                        helpStyle,
                        GUILayout.Width(labelWidth));

                    currentControllerLevel.PrizeString = EditorGUILayout.TextField(currentControllerLevel.PrizeString);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        new GUIContent("Is Event-Based",
                                       "When a level is configured to be event-based, only contributions specifically declared by the game-client " +
                                       "are accumulated by the controller; Contributions are not accumulated based on a percentage of the bets/wagers."),
                        helpStyle,
                        GUILayout.Width(labelWidth));

                    currentControllerLevel.IsEventBased = EditorGUILayout.Toggle(currentControllerLevel.IsEventBased);
                    currentControllerLevel.IsEventBasedSpecified = currentControllerLevel.IsEventBased;
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUI.indentLevel--;
                }
            }

            return false;
        }
    }
}
