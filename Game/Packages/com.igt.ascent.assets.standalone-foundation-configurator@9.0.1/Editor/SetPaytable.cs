//-----------------------------------------------------------------------
// <copyright file = "SetPaytable.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Core.Communication.Foundation.Standalone.Registries;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores individual Paytable.
    /// </summary>
    internal class SetPaytable
    {
        /// <summary>
        /// Individual paytable.
        /// </summary>
        public PaytableListPaytableConfiguration Paytable { get; private set; }

        /// <summary>
        /// Path name where registry files are located.
        /// </summary>
        public const string RegistryPath = "Registries/";

        /// <summary>
        /// Extension name of payvar registry file.
        /// </summary>
        public const string PayvarRegistryExtension = "xpayvarreg";

        /// <summary>
        /// Extension name of theme registry file.
        /// </summary>
        public const string ThemeRegistryExtension = "xthemereg";

        /// <summary>
        /// Const string to indicate registry file name undefined.
        /// </summary>
        public const string UndefinedRegistryFileName = "..";

        /// <summary>
        /// Flag to indicate if this paytable configuration item is fold out in GUI.
        /// </summary>
        private bool paytableFoldout;

        /// <summary>
        /// Registry file name corresponding to this <see cref="Paytable"/>.
        /// </summary>
        private string registryFileName;

        /// <summary>
        /// Registry file name input by user from GUI.
        /// </summary>
        private string registryFileNameValue;

        /// <summary>
        /// Interface to theme registry file related to this paytable configuration.
        /// </summary>
        public IThemeRegistry ThemeRegistry { get; private set; }

        /// <summary>
        /// Interface to parvar registry file related to this paytable configuration.
        /// </summary>
        public IPayvarRegistry PayvarRegistry { get; private set; }

        /// <summary>
        /// List of denomination values for this paytable. Also used in <see cref="SetSystemControlledProgressives"/>.
        /// </summary>
        public List<int> DenominationValues { get; private set; }

        /// <summary>
        /// Value pool of button panel min bet for this paytable.
        /// </summary>
        public IValuePool<long> ButtonPanelMinBetValuePool { get; private set; }

        /// <summary>
        /// Value pool of max bet for this paytable.
        /// </summary>
        public IValuePool<long> MaxBetValuePool { get; private set; }

        /// <summary>
        /// Denomination value for this paytable.
        /// </summary>
        public int DenominationValue { get; private set; }

        /// <summary>
        /// Button panel min bet value for this paytable.
        /// </summary>
        public int? ButtonPanelMinBetValue { get; set; }

        /// <summary>
        /// Max bet value for this paytable.
        /// </summary>
        public int? MaxBetValue { get; set; }

        /// <summary>
        /// Notes definition for different bet resolution.
        /// </summary>
        private static readonly Dictionary<MaxBetResolution, string> BetResolutionNotes =
            new Dictionary<MaxBetResolution, string>
            {
                {
                    MaxBetResolution.PerTheme,
                    "Changing the values below will affect all paytable configurations using the same theme!"
                },
                {
                    MaxBetResolution.PerPayvar,
                    "Changing the values below will affect all paytable configurations using the same payvar!"
                },
                {
                    MaxBetResolution.PerPayvarDenomination,
                    null
                }
            };

        /// <summary>
        /// Constructor.
        /// </summary>
        public SetPaytable()
        {
            DenominationValues = new List<int>();
            Paytable = new PaytableListPaytableConfiguration();
            paytableFoldout = false;
            ButtonPanelMinBetValuePool = null;
            MaxBetValuePool = null;
            ThemeRegistry = null;
            PayvarRegistry = null;
            DenominationValue = 0;
            MaxBetValue = null;
            ButtonPanelMinBetValue = null;
            registryFileName = UndefinedRegistryFileName;
            registryFileNameValue = UndefinedRegistryFileName;
        }

        /// <summary>
        /// Parameterized constructor used to update the paytables from an existing SystemConfig file.
        /// </summary>
        /// <param name="paytable">Paytable from the SystemConfig.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="paytable"/> is null.
        /// </exception>
        public SetPaytable(PaytableListPaytableConfiguration paytable)
        {
            if(paytable == null)
            {
                throw new ArgumentNullException("paytable");
            }
            Paytable = paytable;
            if(!string.IsNullOrEmpty(Paytable.PaytableFileName))
            {
                Paytable.PaytableFileName = Paytable.PaytableFileName.Replace(@"\", "/");
            }

            DenominationValues = new List<int>
            {
                (int)Paytable.Denomination
            };

            ButtonPanelMinBetValuePool = null;
            MaxBetValuePool = null;
            ThemeRegistry = null;
            PayvarRegistry = null;
            DenominationValue = (int)paytable.Denomination;
            MaxBetValue = (int?)paytable.MaxBet;
            ButtonPanelMinBetValue = (int?)paytable.ButtonPanelMinBet;
            registryFileName = GetPayvarRegistryFileName(Paytable.PaytableIdentifier) ?? UndefinedRegistryFileName;
            registryFileName = registryFileName.Replace(@"\", "/");
            registryFileNameValue = registryFileName;
        }

        /// <summary>
        /// By going through all payvar registry files and return the matched payvar registry file name from
        /// provided the paytable name and paytable file name.
        /// </summary>
        /// <param name="paytableIdentifier">Specified paytable identifier.</param>
        /// <returns>The found payvar registry file name, or null if nothing found.</returns>
        private static string GetPayvarRegistryFileName(string paytableIdentifier)
        {
            return
                (from file in
                    Directory.GetFiles(RegistryPath, "*." + PayvarRegistryExtension, SearchOption.AllDirectories)
                    let payvar = RegistryLoader.LoadPayvarRegistry(file)
                    where payvar != null
                    where payvar.PaytableIdentifier == paytableIdentifier
                    select file).FirstOrDefault();
        }

        /// <summary>
        /// Displays individual paytables.
        /// </summary>
        /// <param name="index">The index of the Paytable configuration.</param>
        /// <returns>True if the paytable is deleted.</returns>
        public bool DisplayPaytable(int index)
        {
            var foldoutName = "Paytable Configuration " + (index + 1);

            // The foldout name followed by a "-" button.
            EditorGUILayout.BeginHorizontal();
            paytableFoldout = EditorGUILayout.Foldout(paytableFoldout, foldoutName);

            if(GUILayout.Button("-", GUILayout.Width(SystemConfigEditor.ButtonSize)))
            {
                return true;
            }
            EditorGUILayout.EndHorizontal();

            // Display the foldout of the paytable configuration.
            if(paytableFoldout && Paytable != null)
            {
                // Each foldout needs an indentation.
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                // The button to pick a registry file.
                EditorGUILayout.LabelField("Payvar Registry File", GUILayout.Width(205));

                if(GUILayout.Button(Path.GetFileName(registryFileName)))
                {
                    var folderOpened = new DirectoryInfo(RegistryPath);

                    if(!Directory.Exists(folderOpened.FullName))
                    {
                        throw new NullReferenceException("Could not open the Registries folder");
                    }

                    // Displays the "open file" dialog and returns the selected path name.
                    var path = EditorUtility.OpenFilePanel("", RegistryPath, PayvarRegistryExtension);
                    if(path.Length != 0)
                    {
                        registryFileNameValue = path.Replace(@"\", "/");
                    }
                }
                EditorGUILayout.EndHorizontal();

                // The information on the paytable.  Use labels since it is read only.
                var wrapStyle = new GUIStyle(GUI.skin.GetStyle("label")) { wordWrap = true, alignment = TextAnchor.UpperLeft};
                const int labelWidth = 125;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Theme Identifier", wrapStyle, GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(Paytable.ThemeIdentifier, wrapStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Paytable Identifier",wrapStyle, GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(Paytable.PaytableIdentifier, wrapStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Paytable Name",wrapStyle, GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(Paytable.PaytableName, wrapStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Paytable File Name",wrapStyle, GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(Paytable.PaytableFileName, wrapStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Payvar Type",wrapStyle, GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField(registryFileName == UndefinedRegistryFileName
                                               ? string.Empty
                                               : Paytable.PayvarType.ToString());
                EditorGUILayout.EndHorizontal();

                // Denomination.
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Denomination", GUILayout.Width(160));

                if(DenominationValues.Count == 1)
                {
                    EditorGUILayout.LabelField(DenominationValues[0].ToString(CultureInfo.InvariantCulture));

                    Paytable.Denomination = (uint)(DenominationValues[0]);
                }
                else
                {
                    var denominationString = Array.ConvertAll(
                        DenominationValues.ToArray(),
                        i => i.ToString(CultureInfo.InvariantCulture));

                    DenominationValue = EditorGUILayout.IntPopup(
                        (int)Paytable.Denomination,
                        denominationString,
                        DenominationValues.ToArray());

                    if(DenominationValue == 0 && DenominationValues.Count != 0)
                    {
                        DenominationValue = DenominationValues[0];
                    }

                    Paytable.Denomination = (uint)DenominationValue;
                }

                EditorGUILayout.EndHorizontal();

                // Button Panel Min bet and Max Bet configurations
                if(ButtonPanelMinBetValuePool != null || MaxBetValuePool != null)
                {
                    EditorGUILayout.Space();

                    // Bet resolution information
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Bet Resolution", GUILayout.Width(160));
                    EditorGUILayout.LabelField(ThemeRegistry.MaxBetResolution.ToString());
                    EditorGUILayout.EndHorizontal();

                    var warning = BetResolutionNotes[ThemeRegistry.MaxBetResolution];
                    if(warning != null)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    }

                    // Button Panel Min bet configuration
                    if(ButtonPanelMinBetValuePool != null && ButtonPanelMinBetValue != null)
                    {
                        ButtonPanelMinBetValue = (int?)SetBetValue(
                            "Button Panel Min Bet",
                            ButtonPanelMinBetValuePool,
                            (long)Paytable.ButtonPanelMinBet);
                    }

                    // Max bet configuration
                    if(MaxBetValuePool != null && MaxBetValue != null)
                    {
                        MaxBetValue = (int?)SetBetValue(
                            "Max Bet",
                            MaxBetValuePool,
                            (long)Paytable.MaxBet);
                    }

                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }

            return false;
        }

        /// <summary>
        /// Show a bet label and bet value pool in list or slider, and return the selection of bet value.
        /// </summary>
        /// <param name="betLabel">The specified bet label to show.</param>
        /// <param name="valuePool">The bet value pool to select from.</param>
        /// <param name="configValue">The configuration value.</param>
        /// <returns>The selected bet value.</returns>
        private static long SetBetValue(string betLabel, IValuePool<long> valuePool, long configValue)
        {
            var betValue = configValue;

            EditorGUILayout.BeginHorizontal();

            if((valuePool as ValueList<long>) != null)
            {
                var valueList = (valuePool as ValueList<long>).Values;
                var betString = Array.ConvertAll(
                    valueList.ToArray(),
                    i => i.ToString(CultureInfo.InvariantCulture));

                EditorGUILayout.LabelField(
                    new GUIContent(betLabel, "Select from the popup list"),
                    GUILayout.Width(160));
                betValue = EditorGUILayout.IntPopup(
                    (int)configValue, betString, valueList.Select(i => (int)i).ToArray());
            }
            else if((valuePool as ValueRange<long>) != null)
            {
                var valueRange = valuePool as ValueRange<long>;

                EditorGUILayout.LabelField(
                    new GUIContent(betLabel, "Select from " + valueRange.Min + " to " + valueRange.Max),
                    GUILayout.Width(160));
                betValue = EditorGUILayout.IntSlider(
                    (int)configValue, (int)valueRange.Min, (int)valueRange.Max,
                    GUILayout.MinWidth(240));
            }

            EditorGUILayout.EndHorizontal();

            return betValue;
        }

        /// <summary>
        /// Refresh this paytable configuration in need.
        /// </summary>
        /// <returns>True if the registry is updated.</returns>
        public bool RefreshPaytableConfiguration()
        {
            // Update the registry values at initial when payvar registry values are not yet loaded.
            if(PayvarRegistry == null)
            {
                return UpdateFromGameRegistry();
            }

            // Update the registry values if paytable registry file name changed.
            if(Path.GetFileName(registryFileNameValue) != Path.GetFileName(registryFileName))
            {
                // Reset MaxBetValue and ButtonPanelMinBetValue to be null when payvar registry file changed,
                // this way will force its MaxBetValue and ButtonPanelMinBetValue to be updated later.
                MaxBetValue = null;
                ButtonPanelMinBetValue = null;

                // Reset the interfaces of theme registry and payvar registry by force.
                ThemeRegistry = null;
                PayvarRegistry = null;
                MaxBetValuePool = null;
                ButtonPanelMinBetValuePool = null;

                return UpdateFromGameRegistry();
            }
            return false;
        }

        /// <summary>
        /// Update the paytable configuration data from game registry file.
        /// </summary>
        /// <exception cref="NullReferenceException">Throw if new parvar/theme registry file could not be loaded.</exception>
        /// <returns>True if the registry is updated.</returns>
        private bool UpdateFromGameRegistry()
        {
            // Check if user picks a valid payvar registry file through UI.
            if(string.IsNullOrEmpty(registryFileNameValue) || registryFileNameValue == UndefinedRegistryFileName)
            {
                return false;
            }

            // Open the payvar registry file.
            PayvarRegistry = RegistryLoader.LoadPayvarRegistry(registryFileNameValue);
            if(PayvarRegistry == null)
            {
                throw new NullReferenceException("Could not load payvar registry file.");
            }

            // Update the supported denominations from payvar.
            DenominationValues =
                PayvarRegistry.GetSupportedDenominations().Select(value => (int)value).ToList();

            // Open the theme registry file.
            ThemeRegistry = RegistryLoader.LoadThemeRegistry(PayvarRegistry.ThemeRegistryFileName);
            if(ThemeRegistry == null)
            {
                throw new NullReferenceException("Could not load theme registry file.");
            }

            // Update the paytable name, paytable filename, payvar type from payvar.
            Paytable.PaytableName = PayvarRegistry.PaytableTagName;
            Paytable.PaytableFileName = PayvarRegistry.PaytableTagFileName.Replace(@"\", "/");
            Paytable.PayvarType = (PaytablePayvarType)PayvarRegistry.PayvarRegistryType;
            // Update the theme identifier from the theme registry.
            Paytable.ThemeIdentifier = ThemeRegistry.G2SThemeId;
            // Update the paytable identifier from the payvar registry
            Paytable.PaytableIdentifier = PayvarRegistry.PaytableIdentifier;

            // Update the supported button panel min bet and max bet.
            if(ThemeRegistry.MaxBetResolution == MaxBetResolution.PerTheme)
            {
                ButtonPanelMinBetValuePool = ThemeRegistry.GetSupportedButtonPanelMinBets();
                MaxBetValuePool = ThemeRegistry.GetSupportedMaxBets();
            }
            else
            {
                ButtonPanelMinBetValuePool = PayvarRegistry.GetSupportedButtonPanelMinBets();
                MaxBetValuePool = PayvarRegistry.GetSupportedMaxBets();
            }

            // Get the payvar registry file name synced up.
            registryFileName = registryFileNameValue;
            return true;
        }
    }
}