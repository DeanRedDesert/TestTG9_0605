//-----------------------------------------------------------------------
// <copyright file = "FoundationOwnedSettingsParser.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Standalone;
    using Money;
    using TopScreenGameAdvertisementType = Communication.Standalone.Schemas.TopScreenGameAdvertisementType;

    /// <summary>
    /// This class retrieves values of settings owned by the Foundation,
    /// such as the environment attributes and Foundation owned configurations,
    /// by parsing an xml element that contains the needed information.
    /// </summary>
    internal class FoundationOwnedSettingsParser
    {
        #region Public Property

        /// <summary>
        /// Gets a list of environment attributes.
        /// </summary>
        public List<EnvironmentAttribute> EnvironmentAttributes { get; }

        /// <summary>
        /// Gets the game minimum bet.
        /// </summary>
        public long GameMinBet { get; }

        /// <summary>
        /// Gets the win cap limit amount.
        /// </summary>
        public long WinCapLimit { get; }

        /// <summary>
        /// Gets win cap behavior.
        /// </summary>
        public WinCapBehavior WinCapBehavior { get; }

        /// <summary>
        /// Gets the win cap multiplier of current bet or of max bet.
        /// </summary>
        public uint WinCapMultiplier { get; }

        /// <summary>
        /// Gets the progressive win cap limit.
        /// </summary>
        public long ProgressiveWinCapLimit { get; }

        /// <summary>
        /// Gets the total win cap limit.
        /// </summary>
        public long TotalWinCapLimit { get; }

        /// <summary>
        /// Gets the display video reels for stepper.
        /// </summary>
        public bool DisplayVideoReelsForStepper { get; }

        /// <summary>
        /// Gets the amount to transfer from Bank to Wagerable.
        /// </summary>
        public long TransferBankToWagerable { get; }

        /// <summary>
        /// Gets the minimum base game presentation time.
        /// </summary>
        public int MinimumBaseGamePresentationTime { get; }

        /// <summary>
        /// Gets the minimum free spin time.
        /// </summary>
        public int MinimumFreeSpinTime { get; }

        /// <summary>
        /// Gets the maximum history steps.
        /// </summary>
        public uint MaxHistorySteps { get; }

        /// <summary>
        /// Gets the credit formatter.
        /// </summary>
        public CreditFormatter CreditFormatter { get; }

        /// <summary>
        /// Gets the ancillary game config
        /// </summary>
        public AncillaryConfiguration AncillarySysConfig { get; }

        /// <summary>
        /// Gets the auto play configuration.
        /// </summary>
        public AutoPlayConfiguration AutoPlayConfiguration { get; }

        /// <summary>
        /// Gets if the auto play confirmation is required.
        /// </summary>
        public bool AutoPlayConfirmationRequired { get; }

        /// <summary>
        /// Gets if the auto play speed increase is allowed.
        /// </summary>
        public bool? AutoPlaySpeedIncreaseAllowed { get; }

        /// <summary>
        /// Gets the credit meter behavior.
        /// </summary>
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; }

        /// <summary>
        /// Gets the Jurisdiction as a string.
        /// </summary>
        public string Jurisdiction { get; }

        /// <summary>
        /// Gets if the RoundWagerUpPlayoff is enabled.
        /// </summary>
        public bool RoundWagerUpPlayoffEnabled { get; }

        /// <summary>
        /// Gets the EGM marketing behavior.
        /// </summary>
        public MarketingBehavior MarketingBehavior
        {
            get => marketingBehavior ?? new MarketingBehavior();
            private set => marketingBehavior = value;
        }

        /// <summary>
        /// Gets the settings for the Single Option Auto Advance (SOAA) feature in a Bonus.
        /// </summary>
        public BonusSoaaSettings BonusSoaaSettings { get; }

        /// <summary>
        /// Gets the requirement that whether higher total bets must return a higher RTP than a lesser bet.
        /// </summary>
        public bool RtpOrderedByBetRequired { get; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The EGM wide marketing behavior field.
        /// </summary>
        private MarketingBehavior marketingBehavior = new MarketingBehavior();

        /// <summary>
        /// Used to convert string values of XML elements to actual symbols.
        /// </summary>
        private readonly Dictionary<string, string> separators = new Dictionary<string, string>
        {
            {"NONE", ""},
            {"PERIOD","."},
            {"COMMA", ","},
            {"SPACE", " "},
            {"COMMA SPACE",", "},
            {"APOSTROPHE", "'"}
        };

        #endregion

        /// <summary>
        /// Initialize a new instance of Foundation Owned Settings Parser using
        /// an xml element that contains the needed information for parsing.
        /// </summary>
        /// <param name="settingsElement">
        /// An xml element that contains the foundation owned settings.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when <paramref name="settingsElement"/> has invalid data.
        /// </exception>
        public FoundationOwnedSettingsParser(XElement settingsElement)
        {
            if(settingsElement != null)
            {
                EnvironmentAttributes = new List<EnvironmentAttribute>();

                // Convert environment attribute strings to enum values.
                // .NET 3.5 does not support Enum.TryParse.
                var attributeStrings = settingsElement.Elements("EnvironmentAttribute").Select(e => (string)e).ToList();
                var enumNames = Enum.GetNames(typeof(EnvironmentAttribute));
                foreach(var attributeString in attributeStrings)
                {
                    if(enumNames.Contains(attributeString))
                    {
                        EnvironmentAttributes.Add((EnvironmentAttribute)Enum.Parse(typeof(EnvironmentAttribute), attributeString));
                    }
                }

                var element = settingsElement.Element("GameMinBet");
                if(element != null)
                {
                    GameMinBet = (long)element;
                }

                element = settingsElement.Element("WinCapBehavior");
                WinCapBehavior = element == null
                    ? WinCapBehavior.FixedWinCapAmount
                    : (WinCapBehavior)Enum.Parse(typeof(WinCapBehavior), (string)element);

                element = settingsElement.Element("WinCapLimit");
                WinCapLimit = element == null ? 0 : (long)element;

                element = settingsElement.Element("WinCapMultiplier");
                WinCapMultiplier = element == null ? 0 : (uint)element;

                element = settingsElement.Element("ProgressiveWinCapLimit");
                ProgressiveWinCapLimit = element == null ? 0 : (long)element;

                element = settingsElement.Element("TotalWinCapLimit");
                TotalWinCapLimit = element == null ? 0 : (long)element;

                element = settingsElement.Element("DisplayVideoReelsForStepper");
                DisplayVideoReelsForStepper = element != null && (bool)element;

                element = settingsElement.Element("TransferBankToWagerable");
                if(element != null)
                {
                    TransferBankToWagerable = (long)element;
                }

                element = settingsElement.Element("MaxHistorySteps");
                if(element != null)
                {
                    MaxHistorySteps = (uint)element;
                }

                element = settingsElement.Element("MinimumBaseGamePresentationTime");
                if(element != null)
                {
                    MinimumBaseGamePresentationTime = (int)element;
                }

                element = settingsElement.Element("MinimumFreeSpinTime");
                if (element != null)
                {
                    MinimumFreeSpinTime = (int)element;
                }

                element = settingsElement.Element("CreditFormatter");
                if(element != null)
                {
                    var decimalSeparatorElement = element.Element("DecimalSeparator");
                    var groupSeparatorElement = element.Element("DigitGroupSeparator");
                    var currencySymbolElement = element.Element("CurrencySymbol");
                    var centSymbolElement = element.Element("CurrencyCentSymbol");

                    if(decimalSeparatorElement != null &&
                       groupSeparatorElement != null &&
                       currencySymbolElement != null &&
                       centSymbolElement != null)
                    {
                        if(!separators.TryGetValue((string)decimalSeparatorElement, out var decimalSeparator))
                        {
                            decimalSeparator = ".";
                        }

                        if(!separators.TryGetValue((string)groupSeparatorElement, out var groupSeparator))
                        {
                            groupSeparator = ",";
                        }

                        var creditSeparatorElement = element.Element("UseCreditSeparator");

                        CreditFormatter = new CreditFormatter(decimalSeparator,
                                                              groupSeparator,
                                                              (string)currencySymbolElement,
                                                              (string)centSymbolElement,
                                                              creditSeparatorElement != null && (bool)creditSeparatorElement);
                    }
                    else
                    {
                        throw new InvalidStreamDataException("The XElement of CreditFormatter is invalid.");
                    }
                }

                element = settingsElement.Element("AncillarySetting");
                if (element?.Element("Supported") != null &&
                    element.Element("CycleLimit") != null &&
                    element.Element("MonetaryLimit") != null)
                {
                    var supported = (bool)element.Element("Supported");
                    var cycleLimit = (long)element.Element("CycleLimit");
                    var monetaryLimit = (long)element.Element("MonetaryLimit");

                    AncillarySysConfig = new AncillaryConfiguration(supported, cycleLimit, monetaryLimit);
                }

                element = settingsElement.Element("Jurisdiction");
                if(element != null)
                {
                    Jurisdiction = (string)element;
                }

                element = settingsElement.Element("AutoPlayConfiguration");
                if(element != null)
                {
                    AutoPlayConfiguration = (AutoPlayConfiguration)Enum.Parse(typeof(AutoPlayConfiguration), (string)element);
                }
                else
                {
                    AutoPlayConfiguration = AutoPlayConfiguration.NotAvailable;
                }

                element = settingsElement.Element("AutoPlayConfirmationRequired");
                if(element != null)
                {
                    AutoPlayConfirmationRequired = (bool)element;
                }
                else
                {
                    AutoPlayConfirmationRequired = false;
                }

                element = settingsElement.Element("AutoPlaySpeedIncreaseAllowed");
                if(element != null)
                {
                    AutoPlaySpeedIncreaseAllowed = (bool?)element;
                }
                else
                {
                    AutoPlaySpeedIncreaseAllowed = null;
                }

                element = settingsElement.Element("CreditMeterBehavior");
                if(element != null)
                {
                    CreditMeterBehavior = (CreditMeterDisplayBehaviorMode)Enum.Parse(typeof(CreditMeterDisplayBehaviorMode),
                        (string)element);
                }
                else
                {
                    CreditMeterBehavior = default;
                }

                element = settingsElement.Element("RoundWagerUpPlayoffEnabled");
                if(element != null)
                {
                    RoundWagerUpPlayoffEnabled = (bool)element;
                }
                else
                {
                    RoundWagerUpPlayoffEnabled = false;
                }

                var topScreenGameAdvertisement = TopScreenGameAdvertisementType.Invalid;
                element = settingsElement.Element("MarketingBehavior");
                var advertisementType = element?.Element("TopScreenGameAdvertisement");
                if(advertisementType != null)
                {
                    topScreenGameAdvertisement =
                        (TopScreenGameAdvertisementType)Enum.Parse(
                            typeof(TopScreenGameAdvertisementType), 
                            (string)advertisementType);
                }
                MarketingBehavior = new MarketingBehavior
                {
                    TopScreenGameAdvertisement =
                        (Ascent.Communication.Platform.Interfaces.TopScreenGameAdvertisementType)topScreenGameAdvertisement
                };

                element = settingsElement.Element("BonusSoaaSettings");
                BonusSoaaSettings = element != null && (bool)element.Element("Supported")
                                        ? (bool)element.Element("IsAllowed")
                                              ? new BonusSoaaSettings(true, (uint)element.Element("MinDelaySeconds"))
                                              : new BonusSoaaSettings(false)
                                        : null;

                element = settingsElement.Element("RtpOrderedByBetRequired");
                RtpOrderedByBetRequired = element != null && (bool)element;
            }
        }
    }
}
