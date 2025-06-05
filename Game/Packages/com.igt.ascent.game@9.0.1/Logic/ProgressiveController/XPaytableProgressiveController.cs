//-----------------------------------------------------------------------
// <copyright file = "XPaytableProgressiveController.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.ProgressiveController
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Foundation;
    using Evaluator.Schemas;
    using MappingList = System.Collections.Generic.Dictionary<int, int>;
    using PaytableVariant = System.Collections.Generic.KeyValuePair<string, string>;

    /// <summary>
    /// The very basic game controlled progressive (GCP) controller that can be used as a base
    /// for expanded GCP features. Intended only for games using XPaytable.
    /// </summary>
    /// <remarks>
    /// GCP supports the paytable sliding within a theme, but not the
    /// theme switching within a game executable package.  The reason
    /// is that all the controller information is kept as the theme
    /// scope configuration items, which cannot be shared or validated
    /// across multiple themes.
    /// </remarks>
    public class XPaytableProgressiveController : BaseProgressiveController,
                                                  IProgressiveControllerRestricted,
                                                  IGameLibEventListener
    {
        #region Constants

        /// <summary>
        /// The directory where all paytable files are located.
        /// </summary>
        public const string PaytableDirectory = "Paytables";

        /// <summary>
        /// File name pattern used to search for paytables.
        /// </summary>
        public const string PaytableFilePattern = "*.xpaytable";

        #region Configuration Names

        /// <summary>
        /// Formatting string for the configuration name of
        /// "[ControllerName] Controller Level Count".
        /// </summary>
        protected const string ConfigNameControllerLevelCount = "{0} Controller Level Count";
        /// <summary>
        /// Formatting string for the configuration name of
        /// "[ControllerName] Controller Level X Starting Amount".
        /// </summary>
        protected const string ConfigNameLevelStartingAmount = "{0} Controller Level {1} Starting Amount";
        /// <summary>
        /// Formatting string for the configuration name of
        /// "[ControllerName] Controller Level X Maximum Amount".
        /// </summary>
        protected const string ConfigNameLevelMaximumAmount = "{0} Controller Level {1} Maximum Amount";
        /// <summary>
        /// Formatting string for the configuration name of
        /// "[ControllerName] Controller Level X Contribution Percentage".
        /// </summary>
        protected const string ConfigNameLevelContributionPercentage = "{0} Controller Level {1} Contribution Percentage";
        /// <summary>
        /// Formatting string for the configuration name of
        /// "[ControllerName] Controller Level X Prize String".
        /// </summary>
        protected const string ConfigNameLevelPrizeString = "{0} Controller Level {1} Prize String";

        #endregion

        #endregion

        #region Fields

        /// <summary>
        /// Progressive link setups retrieved from the paytables,
        /// keyed by the combination of paytable and paytable file
        /// names.
        /// </summary>
        protected Dictionary<PaytableVariant, MappingList> ProgressiveSetups;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize a new instance of <see cref="XPaytableProgressiveController"/>
        /// with a name and a game lib that is used to access critical data.
        /// </summary>
        /// <param name="iGameLib">
        /// Reference to a game lib needed for critical data operations.
        /// </param>
        /// <param name="name">
        /// Name of the progressive controller.  It must conform to the
        /// requirement of a critical data path, since the name is used
        /// as the critical data path for this controller.
        /// The default value is XPaytableProgressiveController.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> is null or
        /// <paramref name="name"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> contains illegal characters.
        /// </exception>
        /// <seealso cref="IGT.Game.Core.Communication.Foundation.Utility.ValidateCriticalDataName"/>
        public XPaytableProgressiveController(IGameLib iGameLib, string name = "XPaytableProgressiveController")
                  : base(iGameLib, name)
        {
            GameLibReference.ActivateThemeContextEvent += HandleActivateThemeContextEvent;

            // Initialize to empty collections to avoid null exception.
            ControllerLevels = new List<ControllerLevel>();

            // Search for paytables where GCP progressive links are set up.
            var paytablePath = Path.Combine(GameLibReference.GameMountPoint, PaytableDirectory);
            var paytables = DiscoverPaytables(paytablePath);

            // Retrieve the hard coded progressive link setups from the registries.
            ProgressiveSetups = RetrieveProgressiveSetups(paytables);
        }

        #endregion

        #region AbstractProgressiveController Overrides

        /// <inheritdoc />
        public override event EventHandler<ProgressiveBroadcastEventArgs> ProgressiveBroadcastEvent;

        /// <inheritdoc />
        public override void ContributeToProgressive(int gameLevel, long bet, long denomination)
        {
            base.ContributeToProgressive(gameLevel, bet, denomination);

            BroadcastProgressiveData();
        }

        /// <inheritdoc />
        public override void ContributeToAllProgressives(long bet, long denomination, bool saveToCriticalData)
        {
            base.ContributeToAllProgressives(bet, denomination, saveToCriticalData);

            BroadcastProgressiveData();
        }

        /// <inheritdoc />
        public override void ResetProgressiveHits()
        {
            base.ResetProgressiveHits();

            BroadcastProgressiveData();
        }

        #endregion

        #region IProgressiveControllerRestricted Members

        /// <inheritdoc />
        public virtual void ClearEventSubscriptions()
        {
            ProgressiveBroadcastEvent = null;
        }

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public virtual void UnregisterGameLibEvents(IGameLib gameLib)
        {
            GameLibReference.ActivateThemeContextEvent -= HandleActivateThemeContextEvent;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Search and load the paytables in a specified path.
        /// </summary>
        /// <param name="paytablePath">The path to search for paytable files.</param>
        /// <returns>The list of paytables loaded.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no paytable file is found.
        /// </exception>
        /// <DevDoc>
        /// Assumptions:
        /// 1. There is only one paytable per paytable file.
        /// 2. No duplicate paytable file names.
        /// </DevDoc>
        protected static IDictionary<string, Paytable> DiscoverPaytables(string paytablePath)
        {
            var paytableFiles = Directory.GetFiles(paytablePath, PaytableFilePattern);

            if(paytableFiles.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No paytable file is found in {paytablePath}.");
            }

            var result = new Dictionary<string, Paytable>(paytableFiles.Length);

            foreach(var paytableFile in paytableFiles)
            {
                var paytable = Paytable.Load(paytableFile);
                result.Add(Path.GetFullPath(paytableFile), paytable);
            }

            return result;
        }

        /// <summary>
        /// Retrieve the hard coded GCP progressive setups from the
        /// specified list of paytables.
        /// </summary>
        /// <param name="paytables">
        /// A list of paytables, keyed by the paytable file names.
        /// </param>
        /// <returns>
        /// List of level mappings for GCP progressives for each payvar,
        /// keyed by the pair of paytable name and paytable file name.
        /// </returns>
        /// <exception cref="PaytableException">
        /// Thrown when the paytable contain invalid data.
        /// </exception>
        protected static Dictionary<PaytableVariant, MappingList> RetrieveProgressiveSetups(IDictionary<string, Paytable> paytables)
        {
            var result = new Dictionary<PaytableVariant, MappingList>(paytables.Count);

            foreach(var paytableEntry in paytables)
            {
                if(paytableEntry.Value.ProgressiveLevels != null)
                {
                    var progressiveLevelList = paytableEntry.Value.ProgressiveLevels.ProgressiveLevel;

                    var setupKey = new PaytableVariant(paytableEntry.Value.Abstract.gameID,
                                                       paytableEntry.Key);

                    var setupValue = new MappingList(progressiveLevelList.Count);

                    foreach(var progressiveLevel in progressiveLevelList)
                    {
                        // Skip if no progressive link is defined, or it is not for GCP.
                        if(progressiveLevel.ControllerType != ProgressiveControllerTypes.GCP)
                        {
                            continue;
                        }

                        // Verify the controller level is present for linking.
                        if(!progressiveLevel.ControllerLevelSpecified)
                        {
                            throw new PaytableException(
                                "ProgressiveLevels",
                                $"Missing controller level for GCP link set up in paytable {paytableEntry.Key}.");
                        }

                        setupValue.Add((int)progressiveLevel.Level, (int)progressiveLevel.ControllerLevel);
                    }

                    result.Add(setupKey, setupValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Event handler for ActivateThemeContextEvent.
        /// Cache the progressive configurations and
        /// reload progressive amounts and hit records
        /// upon the activation of a new theme context.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        /// <exception cref="InvalidProgressiveConfigException">
        /// Thrown when the configurations of the game controlled
        /// progressives contain invalid data.
        /// </exception>
        protected virtual void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            var themeContext = eventArgs.ThemeContext;

            // Update progressive settings only in play mode.
            if(themeContext.GameContextMode == GameMode.Play)
            {
                // Only check the number of the controller levels once.
                // The controller level count is not supposed to be modifiable.
                if(ControllerLevels.Count == 0)
                {
                    CreateControllerLevels();
                }

                // Reload the level configurations and data.
                ReloadControllerLevels();

                // Cache the level mapping for the current theme context.
                var setupKey = new PaytableVariant(themeContext.PaytableName,
                                                   Path.GetFullPath(themeContext.PaytableFileName));

                LevelMapping = ProgressiveSetups.ContainsKey(setupKey)
                                   ? ProgressiveSetups[setupKey]
                                   : new MappingList();

                // Post ProgressiveBroadcastEvent.
                BroadcastProgressiveData();
            }
        }

        /// <summary>
        /// Instantiate the controller levels according to the controller level count
        /// defined in the game configurations.
        /// </summary>
        private void CreateControllerLevels()
        {
            var configKey =
                GameConfigurationKey.NewThemeKey(string.Format(ConfigNameControllerLevelCount, ControllerName));
            var levelCount = GameLibReference.ConfigurationRead.IsConfigurationDefined(configKey)
                ? (int)GameLibReference.ConfigurationRead.GetConfiguration<long>(configKey,
                    ConfigurationItemType.Int64)
                : 0;

            if(levelCount <= 0)
            {
                throw new InvalidProgressiveConfigException(
                    "The controller level count defined in the configuration must be greater than 0.");
            }

            ControllerLevels.Clear();
            ControllerLevels.Capacity = levelCount;

            for(var levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                ControllerLevels.Add(new ControllerLevel(levelIndex, GameLibReference, ControllerName));
            }
        }

        /// <summary>
        /// Reload the configuration and data from the critical data for
        /// all the controller levels.
        /// </summary>
        private void ReloadControllerLevels()
        {
            // Read level configurations from the critical data for each controller level.
            // The configurations can be changed via operator menu after game is up and played.
            for(var level = 0; level < ControllerLevels.Count; level++)
            {
                var configName = string.Format(ConfigNameLevelStartingAmount, ControllerName, level);
                var configKey = GameConfigurationKey.NewThemeKey(configName);
                var startingAmount = GameLibReference.ConfigurationRead
                    .GetConfiguration<long>(configKey, ConfigurationItemType.Int64);

                configName = string.Format(ConfigNameLevelMaximumAmount, ControllerName, level);
                configKey = GameConfigurationKey.NewThemeKey(configName);
                var maximumAmount = GameLibReference.ConfigurationRead
                    .GetConfiguration<long>(configKey, ConfigurationItemType.Int64);

                configName = string.Format(ConfigNameLevelContributionPercentage, ControllerName, level);
                configKey = GameConfigurationKey.NewThemeKey(configName);
                var contributionPercentage = GameLibReference.ConfigurationRead
                    .GetConfiguration<float>(configKey, ConfigurationItemType.Float);

                configName = string.Format(ConfigNameLevelPrizeString, ControllerName, level);
                configKey = GameConfigurationKey.NewThemeKey(configName);
                var prizeString = GameLibReference.ConfigurationRead
                    .GetConfiguration<string>(configKey, ConfigurationItemType.String);

                ControllerLevels[level].Configuration = new ProgressiveConfiguration(level,
                                                                                     startingAmount,
                                                                                     maximumAmount,
                                                                                     contributionPercentage,
                                                                                     prizeString);
            }

            // Reload level data from the critical data for each controller level.
            ControllerLevels.ForEach(controllerLevel => controllerLevel.Reload());
        }

        /// <summary>
        /// Post the ProgressiveBroadcastEvent to broadcast the updated
        /// displayable progressive amounts and prize strings.
        /// </summary>
        protected virtual void BroadcastProgressiveData()
        {
            // Create a temporary copy of the event handler for thread safety.
            var handler = ProgressiveBroadcastEvent;

            // Post the broadcast event.
            handler?.Invoke(this, new ProgressiveBroadcastEventArgs(GetAllProgressiveBroadcastData()));
        }

        #endregion
    }
}
