//-----------------------------------------------------------------------
// <copyright file = "TiltManager.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using Interfaces;
    using TiltControl;
    using Tilts;
    using F2LInternal = F2L.Schemas.Internal;

    /// <summary>
    /// A class that allows for tilts to be posted and cleared, while maintaining the foundation driven tilt requirements.
    /// </summary>
    internal sealed class TiltManager : BaseTiltController, ITiltManagement, IInterfaceExtension
    {
        #region Fields

        #region Interface Extension Dependencies

        private readonly IGameTiltCategory tiltCategoryProvider;
        private readonly ITransactionWeightVerificationDependency transactionWeightVerification;
        private readonly ICultureInfoDependency cultureInfoProvider;
        private readonly ICriticalDataDependency criticalDataProvider;
        private readonly IGameModeQuery gameModeQuery;

        #endregion

        #region Constants

        /// <summary>
        /// The critical data path under which all the tilt management data will be kept.
        /// </summary>
        private const string TiltManagerPrefix = "TiltManagement/";

        /// <summary>
        /// The critical data path where the tilt list will be stored.
        /// </summary>
        private const string TiltCriticalPath = TiltManagerPrefix + "TiltList";

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a TiltManager with it's dependencies.
        /// </summary>
        /// <param name="tiltCategoryProvider">The interface that will be used to post/clear tilts with the Foundation.</param>
        /// <param name="criticalDataProvider">The interface for writing/reading to safe storage.</param>
        /// <param name="transactionWeightVerification">The interface used to verify a transaction exists for the functions that require one.</param>
        /// <param name="layeredContextActivationEvents">The interface that allows the tilt manager to listen to context events.</param>
        /// <param name="cultureInfoProvider">The interface provide game configuration data.</param>
        /// <param name="gameModeQuery">The interface used to query game mode.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any or the parameters, except <paramref name="tiltCategoryProvider"/>, is null.
        /// </exception>
        public TiltManager(IGameTiltCategory tiltCategoryProvider,
                           ICriticalDataDependency criticalDataProvider,
                           ITransactionWeightVerificationDependency transactionWeightVerification,
                           ILayeredContextActivationEventsDependency layeredContextActivationEvents,
                           ICultureInfoDependency cultureInfoProvider,
                           IGameModeQuery gameModeQuery)
        {
            // The tiltCategoryProvider is allowed to be null here.  If the tiltCategoryProvider is null
            // this object has been initialized in a standalone environment.

            this.tiltCategoryProvider = tiltCategoryProvider;
            this.criticalDataProvider = criticalDataProvider ?? throw new ArgumentNullException(nameof(criticalDataProvider));
            this.transactionWeightVerification = transactionWeightVerification ?? throw new ArgumentNullException(nameof(transactionWeightVerification));
            this.cultureInfoProvider = cultureInfoProvider ?? throw new ArgumentNullException(nameof(cultureInfoProvider));
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));

            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }
            layeredContextActivationEvents.ActivateLayeredContextEvent += OnActivateLayeredContextEvent;
            layeredContextActivationEvents.InactivateLayeredContextEvent += OnInactivateLayeredContextEvent;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Clears the tilts list when the theme gets inactivated.
        /// </summary>
        /// <param name="sender">
        /// The sender of the message.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        private void OnInactivateLayeredContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle the F2L theme (which is on link level) activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                TiltInfoList.Clear();
            }
        }

        /// <summary>
        /// Restores the posted tilt(s) when the theme gets activated.
        /// </summary>
        /// <param name="sender">
        /// The sender of the message.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        private void OnActivateLayeredContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle the F2L theme (which is on link level) activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                if(gameModeQuery.GameMode == GameMode.Play && !TiltInfoList.Any())
                {
                    // The reason that we don't clear tilts in OnActivateThemeContextEvent is 
                    // when the game doesn't switch theme (just enter TSM and exit TSM),
                    // it doesn't need to reload tilts from critical data.
                    LoadTilts();
                }
            }
        }

        #endregion

        #region Public Methods

        /// <inheritdoc />
        public bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat)
        {
            // Posting tilts is only allowed in game mode play, so for now we will ignore tilts requests
            // in the other game modes.  It may be possible in the future for persistent tilts to be cached
            // and sent when the context mode comes back to play.
            if(gameModeQuery.GameMode != GameMode.Play)
            {
                return false;
            }

            transactionWeightVerification.MustHaveHeavyweightTransaction();

            var success = base.PostTilt(tilt, key, titleFormat, messageFormat);
            if(success)
            {
                criticalDataProvider.WriteCriticalData(InterfaceExtensionDataScope.Theme, TiltCriticalPath, TiltInfoList);
            }

            return success;
        }

        /// <inheritdoc cref="BaseTiltController"/>
        public override bool ClearTilt(string key)
        {
            // Clearing tilts is only allowed in game mode play, so for now we will ignore tilts requests
            // in the other game modes.
            if(gameModeQuery.GameMode != GameMode.Play)
            {
                return false;
            }

            transactionWeightVerification.MustHaveHeavyweightTransaction();

            var success = base.ClearTilt(key);
            if(success)
            {
                criticalDataProvider.WriteCriticalData(InterfaceExtensionDataScope.Theme, TiltCriticalPath, TiltInfoList);
            }

            return success;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <inheritdoc />
        /// <exception cref="TiltCommunicationException">
        /// Thrown if the tilt category provider cannot handle the post tilt request.
        /// </exception>
        protected override bool SendRequestTilt(TiltInfo registeredTiltInfo)
        {
            registeredTiltInfo.Verify(cultureInfoProvider.AvailableCultures);

            if(tiltCategoryProvider == null)
            {
                return true;
            }

            var registeredTilt = registeredTiltInfo.Tilt;
            var localizationList = (from locale in cultureInfoProvider.AvailableCultures
                                    let unformattedMessage = registeredTilt.GetLocalizedMessage(locale)
                                    let unformattedTitle = registeredTilt.GetLocalizedTitle(locale)
                                    where
                                        !string.IsNullOrEmpty(unformattedMessage) &&
                                        !string.IsNullOrEmpty(unformattedTitle)
                                    select new F2LInternal.TiltLocalization
                                               {
                                                   Culture = locale,
                                                   Message =
                                                       string.Format(unformattedMessage,
                                                                     registeredTiltInfo.MessageFormat),
                                                   Title =
                                                       string.Format(unformattedTitle,
                                                                     registeredTiltInfo.TitleFormat),
                                               }).ToList();

            var success = tiltCategoryProvider.PostTilt(registeredTiltInfo.Key,
                                                        registeredTilt.GamePlayBehavior ==
                                                        TiltGamePlayBehavior.Blocking,
                                                        registeredTilt.UserInterventionRequired,
                                                        registeredTilt.ProgressiveLinkDown,
                                                        localizationList);

            if(!success)
            {
                // If we failed to post the tilt with Foundation, then throw an exception.
                throw new TiltCommunicationException(registeredTiltInfo.Key, TiltCommunicationException.Operation.Post);
            }

            return true;
        }

        /// <inheritdoc />
        protected override bool SendClearTilt(string tiltKey)
        {
            if(tiltCategoryProvider == null)
            {
                return true;
            }

            var success = tiltCategoryProvider.ClearTilt(tiltKey);

            if(!success)
            {
                // If we failed to clear the tilt with Foundation, then throw an exception.
                throw new TiltCommunicationException(tiltKey, TiltCommunicationException.Operation.Clear);
            }

            return true;
        }

        #endregion Protected Methods

        #region TiltList Manipulation

        /// <summary>
        /// Reads the tilts from critical data, filters them, and posts the remaining few
        /// (without going over the RegisteredFoundationTiltTotalCount).
        /// </summary>
        /// <remarks>Marked as internal for testing only.</remarks>
        private void LoadTilts()
        {
            transactionWeightVerification.MustHaveHeavyweightTransaction();

            var tempTilts =
                criticalDataProvider.ReadCriticalData<List<TiltInfo>>(InterfaceExtensionDataScope.Theme, TiltCriticalPath)
                ?? new List<TiltInfo>();

            foreach(var tiltInfo in tempTilts.Where(tiltInfo =>
                                                        {
                                                            var tilt = tiltInfo.Tilt as ITilt;
                                                            return tilt == null || tilt.DiscardBehavior == TiltDiscardBehavior.Never;
                                                        }))
            {
                //Ignoring the return value here, because below will just register the required tilts.
                AddTilt(tiltInfo);
            }

            foreach(var tiltInfo in TiltInfoList.Take(RegisteredFoundationTiltTotalCount))
            {
                SendRequestTilt(tiltInfo);
            }

            criticalDataProvider.WriteCriticalData(InterfaceExtensionDataScope.Theme, TiltCriticalPath, TiltInfoList);
        }

        #endregion
    }
}