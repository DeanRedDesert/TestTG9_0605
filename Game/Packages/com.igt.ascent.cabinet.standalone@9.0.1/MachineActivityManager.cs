//-----------------------------------------------------------------------
// <copyright file = "MachineActivityManager.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using Communication.Standalone;

    /// <summary>
    /// This class manages the machine activity for the Cabinet Lib.
    /// </summary>
    public class MachineActivityManager
    {
        #region Private Properties

        /// <summary>
        /// The cabinet lib that owns this object.
        /// </summary>
        private readonly ICabinetLibDemo cabinetLibDemo;

        /// <summary>
        /// Flag indicating if the Foundation is in Idle.
        /// </summary>
        private bool isStateIdle;

        /// <summary>
        /// The current activity status.
        /// </summary>
        private MachineActivityStatus activityStatus;

        /// <summary>
        /// Set/Get for storing if the Foundation is in Idle.
        /// </summary>
        private bool IsStateIdle
        {
            get => isStateIdle;

            set
            {
                if(isStateIdle != value)
                {
                    isStateIdle = value;
                    if(isStateIdle && PlayerBankMeter == 0)
                    {
                        StartInactiveState();
                    }
                    else
                    {
                        StopInactiveState();
                    }
                }
            }
        }

        /// <summary>
        /// Currently reported player bank meter value.
        /// </summary>
        private long playerBankMeter = -1;

        /// <summary>
        /// Set/Get player bank meter.
        /// </summary>
        private long PlayerBankMeter
        {
            get => playerBankMeter;

            set
            {
                if(playerBankMeter != value)
                {
                    playerBankMeter = value;
                    if(playerBankMeter == 0 && IsStateIdle)
                    {
                        StartInactiveState();
                    }
                    else
                    {
                        StopInactiveState();
                    }
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// To be called when the game state is idle and bank is zero.
        /// </summary>
        private void StartInactiveState()
        {
            activityStatus = new MachineActivityStatus
            (
                false,
                activityStatus.AttractInterval,
                activityStatus.InactivityDelay,
                activityStatus.AttractsEnabled,
                activityStatus.NewGame
            );
            cabinetLibDemo.EnqueueEvent(this, new ActivityStatusEventArgs(activityStatus));
        }

        /// <summary>
        /// To bee called when the game state leaves idle, or bank is no longer zero.
        /// </summary>
        private void StopInactiveState()
        {
            activityStatus = new MachineActivityStatus
            (
                true,
                activityStatus.AttractInterval,
                activityStatus.InactivityDelay,
                activityStatus.AttractsEnabled,
                activityStatus.NewGame
            );
            cabinetLibDemo.EnqueueEvent(this, new ActivityStatusEventArgs(activityStatus));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Report a user keyboard or mouse input
        /// </summary>
        public void ReportUserInput()
        {
            if(IsStateIdle && PlayerBankMeter == 0)
            {
                StopInactiveState();

                StartInactiveState();
            }
        }

        /// <summary>
        /// Handles foundation state change event.
        /// </summary>
        /// <param name="eventToPost">Event that was posted.</param>
        /// <param name="eventHandler">Not used.</param>
        public void FoundationStateChanged(EventArgs eventToPost, EventHandler<EventArgs> eventHandler)
        {
            if(!(eventToPost is FoundationStateChangedEventArgs foundationStateChangedEventArgs))
            {
                return;
            }

            IsStateIdle = foundationStateChangedEventArgs.IsStateIdle;
        }

        /// <summary>
        /// Handles player bank meter change event.
        /// </summary>
        /// <param name="eventToPost">Event that was posted.</param>
        /// <param name="eventHandler">Not used.</param>
        public void PlayerBankMeterChanged(EventArgs eventToPost, EventHandler<EventArgs> eventHandler)
        {
            if(!(eventToPost is PlayerBankMeterChangedEventArgs playerBankMeterChangedEventArgs))
            {
                return;
            }

            PlayerBankMeter = playerBankMeterChangedEventArgs.PlayerBankAmt;
        }

        /// <summary>
        /// Requests the machine activity status data.
        /// </summary>
        /// <returns>The machine activity status data.</returns>
        public MachineActivityStatus RequestActivityStatus()
        {
            return activityStatus;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor of MachineActivityManager
        /// </summary>
        /// <param name="owner"> The Game Lib</param>
        /// <param name="machineActivityParser"> An xml element parser that provides a list of machine activity variants.</param>
        public MachineActivityManager(ICabinetLibDemo owner, MachineActivityParser machineActivityParser)
        {
            cabinetLibDemo = owner ?? throw new ArgumentNullException(nameof(owner), "Parameter cannot be null.");
            var settings = machineActivityParser.MachineActivitySettings;
            activityStatus = new MachineActivityStatus(true, settings.AttractInterval, settings.InactivityDelay,
                                                       settings.AttractsEnabled, settings.NewGame);
        }

        #endregion
    }
}
