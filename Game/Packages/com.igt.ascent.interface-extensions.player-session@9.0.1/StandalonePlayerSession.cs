//-----------------------------------------------------------------------
// <copyright file = "StandalonePlayerSession.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSession
{
    using System;
    using System.Timers;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the PlayerSession extended interface.
    /// </summary>
    internal class StandalonePlayerSession : IPlayerSession, IInterfaceExtension, IDisposable
    {
        #region Constants

        // Must be less than the delay time in StandalonePlayerSessionParameters.
        // Parameters reset happens after some time has passed since session turns inactive.
        private const int DelayMs = 5000; // 5 seconds

        #endregion

        #region Private Fields

        private readonly IGameModeQuery gameModeQuery;
        private readonly IStandaloneEventPosterDependency eventPoster;
        private readonly IStandalonePlayStatusDependency playStatus;
        private readonly Timer delayTimer;

        private bool gameInProgress;
        private bool moneyOnMachine;
        private bool realtimeActive;

        private volatile bool isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandalonePlayerSession"/>.
        /// </summary>
        public StandalonePlayerSession(ILayeredContextActivationEventsDependency layeredContextActivationEvents,
                                       IGameModeQuery gameModeQuery,
                                       IEventDispatcher transactionalEventDispatcher,
                                       IStandaloneEventPosterDependency eventPoster,
                                       IStandalonePlayStatusDependency playStatus)
        {
            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));
            this.eventPoster = eventPoster ?? throw new ArgumentNullException(nameof(eventPoster));
            this.playStatus = playStatus ?? throw new ArgumentNullException(nameof(playStatus));

            playStatus.GameInProgressStatusEvent += (s, e) =>
                                                        {
                                                            gameInProgress = e.GameInProgress;
                                                            UpdateSessionStatus();
                                                        };

            playStatus.MoneyOnMachineStatusEvent += (s, e) =>
                                                        {
                                                            moneyOnMachine = e.MoneyOnMachine;
                                                            UpdateSessionStatus();
                                                        };

            delayTimer = new Timer
                             {
                                 Interval = DelayMs,
                                 AutoReset = false, // Only raise event once.
                                 Enabled = false,
                             };

            delayTimer.Elapsed += OnDelayElapsed;

            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;
            layeredContextActivationEvents.InactivateLayeredContextEvent += HandleInactivateThemeContextEvent;

            transactionalEventDispatcher.EventDispatchedEvent += (s, e) => e.RaiseWith(this, PlayerSessionStatusChangedEvent);

            PlayerSessionStatus = new PlayerSessionStatus(false, DateTime.Now);
        }

        #endregion

        #region IPlayerSession Implementation

        /// <inheritdoc />
        public event EventHandler<PlayerSessionStatusChangedEventArgs> PlayerSessionStatusChangedEvent;

        /// <inheritdoc/>
        public PlayerSessionStatus PlayerSessionStatus { get; private set; }

        /// <inheritdoc />
        public bool SessionTimerDisplayEnabled => true;

        #endregion

        #region IDisposable

        /// <inheritdoc />
        public void Dispose()
        {
            delayTimer.Stop();

            isDisposed = true;
            delayTimer.Dispose();
        }

        #endregion

        #region Private Methods

        private void HandleActivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme &&
               gameModeQuery.GameMode == GameMode.Play)
            {
                gameInProgress = playStatus.IsGameInProgress();
                moneyOnMachine = playStatus.IsMoneyOnMachine();

                lock(delayTimer)
                {
                    // Initialize player session status.
                    realtimeActive = gameInProgress || moneyOnMachine;
                    PlayerSessionStatus = new PlayerSessionStatus(realtimeActive, DateTime.Now);

                    // No need to start timer.
                    delayTimer.Stop();
                }
            }
        }

        private void HandleInactivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                lock(delayTimer)
                {
                    delayTimer.Stop();

                    realtimeActive = false;
                    PlayerSessionStatus = new PlayerSessionStatus(false, DateTime.Now);
                }
            }
        }

        private void UpdateSessionStatus()
        {
            var newStatus = gameInProgress || moneyOnMachine;

            PlayerSessionStatusChangedEventArgs toPost = null;
            lock(delayTimer)
            {
                if(realtimeActive != newStatus)
                {
                    // Update the realtime flag.
                    realtimeActive = newStatus;

                    if(realtimeActive)
                    {
                        // Inactive to Active takes effect immediately.
                        delayTimer.Stop();

                        // Do not update if session status is till active
                        // (e.g. when a previous delay has not expired yet).
                        if(!PlayerSessionStatus.SessionActive)
                        {
                            PlayerSessionStatus = new PlayerSessionStatus(true, DateTime.Now);
                            toPost = new PlayerSessionStatusChangedEventArgs(PlayerSessionStatus);
                        }
                    }
                    else
                    {
                        // Active to Inactive needs to delay.
                        delayTimer.Start();
                    }
                }
            }

            // It's better practice to post the event outside the lock.
            if(toPost != null)
            {
                eventPoster.PostTransactionalEvent(toPost);
            }
        }

        /// <devdoc>
        /// Must be thread safe.
        /// </devdoc>
        private void OnDelayElapsed(object sender, ElapsedEventArgs e)
        {
            if(isDisposed)
            {
                return;
            }

            PlayerSessionStatusChangedEventArgs toPost = null;

            lock(delayTimer)
            {
                delayTimer.Stop();

                // Make sure things haven't changed during the delay.
                if(!realtimeActive)
                {
                    // Session is now officially Inactive.
                    PlayerSessionStatus = new PlayerSessionStatus(false, DateTime.Now);
                    toPost = new PlayerSessionStatusChangedEventArgs(PlayerSessionStatus);
                }
            }

            if(toPost != null)
            {
                eventPoster.PostTransactionalEvent(toPost);
            }
        }

        #endregion
    }
}
