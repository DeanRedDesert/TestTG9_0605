// -----------------------------------------------------------------------
// <copyright file = "StandalonePlayerSessionParameters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Timers;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended interface of Player Session Parameters
    /// that is to be used in standalone configurations.
    /// </summary>
    /// <remarks>
    /// This standalone implementation does NOT support power hit.  That is,
    /// if a parameter has been reset before power hit, when it is time to reset after power hit recovery,
    /// the parameter will appear in the list of pending parameters to reset again.
    /// </remarks>
    internal class StandalonePlayerSessionParameters : IPlayerSessionParameters, IInterfaceExtension, IDisposable
    {
        #region Constants

        // Must be greater than the delay time in StandalonePlayerSession.
        // Parameters reset happens after some time has passed since session turns inactive.
        private const int DelayMs = 10000; // 10 seconds

        // Serving game clients only... at the moment.
        private static readonly PlayerSessionParameterType[] ExcludedParameters = new[] { PlayerSessionParameterType.ChooserSpecific };

        #endregion

        #region Private Fields

        private readonly IGameModeQuery gameModeQuery;
        private readonly IStandaloneEventPosterDependency eventPoster;
        private readonly IStandalonePlayStatusDependency playStatus;
        private readonly Timer delayTimer;

        private readonly List<PlayerSessionParameterType> allParameters;
        private readonly List<PlayerSessionParameterType> pendingParametersToReset;

        private bool gameInProgress;
        private bool moneyOnMachine;
        private bool sessionActive;

        private volatile bool isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandalonePlayerSessionParameters"/>.
        /// </summary>
        public StandalonePlayerSessionParameters(ILayeredContextActivationEventsDependency layeredContextActivationEvents,
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

            allParameters = Enum.GetValues(typeof(PlayerSessionParameterType))
                                .Cast<PlayerSessionParameterType>()
                                .Except(ExcludedParameters)
                                .ToList();

            pendingParametersToReset = new List<PlayerSessionParameterType>();

            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;
            layeredContextActivationEvents.InactivateLayeredContextEvent += HandleInactivateThemeContextEvent;

            transactionalEventDispatcher.EventDispatchedEvent += (s, e) => e.RaiseWith(this, CurrentResetParametersChangedEvent);
        }

        #endregion

        #region IPlayerSessionParameters Members

        /// <inheritdoc />
        public event EventHandler<CurrentResetParametersChangedEventArgs> CurrentResetParametersChangedEvent;

        /// <inheritdoc/>
        public bool IsPlayerSessionParameterResetEnabled => true;

        /// <inheritdoc/>
        public IList<PlayerSessionParameterType> PendingParametersToReset
        {
            get
            {
                lock(pendingParametersToReset)
                {
                    return pendingParametersToReset.ToList();
                }
            }
        }

        /// <inheritdoc/>
        public void ReportParametersBeingReset(IEnumerable<PlayerSessionParameterType> resetParameters)
        {
            var resetParametersList = resetParameters.ToList();
            CurrentResetParametersChangedEventArgs toPost;

            lock(pendingParametersToReset)
            {
                foreach(var parameter in resetParametersList)
                {
                    pendingParametersToReset.Remove(parameter);
                }

                toPost = new CurrentResetParametersChangedEventArgs(pendingParametersToReset, resetParametersList);
            }

            eventPoster.PostTransactionalEvent(toPost);
        }

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
                    sessionActive = gameInProgress || moneyOnMachine;

                    // Start timer if session is not active.
                    if(!sessionActive)
                    {
                        delayTimer.Start();
                    }
                }

                // Clear pending parameters upon context activation.
                lock(pendingParametersToReset)
                {
                    pendingParametersToReset.Clear();
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

                    sessionActive = false;
                }

                lock(pendingParametersToReset)
                {
                    pendingParametersToReset.Clear();
                }
            }
        }

        private void UpdateSessionStatus()
        {
            var newStatus = gameInProgress || moneyOnMachine;

            lock(delayTimer)
            {
                if(sessionActive != newStatus)
                {
                    sessionActive = newStatus;

                    if(sessionActive)
                    {
                        delayTimer.Stop();
                    }
                    else
                    {
                        delayTimer.Start();
                    }
                }
            }

            // Clear pending parameters upon activity change.
            lock(pendingParametersToReset)
            {
                pendingParametersToReset.Clear();
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

            CurrentResetParametersChangedEventArgs toPost = null;

            lock(delayTimer)
            {
                delayTimer.Stop();

                // Make sure things haven't changed during the delay.
                if(!sessionActive)
                {
                    toPost = new CurrentResetParametersChangedEventArgs(allParameters, new List<PlayerSessionParameterType>());
                }
            }

            if(toPost != null)
            {
                lock(pendingParametersToReset)
                {
                    pendingParametersToReset.Clear();
                    pendingParametersToReset.AddRange(toPost.PendingParameters);
                }

                eventPoster.PostTransactionalEvent(toPost);
            }
        }

        #endregion
    }
}
