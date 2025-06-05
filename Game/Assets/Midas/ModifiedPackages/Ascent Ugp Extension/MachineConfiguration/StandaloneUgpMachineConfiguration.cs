//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpMachineConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpMachineConfiguration extended interface.
    /// </summary>
    internal sealed class StandaloneUgpMachineConfiguration : IUgpMachineConfiguration,
                                                              IStandaloneHelperUgpMachineConfiguration,
                                                              IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The interface for posting foundation events in the main event queue.
        /// </summary>
        private readonly IStandaloneEventPosterDependency eventPoster;

		/// <summary>
		/// Local cache of the machine configuration parameters.
		/// </summary>
		private readonly MachineConfigurationParameters parameters;

		#endregion
        
        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpMachineConfiguration"/>.
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
        public StandaloneUgpMachineConfiguration(IStandaloneEventPosterDependency eventPosterInterface,
                                                 IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            eventPoster = eventPosterInterface ?? throw new ArgumentNullException(nameof(eventPosterInterface));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, MachineConfigurationChanged);

			parameters = new MachineConfigurationParameters(true, string.Empty, 100, 3000, false, false, int.MaxValue,
				false, UgpMachineConfigurationWinCapStyle.None, 0, null, null, null);
        }

        #endregion

        #region IUgpMachineConfiguration Implementation

        /// <inheritdoc/>
        public event EventHandler<MachineConfigurationChangedEventArgs> MachineConfigurationChanged;

		public MachineConfigurationParameters GetMachineConfigurationParameters()
		{
			return parameters;
		}

        #endregion

        #region IStandaloneHelperUgpMachineConfiguration Implementation

        /// <inheritdoc/>
        public void SetClockConfiguration(bool isClockVisible, string clockFormat)
        {
            parameters.IsClockVisible = isClockVisible;
            parameters.ClockFormat = clockFormat;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetTokenisation(long tokenisation)
        {
            parameters.Tokenisation = tokenisation;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetGameCycleTime(long gameCycleTime)
        {
            parameters.GameCycleTime = gameCycleTime;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetContinuousPlayAllowed(bool continuousPlayAllowed)
        {
            parameters.IsContinuousPlayAllowed = continuousPlayAllowed;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetFeatureAutoStartEnabled(bool featureAutoStartEnabled)
        {
            parameters.IsFeatureAutoStartEnabled = featureAutoStartEnabled;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetCurrentMaximumBet(long currentMaximumBet)
        {
            parameters.CurrentMaximumBet = currentMaximumBet;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetWinCapStyle(UgpMachineConfigurationWinCapStyle winCapStyle)
        {
            parameters.WinCapStyle = winCapStyle;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetSlamSpinAllowed(bool slamSpinAllowed)
        {
            parameters.IsSlamSpinAllowed = slamSpinAllowed;

            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        /// <inheritdoc/>
        public void SetMachineConfiguration(bool isClockVisible, string clockFormat, long tokenisation,
                long gameCycleTime, bool isContinuousPlayAllowed, bool isFeatureAutoStartEnabled,
                long currentMaximumBet, UgpMachineConfigurationWinCapStyle winCapStyle, bool isSlamSpinAllowed,
                int qcomJurisdiction, string cabinetId, string brainBoxId, string gpu)
        {
            parameters.IsClockVisible = isClockVisible;
            parameters.ClockFormat = clockFormat;
            parameters.Tokenisation = tokenisation;
            parameters.GameCycleTime = gameCycleTime;
            parameters.IsContinuousPlayAllowed = isContinuousPlayAllowed;
            parameters.IsFeatureAutoStartEnabled = isFeatureAutoStartEnabled;
            parameters.CurrentMaximumBet = currentMaximumBet;
            parameters.WinCapStyle = winCapStyle;
            parameters.IsSlamSpinAllowed = isSlamSpinAllowed;
            parameters.QcomJurisdiction = qcomJurisdiction;
            parameters.CabinetId = cabinetId;
            parameters.BrainboxId = brainBoxId;
            parameters.Gpu = gpu;
            eventPoster.PostTransactionalEvent(new MachineConfigurationChangedEventArgs(parameters));
        }

        #endregion
    }
}