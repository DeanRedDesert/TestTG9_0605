//-----------------------------------------------------------------------
// <copyright file = "SetFoundationOwnedSettings.cs" company = "IGT">
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
    /// Class which stores the Foundation Owned Settings.
    /// </summary>
    internal class SetFoundationOwnedSettings
    {
        /// <summary>
        /// Foundation Owned Settings, also used in <see cref="SystemConfigFileHelper"/>.
        /// <returns> null if the <see cref="FoundationOwnedSettings"/> is not supported
        /// else returns the data in <see cref="FoundationOwnedSettings"/>. </returns>
        /// </summary>
        public FoundationOwnedSettings Data => supported ? EditorData : null;

        /// <summary>
        /// Foundation owned settings used to write to the system config editor file
        /// in <see cref="SystemConfigFileHelper"/>.
        /// <returns>Data in <see cref="FoundationOwnedSettings"/>.</returns>
        /// </summary>
        public FoundationOwnedSettings EditorData { get; private set; }

        /// <summary>
        /// Flag to show if the <see cref="FoundationOwnedSettings"/> is to be written into the SystemConfig file or not.
        /// </summary>
        private bool supported;

        private bool showFoundation;
        private bool showCredit;
        private bool showWinCap;
        private bool showAncillary;
        private bool showMarketingBehavior;
        private bool showBonusSoaaSettings;
        private readonly string[] environmentArray;
        private int environmentMask;
        private int indexDecimal;
        private int indexCurrency;
        private int indexDigit;

        private readonly Dictionary<string, string> separators = new Dictionary<string, string>
        {
            {"NONE", ""},
            {"PERIOD", "."},
            {"COMMA", ","},
            {"SPACE", " "},
            {"COMMA SPACE", ", "},
            {"APOSTROPHE", "'"}
        };

        private readonly string[] digitGroupSeparatorList =
        {
            "NONE", "COMMA", "PERIOD", "SPACE", "COMMA SPACE",
            "APOSTROPHE"
        };

        private readonly string[] decimalGroupSeparatorList = {"PERIOD", "COMMA", "SPACE", "COMMA SPACE", "APOSTROPHE"};

        // Taken from LocaleManagerCore/inc and default.config for all MonetaryTypes. Many of these are not
        // used in practice, but these are the ones that exist as a possibility in the Foundation.
        private readonly Dictionary<string, string> currencyList = new Dictionary<string, string>
        {
            {@"$", "¢"}, // US, Argentina, Canada, Colombia, Mexico, Namibia: $
            {@"€", ""}, // Euro, Malta, Slovenia: €
            {@"Lek", ""}, // Albania: Lek
            {@"դր.", ""}, // Armenia: դր.
            {@"A$", "¢"}, // Australia: A$
            {@"p.", ""}, // Belarus: p.
            {@"Bs.", ""}, // Bolivia: Bs.
            {@"P", ""}, // Botswana: P
            {@"R$", ""}, // Brazil: R$
            {@"лв", ""}, // Bulgaria: лв
            {@"Ps", ""}, // Chile: Ps
            {@"HRK", ""}, // Croatia: HRK
            {@"Kč", ""}, // Czech Republic: Kč
            {@"FC", ""}, // Congo: FC
            {@"kr", ""}, // Denmark, Estonia, Iceland, Norway, Sweden: kr
            {@"RD$", ""}, // Dominican Republic: RD$
            {@"E£", ""}, // Egypt: E£
            {@"£", "p"}, // Gibraltar, Isle of Man, Northern Ireland, Scotland, UK: £
            {@"Q", ""}, // Guatemala: Q
            {@"L", ""}, // Honduras: L
            {@"HK$", ""}, // Hong Kong: HK$
            {@"Ft", ""}, // Hungary: Ft
            {@"₹", ""}, // India: ₹
            {@"¥", ""}, // Japan: ¥
            {@"₸", ""}, // Kazakhstan: ₸
            {@"KSh", ""}, // Kenya: KSh
            {@"Ls", ""}, // Latvia: Ls
            {@"Lt", ""}, // Lithuania: Lt
            {@"MOP", ""}, // Macau: MOP
            {@"MK", ""}, // Malawi: MK
            {@"RM", ""}, // Malaysia: RM
            {@"₨", ""}, // Mauritius: ₨
            {@"DH", ""}, // Morocco: DH
            {@"ƒ", ""}, // NetherlandsAntilles: ƒ
            {@"NZ$", ""}, // New Zealand: NZ$
            {@"₩", ""}, // North Korea, South Korea: ₩
            {@"S/.", ""}, // Peru: S/.
            {@"₱", ""}, // Philippines: ₱
            {@"zł", ""}, // Poland: zł
            {@"Lei", ""}, // Romania: Lei
            {@"руб", ""}, // Russia: руб
            {@"Fr", ""}, // Senegal, Switzerland: Fr
            {@"дин", ""}, // Serbia: дин
            {@"SR", ""}, // Seychelles: SR
            {@"S$", ""}, // Singapore: S$
            {@"R", ""}, // South Africa: R
            {@"NT$", ""}, // Taiwan: NT$
            {@"x/y", ""}, // Tanzania: x/y
            {@"฿", ""}, // Thailand: ฿
            {@"TL", ""}, // Turkey: TL
            {@"USh", ""}, // Uganda: USh
            {@"ГрН", ""}, // Ukraine: ГрН
            {@"N$", ""}, // Uruguay: N$
            {@"Bs", ""}, // Venezuela: Bs
            {@"ZK", ""}, // Zambia: ZK
            {@"Z$", ""}, // Zimbabwe: Z$
            {@"¢", ""}, // default.config: ¢
            {@"p", ""}, // default.config: p
            {@"FF", ""}, // default.config: FF
            {@"fE", ""}, // default.config: fE
            {@"K", ""}, // default.config: K
            {@"Z", ""}, // default.config: Z
            {@"E", ""}, // default.config: E
            {@"F", ""}, // default.config: F
            {@"Br", ""}, // default.config: Br
            {@"Kr", ""}, // default.config: Kr
            {@"LE", ""}, // default.config: LE
            {@"", ""}, // BLANK
        };

        private const string DefaultJurisdiction = "USDM";

        /// <summary>
        /// Some currencies use forward slashes in them, because Unity GUI will create submenu when
        /// forward slashes are used, replace forward slash with U+2215 (divisor slash) so it looks similar.
        /// Make sure to use forward slash when setting the actual currency though.
        /// </summary>
        private string[] currencyKeys;

        private string[] CurrencyKeys
        {
            get
            {
                return currencyKeys ?? (currencyKeys = currencyList.Keys.Select(key =>
                {
                    key = key.Replace(@"/", "\u2215");
                    return key;
                }).ToArray());
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public SetFoundationOwnedSettings()
        {
            EditorData = new FoundationOwnedSettings
                         {
                             CreditFormatter = new FoundationOwnedSettingsCreditFormatter
                                               {
                                                   CurrencyCentSymbol = string.Empty,
                                                   CurrencySymbol = currencyList.FirstOrDefault().Key,
                                                   DecimalSeparator = decimalGroupSeparatorList[0],
                                                   DigitGroupSeparator = digitGroupSeparatorList[0],
                                               },
                             AncillarySetting = new FoundationOwnedSettingsAncillarySetting(),
                             EnvironmentAttribute = new List<EnvironmentAttributeString>(),
                             MarketingBehavior = new MarketingBehaviorType(),
                             Jurisdiction = DefaultJurisdiction
                         };

            if(EditorData.MaxHistorySteps == 0)
            {
                // Default value of MaxHistorySteps = 50
                EditorData.MaxHistorySteps = 50;
            }

            environmentArray = Enum.GetNames(typeof(EnvironmentAttributeString));
        }

        /// <summary>
        /// Sets the supported flag to true if the <see cref="FoundationOwnedSettings"/>
        /// exist in SystemConfig.xml.
        /// </summary>
        public void SetSupportedFlag()
        {
            supported = true;
        }

        /// <summary>
        /// Sets the values for Foundation Owned Settings from the XML file.
        /// This is used to load the data into the Editor window from the xml file, which is either
        /// the editor cache file when opening the editor, or the system config file (copied to be
        /// editor cache file) when clearing editor cache.
        /// </summary>
        /// <param name="foundationOwnedSettings">foundationOwnedSettings from the XML file.</param>
        public void UpdateFoundation(FoundationOwnedSettings foundationOwnedSettings)
        {
            if(foundationOwnedSettings != null)
            {
                EditorData = foundationOwnedSettings;

                if(foundationOwnedSettings.CreditFormatter.UseCreditSeparator)
                {
                    foundationOwnedSettings.CreditFormatter.UseCreditSeparatorSpecified = true;
                }

                if(foundationOwnedSettings.Jurisdiction == null)
                {
                    foundationOwnedSettings.Jurisdiction = DefaultJurisdiction;
                }

                indexDecimal = Array.IndexOf(decimalGroupSeparatorList,
                                             foundationOwnedSettings.CreditFormatter.DecimalSeparator);

                if(indexDecimal == -1)
                {
                    indexDecimal = 0;
                }

                indexDigit = Array.IndexOf(digitGroupSeparatorList,
                                           foundationOwnedSettings.CreditFormatter.DigitGroupSeparator);

                if(indexDigit == -1)
                {
                    indexDigit = 0;
                }

                indexCurrency = Array.IndexOf(currencyList.Keys.ToArray(),
                                              foundationOwnedSettings.CreditFormatter.CurrencySymbol);

                if(indexCurrency == -1)
                {
                    indexCurrency = 0;
                }

                environmentMask = 0;
                foreach(var value in foundationOwnedSettings.EnvironmentAttribute)
                {
                    environmentMask += (int)Math.Pow(2, (int)value);
                }

                if(foundationOwnedSettings.MarketingBehavior == null)
                {
                    foundationOwnedSettings.MarketingBehavior = new MarketingBehaviorType();
                }

                if(foundationOwnedSettings.BonusSoaaSettings == null)
                {
                    foundationOwnedSettings.BonusSoaaSettings = new BonusSoaaSettingsType
                                                                {
                                                                    Supported = false,
                                                                    IsAllowed = true,
                                                                    MinDelaySeconds = 120
                                                                };
                }
            }
        }

        #region Foundation foldout

        /// <summary>
        /// Foldout for the Foundation Owned settings.
        /// </summary>
        public void DisplayFoundation()
        {
            showFoundation = EditorGUILayout.Foldout(showFoundation, "Foundation Owned Settings");
            if(showFoundation)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                EditorGUILayout.BeginVertical();

                supported = EditorGUILayout.BeginToggleGroup("Supported", supported);
                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Game Min Bet Amount", "Min bet in base units");
                EditorData.GameMinBet = EditorGUILayout.IntField(SystemConfigEditor.CheckNegative((int)EditorData.GameMinBet));
                EditorData.GameMinBetSpecified = EditorData.GameMinBet != 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Progressive Win Capping",
                             "The progressive win cap limit, \n0 means the jurisdiction doesn't support it.");
                EditorData.ProgressiveWinCapLimit = EditorGUILayout.IntField(
                    SystemConfigEditor.CheckNegative((int)EditorData.ProgressiveWinCapLimit));
                EditorData.ProgressiveWinCapLimitSpecified = EditorData.ProgressiveWinCapLimit != 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Total Win Capping",
                             "The total win cap limit, \n0 means the jurisdiction doesn't support it.");

                EditorData.TotalWinCapLimit = EditorGUILayout.IntField(SystemConfigEditor.CheckNegative((int)EditorData.TotalWinCapLimit));
                EditorData.TotalWinCapLimitSpecified = EditorData.TotalWinCapLimit != 0;
                EditorGUILayout.EndHorizontal();

                showWinCap = EditorGUILayout.Foldout(showWinCap, "Win Cap Settings");
                if(showWinCap)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Win Cap Behavior", 1);
                    EditorData.WinCapBehavior = (WinCapBehaviorType)EditorGUILayout.EnumPopup(EditorData.WinCapBehavior);
                    EditorGUILayout.EndHorizontal();

                    if(EditorData.WinCapBehavior == WinCapBehaviorType.FixedWinCapAmount)
                    {
                        EditorGUILayout.BeginHorizontal();
                        DisplayLabel("Win Cap Limit",
                                     "The overall limit to the win amount, \n0 means no win cap limit is in effect.",
                            1);
                        EditorData.WinCapLimit = EditorGUILayout.IntField(SystemConfigEditor.CheckNegative((int)EditorData.WinCapLimit));
                        EditorGUILayout.EndHorizontal();

                        EditorData.WinCapLimitSpecified = true;
                        EditorData.WinCapMultiplierSpecified = false;
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        DisplayLabel("Win Cap Multiplier",
                                     "The multiplier limit of current bet or multiplier of Max bet, \n0 means no multiplier limit is in effect.",
                            1);
                        EditorData.WinCapMultiplier = (uint)EditorGUILayout.IntField((int)EditorData.WinCapMultiplier);
                        EditorGUILayout.EndHorizontal();

                        EditorData.WinCapMultiplierSpecified = true;
                        EditorData.WinCapLimitSpecified = false;
                    }

                    EditorData.WinCapBehaviorSpecified = true;

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Transfer Amount",
                             "The amount to transfer from the Bank to Credit Meter\nAlways in units of Cents");
                EditorData.TransferBankToWagerable = EditorGUILayout.IntField(
                    SystemConfigEditor.CheckNegative((int)EditorData.TransferBankToWagerable));
                EditorData.TransferBankToWagerableSpecified = EditorData.TransferBankToWagerable != 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Max History Steps",
                             "The maximum number of play-steps within each game-cycle in history");
                EditorData.MaxHistorySteps = (uint)EditorGUILayout.IntField(
                    SystemConfigEditor.CheckNegative((int)EditorData.MaxHistorySteps));
                EditorGUILayout.EndHorizontal();

                showCredit = EditorGUILayout.Foldout(showCredit, "Credit Formatter");
                if(showCredit)
                {
                    EditorGUI.indentLevel++;

                    if(!separators.TryGetValue(EditorData.CreditFormatter.DecimalSeparator, out var decimalSeparator))
                    {
                        decimalSeparator = ".";
                    }

                    if(!separators.TryGetValue(EditorData.CreditFormatter.DigitGroupSeparator, out var groupSeparator))
                    {
                        groupSeparator = ",";
                    }

                    var outputExample = string.Format("{0} 1 {1} 525 {2} 213 {3} 32",
                                                      EditorData.CreditFormatter.CurrencySymbol,
                                                      groupSeparator,
                                                      groupSeparator,
                                                      decimalSeparator);

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Example: ", 1);
                    EditorGUILayout.LabelField(outputExample);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Decimal Separator", 1);
                    indexDecimal = EditorGUILayout.Popup(indexDecimal, decimalGroupSeparatorList);
                    EditorData.CreditFormatter.DecimalSeparator = decimalGroupSeparatorList[indexDecimal];
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Digit Group Separator", 1);
                    indexDigit = EditorGUILayout.Popup(indexDigit, digitGroupSeparatorList);
                    EditorData.CreditFormatter.DigitGroupSeparator = digitGroupSeparatorList[indexDigit];
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Currency Symbol", 1);
                    indexCurrency = EditorGUILayout.Popup(indexCurrency, CurrencyKeys);
                    EditorData.CreditFormatter.CurrencySymbol = currencyList.Keys.ToArray()[indexCurrency];
                    EditorGUILayout.EndHorizontal();

                    if(currencyList.ContainsKey(EditorData.CreditFormatter.CurrencySymbol))
                    {
                        // The currency cent symbol in the value associated with the currency symbol in the dictionary.
                        EditorData.CreditFormatter.CurrencyCentSymbol =
                            currencyList[EditorData.CreditFormatter.CurrencySymbol];
                    }

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Use Credit Separator",
                                 "If Digit Group Separator should be used for non monetary numbers, such as credits.",
                                 1);
                    EditorData.CreditFormatter.UseCreditSeparator = EditorGUILayout.Toggle(EditorData.CreditFormatter.UseCreditSeparator);
                    EditorData.CreditFormatter.UseCreditSeparatorSpecified = EditorData.CreditFormatter.UseCreditSeparator;
                    EditorGUILayout.EndHorizontal();

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                showAncillary = EditorGUILayout.Foldout(showAncillary,
                                                        new GUIContent("Ancillary (Double Up) Settings",
                                                                       "The foundation owned configuration for ancillary games"));

                if(showAncillary)
                {
                    EditorGUI.indentLevel++;

                    EditorData.AncillarySetting.Supported = EditorGUILayout.BeginToggleGroup("Supported",
                                                                                             EditorData.AncillarySetting.Supported);

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Cycle Limit",
                                 "Number of game cycles permitted in a single ancillary play",
                        1);
                    EditorData.AncillarySetting.CycleLimit = EditorGUILayout.IntField(
                        SystemConfigEditor.CheckNegative((int)EditorData.AncillarySetting.CycleLimit));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Monetary Limit",
                                 "Max win amount, in base units, allowed in a single ancillary play",
                        1);
                    EditorData.AncillarySetting.MonetaryLimit = EditorGUILayout.IntField(
                        SystemConfigEditor.CheckNegative((int)EditorData.AncillarySetting.MonetaryLimit));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndToggleGroup();

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                // Making sure the user does not enter zero in any of the fields as the minimum value allowed is 1.
                if(EditorData.AncillarySetting.CycleLimit == 0)
                {
                    EditorData.AncillarySetting.CycleLimit = 1;
                }

                if(EditorData.AncillarySetting.MonetaryLimit == 0)
                {
                    EditorData.AncillarySetting.MonetaryLimit = 1;
                }

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Environment Attribute",
                             "Represents the attributes of the environment in which the gaming system runs");
                // More than one Environment attribute can be selected to be put in the game
                environmentMask = EditorGUILayout.MaskField(environmentMask, environmentArray);
                EditorGUILayout.EndHorizontal();

                var tempEnvironmentMask = environmentMask;
                if(environmentMask == -1)
                {
                    environmentMask = (1 << environmentArray.Length) - 1;
                }

                EditorData.EnvironmentAttribute.Clear();
                for(var index = 0; index < 32; index++)
                {
                    if(tempEnvironmentMask % 2 == 1)
                    {
                        EditorData.EnvironmentAttribute.Add((EnvironmentAttributeString)index);
                    }

                    tempEnvironmentMask >>= 1;
                }

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("AutoPlay Configuration",
                             "Enumeration used to represent the configuration of auto play");
                EditorData.AutoPlayConfiguration = (AutoPlayConfigurationType)EditorGUILayout.EnumPopup(EditorData.AutoPlayConfiguration);
                EditorData.AutoPlayConfigurationSpecified = EditorData.AutoPlayConfiguration != AutoPlayConfigurationType.NotAvailable;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("AutoPlay Confirmation Required",
                             "If AutoPlay confirmation is required");
                EditorData.AutoPlayConfirmationRequired = EditorGUILayout.Toggle(EditorData.AutoPlayConfirmationRequired);
                EditorData.AutoPlayConfirmationRequiredSpecified = EditorData.AutoPlayConfirmationRequired;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Jurisdiction", "Jurisdiction supported.");
                EditorData.Jurisdiction = EditorGUILayout.TextField(EditorData.Jurisdiction);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Credit Meter Behavior",
                             "Enumeration used to represent the behavior type of credit meter");
                EditorData.CreditMeterBehavior = (CreditMeterBehaviorType)EditorGUILayout.EnumPopup(EditorData.CreditMeterBehavior);
                EditorData.CreditMeterBehaviorSpecified = EditorData.CreditMeterBehavior != CreditMeterBehaviorType.Invalid;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Min Base Game Presentation Time (ms)",
                             "Minimum spin time allowed for the reels");
                EditorData.MinimumBaseGamePresentationTime = (uint)EditorGUILayout.IntField(
                    SystemConfigEditor.CheckNegative((int)EditorData.MinimumBaseGamePresentationTime));
                EditorData.MinimumBaseGamePresentationTimeSpecified = EditorData.MinimumBaseGamePresentationTime != 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Min Free Spin Presentation Time (ms)",
                             "The minimum time for a single slot free spin cycle in milliseconds.");
                EditorData.MinimumFreeSpinTime = (uint)EditorGUILayout.IntField(
                    SystemConfigEditor.CheckNegative((int)EditorData.MinimumFreeSpinTime));
                EditorData.MinimumFreeSpinTimeSpecified = EditorData.MinimumFreeSpinTime != 0;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Round Wager Up Play-off Enabled",
                             "If the RoundWagerUpPlayoff feature is enabled or not.");
                EditorData.RoundWagerUpPlayoffEnabled = EditorGUILayout.Toggle(EditorData.RoundWagerUpPlayoffEnabled);
                EditorData.RoundWagerUpPlayoffEnabledSpecified = EditorData.RoundWagerUpPlayoffEnabled;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Display Video Reels for Stepper",
                             "Should the video reels for stepper be displayed");
                EditorData.DisplayVideoReelsForStepper = EditorGUILayout.Toggle(EditorData.DisplayVideoReelsForStepper);
                EditorData.DisplayVideoReelsForStepperSpecified = EditorData.DisplayVideoReelsForStepper;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                DisplayLabel("Rtp Ordered By Bet Required",
                             "Whether higher total bets must return a higher RTP than a lesser bet");
                EditorData.RtpOrderedByBetRequired = EditorGUILayout.Toggle(EditorData.RtpOrderedByBetRequired);
                EditorData.RtpOrderedByBetRequiredSpecified = EditorData.RtpOrderedByBetRequired;
                EditorGUILayout.EndHorizontal();

                showMarketingBehavior = EditorGUILayout.Foldout(showMarketingBehavior, "Marketing Behavior");
                if(showMarketingBehavior)
                {
                    EditorGUI.indentLevel++;

                    var topScreenGameAdvertisement = EditorData.MarketingBehavior.TopScreenGameAdvertisement;

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Top screen marketing behavior",
                                 "The behavior type of top screen advertisement.",
                        1);
                    topScreenGameAdvertisement = (TopScreenGameAdvertisementType)EditorGUILayout.EnumPopup(topScreenGameAdvertisement);
                    EditorGUILayout.EndHorizontal();

                    EditorData.MarketingBehavior.TopScreenGameAdvertisement = topScreenGameAdvertisement;
                    EditorData.MarketingBehavior.TopScreenGameAdvertisementSpecified = true;

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                showBonusSoaaSettings = EditorGUILayout.Foldout(showBonusSoaaSettings, "Bonus SOAA (Single Option Auto Advance) Settings");
                if(showBonusSoaaSettings)
                {
                    EditorGUI.indentLevel++;

                    var bonusSoaaSettings = EditorData.BonusSoaaSettings;

                    bonusSoaaSettings.Supported = EditorGUILayout.BeginToggleGroup(
                        new GUIContent("Supported",
                                       "Set to False to simulate running with older Foundations"),
                        bonusSoaaSettings.Supported);

                    EditorGUILayout.BeginHorizontal();
                    DisplayLabel("Is Allowed",
                                 "If there is a single player option presented in bonus, " +
                                 "whether the bonus can auto advance if there is no player interaction " +
                                 "for MinDelay seconds.",
                        1);
                    bonusSoaaSettings.IsAllowed = EditorGUILayout.Toggle(bonusSoaaSettings.IsAllowed);
                    EditorGUILayout.EndHorizontal();

                    if(bonusSoaaSettings.IsAllowed)
                    {
                        EditorGUILayout.BeginHorizontal();
                        DisplayLabel("Min Delay (s)",
                                     "If there is a single player option presented in bonus, " +
                                     "the minimum time (in seconds) the bonus has to wait for " +
                                     "the player interaction before auto advancing.",
                            1);
                        bonusSoaaSettings.MinDelaySeconds = EditorGUILayout.IntField(SystemConfigEditor.CheckNegative(bonusSoaaSettings.MinDelaySeconds));
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndToggleGroup();

                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndToggleGroup();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region Private Helper Fields and Functions

        private static readonly int LabelWidth = 160;
        private static readonly int IndentWidth = 40;

        private static void DisplayLabel(string text, int indentLevel = 0)
        {
            DisplayLabel(text, null, indentLevel);
        }

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
