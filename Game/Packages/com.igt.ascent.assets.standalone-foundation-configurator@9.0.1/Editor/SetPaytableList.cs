//-----------------------------------------------------------------------
// <copyright file = "SetPaytableList.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Communication.Foundation.Standalone.Registries;
    using Core.Communication.Standalone.Schemas;
    using IGT.Ascent.Assets.StandaloneSafeStorage.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores the list of Paytables
    /// </summary>
    internal class SetPaytableList
    {
        private readonly List<SetPaytable> setPaytables;

        /// <summary>
        /// Array to store the theme name list of the popup editor.
        /// </summary>
        private string[] themePopupArray;

        /// <summary>
        /// Active theme index of the popup editor.
        /// </summary>
        private int themePopupIndex;

        /// <summary>
        /// Array to store the paytable configuration list of the popup editor.
        /// </summary>
        private string[] paytableConfigurationPopupArray;

        /// <summary>
        /// Active paytable configuration index of the popup editor.
        /// </summary>
        private int paytableConfigurationPopupIndex;

        /// <summary>
        /// List of paytables belongs to current active theme.
        /// </summary>
        private List<SetPaytable> activeThemePaytableList;

        /// <summary>
        /// The theme foldout state selected by the user.
        /// </summary>
        private bool themeFoldout = true;

        /// <summary>
        /// The selected state of auto clear safe storage toggle.
        /// </summary>
        private bool autoClearSafeStorage = true;

        /// <summary>
        /// The tooltip content of auto clear safe storage toggle.
        /// </summary>
        private const string AutoClearSafeStorageTooltip =
            "Theme changing takes effect until the current safe storage is cleared,  use Standalone Safe Storage Menu to store/reload it.";

        /// <summary>
        /// The paytable configuration foldout state selected by the user.
        /// </summary>
        private bool paytableFoldout;

        /// <summary>
        /// List of paytable configurations used in <see cref="SystemConfigFileHelper"/> and in <see cref="SetProgressiveSetup"/>
        /// </summary>
        public List<PaytableListPaytableConfiguration> Data
        {
            get { return setPaytables.Select(value => value.Paytable).ToList(); }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public SetPaytableList()
        {
            setPaytables = new List<SetPaytable>();
        }

        /// <summary>
        /// Sets the list from the XML file
        /// </summary>
        /// <param name="paytableList">paytable list from the XML</param>
        public void UpdatePaytableList(List<PaytableListPaytableConfiguration> paytableList)
        {
            setPaytables.Clear();
            if(paytableList != null)
            {
                foreach(var paytable in paytableList)
                {
                    setPaytables.Add(new SetPaytable(paytable));
                }
                GenerateActiveThemePopup();
            }
        }

        /// <summary>
        /// Foldout for the Paytable list.
        /// </summary>
        public void DisplayPaytable()
        {
            if(themePopupArray == null || paytableConfigurationPopupArray == null)
            {
                GenerateActiveThemePopup();
            }
            themeFoldout = EditorGUILayout.Foldout(themeFoldout, "Theme and Paytable Configurations");
            if(themeFoldout)
            {
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;
                // Disable Gui changed caused by scroll position and foldout.
                GUI.changed = false;
                // Display active theme and paytable configuration.
                themePopupIndex = EditorGUILayout.Popup("Active Theme", themePopupIndex, themePopupArray);
                EditorGUILayout.Space();
                if(GUI.changed)
                {
                    GenerateActivePaytablePopup(themePopupArray[themePopupIndex]);
                }
                paytableConfigurationPopupIndex = EditorGUILayout.Popup("Active Paytable",
                    paytableConfigurationPopupIndex, paytableConfigurationPopupArray);

                // Check if nothing is showing in the dropdown and prompt the user to create a paytable configuration.
                if(paytableConfigurationPopupArray == null || paytableConfigurationPopupArray.Length == 0)
                {
                    var error = "No theme or paytable available. Create a paytable configuration.";
                    // Check for non-null items here because if a paytable configuration is created but doesn't have 
                    // a payvar associated yet the list will be non-empty but contain only null items.
                    if(themePopupArray != null && themePopupArray.Any(theme => theme != null))
                    {
                        error = "Select a theme to populate the Active Paytables list.";
                    }
                    EditorGUILayout.HelpBox(error, MessageType.Info);
                }
                EditorGUILayout.Space();
                if(GUI.changed)
                {
                    foreach(var setPaytable in setPaytables.Where(setPaytable => setPaytable.Paytable.IsDefault))
                    {
                        setPaytable.Paytable.IsDefault = false;
                    }
                    activeThemePaytableList[paytableConfigurationPopupIndex].Paytable.IsDefault = true;
                    if(autoClearSafeStorage && SafeStorageMenu.ValidateClear())
                    {
                        SafeStorageMenu.Clear();
                    }
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Auto Clear Safe Storage", AutoClearSafeStorageTooltip),GUILayout.Width(345));
                autoClearSafeStorage = EditorGUILayout.Toggle(autoClearSafeStorage);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                // Display the paytable configurations and get user input.
                paytableFoldout = EditorGUILayout.Foldout(paytableFoldout, "Paytable Configurations");
                if(paytableFoldout)
                {
                    if (GUILayout.Button("+", GUILayout.MaxWidth(SystemConfigEditor.ButtonSize)))
                    {
                        setPaytables.Add(new SetPaytable());
                        GenerateActiveThemePopup();
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (paytableFoldout)
                {
                    EditorGUILayout.BeginHorizontal(
                        GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                    EditorGUILayout.BeginVertical();
                    // Adds a new paytable configuration to the list.
                    EditorGUI.indentLevel++;
                    EditorGUI.indentLevel++;
                    for(var index = 0; index < setPaytables.Count; index++)
                    {
                        EditorGUILayout.Separator();
                        if(setPaytables[index].DisplayPaytable(index))
                        {
                            setPaytables.RemoveAt(index);
                            GenerateActiveThemePopup();
                            // Break statement is to make sure that Unity does not give ArgumentOutOfRange exception if
                            // the last paytable is deleted while in the editor mode.
                            break;
                        }
                    }
                    EditorGUI.indentLevel--;
                    EditorGUI.indentLevel--;
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }

            // Sync up paytable configurations around with user input.
            SyncUpPaytableConfigurations();
        }

        public void SyncUpPaytableConfigurations()
        {
            for(var index = 0; index < setPaytables.Count; index++)
            {
                // Refresh the paytable configuration item.
                if(setPaytables[index].RefreshPaytableConfiguration())
                {
                    GenerateActiveThemePopup();
                }

                // Keep the max bet of current paytable configuration syncing up with
                // the rest of paytable configurations.
                SyncUpMaxBet(index);

                // Keep the button panel min bet of current paytable configuration syncing up with
                // the rest of paytable configurations.
                SyncUpButtonPanelMinBet(index);
            }
        }

        /// <summary>
        /// Function to check for errors before writing to the XML file
        /// </summary>
        /// <returns>The error message.</returns>
        public string CheckErrors()
        {
            var errors = "";

            // Notify the user that there are empty paytable configurations.
            for(var index = setPaytables.Count - 1; index >= 0; index--)
            {
                if(setPaytables[index].Paytable.ThemeIdentifier == null && setPaytables[index].Paytable.PaytableName == null)
                {
                    errors += "There are empty paytable configurations.";
                    break;
                }
            }

            // Check if all denominations are valid and no duplication.
            for(var index = 0; index < setPaytables.Count; index++)
            {
                var current = setPaytables[index];
                if(current.Paytable.Denomination == 0 && current.PayvarRegistry != null)
                {
                    errors += $"\nDenom in configuration {index + 1} should not be 0.";
                }
                for(var i = index + 1; i < setPaytables.Count; i++)
                {
                    if((current.Paytable.Denomination == setPaytables[i].Paytable.Denomination) &&
                       (current.Paytable.ThemeIdentifier == setPaytables[i].Paytable.ThemeIdentifier))
                    {
                        errors += $"\nDenom {current.Paytable.Denomination} in configuration {i + 1} is duplicated with configuration {index + 1}.";
                    }
                }
            }

            if(setPaytables.Count == 0)
            {
                errors += "\nPaytable List cannot be empty. Add a valid Paytable Configuration.";
            }

            if(setPaytables.Count > 0)
            {
                var payvarType = setPaytables[0].Paytable.PayvarType;
                var query = setPaytables.Where(paytable => paytable.Paytable.PayvarType != payvarType &&
                                                           paytable.PayvarRegistry != null);
                if(query.Any())
                {
                    errors += "\nThe paytables configured have different payvar types. The game sub-mode " +
                              "will be determined based on the default paytable selected.";
                }
            }

            return errors;
        }

        /// <summary>
        /// Set active theme array for its popup widget.
        /// </summary>
        private void GenerateActiveThemePopup()
        {
            themePopupArray = setPaytables.Select(setPaytable => setPaytable.Paytable.ThemeIdentifier).Distinct().ToArray();
            var activePaytable = setPaytables.FirstOrDefault(setPaytable => setPaytable.Paytable.IsDefault && (setPaytable.Paytable.ThemeIdentifier != null));
            if(activePaytable != null)
            {
                var activeThemeIdentifier = activePaytable.Paytable.ThemeIdentifier;
                themePopupIndex = Array.IndexOf(themePopupArray, activeThemeIdentifier);
                GenerateActivePaytablePopup(activeThemeIdentifier);
            }
            else
            {
                themePopupIndex = -1;
                paytableConfigurationPopupArray = new string[0];
                paytableConfigurationPopupIndex = -1;
            }
        }

        /// <summary>
        /// Set active paytable array for its popup widget.
        /// </summary>
        /// <param name="activeThemeIdentifier">Active Theme Identifier.</param>
        /// <exception cref="ArgumentNullException">Parameters may not be null.</exception>
        private void GenerateActivePaytablePopup(string activeThemeIdentifier)
        {
            if(activeThemeIdentifier == null)
            {
                throw new ArgumentNullException(nameof(activeThemeIdentifier));
            }
            activeThemePaytableList =
                setPaytables.Where(setPaytable => setPaytable.Paytable.ThemeIdentifier == activeThemeIdentifier).ToList();
            var activePaytablesIndex =
                setPaytables.Select(
                    (setPaytable, index) =>
                        setPaytable.Paytable.ThemeIdentifier == activeThemeIdentifier ? index : -1)
                    .Where(index => index != -1)
                    .ToArray();
            paytableConfigurationPopupArray = new string[activePaytablesIndex.Length];
            for(var index = 0; index < activePaytablesIndex.Length; index++)
            {
                paytableConfigurationPopupArray[index] = "Paytable Configuration " + (activePaytablesIndex[index] + 1);
            }
            paytableConfigurationPopupIndex =
                activeThemePaytableList.FindIndex(setPaytable => setPaytable.Paytable.IsDefault);
            if(paytableConfigurationPopupIndex == -1)
            {
                paytableConfigurationPopupIndex = 0;
            }
        }

        /// <summary>
        /// Keep the max bet of current paytable configuration syncing up with the rest
        /// of paytable configurations.
        /// </summary>
        /// <param name="index">Index of paytable configuration.</param>
        /// <exception cref="NullReferenceException">
        /// Thrown if max bet value is null at sync up. In such case, there is no default value defined
        /// in game registry file.
        /// </exception>
        private void SyncUpMaxBet(int index)
        {
            var current = setPaytables[index];

            // Sync up max bet specified flag.
            current.Paytable.MaxBetSpecified = (current.MaxBetValuePool != null);

            // Simply skip the paytable configuration item in case of that theme registry is null or
            // max bet is not supported by game registry.
            if(current.ThemeRegistry == null || !current.Paytable.MaxBetSpecified)
            {
                return;
            }

            // For newly added paytable configuration or newly changed paytable registry configuration,
            // initialize its max bet as per the corresponding game registry.
            if(current.MaxBetValue == null || current.MaxBetValue == 0)
            {
                var target = setPaytables.Where((p, i) => i != index && (
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme &&
                     current.Paytable.ThemeIdentifier == p.Paytable.ThemeIdentifier) ||
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar &&
                     current.Paytable.PaytableName == p.Paytable.PaytableName) ||
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                     current.Paytable.PaytableName == p.Paytable.PaytableName &&
                     current.Paytable.Denomination == p.Paytable.Denomination)
                    ) && p.MaxBetValue != null && p.MaxBetValue != 0).FirstOrDefault();

                // If found a matched configuration, copy its value.
                if(target != null)
                {
                    current.MaxBetValue = target.MaxBetValue;
                }
                // Otherwise, use the default value defined in game registry.
                else
                {
                    if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme)
                    {
                        current.MaxBetValue = (int)current.ThemeRegistry.GetMaxBet();
                    }
                    else if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar)
                    {
                        current.MaxBetValue = (int)current.PayvarRegistry.GetMaxBet();
                    }
                    else if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                            current.Paytable.Denomination != 0)
                    {
                        current.MaxBetValue = (int)current.PayvarRegistry.GetMaxBetPerDenomination(current.Paytable.Denomination);
                    }
                }

                if(current.MaxBetValue != null)
                {
                    current.Paytable.MaxBet = (ulong)current.MaxBetValue;
                }
                else if(current.Paytable.Denomination != 0)
                {
                    throw new NullReferenceException(
                        $@"Max bet value should not be null for configuration {index + 1},
                                    no default value is found in its game registry.");
                }

            }
            // Sync up the modification of current max bet around to other paytable configurations.
            else if(current.MaxBetValue != (int)current.Paytable.MaxBet)
            {
                current.Paytable.MaxBet = (ulong)current.MaxBetValue;

                foreach(var p in setPaytables.Where(
                    (p, i) => i != index && (
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme &&
                         p.Paytable.ThemeIdentifier == current.Paytable.ThemeIdentifier) ||
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar &&
                         p.Paytable.PaytableName == current.Paytable.PaytableName) ||
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                         p.Paytable.PaytableName == current.Paytable.PaytableName &&
                         p.Paytable.Denomination == current.Paytable.Denomination))
                    ).ToList())
                {
                    p.Paytable.MaxBet = current.Paytable.MaxBet;
                    p.MaxBetValue = (int)current.Paytable.MaxBet;
                }
            }
        }

        /// <summary>
        /// Keep the button panel min bet of current paytable configuration syncing up with the rest
        /// of paytable configurations.
        /// </summary>
        /// <param name="index">Index of paytable configuration.</param>
        /// <exception cref="NullReferenceException">
        /// Thrown if button panel min bet value is null at sync up. In such case, there is no default value defined
        /// in game registry file.
        /// </exception>
        private void SyncUpButtonPanelMinBet(int index)
        {
            var current = setPaytables[index];

            // Sync up button panel min bet specified flag.
            current.Paytable.ButtonPanelMinBetSpecified = (current.ButtonPanelMinBetValuePool != null);

            // Simply skip the paytable configuration item in case of that theme registry is null or
            // button panel min bet is not supported by game registry.
            if(current.ThemeRegistry == null || !current.Paytable.ButtonPanelMinBetSpecified)
            {
                return;
            }

            // For newly added paytable configuration, initialize its button panel min bet as per the
            // corresponding game registry.
            if(current.ButtonPanelMinBetValue == null)
            {
                var target = setPaytables.Where((p, i) => i != index && (
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme &&
                     current.Paytable.ThemeIdentifier == p.Paytable.ThemeIdentifier) ||
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar &&
                     current.Paytable.PaytableName == p.Paytable.PaytableName) ||
                    (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                     current.Paytable.PaytableName == p.Paytable.PaytableName &&
                     current.Paytable.Denomination == p.Paytable.Denomination)
                    ) && p.ButtonPanelMinBetValue != null && p.ButtonPanelMinBetValue != 0).FirstOrDefault();

                // If found a matched configuration, copy its value.
                if(target != null)
                {
                    current.ButtonPanelMinBetValue = target.ButtonPanelMinBetValue;
                }
                // Otherwise, use the default value defined in game registry.
                else
                {
                    if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme)
                    {
                        current.ButtonPanelMinBetValue = (int)current.ThemeRegistry.GetButtonPanelMinBet();
                    }
                    else if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar)
                    {
                        current.ButtonPanelMinBetValue = (int)current.PayvarRegistry.GetButtonPanelMinBet();
                    }
                    else if(current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                            current.Paytable.Denomination != 0)
                    {
                        current.ButtonPanelMinBetValue = (int)current.PayvarRegistry.GetButtonPanelMinBetPerDenomination(current.Paytable.Denomination);
                    }
                }

                if(current.ButtonPanelMinBetValue != null)
                {
                    current.Paytable.ButtonPanelMinBet = (ulong)current.ButtonPanelMinBetValue;
                }
                else if(current.Paytable.Denomination != 0)
                {
                    throw new NullReferenceException(
                        $@"Button panel min bet value should not be null for configuration {index + 1},
                                    no default value is found in its game registry.");
                }
            }
            // Sync up the modification of current button panel min bet around to other paytable configurations.
            else if(current.ButtonPanelMinBetValue != (int)current.Paytable.ButtonPanelMinBet)
            {
                current.Paytable.ButtonPanelMinBet = (ulong)current.ButtonPanelMinBetValue;

                foreach(var p in setPaytables.Where(
                    (p, i) => i != index && (
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme &&
                         p.Paytable.ThemeIdentifier == current.Paytable.ThemeIdentifier) ||
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvar &&
                         p.Paytable.PaytableName == current.Paytable.PaytableName) ||
                        (current.ThemeRegistry.MaxBetResolution == MaxBetResolution.PerPayvarDenomination &&
                         p.Paytable.PaytableName == current.Paytable.PaytableName &&
                         p.Paytable.Denomination == current.Paytable.Denomination))
                    ).ToList())
                {
                    p.Paytable.ButtonPanelMinBet = current.Paytable.ButtonPanelMinBet;
                    p.ButtonPanelMinBetValue = (int)current.Paytable.ButtonPanelMinBet;
                }
            }
        }

        /// <summary>
        /// Returns the setPaytable of the given index.
        /// </summary>
        /// <param name="index">The index of the setPaytable in the list.</param>
        /// <returns>Returns the setPaytable of the given index.</returns>
        /// <exception cref="NullReferenceException">Thrown if the <see cref="setPaytables"/> has not been set.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when index is out of range.</exception>
        public SetPaytable GetSetPaytable(int index)
        {
            if(setPaytables == null)
            {
                throw new NullReferenceException("The setPaytables list is null.");
            }
            if(index > setPaytables.Count || index < 0)
            {
                throw new ArgumentOutOfRangeException($"Index:{index} is invalid. Please make sure index is between 0 and {setPaytables.Count - 1}");
            }

            return setPaytables[index];
        }

        /// <summary>
        /// The number of <see cref="setPaytables"/> in the list.
        /// </summary>
        /// <returns>The number of setPaytables in the list.</returns>
        public int CountSetPaytableList()
        {
            return setPaytables.Count;
        }

        /// <summary>
        /// Gets the first paytable type.
        /// </summary>
        /// <returns>
        /// The paytable type of the first paytable.
        /// </returns>
        public PayvarType GetFirstPaytableType()
        {
            var payvarType = PayvarType.Standard;

            if(setPaytables.Count > 0)
            {
                var firstPaytable = setPaytables.First();

                if(firstPaytable.PayvarRegistry != null)
                {
                    payvarType = firstPaytable.PayvarRegistry.PayvarRegistryType;
                }
            }

            return payvarType;
        }
    }
}
