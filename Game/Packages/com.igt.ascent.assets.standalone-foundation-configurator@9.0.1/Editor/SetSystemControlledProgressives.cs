//-----------------------------------------------------------------------
// <copyright file = "SetSystemControlledProgressives.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores the System Controlled Progressives.
    /// </summary>
    internal class SetSystemControlledProgressives
    {
        /// <summary>
        /// List of Progressive Controllers.
        /// </summary>
        private readonly List<SetProgressiveController> setProgressiveControllers;

        /// <summary>
        /// List of Progressive Setups.
        /// </summary>
        private readonly List<SetProgressiveSetup> setProgressiveSetups;

        /// <summary>
        /// List of Paytables set in <see cref="SetPaytableList"/>.
        /// </summary>
        private readonly SetPaytableList setPaytableList;

        /// <summary>
        /// Flag to show if the <see cref="SystemControlledProgressives"/> is to be written into the SystemConfig file or not.
        /// </summary>
        private bool supported;

        private bool showSystemControlled;

        /// <summary>
        /// List of System Controlled Progressives used in <see cref="SystemConfigFileHelper"/>.
        /// <returns> null if the <see cref="SystemControlledProgressives"/> is not supported 
        /// else returns the data in <see cref="SystemControlledProgressives"/>. </returns>
        /// </summary>
        public SystemControlledProgressives Data => supported ? EditorData : null;

        /// <summary>
        /// List of System Controlled Progressives used to write to the system config editor file
        /// in <see cref="SystemConfigFileHelper"/>.
        /// <returns>Data in <see cref="SystemControlledProgressives"/>.</returns>
        /// </summary>
        public SystemControlledProgressives EditorData
        {
            get
            {
                var systemControlledProgressives = new SystemControlledProgressives
                {
                    ProgressiveControllers =
                        setProgressiveControllers.Select(
                            setController => setController.Data).
                            ToList(),
                    ProgressiveSetups =
                        setProgressiveSetups.Select(
                            setSetup => setSetup.Data).ToList()
                };
                return systemControlledProgressives;
            }
        }

        /// <summary>
        /// Constructor which accepts the System Controlled progressives to set its values.
        /// </summary>
        /// <param name="setPaytableList">Allows to choose from <see cref="PaytableListPaytableConfiguration"/> a value to enter.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="setPaytableList"/> is null.
        /// </exception>
        public SetSystemControlledProgressives(SetPaytableList setPaytableList)
        {
            this.setPaytableList = setPaytableList ?? throw new ArgumentNullException("setPaytableList");

            setProgressiveControllers = new List<SetProgressiveController>();
            setProgressiveSetups = new List<SetProgressiveSetup>();
        }

        /// <summary>
        /// Sets the supported flag to true if the <see cref="SystemControlledProgressives"/>
        /// exist in SystemConfig.xml.
        /// </summary>
        public void SetSupportedFlag()
        {
            supported = true;
        }

        /// <summary>
        /// Sets the values from the XML file.
        /// </summary>
        /// <param name="systemControlledProgressives">systemControlledProgressives from the XML</param>
        public void UpdateSystemControlledProgressives(SystemControlledProgressives systemControlledProgressives)
        {
            if(systemControlledProgressives != null)
            {
                setProgressiveControllers.Clear();
                setProgressiveSetups.Clear();
                foreach(var systemControlledProgressiveController in systemControlledProgressives.ProgressiveControllers
                    )
                {
                    setProgressiveControllers.Add(new SetProgressiveController(systemControlledProgressiveController));
                }
                foreach(var systemControlledProgressivesSetup in systemControlledProgressives.ProgressiveSetups)
                {
                    setProgressiveSetups.Add(new SetProgressiveSetup(systemControlledProgressivesSetup, setPaytableList,
                        setProgressiveControllers));
                }
            }
        }

        /// <summary>
        /// Displays the System Progressives having two fields: Progressive Controller and Progressive Setup.
        /// </summary>
        public void DisplaySystemControlledProgressives()
        {
            showSystemControlled = EditorGUILayout.Foldout(showSystemControlled, "System Controlled Progressives");

            if(showSystemControlled)
            {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                supported = EditorGUILayout.BeginToggleGroup("Supported", supported);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Progressive Controllers", GUILayout.MaxWidth(SystemConfigEditor.BoxSize));

                EditorGUILayout.BeginHorizontal();
                //Adding Space to align the "+" button.
                GUILayout.Space(32);
                if(GUILayout.Button("+", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                {
                    setProgressiveControllers.Add(new SetProgressiveController());
                }
                EditorGUILayout.EndHorizontal();

                for(var index = 0; index < setProgressiveControllers.Count; index++)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.Separator();
                    if(setProgressiveControllers[index].DisplayController(index))
                    {
                        //If the function returns true, we need to remove that Progressive controller from the list.
                        setProgressiveControllers.RemoveAt(index);
                        break;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Separator();

                EditorGUILayout.LabelField("Progressive Setups", GUILayout.MaxWidth(SystemConfigEditor.BoxSize));

                EditorGUILayout.BeginHorizontal();
                //Adding Space to align the "+" button.
                GUILayout.Space(32);
                if(GUILayout.Button("+", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                {
                    setProgressiveSetups.Add(new SetProgressiveSetup(setPaytableList, setProgressiveControllers));
                }
                EditorGUILayout.EndHorizontal();

                for(var index = 0; index < setProgressiveSetups.Count; index++)
                {
                    EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                    EditorGUILayout.Separator();
                    if(setProgressiveSetups[index].DisplayProgressiveSetup(index))
                    {
                        //If the function returns true, we need to remove that Progressive setup from the list.
                        setProgressiveSetups.RemoveAt(index);
                        break;
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Function to check for errors before writing to the XML file.
        /// </summary>
        /// <returns>The error message.</returns>
        public string CheckErrors()
        {
            var errorMessage = "";
            if(supported)
            {
                if(Data.ProgressiveControllers.Count == 0)
                {
                    errorMessage += "\nController list cannot be left blank";
                }
                if(Data.ProgressiveSetups.Count != 0)
                {
                    foreach(var progressiveSetup in Data.ProgressiveSetups)
                    {
                        if(progressiveSetup.PaytableConfiguration.PaytableFileName == "Null")
                        {
                            errorMessage += "\nPaytable File in Setup cannot be left blank";
                        }
                        if(progressiveSetup.ProgressiveLink.Count == 0)
                        {
                            errorMessage += "\nProgressive Links missing";
                        }
                    }
                }
            }
            return errorMessage;
        }
    }
}
