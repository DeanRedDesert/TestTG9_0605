//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpPid.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpPid extended interface.
    /// </summary>
    internal sealed class StandaloneUgpPid : IUgpPid, IStandaloneHelperUgpPid, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The current Pid configuration cached in standalone UGP PID extended interface.
        /// </summary>
        private readonly PidConfiguration currentPidConfiguration = new PidConfiguration
        {
            IsMainEntryEnabled = true,
            IsRequestServiceEnabled = true,
            GameInformationDisplayStyle = GameInformationDisplayStyleEnum.Victoria.ToPublic(),
            SessionTrackingOption = SessionTrackingOptionEnum.PlayerControlled.ToPublic(),
            IsGameRulesEnabled = true,
            InformationMenuTimeout = new TimeSpan(0, 0, 30),
            SessionStartMessageTimeout = new TimeSpan(0, 0, 30),
            ViewSessionScreenTimeout = new TimeSpan(0, 0, 30),
            ViewGameInformationTimeout = new TimeSpan(0, 0, 30),
            ViewGameRulesTimeout = new TimeSpan(0, 0, 30),
            ViewPayTableTimeout = new TimeSpan(0, 0, 30),
            SessionTimeoutInterval = new TimeSpan(0, 0, 30),
            SessionTimeoutStartOnZeroCredits = true,
            TotalNumberLinkEnrolments = 0,
            TotalLinkPercentageContributions = "",
            ShowLinkJackpotCount = true,
            LinkRtpForGameRtp = 1.0
        };

        /// <summary>
        /// The current PID session data.
        /// </summary>
        private readonly PidSessionData currentSessionData;

        /// <summary>
        /// The interface for posting foundation events in the main event queue.
        /// </summary>
        private readonly IStandaloneEventPosterDependency eventPoster;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpPid"/>.
        /// </summary>
        /// <param name="eventPosterInterface">
        /// The interface for processing and posting foundation events in the main event queue.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public StandaloneUgpPid(IStandaloneEventPosterDependency eventPosterInterface,
                                IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            eventPoster = eventPosterInterface ?? throw new ArgumentNullException(nameof(eventPosterInterface));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, PidActivated);
            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, IsServiceRequestedChanged);
            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, PidConfigurationChanged);

            currentSessionData = new PidSessionData();
        }

        #endregion

        #region IUgpPid Implementation

        /// <inheritdoc/>
        public event EventHandler<PidActivationEventArgs> PidActivated;

        /// <inheritdoc/>
        public event EventHandler<PidServiceRequestedChangedEventArgs> IsServiceRequestedChanged;

        /// <inheritdoc/>
        public event EventHandler<PidConfigurationChangedEventArgs> PidConfigurationChanged;

        /// <inheritdoc/>
        public bool IsServiceRequested { get; private set; }

        /// <inheritdoc/>
        public void StartTracking()
        {
            currentSessionData.IsSessionTrackingActive = true;
            currentSessionData.SessionStarted = DateTime.Now;
        }

        /// <inheritdoc/>
        public void StopTracking()
        {
            currentSessionData.IsSessionTrackingActive = false;
        }

        /// <inheritdoc/>
        public PidSessionData GetSessionData()
        {
            return currentSessionData;
        }

        /// <inheritdoc/>
        public PidConfiguration GetPidConfiguration()
        {
            return currentPidConfiguration;
        }

        /// <inheritdoc/>
        public void ActivationStatusChanged(bool currentStatus)
        {
        }

        /// <inheritdoc/>
        public void GameInformationScreenEntered()
        {
        }

        /// <inheritdoc/>
        public void SessionInformationScreenEntered()
        {
        }

        /// <inheritdoc/>
        public void AttendantServiceRequested()
        {
            IsServiceRequested = !IsServiceRequested;
            currentPidConfiguration.IsRequestServiceActivated = IsServiceRequested;
            var pidServiceRequestedChangedEventArgs = new PidServiceRequestedChangedEventArgs
                                                            { IsServiceRequested = IsServiceRequested };

            eventPoster.PostTransactionalEvent(pidServiceRequestedChangedEventArgs);
        }

        /// <inheritdoc/>
        public void RequestForcePayout()
        {
        }

        #endregion

        #region IStandaloneHelperUgpPid Implementation

        /// <inheritdoc/>
        public void SetPidConfiguration(PidConfiguration configuration)
        {
            if(configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            currentPidConfiguration.IsMainEntryEnabled = configuration.IsMainEntryEnabled;
            currentPidConfiguration.IsRequestServiceEnabled = configuration.IsRequestServiceEnabled;
            currentPidConfiguration.GameInformationDisplayStyle = configuration.GameInformationDisplayStyle;
            currentPidConfiguration.SessionTrackingOption = configuration.SessionTrackingOption;
            currentPidConfiguration.IsGameRulesEnabled = configuration.IsGameRulesEnabled;
            currentPidConfiguration.InformationMenuTimeout = configuration.InformationMenuTimeout;
            currentPidConfiguration.SessionStartMessageTimeout = configuration.SessionStartMessageTimeout;
            currentPidConfiguration.ViewSessionScreenTimeout = configuration.ViewSessionScreenTimeout;
            currentPidConfiguration.ViewGameInformationTimeout = configuration.ViewGameInformationTimeout;
            currentPidConfiguration.ViewGameRulesTimeout = configuration.ViewGameRulesTimeout;
            currentPidConfiguration.ViewPayTableTimeout = configuration.ViewPayTableTimeout;
            currentPidConfiguration.SessionTimeoutInterval = configuration.SessionTimeoutInterval;
            currentPidConfiguration.SessionTimeoutStartOnZeroCredits = configuration.SessionTimeoutStartOnZeroCredits;
            currentPidConfiguration.TotalNumberLinkEnrolments = configuration.TotalNumberLinkEnrolments;
            currentPidConfiguration.TotalLinkPercentageContributions = configuration.TotalLinkPercentageContributions;
            currentPidConfiguration.ShowLinkJackpotCount = configuration.ShowLinkJackpotCount;
            currentPidConfiguration.LinkRtpForGameRtp = configuration.LinkRtpForGameRtp;

            eventPoster.PostTransactionalEvent(new PidConfigurationChangedEventArgs());
        }

        #endregion
    }
}