//-----------------------------------------------------------------------
// <copyright file = "ProgressiveManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList;
    using Ascent.OutcomeList.Interfaces;
    using Communication.Standalone;
    using InterfaceExtensions.Interfaces;
    using Logic.ProgressiveController;
    using Registries;

    /// <summary>
    /// This class manages the progressive controllers and progressive links.
    /// </summary>
    internal class ProgressiveManager : IStandaloneProgressiveManagerDependency
    {
        #region Fields

        /// <summary>
        /// The absolute path root of the paytable file name in a paytable variant.
        /// All progressive setups use the absolute file path in their keys.
        /// </summary>
        private readonly string rootPath;

        /// <summary>
        /// List of progressive controllers, keyed by the controller names.
        /// </summary>
        private readonly Dictionary<string, ISystemProgressiveController> progressiveControllers;

        /// <summary>
        /// List of progressive setups, keyed by the combination of
        /// denomination and paytable variant.
        /// For each denomination of each paytable, there is a list
        /// of game level mappings, a.k.a progressive links.
        ///
        /// This is for the progressives that can be dynamically set up
        /// via the operator menu, which is represented by the system
        /// configuration file in Standalone Game Lib.
        /// </summary>
        private Dictionary<KeyValuePair<long, PaytableVariant>, List<ProgressiveLink>> dynamicProgressiveSetups;

        /// <summary>
        /// List of progressive setups defined in game registries,
        /// keyed by paytable variant.  These setups are valid for
        /// all denominations.
        ///
        /// This is for the progressive setups that must be hard coded
        /// in game registries.
        /// </summary>
        private IDictionary<PaytableVariant, IList<ProgressiveLink>> hardCodedProgressiveSetups;

        /// <summary>
        /// List of game controlled progressives defined in the game registries.
        /// They are keyed by the paytable variant and valid for all denominations.
        /// </summary>
        private IDictionary<PaytableVariant, IList<ProgressiveLink>> gameControlledProgressiveSetups;

        /// <summary>
        /// Name of the current theme.
        /// </summary>
        private string currentThemeName;

        /// <summary>
        /// List of progressive links for the current active theme context.
        /// It is the value of the progressiveSetups entry that is keyed by
        /// the denomination and paytable variant of current theme context.
        /// </summary>
        private readonly List<ProgressiveLink> currentProgressiveLinks = new List<ProgressiveLink>();

        /// <summary>
        /// The flag indicates whether the controller levels' data fields
        /// have been loaded from the critical data.
        /// The controller levels only need to be loaded once,
        /// since system progressive controller's configuration
        /// is not modifiable after power up.
        /// </summary>
        private bool controllerLevelsLoaded;

        /// <summary>
        /// The current paytable variant.
        /// </summary>
        private PaytableVariant currentPaytableVariant;

        /// <summary>
        /// Locker object to synchronize contributions from different threads.
        /// </summary>
        private readonly object progressiveContributionLocker = new object();

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize a new instance of ProgressiveManager using a game lib interface,
        /// a parser that provides the controller settings and link setups for the
        /// dynamic system controlled progressives, and list of link setups for the
        /// hard coded system controlled progressive as defined by game registries.
        /// </summary>
        /// <param name="iGameLib">
        /// Interface of the game lib that owns this object.
        /// </param>
        /// <param name="progressiveParser">
        /// An xml element parser that provides the controller settings and link setups
        /// for the system controlled progressives as the parsing result.
        /// </param>
        /// <param name="gameRegistryInfoProvider">
        /// A reference to the game registry info provider that helps validate the paytable list.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/>, <paramref name="progressiveParser"/>
        /// or <paramref name="gameRegistryInfoProvider"/> is null.
        /// </exception>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when there is conflicting information on the progressive settings
        /// from the system configuration file and the game registries.
        /// </exception>
        public ProgressiveManager(IGameLib iGameLib,
                                  ProgressiveParser progressiveParser,
                                  IGameRegistryInfoProvider gameRegistryInfoProvider)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException(nameof(iGameLib));
            }

            if(progressiveParser == null)
            {
                throw new ArgumentNullException(nameof(progressiveParser));
            }

            if(gameRegistryInfoProvider == null)
            {
                throw new ArgumentNullException(nameof(gameRegistryInfoProvider));
            }

            // Register the event handler.
            iGameLib.ActivateThemeContextEvent += HandleActivateThemeContextEvent;

            // Create the controller list using the settings output by the parser.
            // The reason why it is done here rather than inside the parser is
            // to avoid having to pass iGameLib to the parser.
            progressiveControllers = new Dictionary<string, ISystemProgressiveController>(progressiveParser.ControllerSettings.Count);

            foreach(var controllerSetting in progressiveParser.ControllerSettings)
            {
                // This is just for better readability.
                var controllerName = controllerSetting.Key;
                var configurationList = controllerSetting.Value;

                // Create the controller, and add to the manager's controller list.
                progressiveControllers[controllerName] =
                    new SystemProgressiveController(iGameLib, controllerName, configurationList);
            }

            // Dynamic setups are provided by the parser.
            dynamicProgressiveSetups = progressiveParser.ProgressiveSetups;

            // Get progressive setups provided by the game registries.
            gameRegistryInfoProvider.GetProgressiveSetups(out hardCodedProgressiveSetups, out gameControlledProgressiveSetups);

            // Validate the theme names appeared in the progressive setups.
            ValidateThemeNames(gameRegistryInfoProvider.GetThemeList());

            // Validate and cross check two lists of progressive setups.
            ValidateProgressiveSetups();

            // Convert the paytable file names used in the setup keys
            // from relative path to absolute path.
            rootPath = iGameLib.GameMountPoint;
            NormalizeProgressiveSetupKeys();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Update the current theme name.
        /// </summary>
        public void UpdateThemeSelection(string themeName)
        {
            currentThemeName = themeName;
        }

        /// <summary>
        /// Validate a progressive hit specified by a progressive award.
        /// Fill in the hit state, amount and prize string fields of the
        /// progressive award accordingly.
        /// </summary>
        /// <param name="progressiveAward">The progressive award to validate.</param>
        public void ValidateProgressiveHit(FeatureProgressiveAward progressiveAward)
        {
            if(currentProgressiveLinks != null && progressiveAward.GameLevel.HasValue)
            {
                var progressiveLink = currentProgressiveLinks.FirstOrDefault(link => link.GameLevel == progressiveAward.GameLevel);

                // The progressive link is found.
                if(progressiveLink != null)
                {
                    var progressiveController = progressiveControllers[progressiveLink.ControllerName];
                    var progressiveAmount = progressiveController.GetProgressiveAmount(progressiveLink.GameLevel);

                    // If the consolation amount is greater than the current progressive amount,
                    // then set the hit state to NotHit.
                    // This will prevent the lower progressive amount from being awarded.
                    // The consolation amount will be awarded instead, and metered as a payvar win.
                    if(progressiveAward.ConsolationAmountValue > progressiveAmount)
                    {
                        progressiveAward.UpdateAmountValue(0);
                        progressiveAward.UpdateHitState(ProgressiveAwardHitState.NotHit);
                        return;
                    }

                    progressiveController.ValidateProgressiveHit(progressiveAward);
                    return;
                }

                // Check to see if the game level belongs to the GCP controller.
                if(gameControlledProgressiveSetups == null ||
                   !(gameControlledProgressiveSetups.ContainsKey(currentPaytableVariant) &&
                     gameControlledProgressiveSetups[currentPaytableVariant]
                         .Any(link => link.GameLevel == progressiveAward.GameLevel)))
                {
                    progressiveAward.UpdateAmountValue(0);
                    progressiveAward.UpdateHitState(ProgressiveAwardHitState.NotHit);
                }
            }
        }

        /// <summary>
        /// Reset the progressive levels that have been hit.
        /// </summary>
        /// <remarks>
        /// The displayable amount will be reset to the starting amount
        /// plus amount escrowed in the overflow meter, up to the
        /// maximum amount.
        /// </remarks>
        public void ResetProgressiveHits()
        {
            // Get the list of the linked controllers.
            var controllerNames = GetLinkedControllerNames();

            // Have controllers reset its controller levels that have been marked hit.
            foreach(var controllerName in controllerNames)
            {
                progressiveControllers[controllerName].ResetProgressiveHits();
            }
        }

        /// <summary>
        /// Get the progressive broadcast data of all linked game levels.
        /// </summary>
        /// <returns>A list of game levels and their progressive broadcast data, keyed by the game level.</returns>
        public IDictionary<int, ProgressiveBroadcastData> GetAllProgressiveBroadcastData()
        {
            var result = new Dictionary<int, ProgressiveBroadcastData>();

            // Merge the broadcast data lists from linked controllers.
            foreach(var controllerName in GetLinkedControllerNames())
            {
                var broadcastDataList = progressiveControllers[controllerName].GetAllProgressiveBroadcastData();

                result = result.Concat(broadcastDataList).ToDictionary(x => x.Key, y => y.Value);
            }

            return result;
        }

        /// <summary>
        /// Get the progressive broadcast data for a given denomination.
        /// </summary>
        /// <param name="denomination">
        /// The denomination to get the progressive data for.
        /// </param>
        /// <param name="paytableVariant">
        /// The paytable information associated with the denomination.
        /// </param>
        /// <returns>
        /// A dictionary of the progressive data for the denomination.
        /// If the dictionary is empty, it means there are no progressives for the specified
        /// denomination.
        /// </returns>
        public IDictionary<int, ProgressiveBroadcastData>
            GetProgressiveBroadcastDataForDenominationAndPaytable(long denomination, PaytableVariant paytableVariant)
        {
            var broadcastData = new Dictionary<int, ProgressiveBroadcastData>();

            var normalizePaytableVariant = NormalizePaytableVariant(paytableVariant);

            var links = dynamicProgressiveSetups.Where(setup => setup.Key.Key == denomination &&
                                                                setup.Key.Value == normalizePaytableVariant)
                                                .Select(setup => setup.Value)
                                                .SelectMany(link => link);

            if(hardCodedProgressiveSetups != null)
            {
                var hardLinks = hardCodedProgressiveSetups.Where(setup => setup.Key == normalizePaytableVariant)
                                                          .Select(setup => setup.Value)
                                                          .SelectMany(link => link);
                links = links.Union(hardLinks);
            }

            foreach(var link in links)
            {
                var controller = progressiveControllers[link.ControllerName];

                broadcastData.Add(link.GameLevel,
                                  controller.GetProgressiveBroadcastDataForControllerLevel(link.ControllerLevel));
            }

            return broadcastData;
        }

        /// <summary>
        /// Determine if the specified paytable has GCP.
        /// </summary>
        /// <param name="paytableVariant">The paytable information.</param>
        /// <returns><b>true</b> if the GCP exists, <b>false</b> otherwise.</returns>
        public bool HasGameControlledProgressive(PaytableVariant paytableVariant)
        {
            var normalizePaytableVariant = NormalizePaytableVariant(paytableVariant);

            return gameControlledProgressiveSetups?.ContainsKey(normalizePaytableVariant) == true && gameControlledProgressiveSetups[normalizePaytableVariant].Count > 0;
        }

        #endregion

        #region IStandaloneProgressiveManagerDependency

        /// <inheritdoc />
        public IDictionary<int, IProgressiveSettings> GetCurrentLinkedProgressiveSettings()
        {
            IDictionary<int, IProgressiveSettings> linkedSettings = new Dictionary<int, IProgressiveSettings>();
            foreach(var currentLink in currentProgressiveLinks)
            {
                var controller = progressiveControllers[currentLink.ControllerName];
                var allConfigurations = controller.GetAllProgressiveConfigurations().Select(keyValue => keyValue.Value);
                var configuration = allConfigurations.FirstOrDefault(setting => setting.LevelId == currentLink.ControllerLevel);
                var controllerLevelSettings = configuration != default(ProgressiveConfiguration)
                    ? new ProgressiveSettings
                    {
                        ContributionPercentage = Convert.ToDecimal(configuration.ContributionPercentage),
                        StartAmount = configuration.StartingAmount,
                        MaxAmount = configuration.MaximumAmount
                    }
                    : null;
                linkedSettings[currentLink.GameLevel] = controllerLevelSettings;
            }
            return linkedSettings;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<int, IProgressiveBroadcastData> GetIProgressiveBroadcastData()
        {
            return GetAllProgressiveBroadcastData().ToDictionary(kvp => kvp.Key, kvp => (IProgressiveBroadcastData)kvp.Value);
        }

        /// <inheritdoc />
        public void AddEventBasedContribution(int gameLevel, long amountNumerator, long amountDenominator = 1)
        {
            // This is locked in case of additional threads contributing to event based progressive levels.
            lock(progressiveContributionLocker)
            {
                var progressiveLink = currentProgressiveLinks?.FirstOrDefault(link => link.GameLevel == gameLevel);

                // The progressive link is found.
                if(progressiveLink != null)
                {
                    var progressiveController = progressiveControllers[progressiveLink.ControllerName];
                    progressiveController.ContributeToEventBasedProgressive(gameLevel, amountNumerator,
                        amountDenominator);
                }
            }
        }

        /// <inheritdoc/>
        public void ContributeToAllEventBasedProgressives(long bet, long denomination, bool saveToCriticalData)
        {
            // This is locked in case of additional threads contributing to progressive levels.
            lock(progressiveContributionLocker)
            {
                // Have linked controllers apply the contribution to its levels.
                foreach(var controllerName in GetLinkedControllerNames())
                {
                    progressiveControllers[controllerName].ContributeToAllEventBasedProgressives(bet, denomination, saveToCriticalData);
                }
            }
        }

        /// <inheritdoc/>
        public void ContributeToAllProgressives(long bet, long denomination, bool saveToCriticalData)
        {
            // This is locked in case of additional threads contributing to progressive levels.
            lock(progressiveContributionLocker)
            {
                // Have linked controllers apply the contribution to its linked controller levels.
                foreach(var controllerName in GetLinkedControllerNames())
                {
                    progressiveControllers[controllerName].ContributeToAllProgressives(bet, denomination, saveToCriticalData);
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Validate the theme names appeared in the progressive setups.
        /// </summary>
        /// <param name="registryThemeList">
        /// The list of theme names specified in the theme registries.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when there are invalid theme names in the progressive setups.
        /// </exception>
        private void ValidateThemeNames(IList<string> registryThemeList)
        {
            if(registryThemeList != null)
            {
                // Make sure that the theme names appeared in the dynamic
                // progressive setups can be found in the game registries.
                if(dynamicProgressiveSetups != null)
                {
                    foreach(var dynamicProgressiveSetup in dynamicProgressiveSetups)
                    {
                        var paytableVariant = dynamicProgressiveSetup.Key.Value;

                        if(!registryThemeList.Contains(paytableVariant.ThemeIdentifier))
                        {
                            throw new InvalidStreamDataException(
                                $"Theme \"{paytableVariant.ThemeIdentifier}\" is defined in the dynamic progressive setups, but cannot be found in game registries.");
                        }
                    }
                }

                // The hard coded progressive setups came from game registries,
                // thus no need to validate the theme names.
            }
        }

        /// <summary>
        /// Validate the progressive setups.
        /// </summary>
        private void ValidateProgressiveSetups()
        {
            var controllerLevelCounts = progressiveControllers.ToDictionary(x => x.Key, y => y.Value.ControllerLevelCount);

            // Validate dynamic progressive links.
            if(dynamicProgressiveSetups != null)
            {
                foreach(var dynamicProgressiveSetup in dynamicProgressiveSetups)
                {
                    ValidateProgressiveLinks(controllerLevelCounts, dynamicProgressiveSetup.Value);
                }
            }

            // Validate hard coded progressive links.
            if(hardCodedProgressiveSetups != null)
            {
                foreach(var hardCodedProgressiveSetup in hardCodedProgressiveSetups)
                {
                    ValidateProgressiveLinks(controllerLevelCounts, hardCodedProgressiveSetup.Value);
                }
            }

            // Cross check two list of setups.
            CrossCheckProgressiveSetups();
        }

        /// <summary>
        /// Validate that each and every link in the given progressive link list is linked
        /// to a valid controller level as described in the given list of controller level counts.
        /// </summary>
        /// <param name="controllerLevelCounts">
        /// The list of controller level counts, keyed by the controller names.
        /// </param>
        /// <param name="progressiveLinks">
        /// The list of progressive links to be validated.
        /// </param>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when there are errors in the progressive links.
        /// </exception>
        private static void ValidateProgressiveLinks(IDictionary<string, int> controllerLevelCounts,
                                                     IList<ProgressiveLink> progressiveLinks)
        {
            // Check if any progressive link has invalid controller name or controller level.
            var invalidLinks = from progressiveLink in progressiveLinks
                               where !controllerLevelCounts.ContainsKey(progressiveLink.ControllerName) ||
                                     progressiveLink.ControllerLevel >=
                                     controllerLevelCounts[progressiveLink.ControllerName]
                               select progressiveLink;

            var invalidLinksList = invalidLinks as IList<ProgressiveLink> ?? invalidLinks.ToList();
            if(invalidLinksList.Any())
            {
                var invalidLink = invalidLinksList.First();

                throw new InvalidStreamDataException(
                    $"Game level {invalidLink.GameLevel} is linked to a non existent {invalidLink.ControllerName} controller level {invalidLink.ControllerLevel}.");
            }
        }

        /// <summary>
        /// Validate that the dynamic and hard coded progressive setups don't link
        /// the same game level.
        /// </summary>
        /// <exception cref="InvalidStreamDataException">
        /// Thrown when there are conflicting links in the dynamic and hard coded
        /// progressive setups.
        /// </exception>
        private void CrossCheckProgressiveSetups()
        {
            if(dynamicProgressiveSetups != null && hardCodedProgressiveSetups != null)
            {
                foreach(var dynamicProgressiveSetup in dynamicProgressiveSetups)
                {
                    var paytableVariant = dynamicProgressiveSetup.Key.Value;

                    var dynamicLinks = dynamicProgressiveSetup.Value;
                    var hardCodeLinks = hardCodedProgressiveSetups.ContainsKey(paytableVariant)
                                            ? hardCodedProgressiveSetups[paytableVariant]
                                            : null;

                    if(hardCodeLinks != null)
                    {
                        var invalidLinks = from dynamicLink in dynamicLinks
                                           join hardCodedLink in hardCodeLinks
                                               on dynamicLink.GameLevel equals hardCodedLink.GameLevel
                                           select dynamicLink.GameLevel;

                        var invalidLinksList = invalidLinks as IList<int> ?? invalidLinks.ToList();
                        if(invalidLinksList.Any())
                        {
                            throw new InvalidStreamDataException(
                                $"Game level {invalidLinksList.First()} is linked to both a dynamic and a hard coded progressive.");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Make sure all the progressive setup keys use the
        /// absolute path of the paytable file names.
        /// </summary>
        /// <remarks>
        /// This is needed because the paytable file name in
        /// theme context events is an absolute path.
        /// </remarks>
        private void NormalizeProgressiveSetupKeys()
        {
            // Normalize keys of dynamic progressive setups.
            if(dynamicProgressiveSetups != null)
            {
                var oldDynamicCollection = dynamicProgressiveSetups;
                dynamicProgressiveSetups =
                    new Dictionary<KeyValuePair<long, PaytableVariant>, List<ProgressiveLink>>(oldDynamicCollection.Count);

                foreach(var setup in oldDynamicCollection)
                {
                    var newKey = new KeyValuePair<long, PaytableVariant>(setup.Key.Key,
                                                                         NormalizePaytableVariant(setup.Key.Value));

                    dynamicProgressiveSetups.Add(newKey, setup.Value);
                }
            }

            // Normalize keys of hard coded progressive setups.
            if(hardCodedProgressiveSetups != null)
            {
                var oldHardCodedCollection = hardCodedProgressiveSetups;
                hardCodedProgressiveSetups =
                    new Dictionary<PaytableVariant, IList<ProgressiveLink>>(oldHardCodedCollection.Count);

                foreach(var setup in oldHardCodedCollection)
                {
                    var newKey = NormalizePaytableVariant(setup.Key);

                    hardCodedProgressiveSetups.Add(newKey, setup.Value);
                }
            }

            if(gameControlledProgressiveSetups != null)
            {
                var oldGameControlledCollection = gameControlledProgressiveSetups;
                gameControlledProgressiveSetups =
                    new Dictionary<PaytableVariant, IList<ProgressiveLink>>(oldGameControlledCollection.Count);

                foreach(var setup in oldGameControlledCollection)
                {
                    var newKey = new PaytableVariant(setup.Key.ThemeIdentifier,
                                                     setup.Key.PaytableName,
                                                     Path.Combine(rootPath, setup.Key.PaytableFileName));
                    gameControlledProgressiveSetups.Add(newKey, setup.Value);
                }
            }
        }

        /// <summary>
        /// Convert the paytable file name in a <see cref="PaytableVariant"/>
        /// to be the absolute file path.
        /// </summary>
        /// <param name="original">The paytable variant to convert.</param>
        /// <returns>The converted paytable variant.</returns>
        private PaytableVariant NormalizePaytableVariant(PaytableVariant original)
        {
            return new PaytableVariant(original.ThemeIdentifier,
                                       original.PaytableName,
                                       Path.Combine(rootPath, original.PaytableFileName));
        }

        /// <summary>
        /// Event handler for ActivateThemeContextEvent.
        /// When in play mode, cache the progressive links
        /// for the current denomination and paytable variant.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            var themeContext = eventArgs.ThemeContext;

            // Only handle the play mode.
            if(themeContext.GameContextMode == GameMode.Play)
            {
                // Load the controller levels' data fields from the critical data.
                // Only need to be done once per power up.
                if(!controllerLevelsLoaded)
                {
                    foreach(var progressiveController in progressiveControllers)
                    {
                        progressiveController.Value.ReloadControllerLevels();
                    }

                    controllerLevelsLoaded = true;
                }

                // Clear all controllers' level mappings upon each activate theme context.
                foreach(var progressiveController in progressiveControllers)
                {
                    progressiveController.Value.ClearLevelMapping();
                }

                currentPaytableVariant = new PaytableVariant(currentThemeName,
                                                             themeContext.PaytableName,
                                                             themeContext.PaytableFileName);

                // Build the key used to look up the dynamic progressive setup.
                var setupKey = new KeyValuePair<long, PaytableVariant>(themeContext.Denomination,
                                                                       currentPaytableVariant);

                // Cache the progressive links for the current theme context.
                // This is the combination of dynamic progressive links and
                // hard coded progressive links.
                currentProgressiveLinks.Clear();

                if(dynamicProgressiveSetups?.ContainsKey(setupKey) == true)
                {
                    currentProgressiveLinks.AddRange(dynamicProgressiveSetups[setupKey]);
                }

                if(hardCodedProgressiveSetups?.ContainsKey(currentPaytableVariant) == true)
                {
                    currentProgressiveLinks.AddRange(hardCodedProgressiveSetups[currentPaytableVariant]);
                }

                // Group the progressive links by the controller names.
                var linkGroupsByControllerNames = from link in currentProgressiveLinks
                                                  group link by link.ControllerName;

                // Update the level mappings of the controllers linked with this theme context.
                foreach(var linkGroup in linkGroupsByControllerNames)
                {
                    var controllerName = linkGroup.Key;
                    var mappingList = new Dictionary<int, int>(linkGroup.Count());

                    foreach(var link in linkGroup)
                    {
                        mappingList.Add(link.GameLevel, link.ControllerLevel);
                    }

                    progressiveControllers[controllerName].SetLevelMapping(mappingList);
                }
            }
        }

        /// <summary>
        /// Get the names of the controllers that are linked to any
        /// game level in the current theme context.
        /// </summary>
        /// <returns>
        /// List of names of the controllers that are currently linked.
        /// </returns>
        private IEnumerable<string> GetLinkedControllerNames()
        {
            IEnumerable<string> result;

            if(currentProgressiveLinks != null)
            {
                // Group the progressive links by the controller names,
                // then get the list of names that have valid links.
                result = from link in currentProgressiveLinks
                         group link by link.ControllerName
                         into linkGroup
                         select linkGroup.Key;
            }
            else
            {
                result = new List<string>();
            }

            return result;
        }

        #endregion

        #endregion
    }
}
