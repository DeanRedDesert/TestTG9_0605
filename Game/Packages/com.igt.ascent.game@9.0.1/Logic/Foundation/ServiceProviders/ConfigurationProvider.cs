//-----------------------------------------------------------------------
// <copyright file = "ConfigurationProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;
    using Money;

    /// <summary>
    /// Core Logic Service Provider for Configuration Data. 
    /// Exposes specific configuration items for use in the presentation.
    /// </summary>
    public class ConfigurationProvider : IGameLibEventListener, INotifyAsynchronousProviderChanged
    {
        #region Fields

        private readonly IGameLib gameLib;

        /// <summary>
        /// Default format to display the time of day when foundation doesn't specify one.
        /// </summary>
        private TimeOfDayFormat defaultTimeOfDayFormat = TimeOfDayFormat.TwelveHourWithAmPm;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the default format to display the time of day values when foundation doesn't specify one.
        /// </summary>
        public TimeOfDayFormat DefaultTimeOfDayFormat
        {
            get => defaultTimeOfDayFormat;
            set
            {
                if(value != TimeOfDayFormat.Invalid)
                {
                    defaultTimeOfDayFormat = value;
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Configuration Provider.
        /// </summary>
        /// <param name="gameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameLib"/> is null.</exception>
        public ConfigurationProvider(IGameLib gameLib)
        {
            this.gameLib = gameLib ?? throw new ArgumentNullException(nameof(gameLib), "Parameter cannot be null.");
            gameLib.ThemeSelectionMenuOfferableStatusChangedEvent += ThemeSelectionMenuOfferableStatusChanged;
            gameLib.ActivateThemeContextEvent += HandleActivateThemeContextEvent;
        }

        #endregion

        #region Services for Foundation Owned Configuration Items

        /// <summary>
        /// Provides the current maximum bet maintained by the foundation.
        /// </summary>
        /// <returns>The maximum bet for the current game, in units of credit.</returns>
        [GameService]
        public long MaximumBet()
        {
            return gameLib.GameMaxBet;
        }

        /// <summary>
        /// Provides the format to display the time of day values.
        /// </summary>
        /// <returns>
        /// This property returns the format specified by the Foundation.
        /// If foundation doesn't specify one, it returns the format specified by <see cref="DefaultTimeOfDayFormat"/>.
        /// </returns>
        [GameService]
        public TimeOfDayFormat TimeOfDayFormat
        {
            get
            {
                var result = gameLib.LocalizationInformation.GetTimeOfDayFormat();
                if(result == TimeOfDayFormat.Invalid)
                {
                    result = defaultTimeOfDayFormat;
                }

                return result;
            }
        }

        #endregion

        #region Services for Game Play Configurations

        /// <summary>
        /// Provides the base denomination for the gaming system.
        /// </summary>
        /// <returns>The base denomination for the gaming system.</returns>
        [GameService]
        public long BaseDenomination()
        {
            //TODO: Get base denomination from game lib once the F2L supports it.
            return 1;
        }

        /// <summary>
        /// Provides the game denomination for the current game.
        /// </summary>
        /// <returns>Current game denomination.</returns>
        [AsynchronousGameService]
        public long GameDenomination()
        {
            return gameLib.GameDenomination;
        }

        /// <summary>
        /// Provides the minimum spin time of the base game.
        /// </summary>
        /// <returns>Minimum spin time of the base game.</returns>
        [GameService]
        public int MinimumBaseGameTime()
        {
            //For now environments that do not use a GameLib have no enforced minimum spin time.
            return gameLib.PresentationBehaviorConfigs.MinimumBaseGameTime;
        }

        /// <summary>
        /// Provides the minimum time for a single slot free spin cycle.
        /// </summary>
        /// <returns>Minimum free spin time.</returns>
        [GameService]
        public int MinimumFreeSpinTime()
        {
            //For now environments that do not use a GameLib have no enforced minimum free spin time.
            return gameLib.PresentationBehaviorConfigs.MinimumFreeSpinTime;
        }

        /// <summary>
        /// Provides the requirement of whether a video reels presentation should be displayed for a stepper game.
        /// </summary>
        /// <returns>True if video reels should be displayed. Otherwise, false.</returns>
        [GameService]
        public bool DisplayVideoReelsForStepper()
        {
            return gameLib.PresentationBehaviorConfigs.DisplayVideoReelsForStepper;
        }

        /// <summary>
        /// Provides the settings of Bonus Single Option Auto Advance settings.
        /// </summary>
        /// <returns>
        /// The settings provided by Foundation.  Null if none is available.
        /// The settings could be null if the game is running with older Foundations,
        /// in which case, it is up to the game to make sure that all jurisdictional
        /// requirements are met.
        /// </returns>
        [GameService]
        public BonusSoaaSettings BonusSoaaSettings()
        {
            return gameLib.PresentationBehaviorConfigs.BonusSoaaSettings;
        }

        /// <summary>
        /// Provides the list of denominations available for the player to pick.
        /// </summary>
        /// <returns>List of denominations.
        /// Returns a sorted list of denominations in ascending order if
        /// the game is in Play mode.
        /// Returns an list with the current Game Denom in all other modes.</returns>
        [GameService]
        public ICollection<long> EnabledDenominations()
        {
            return gameLib.GameContextMode == GameMode.Play
                       ? gameLib.GetAvailableDenominations()
                                .OrderBy(denomination => denomination).ToList()
                       : new List<long> { GameDenomination() };
        }

        /// <summary>
        /// Provides the list of denominations with system progressive or GCP linked available for the player to pick.
        /// </summary>
        /// <returns>
        /// Returns a sorted list of denominations with system progressive or GCP linked in ascending order if
        /// the game is in Play mode.
        /// </returns>
        [GameService]
        public ICollection<long> EnabledProgressiveDenominations()
        {
            if(gameLib.GameContextMode == GameMode.Play)
            {
                return gameLib.GetAvailableProgressiveDenominations()
                    .OrderBy(denomination => denomination).ToList();
            }

            return new List<long>();
        }

        /// <summary>
        /// Provides the flag indicating whether there are multiple denominations
        /// available for the player to pick.
        /// </summary>
        /// <returns>True if multiple denominations are available, false otherwise.</returns>
        [GameService]
        public bool MultipleDenominationsAvailable()
        {
            // If we are in Utility, just return false.
            // Get Available Denominations is not allowed to be called in Utility mode.
            if(gameLib.GameContextMode == GameMode.Utility)
            {
                return false;
            }

            return gameLib.GetAvailableDenominations().Count > 1;
        }

        /// <summary>
        /// Provides the credit formatter information for the game.
        /// </summary>
        /// <returns>The credit formatter information.</returns>
        [GameService]
        public CreditFormatter CreditFormatter()
        {
            return gameLib.LocalizationInformation.GetCreditFormatter();
        }

        /// <summary>
        /// Provides the Jurisdiction for the game.
        /// </summary>
        /// <remarks>
        /// IMPORTANT:
        /// 
        /// DO NOT rely on a specific jurisdiction string value to implement a feature,
        /// as the jurisdiction string value is not enumerated, and could change over time.
        /// 
        /// For example, Nevada used to be reported as USDM, but later as 00NV.
        /// 
        /// This API is kept only for the purpose of temporary work-around, when the time-line
        /// of the official support for a feature in Foundation and/or SDK could not meet a game's
        /// specific timetable requirement.  The game should use this jurisdiction string at
        /// its own risks of breaking compatibility with future Foundation and/or SDK.
        /// </remarks>
        /// <returns>The a string for the Jurisdiction.</returns>
        [GameService]
        public string Jurisdiction()
        {
            return gameLib.Jurisdiction;
        }

        /// <summary>
        /// Provides the flag indicating whether there are multiple games
        /// available for the player to pick.
        /// </summary>
        /// <returns>True if multiple games are available, false otherwise.</returns>
        [AsynchronousGameService]
        public bool MultipleGamesAvailable()
        {
            return gameLib.IsThemeSelectionMenuOfferable;
        }

        /// <summary>
        /// Provides the mount point for the game.
        /// </summary>
        /// <returns>Game mount point location.</returns>
        [GameService]
        public string GameMountPoint()
        {
            return gameLib.GameMountPoint;
        }

        #endregion

        #region Services for EGM Configuration Data

        /// <summary>
        /// Provides the top screen advertisement type for the game.
        /// </summary>
        /// <returns>The top screen advertisement type.</returns>
        [GameService]
        public TopScreenGameAdvertisementType TopScreenGameAdvertisement()
        {
            return gameLib.TopScreenGameAdvertisement;
        }

        /// <summary>
        /// Get the flag indicating if the ancillary game feature is enabled. 
        /// </summary>
        [GameService]
        public bool AncillaryGameEnabled()
        {
            return gameLib.AncillaryEnabled;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler for theme selection menu offerable status changed event.
        /// Triggers an asynchronous update.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void ThemeSelectionMenuOfferableStatusChanged(object sender, ThemeSelectionMenuOfferableStatusChangedEventArgs e)
        {
            var handler = AsynchronousProviderChanged;
            handler?.Invoke(this, new AsynchronousProviderChangedEventArgs("MultipleGamesAvailable"));
        }

        /// <summary>
        /// Handler for the Activate ThemeContext Event.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="activateThemeContextEventArgs">The payload of the event.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs activateThemeContextEventArgs)
        {
            var handler = AsynchronousProviderChanged;
            handler?.Invoke(this, new AsynchronousProviderChangedEventArgs("GameDenomination"));
        }

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            if(gameLib != null)
            {
                iGameLib.ThemeSelectionMenuOfferableStatusChangedEvent -= ThemeSelectionMenuOfferableStatusChanged;
                iGameLib.ActivateThemeContextEvent -= HandleActivateThemeContextEvent;
            }
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion
    }
}
