//-----------------------------------------------------------------------
// <copyright file = "PaytableListManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Standalone;
    using Registries;

    /// <summary>
    /// This class manages enabled themes in the game package, the paytable list for
    /// each theme, as well as the associated denominations for each paytable.
    /// This information is used for the theme selection and theme context operations. 
    /// </summary>
    internal class PaytableListManager
    {
        #region Fields

        /// <summary>
        /// List of paytable variants that have been assigned a denomination in the
        /// system configuration file.  They are grouped by the theme identifiers.
        /// If no system configuration file is provided, this field would be null.
        /// </summary>
        private readonly Dictionary<string, Dictionary<long, PaytableConfiguration>> paytableList;

        /// <summary>
        /// A reference to the game registry info provider that helps validate the
        /// paytable list.
        /// </summary>
        private readonly IGameRegistryInfoProvider gameRegistryInfoProvider;

        #endregion

        #region Properties

        /// Get the paytable variant that is specified as default in the system
        /// configuration file.
        public PaytableConfiguration DefaultPaytableConfiguration { get; }

        /// Get the denomination that is associated with the default paytable
        /// in the system configuration file.
        public long DefaultDenomination { get; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize a new instance of Paytable List Manager using a parser that
        /// provides the list of paytable variants and their associated denominations,
        /// and a game registry info provider that helps validate the paytable list.
        /// </summary>
        /// <param name="paytableListParser">
        /// An xml element parser that provides the list of paytable variants and their
        /// associated denominations.
        /// </param>
        /// <param name="gameRegistryInfo">
        /// A reference to the game registry info provider that helps validate the paytable list.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="paytableListParser"/> or
        /// <paramref name="gameRegistryInfo"/> is null.
        /// </exception>
        public PaytableListManager(PaytableListParser paytableListParser,
                                   IGameRegistryInfoProvider gameRegistryInfo)
        {
            if(paytableListParser == null)
            {
                throw new ArgumentNullException(nameof(paytableListParser));
            }

            paytableList = paytableListParser.PaytableList;
            gameRegistryInfoProvider = gameRegistryInfo ?? throw new ArgumentNullException(nameof(gameRegistryInfo));

            if(paytableList == null)
            {
                // Initialize the default paytable to a hard coded value.
                DefaultPaytableConfiguration = new PaytableConfiguration(
                                                    new PaytableVariant("Theme Name", "Paytable", "Paytable File")
                                                    );
            }
            else
            {
                // Retrieve the default paytable variant and denomination.
                DefaultPaytableConfiguration = new PaytableConfiguration(paytableListParser.DefaultPaytableVariant);
                DefaultDenomination = paytableListParser.DefaultDenomination;

                // Validate the theme names.
                ValidateThemeNames(gameRegistryInfoProvider.GetThemeList());

                // Validate the paytable list for each theme.
                foreach(var themeEntry in paytableList.Where(themeEntry => themeEntry.Value != null))
                {
                    ValidateThemePaytableList(themeEntry.Key,
                                              themeEntry.Value);
                }
            }
        }

        /// <summary>
        /// Get the paytable configuration of the given paytable variant and denomination.
        /// </summary>
        /// <param name="paytableVariant">The specified paytable variant.</param>
        /// <param name="denomination">The denomination as the searching key.</param>
        /// <returns>
        /// By the given paytable variant and denomination, search the existing paytable configurations and return
        /// one if any.
        /// If no existing paytable configuration is found, check if relevant max bet/button panel min bet is
        /// defined in existing paytable configurations as per bet resolution for the specified theme. For example,
        /// if the specified theme gets bet resolution being PerTheme, then go through paytable configurations and
        /// find one item with the same theme, retrieve its max bet/button panel min bet and return a new instance
        /// of paytable configuration item with the retrieved max bet/button panel min bet.
        /// If, at worst, no relevant paytable configuration item is found, return a new instance of paytable
        /// configuration item with the default max bet/button panel min bet defined in game registry file.
        /// </returns>
        public PaytableConfiguration GetPaytableConfiguration(PaytableVariant paytableVariant, long denomination)
        {
            PaytableConfiguration result = null;

            // Go through the paytablelist to see if system config file defines the max bet/
            // button panel min bet value for the relevant paytable variant.
            if(paytableList != null)
            {
                var betResolution = gameRegistryInfoProvider.GetBetResolution(paytableVariant.ThemeIdentifier);

                if(paytableList.ContainsKey(paytableVariant.ThemeIdentifier))
                {
                    var paytableListItem = paytableList[paytableVariant.ThemeIdentifier];

                    if(paytableListItem.ContainsKey(denomination))
                    {
                        result = paytableListItem[denomination];
                    }
                    else if(betResolution != MaxBetResolution.PerPayvarDenomination &&
                            betResolution != MaxBetResolution.Unknown)
                    {
                        var target = paytableListItem.Values.FirstOrDefault(item => betResolution == MaxBetResolution.PerTheme ||
                                                                                    (betResolution == MaxBetResolution.PerPayvar &&
                                                                                     paytableVariant.PaytableName == item.PaytableVariant.PaytableName));
                        if(target != null)
                        {
                            result = new PaytableConfiguration(paytableVariant)
                                {
                                    MaxBet = target.MaxBet,
                                    ButtonPanelMinBet = target.ButtonPanelMinBet
                                };
                        }
                    }
                }
            }

            // If there is no related max bet/button panel min bet found from system configurations,
            // get max bet/button panel min bet defined in registry file.
            return result ?? new PaytableConfiguration(paytableVariant)
                    {
                        MaxBet = gameRegistryInfoProvider.GetMaxBet(paytableVariant, denomination),
                        ButtonPanelMinBet = gameRegistryInfoProvider.GetButtonPanelMinBet(paytableVariant, denomination)
                    };
        }

        /// <summary>
        /// Get the paytable variant of a given theme that is associated with the
        /// specified denomination.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <param name="denomination">The denomination as the searching key.</param>
        /// <returns>
        /// The paytable variant of the given theme that is associated with the denomination.
        /// Returns empty paytable variant if none is found.
        /// Returns hard coded paytable variant if no paytable list is defined.
        /// </returns>
        public PaytableVariant GetPaytableVariant(string themeIdentifier, long denomination)
        {
            var result = new PaytableVariant();

            if(paytableList == null)
            {
                result = DefaultPaytableConfiguration.PaytableVariant;
            }
            else if(paytableList.ContainsKey(themeIdentifier) &&
                    paytableList[themeIdentifier].ContainsKey(denomination))
            {
                result = paytableList[themeIdentifier][denomination].PaytableVariant;
            }

            return result;
        }

         /// <summary>
        /// Check if a denomination is associated with a paytable variant of the given theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <param name="denomination">The denomination as the searching key.</param>
        /// <returns>
        /// True if the denomination is associated with a paytable variant of the given theme.
        /// Always return true if no paytable list is defined.
        /// </returns>
        public bool IsDenominationEnabled(string themeIdentifier, long denomination)
        {
            var availableDenominations = GetAvailableDenominations(themeIdentifier);

            return paytableList == null ||
                   availableDenominations?.Contains(denomination) == true;
        }

        /// <summary>
        /// Get the list of denominations available for the player to pick for the given theme.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>
        /// The list of denominations available for the player to pick for the given theme.
        /// Null if the paytable list is not defined.
        /// </returns>
        public IList<long> GetAvailableDenominations(string themeIdentifier)
        {
            return paytableList?.ContainsKey(themeIdentifier) == true
                       ? paytableList[themeIdentifier].Keys.ToList()
                       : null;
        }

        /// <summary>
        /// Check if a theme is enabled.
        /// </summary>
        /// <param name="themeIdentifier">The theme identifier, typically the G2S theme id.</param>
        /// <returns>True if the theme is enabled.</returns>
        public bool IsThemeEnabled(string themeIdentifier)
        {
            var availableThemes = GetAvailableThemes();

            return availableThemes?.Contains(themeIdentifier) == true;
        }

        /// <summary>
        /// Get the list of theme identifiers available for the player to pick.
        /// </summary>
        /// <returns>The list of theme identifiers available for the player to pick.</returns>
        public IList<string> GetAvailableThemes()
        {
            return paytableList != null
                       ? paytableList.Keys.ToList()
                       : new List<string> { DefaultPaytableConfiguration.PaytableVariant.ThemeIdentifier };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate the theme names appeared in the paytable list.
        /// </summary>
        /// <param name="registryThemeList">
        /// The list of theme names specified in the theme registries.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when the paytable list contains an invalid theme name
        /// when validated by the game registries.
        /// </exception>
        private void ValidateThemeNames(IList<string> registryThemeList)
        {
            if(registryThemeList != null)
            {
                // Make sure that the theme identifiers, typically the G2S theme ids, can be found in the registries.
                foreach(var themeIdentifier in paytableList.Keys)
                {
                    if(!registryThemeList.Contains(themeIdentifier))
                    {
                        throw new InvalidStreamDataException(
                            $"Theme \"{themeIdentifier}\" is defined in the paytable list, but cannot be found in game registries.");
                    }
                }
            }
        }

        /// <summary>
        /// Validate a theme's paytable list against information retrieved from
        /// game registries.
        /// </summary>
        /// <param name="themeIdentifier">
        /// The specific theme identifier, typically the G2S theme id.
        /// </param>
        /// <param name="themePaytableList">
        /// List of paytable configuration in a specific theme that have been assigned
        /// with a denomination.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when the paytable list contains invalid data when validated by
        /// the game registries.
        /// </exception>
        private void ValidateThemePaytableList(string themeIdentifier,
                                               IDictionary<long, PaytableConfiguration> themePaytableList)
        {
            var maxNumberOfEnabledDenominations =
                gameRegistryInfoProvider.GetMaxNumberOfEnabledDenominations(themeIdentifier);

            // Verify that the number of available denominations do not exceed limit.
            if(maxNumberOfEnabledDenominations != GameRegistryManager.NoRegistryInUse &&
                themePaytableList.Count > maxNumberOfEnabledDenominations)
            {
                throw new InvalidStreamDataException(
                    $"Too many denominations enabled.  Enabled: {themePaytableList.Count}  Allowed: {maxNumberOfEnabledDenominations}");
            }

            // Cross validate the paytable list.
            var supportedDenominations = gameRegistryInfoProvider.GetSupportedDenominations(themeIdentifier);
            foreach(var entry in themePaytableList)
            {
                var paytableVariant = entry.Value.PaytableVariant;
                var denomination = entry.Key;

                // Validate the denominations defined in system config.
                if(supportedDenominations != null)
                {
                    // Verify the paytable variant has a corresponding payvar registry.
                    if(!supportedDenominations.ContainsKey(paytableVariant))
                    {
                        throw new InvalidStreamDataException(
                            $"No payvar registry is defined for {paytableVariant}.");
                    }

                    // Verify the associated denomination is supported by the paytable variant.
                    if(!supportedDenominations[paytableVariant].Contains(denomination))
                    {
                        throw new InvalidStreamDataException(
                            $"Denomination {denomination} is not supported by {paytableVariant}.");
                    }
                }

                // Validate the min/max bets defined in system config.
                var buttonPanelMinBet = entry.Value.ButtonPanelMinBet;
                var maxBet = entry.Value.MaxBet;
                var supportedButtonPanelMinBets =
                                    gameRegistryInfoProvider.GetSupportedButtonPanelMinBets(paytableVariant);
                var supportedMaxBets =
                                    gameRegistryInfoProvider.GetSupportedMaxBets(paytableVariant);

                // Button panel min bet must not be larger than max bet.
                if(buttonPanelMinBet != null && maxBet != null && (long)buttonPanelMinBet > (long)maxBet)
                {
                    throw new InvalidStreamDataException(
                        $"Button Panel Min Bet value {(long)buttonPanelMinBet} is larger than Max Bet {(long)maxBet} in paytable variant {paytableVariant}.");
                }

                // Validate if button panel min bet user defined is supported by game registry.
                if(supportedButtonPanelMinBets != null && buttonPanelMinBet != null &&
                    !supportedButtonPanelMinBets.Contains((long)buttonPanelMinBet))
                {
                    throw new InvalidStreamDataException(
                        $"Button Panel Min Bet value {(long)buttonPanelMinBet} is not supported by game registry related to {paytableVariant}.");
                }

                // Button panel min bet shall be defaulted to 0 if there is no button panel min bet is defined.
                if(supportedButtonPanelMinBets == null && buttonPanelMinBet != null && buttonPanelMinBet != 0)
                {
                    throw new InvalidStreamDataException(
                        $"Button Panel Min Bet value {(long)buttonPanelMinBet} is not 0 but there is no supported button panel" +
                        $" min bets defined in game registry related to {paytableVariant}");
                }

                // Validate if max bet user defined is supported by game registry.
                if(supportedMaxBets != null && maxBet != null && !supportedMaxBets.Contains((long)maxBet))
                {
                    throw new InvalidStreamDataException(
                        $"Max Bet value {(long)maxBet} is not supported by game registry related to {paytableVariant}.");
                }

                // If user didn't define the max bet, assign it the value defined in game registry file.
                if(maxBet == null)
                {
                    var maxBetInRegistry = gameRegistryInfoProvider.GetMaxBet(paytableVariant, denomination);

                    if(maxBetInRegistry != GameRegistryManager.NoRegistryInUse)
                    {
                        entry.Value.MaxBet = maxBetInRegistry;
                    }
                }

                // If user didn't define the button panel min bet, assign it the value defined in game registry file.
                if(buttonPanelMinBet == null)
                {
                    var buttonPanelMinBetInRegistry = gameRegistryInfoProvider.GetButtonPanelMinBet(
                                                                paytableVariant, denomination);

                    if(buttonPanelMinBetInRegistry != GameRegistryManager.NoRegistryInUse)
                    {
                        entry.Value.ButtonPanelMinBet = buttonPanelMinBetInRegistry;
                    }
                }
            }
        }

        #endregion
    }
}
