//-----------------------------------------------------------------------
// <copyright file = "SetProgressiveSetup.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores individual Progressive Setups.
    /// </summary>
    internal class SetProgressiveSetup
    {
        /// <summary>
        /// Individual Progressive Setup.
        /// </summary>
        public SystemControlledProgressivesProgressiveSetup Data { get; private set; }

        private readonly SetPaytableList setPaytableList;
        private int indexPaytable;
        private bool setupFoldout;
        private readonly List<bool> progressiveLinkFoldout = new List<bool>();
        private readonly List<SetProgressiveController> setProgressiveControllers;
        private readonly List<int> controllerNameIndexes = new List<int>();
        private readonly List<int> controllerLevels = new List<int>();
        private bool paytableFoldout;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="setPaytableList">List of paytables set in <see cref="SetPaytableList"/>.</param>
        /// <param name="setProgressiveControllers">List of Progressive controllers set in <see cref=" SetProgressiveController"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="setPaytableList"/> or <paramref name="setProgressiveControllers"/> is null.
        /// </exception>
        public SetProgressiveSetup(SetPaytableList setPaytableList,
                                   List<SetProgressiveController> setProgressiveControllers)
        {
            this.setPaytableList = setPaytableList ?? throw new ArgumentNullException(nameof(setPaytableList));
            this.setProgressiveControllers = setProgressiveControllers ?? throw new ArgumentNullException(nameof(setProgressiveControllers));

            Data = new SystemControlledProgressivesProgressiveSetup
                   {
                       PaytableConfiguration = new PaytableBinding(),
                       ProgressiveLink =
                           new List<SystemControlledProgressivesProgressiveSetupProgressiveLink>()
                   };
        }

        /// <summary>
        /// Parameterized constructor used to update the ProgressiveSetup from an existing SystemConfig file.
        /// </summary>
        /// <param name="progressiveSetup">Existing Progressive setup in the XML.</param>
        /// <param name="setPaytableList">List of paytables set in <see cref="SetPaytableList"/>.</param>
        /// <param name="setProgressiveControllers">List of Progressive controllers set in <see cref=" SetProgressiveController"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="progressiveSetup"/> or <paramref name="setPaytableList"/> or 
        /// <paramref name="setProgressiveControllers"/> is null.
        /// </exception>
        public SetProgressiveSetup(SystemControlledProgressivesProgressiveSetup progressiveSetup,
                                   SetPaytableList setPaytableList,
                                   List<SetProgressiveController> setProgressiveControllers)
        {
            if(progressiveSetup == null)
            {
                throw new ArgumentNullException(nameof(progressiveSetup));
            }

            this.setPaytableList = setPaytableList ?? throw new ArgumentNullException(nameof(setPaytableList));
            this.setProgressiveControllers = setProgressiveControllers ?? throw new ArgumentNullException(nameof(setProgressiveControllers));

            var progressiveControllerNames = setProgressiveControllers.Select(value => value.Data.Name).ToList();

            //This loop finds the paytable that is stored in the XML file for the progressive setup.
            for(var index = 0; index < setPaytableList.Data.Count; index++)
            {
                var value = setPaytableList.Data[index];
                if(progressiveSetup.PaytableConfiguration != null &&
                   progressiveSetup.PaytableConfiguration.PaytableFileName == value.PaytableFileName &&
                   progressiveSetup.PaytableConfiguration.PaytableName == value.PaytableName &&
                   progressiveSetup.PaytableConfiguration.Denomination == value.Denomination)
                {
                    indexPaytable = index;
                    break;
                }
            }

            foreach(var link in progressiveSetup.ProgressiveLink)
            {
                var indexController = progressiveControllerNames.IndexOf(link.ControllerName);
                progressiveLinkFoldout.Add(false);
                controllerNameIndexes.Add(indexController);
                controllerLevels.Add(link.ControllerLevel);
            }

            Data = progressiveSetup;
        }

        /// <summary>
        /// Displays the values for the Progressive Setup.
        /// </summary>
        /// <param name="index">The index of the Progressive Setup.</param>
        /// <returns>True if the progressive setup is deleted.</returns>
        public bool DisplayProgressiveSetup(int index)
        {
            var foldoutName = "Progressive Setup: " + (index + 1);
            EditorGUILayout.BeginHorizontal();

            setupFoldout = EditorGUILayout.Foldout(setupFoldout, foldoutName);

            if(GUILayout.Button("-", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
            {
                return true;
            }

            EditorGUILayout.EndHorizontal();

            if(setupFoldout)
            {
                //The paytable configuration associated with the Progressive setup.
                DisplayPaytableConfiguration();
                EditorGUILayout.Separator();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("Progressive Links");
                if(GUILayout.Button("+", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                {
                    Data.ProgressiveLink.Add(new SystemControlledProgressivesProgressiveSetupProgressiveLink
                                             {
                                                 ControllerLevel = 0,
                                                 ControllerName = "Null",
                                                 GameLevel = 0
                                             });

                    progressiveLinkFoldout.Add(false);
                    controllerNameIndexes.Add(0);
                    controllerLevels.Add(0);
                }

                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel++;
                for(var linkIndex = 0; linkIndex < Data.ProgressiveLink.Count; linkIndex++)
                {
                    //If the function returns true, we need to remove that Progressive link from the list.
                    if(DisplayProgressiveLink(linkIndex))
                    {
                        Data.ProgressiveLink.RemoveAt(linkIndex);
                        progressiveLinkFoldout.RemoveAt(linkIndex);
                        controllerLevels.RemoveAt(linkIndex);
                        controllerNameIndexes.RemoveAt(linkIndex);
                        break;
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }

            return false;
        }

        /// <summary>
        /// Displays the Paytable Configuration for the Progressive Setup.
        /// </summary>
        private void DisplayPaytableConfiguration()
        {
            EditorGUI.indentLevel++;

            var setPaytableListCount = setPaytableList != null ? setPaytableList.CountSetPaytableList() : 0;

            if(setPaytableListCount == 0)
            {
                EditorGUILayout.HelpBox("No paytable configuration was found.  Please configure Paytable Configurations first.",
                                        MessageType.Warning);
            }
            else
            {
                var paytableFiles = new string[setPaytableListCount];
                for(var paytableIndex = 0; paytableIndex < setPaytableListCount; paytableIndex++)
                {
                    paytableFiles[paytableIndex] = "Paytable Configuration " + (paytableIndex + 1);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select:", GUILayout.MaxWidth(120));
                indexPaytable = EditorGUILayout.Popup(indexPaytable, paytableFiles);
                EditorGUILayout.EndHorizontal();

                // If the paytable configuration is deleted while we are in this tab.
                if(indexPaytable >= setPaytableListCount)
                {
                    indexPaytable = 0;
                }

                var setPaytable = setPaytableList?.GetSetPaytable(indexPaytable);
                if(setPaytable != null && setPaytable.Paytable.ThemeIdentifier != null)
                {
                    var selectedConfiguration = setPaytable.Paytable;
                    var savedData = Data.PaytableConfiguration;

                    // Save the data
                    savedData.ThemeIdentifier = selectedConfiguration.ThemeIdentifier;
                    savedData.PaytableIdentifier = selectedConfiguration.PaytableIdentifier;
                    savedData.PaytableFileName = selectedConfiguration.PaytableFileName;
                    savedData.PaytableName = selectedConfiguration.PaytableName;
                    savedData.Denomination = selectedConfiguration.Denomination;

                    // Display the info on the paytable configuration
                    var builder = new StringBuilder()
                                  .AppendLine($"Theme Identifier:\t{savedData.ThemeIdentifier}")
                                  .AppendLine($"Paytable Name:\t{savedData.PaytableName}")
                                  .AppendLine($"Denomination:\t{savedData.Denomination}");

                    EditorGUILayout.HelpBox(builder.ToString(), MessageType.None);
                }
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Sets and displays the Progressive Link in the Progressive Setup.
        /// </summary>
        /// <param name="linkIndex">The index of the Progressive Link.</param>
        /// <returns>True if the progressive link is deleted.</returns>
        private bool DisplayProgressiveLink(int linkIndex)
        {
            const int labelWidth = 200;

            //Gets the controller names from the Progressive controller list that is added.
            var progressiveControllerNames = setProgressiveControllers != null
                                                 ? setProgressiveControllers.Select(value => value.Data.Name).ToArray()
                                                 : new string[0];

            if(linkIndex >= Data.ProgressiveLink.Count)
            {
                return false;
            }

            EditorGUILayout.BeginHorizontal();

            var foldoutName = "Progressive Link " + (linkIndex + 1);
            progressiveLinkFoldout[linkIndex] = EditorGUILayout.Foldout(progressiveLinkFoldout[linkIndex], foldoutName);
            if(GUILayout.Button("-", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
            {
                return true;
            }

            EditorGUILayout.EndHorizontal();

            if(progressiveLinkFoldout[linkIndex] && setProgressiveControllers != null && setProgressiveControllers.Count != 0)
            {
                var currentLink = Data.ProgressiveLink[linkIndex];

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Game Level", GUILayout.Width(labelWidth));
                currentLink.GameLevel =EditorGUILayout.IntField(SystemConfigEditor.CheckNegative(currentLink.GameLevel));
                EditorGUILayout.EndHorizontal();

                var controllerNameIndex = controllerNameIndexes[linkIndex];

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller Name", GUILayout.Width(labelWidth));
                controllerNameIndex = EditorGUILayout.Popup(controllerNameIndex, progressiveControllerNames);
                EditorGUILayout.EndHorizontal();

                // Validate and save the controller name.
                if(controllerNameIndex >= setProgressiveControllers.Count)
                {
                    controllerNameIndex = 0;
                }

                controllerNameIndexes[linkIndex] = controllerNameIndex;

                if(progressiveControllerNames[controllerNameIndex] != null)
                {
                    currentLink.ControllerName = progressiveControllerNames[controllerNameIndex];
                }

                // Get the list of controller levels within the selected progressive controller.
                var controllerLevelIds = setProgressiveControllers[controllerNameIndex]
                                         .Data.ControllerLevel.Select(value => value.Id)
                                         .ToArray();

                var controllerLevelStrings = Array.ConvertAll(controllerLevelIds,
                                                              i => i.ToString(CultureInfo.InvariantCulture));

                // Display the pop up for selecting controller level.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Controller Level", GUILayout.Width(labelWidth));
                controllerLevels[linkIndex] = EditorGUILayout.IntPopup(controllerLevels[linkIndex],
                                                                       controllerLevelStrings,
                                                                       controllerLevelIds);
                EditorGUILayout.EndHorizontal();

                currentLink.ControllerLevel = controllerLevels[linkIndex];

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            return false;
        }
    }
}