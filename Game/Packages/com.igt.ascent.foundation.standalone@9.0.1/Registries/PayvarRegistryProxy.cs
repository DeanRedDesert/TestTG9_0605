//-----------------------------------------------------------------------
// <copyright file = "PayvarRegistryProxy.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using F2LPayvarVerTip = Core.Registries.Internal.F2L.F2LPayvarRegistryVer3;

    /// <summary>
    /// A proxy for the <see cref="F2LPayvarVerTip.PayvarRegistry"/> object that is used to
    /// retrieve information from the tip version of the theme registry.
    /// </summary>
    internal class PayvarRegistryProxy : IPayvarRegistry
    {
        #region Fields

        /// <summary>
        /// The name of the payvar registry file, used to report errors.
        /// </summary>
        private readonly string payvarRegistryFileName;

        /// <summary>
        /// The payvar registry that this proxy represents.
        /// </summary>
        private readonly F2LPayvarVerTip.PayvarRegistry payvarRegistry;

        /// <summary>
        /// The system progressive mapping list.
        /// </summary>
        private List<ProgressiveLink> systemMappingList;

        /// <summary>
        /// The game controlled progressive mapping list.
        /// </summary>
        private List<ProgressiveLink> gameMappingList;

        /// <summary>
        /// The custom configuration reader used to retrieve the values of the configuration items.
        /// </summary>
        private readonly CustomConfigurationReader customConfigReader;

        // List of controller types that are allowed for hard coded setup.
        private readonly List<string> controllerTypesAllowed = new List<string>
            {
                ProgressiveControllerTypes.WAP,
                ProgressiveControllerTypes.GCP,
            };

        #endregion

        #region Constructor

        /// <summary>
        /// Construct the proxy instance with the payvar registry object, the payvar name and the payvar ID.
        /// </summary>
        /// <param name="payvarRegistry">The payvar registry object.</param>
        /// <param name="payvarRegistryFileName">The name of payvar file, used to report errors.</param>
        /// <param name="payvarGroupRegistries">
        /// The <see cref="IPayvarGroupRegistry"/> belonging to this payvar group if this payvar registry is a group template.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="payvarRegistry"/> or <paramref name="payvarRegistryFileName"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when there is no payvar group registry initialized with this payvar group template.
        /// </exception>
        public PayvarRegistryProxy(F2LPayvarVerTip.PayvarRegistry payvarRegistry, string payvarRegistryFileName,
                                   IEnumerable<IPayvarGroupRegistry> payvarGroupRegistries)
        {
            this.payvarRegistryFileName = payvarRegistryFileName ?? throw new ArgumentNullException(nameof(payvarRegistryFileName), "Parameters may not be null.");
            this.payvarRegistry = payvarRegistry ?? throw new ArgumentNullException(nameof(payvarRegistry), "Parameters may not be null.");
            customConfigReader = new CustomConfigurationReader(payvarRegistry.CustomConfigItems?.ConfigItem);

            // Only lookup payvar group registry when payvar group registries have been discovered and loaded
            // because that means they are actually needed by the client.
            if((payvarRegistry.PayvarType == F2LPayvarVerTip.PayvarType.PayvarGroupTemplate ||
                payvarRegistry.PayvarType == F2LPayvarVerTip.PayvarType.SingleMultiTemplate) &&
               payvarGroupRegistries != null)
            {
                var payvarGroupRegistry = payvarGroupRegistries.FirstOrDefault(
                    groupRegistry => groupRegistry.GroupTemplateName == payvarRegistryFileName);

                if(payvarGroupRegistry == null)
                {
                    throw new InvalidOperationException(
                        "There is no payvar group registry initialized with this group template.");
                }

                payvarGroupRegistry.ThemeRegistryFileName = ThemeRegistryFileName;
                PayvarGroupRegistry = payvarGroupRegistry;
                UpdatePaybackPercentagesInEachPayvar();
            }
        }

        #endregion

        #region IPayvarRegistry Members

        /// <inheritDoc />
        public ICustomConfigurationReader CustomConfiguration => customConfigReader;

        /// <inheritDoc />
        public string PaytableIdentifier => $"{(string.IsNullOrEmpty(ThemeRegistryFileName) ? string.Empty : $"{ThemeRegistryFileName.Replace(@"\", "/")}/")}{payvarRegistry.PaytableName}";

        /// <inheritDoc />
        public string PaytableTagName => payvarRegistry.TagDataFile.Tag;

        /// <inheritDoc />
        public string PaytableTagFileName => payvarRegistry.TagDataFile.Value;

        /// <inheritDoc />
        public string ThemeRegistryFileName => payvarRegistry.ThemeRegistry;

        /// <inheritDoc />
        public bool HasProgressive => payvarRegistry.ProgressiveGameLevels.Count > 0;

        /// <inheritDoc />
        public bool DoubleUpSupported =>
            payvarRegistry.DoubleUpSupportedSpecified &&
            payvarRegistry.DoubleUpSupported;

        /// <inheritDoc />
        public PayvarType PayvarRegistryType => (PayvarType)payvarRegistry.PayvarType;

        /// <inheritDoc />
        public IPayvarGroupRegistry PayvarGroupRegistry { get; }

        /// <inheritDoc />
        public PaytablePaybackInfo PaybackInfo =>
            new PaytablePaybackInfo(PaytableIdentifier,
                payvarRegistry.PaybackPercentage,
                payvarRegistry.MinimumPaybackPercentage,
                payvarRegistry.MinimumPaybackPercentageWithoutProgressivesSpecified
                    ? payvarRegistry.MinimumPaybackPercentageWithoutProgressives
                    : 0);

        /// <inheritDoc />
        public IEnumerable<long> GetSupportedDenominations()
        {
            return payvarRegistry.SupportedDenominations.ConvertAll(denomination => (long)denomination);
        }

        /// <inheritDoc />
        public IDictionary<string, KeyValuePair<ConfigurationProfile, object>> GetConfigurationItemValues()
        {
            var results = new Dictionary<string, KeyValuePair<ConfigurationProfile, object>>();

            try
            {
                results = (Dictionary<string, KeyValuePair<ConfigurationProfile, object>>)customConfigReader.GetConfigurationItems();
            }
            catch(GameRegistryException exception)
            {
                throw new GameRegistryException(
                    $"Error occurred while reading the payvar registry --> \"{payvarRegistryFileName}\"",
                    exception);
            }

            return results;
        }

        /// <inheritDoc />
        public IEnumerable<ProgressiveLink> GetSystemProgressives()
        {
            if(systemMappingList == null)
            {
                LoadProgressives();
            }

            return systemMappingList;
        }

        /// <inheritDoc />
        public IEnumerable<ProgressiveLink> GetGameControlledProgressives()
        {
            if(gameMappingList == null)
            {
                LoadProgressives();
            }

            return gameMappingList;
        }

        /// <inheritDoc />
        public bool IsValidWinLevel(int winLevelIndex)
        {
            return payvarRegistry.GetWinLevel(winLevelIndex) != null;
        }

        /// <inheritDoc />
        public bool IsWinLevelProgressiveSupport(int winLevelIndex)
        {
            var winLevelInfo = payvarRegistry.GetWinLevel(winLevelIndex);
            return winLevelInfo?.ProgressiveSupport != null;
        }

        /// <inheritDoc />
        public long GetButtonPanelMinBet()
        {
            long result = 0;
            if(payvarRegistry.ButtonPanelMinBet != null)
            {
                RegistryUtility.ValidateBetLimitValue(payvarRegistry.ButtonPanelMinBet);
                checked
                {
                    result = (long)payvarRegistry.ButtonPanelMinBet.Value[0].Value;
                }
            }

            return result;
        }

        /// <inheritDoc />
        public long GetButtonPanelMinBetPerDenomination(long denomination)
        {
            long result = 0;
            if(payvarRegistry.ButtonPanelMinBet != null)
            {
                RegistryUtility.ValidateBetLimitValue(payvarRegistry.ButtonPanelMinBet);

                // For PerDenomination bet limit, search for the value associated with
                // the current game denomination.
                var betLimitFound =
                    payvarRegistry.ButtonPanelMinBet.Value.FirstOrDefault(
                        betLimitValue => betLimitValue.Denom == denomination);

                if(betLimitFound == null)
                {
                    throw new GameRegistryException(
                        $"Missing PerPayvarDenomination bet limit for denomination {denomination} in payvar registry of {payvarRegistryFileName}.");
                }

                checked
                {
                    result = (long)betLimitFound.Value;
                }
            }
            return result;
        }

        /// <inheritDoc />
        public IValuePool<long> GetSupportedButtonPanelMinBets()
        {
            IValuePool<long> result = null;

            if(payvarRegistry.ButtonPanelMinBet != null)
            {
                result = RegistryUtility.GetSupportedBetValuePool(payvarRegistry.ButtonPanelMinBet.ValuePool.Item);
            }
            return result;
        }

        /// <inheritDoc />
        public long GetMaxBet()
        {
            if(payvarRegistry.MaxBet == null)
            {
                throw new GameRegistryException(
                    $"Missing PerPayvar max bet in payvar registry of {payvarRegistryFileName}.");
            }

            RegistryUtility.ValidateBetLimitValue(payvarRegistry.MaxBet);
            checked
            {
                return (long)payvarRegistry.MaxBet.Value[0].Value;
            }
        }

        /// <inheritDoc />
        public long GetMaxBetPerDenomination(long denomination)
        {
            if(payvarRegistry.MaxBet == null)
            {
                throw new GameRegistryException(
                    $"Missing PerPayvarDenomination max bet in payvar registry of {payvarRegistryFileName}.");
            }

            RegistryUtility.ValidateBetLimitValue(payvarRegistry.MaxBet);

            // For PerDenomination bet limit, search for the value associated with
            // the current game denomination.
            var betLimitFound =
                payvarRegistry.MaxBet.Value.FirstOrDefault(
                    betLimitValue => betLimitValue.Denom == denomination);
            if(betLimitFound == null)
            {
                throw new GameRegistryException(
                    $"Missing PerPayvarDenomination bet limit for denomination {denomination} in payvar registry of {payvarRegistryFileName}.");
            }

            checked
            {
                return (long)betLimitFound.Value;
            }
        }

        /// <inheritDoc />
        public IValuePool<long> GetSupportedMaxBets()
        {
            if(payvarRegistry.MaxBet == null)
            {
                throw new GameRegistryException(
                    $"Missing PerPayvar max bet in payvar registry of {payvarRegistryFileName}.");
            }
            return RegistryUtility.GetSupportedBetValuePool(payvarRegistry.MaxBet.ValuePool.Item);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Load the progressives from the payvar registry.
        /// </summary>
        private void LoadProgressives()
        {
            systemMappingList = new List<ProgressiveLink>();
            gameMappingList = new List<ProgressiveLink>();

            for(var levelIndex = 0; levelIndex < payvarRegistry.ProgressiveGameLevels.Count; levelIndex++)
            {
                var progressiveGameLevel = payvarRegistry.ProgressiveGameLevels[levelIndex];

                // Skip if no progressive link is defined.
                if(progressiveGameLevel.ControllerType == null)
                {
                    continue;
                }

                // Verify the controller type is allowed for hard coded setup.
                if(!controllerTypesAllowed.Contains(progressiveGameLevel.ControllerType))
                {
                    throw new GameRegistryException(
                        $"{progressiveGameLevel.ControllerType} links cannot be hard coded in game registries.");
                }

                // Verify the controller level is present for linking.
                if(!progressiveGameLevel.ControllerLevelSpecified)
                {
                    throw new GameRegistryException(
                        $"Missing controller level for {progressiveGameLevel.ControllerType} link set up.");
                }

                if(progressiveGameLevel.ControllerType == ProgressiveControllerTypes.GCP)
                {
                    checked
                    {
                        gameMappingList.Add(new ProgressiveLink
                                                {
                                                    GameLevel = levelIndex,
                                                    ControllerName = progressiveGameLevel.ControllerType,
                                                    ControllerLevel = (int)progressiveGameLevel.ControllerLevel,
                                                });
                    }
                }
                else
                {
                    checked
                    {
                        systemMappingList.Add(new ProgressiveLink
                                                  {
                                                      GameLevel = levelIndex,
                                                      ControllerName = progressiveGameLevel.ControllerType,
                                                      ControllerLevel = (int)progressiveGameLevel.ControllerLevel,
                                                  });
                    }
                }
            }
        }

        /// <summary>
        /// Update the payback percentages for all payvars in the payvar group.
        /// </summary>
        /// <remarks>
        /// The payback percentages are taken from the payvar group template if they are not specified in the payvar
        /// group registry.
        /// </remarks>
        private void UpdatePaybackPercentagesInEachPayvar()
        {
            foreach(var payvar in PayvarGroupRegistry.Payvars)
            {
                if(payvar.MinimumPaybackPercentage.Equals(0.0f))
                {
                    payvar.MinimumPaybackPercentage = decimal.ToSingle(payvarRegistry.MinimumPaybackPercentage);
                }
                if(payvar.PaybackPercentage.Equals(0.0f))
                {
                    payvar.PaybackPercentage = decimal.ToSingle(payvarRegistry.PaybackPercentage);
                }
                // Always use the template value for MinPaybackPercentageWithoutProgressives
                // because there is no such field in PayvarGroup registry.
                payvar.MinPaybackPercentageWithoutProgressives = decimal.ToSingle(payvarRegistry.MinimumPaybackPercentageWithoutProgressives);
            }
        }

        #endregion
    }
}
