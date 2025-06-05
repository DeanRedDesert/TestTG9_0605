//-----------------------------------------------------------------------
// <copyright file = "MechanicalReelLocalizedTiltHelper.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System.Collections.Generic;
    using Tilts;

    /// <summary>
    /// Helper to handle the posting and clearing of localized tilt objects related to mechanical reels.
    /// </summary>
    public static class MechanicalReelLocalizedTiltHelper
    {
        #region Public const string tilts

        /// <summary>
        /// The tilt key for reel device not found or acquired.
        /// </summary>
        public const string ReelDeviceNotFoundTiltKey = "Reel_device_not_found_or_acquired";

        /// <summary>
        /// The tilt key for reel count insufficient.
        /// </summary>
        public const string ReelCountInsufficientTiltKey = "Reel_count_insufficient";

        /// <summary>
        /// The tilt key for reels in recovery spin mode.
        /// </summary>
        public const string ReelsInRecoveryModeTiltKey = "Reels_in_recovery_mode";

        #endregion
        
        #region Private Data

        /// <summary>
        /// A list of currently support tilts.
        /// </summary>
        private static readonly List<string> SupportedTilts = new List<string>
        {
            ReelDeviceNotFoundTiltKey, 
            ReelCountInsufficientTiltKey,
            ReelsInRecoveryModeTiltKey
        };

        private static IPresentationTiltPosterProxy tiltPoster;
        private static PresentationTiltPosterReflection defaultPoster;

        #endregion

        #region Public methods

        /// <summary>
        /// Set a custom <see cref="IPresentationTiltPosterProxy"/> implementation.
        /// </summary>
        public static void SetTiltPoster(IPresentationTiltPosterProxy proxy)
        {
            tiltPoster = proxy;
        }
        
        #endregion
        
        #region Internal Methods

        /// <summary>
        /// Creates and posts a tilt.
        /// </summary>
        internal static void PostTilt(string tiltKey)
        {
            var poster = GetPoster();
            if(poster.Initialized)
            {
                var gameTilt = GetTilt(tiltKey);
                if(gameTilt != null)
                {
                    poster.PostTilt(tiltKey, gameTilt);
                }
            }
        }

        /// <summary>
        /// Clears an existing tilt.
        /// </summary>
        internal static void ClearTilt(string tiltKey)
        {
            var poster = GetPoster();
            if(poster.Initialized)
            {
                poster.ClearTilt(tiltKey);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets a <see cref="GameTilt"/> containing game-side localized tilt information.
        /// </summary>
        /// <param name="tiltKey">The key of the specific tilt.</param>
        /// <returns>Null if the tiltKey is not supported, a new <see cref="GameTilt"/> object if so.</returns>
        private static GameTilt GetTilt(string tiltKey)
        {
            GameTilt result = null;

            if(SupportedTilts.Contains(tiltKey))
            {
                var titleLocalizations = new List<Localization>();
                var messageLocalizations = new List<Localization>();
                var tiltRequiresUserIntervention = true;

                // These hard coded localization strings are temporary and will be replaced in the future
                // with localized tilt file usage.
                switch(tiltKey)
                {
                    // Reel device is not found.
                    case ReelDeviceNotFoundTiltKey:
                        titleLocalizations.Add(new Localization { Culture = "en-US", Content = "Reel Shelf Not Found or Not Acquired"});
                        messageLocalizations.Add(new Localization { Culture = "en-US", Content = "A required reel shelf was not found or is acquired by another client." });
                        break;

                    // Too few reels available for what the game requires.
                    case ReelCountInsufficientTiltKey:
                        titleLocalizations.Add(new Localization { Culture = "en-US", Content = "Reel Count Insufficient" });
                        messageLocalizations.Add(new Localization { Culture = "en-US", Content = "The count of available reels is less than that required by the game." });

                        break;

                    // Reels are in game-side recovery mode.
                    case ReelsInRecoveryModeTiltKey:
                        titleLocalizations.Add(new Localization { Culture = "en-US", Content = "Reel Tilt" });
                        messageLocalizations.Add(new Localization { Culture = "en-US", Content = "Waiting for Reels to Recover." });
                        tiltRequiresUserIntervention = false;
                        break;
                }

                result = new GameTilt(new GameTiltDefinition
                {
                    Priority = GameTiltDefinitionPriority.High,
                    GamePlayBehavior = GameTiltDefinitionGamePlayBehavior.Blocking,
                    DiscardBehavior = GameTiltDefinitionDiscardBehavior.OnGameTermination,
                    UserInterventionRequired = tiltRequiresUserIntervention,
                    GameControlledProgressiveLinkDown = false,
                    TitleLocalizations = titleLocalizations,
                    MessageLocalizations = messageLocalizations
                });
            }

            return result;
        }

        /// <summary>
        /// Get the tilt poster proxy instance.
        /// </summary>
        private static IPresentationTiltPosterProxy GetPoster()
        {
            return tiltPoster ?? defaultPoster ?? (defaultPoster = new PresentationTiltPosterReflection());
        }
        
        #endregion
    }
}
