// -----------------------------------------------------------------------
// <copyright file = "StandaloneService.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Cabinet;

    /// <summary>
    /// Provide a virtual implementation of the service category for use in standalone.
    /// </summary>
    internal class StandaloneService: IService, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// The cabinet lib that owns this object.
        /// </summary>
        private readonly ICabinetLibDemo cabinetLibDemo;

        /// <summary>
        /// Indicate whether to prompt player on cash out
        /// </summary>
        private readonly bool promptPlayerOnCashout;

        /// <summary>
        /// List of emulated buttons that the EGM requires
        /// </summary>
        private readonly IReadOnlyList<EmulatableButton> emulatableButtons;

        /// <summary>
        /// Tracks the player call attendant state.
        /// </summary>
        private bool playerCallAttendantState;

        /// <summary>
        /// Whether to raise the <see cref="PlayerCallAttendantStateChangedEvent"/>.
        /// </summary>
        private bool playerCallAttendantStateEventRegistered;

        /// <summary>
        /// Tracks the player service request state.
        /// </summary>
        private bool playerServiceRequestState;

        /// <summary>
        /// Whether to raise the <see cref="PlayerServiceRequestStateChangedEvent"/>.
        /// </summary>
        private bool playerServiceRequestStateEventRegistered;

        /// <summary>
        /// The queue of pending events to be posted in the next Update call.
        /// </summary>
        private readonly ConcurrentQueue<EventArgs> eventQueue = new ConcurrentQueue<EventArgs>();

        #endregion

        #region Construtor

        /// <summary>
        /// Construct a Standalone Service based on the given configuration.
        /// </summary>
        /// <param name="cabinetLibDemo">
        /// The cabinet lib that owns this object.
        /// </param>
        /// <param name="parser">
        /// Parser that provide service settings
        /// </param>
        public StandaloneService(ICabinetLibDemo cabinetLibDemo = null, ServiceSettingsParser parser = null)
        {
            this.cabinetLibDemo = cabinetLibDemo;

            if(parser?.SettingsAvailable != true)
            {
                promptPlayerOnCashout = true;
                emulatableButtons = new List<EmulatableButton> { EmulatableButton.Cashout, EmulatableButton.Service };
            }
            else
            {
                promptPlayerOnCashout = parser.PromptPlayerOnCashout;
                emulatableButtons = parser.EmulatableButtons;
            }
        }

        #endregion

        #region IService Implementation

        /// <inheritdoc/>
        public event EventHandler<PromptPlayerOnCashoutConfigItemChangedEventArgs> PromptPlayerOnCashoutConfigItemChangedEvent
        {
            add { }
            remove { }
        }

        /// <inheritdoc/>
        public event EventHandler<EmulatedServiceButtonEnabledConfigItemChangedEventArgs> EmulatedServiceButtonEnabledConfigItemChangedEvent
        {
            add { }
            remove { }
        }

        /// <inheritdoc />
        public event EventHandler<PlayerCallAttendantStateChangedEventArgs> PlayerCallAttendantStateChangedEvent;

        /// <inheritdoc />
        public event EventHandler<PlayerServiceRequestStateChangedEventArgs> PlayerServiceRequestStateChangedEvent;

        /// <inheritdoc/>
        public void RequestService()
        {
            CandleStateController.ToggleCandleState();
            TogglePlayerServiceStates();
        }

        /// <inheritdoc/>
        public void RequestCashOut()
        {
            cabinetLibDemo?.EnqueueEvent(this, new CabinetButtonPressedEventArgs((int)SwitchId.CashOutId, true, new List<ButtonFunction> { ButtonFunction.CashOut }));
            cabinetLibDemo?.EnqueueEvent(this, new CabinetButtonPressedEventArgs((int)SwitchId.CashOutId, false, new List<ButtonFunction> { ButtonFunction.CashOut }));
        }

        /// <inheritdoc/>
        public bool RegisterForPromptPlayerOnCashoutConfigItemChangedEvents()
        {
            return true;
        }

        /// <inheritdoc/>
        public bool UnregisterForPromptPlayerOnCashoutConfigItemChangedEvents()
        {            
            return true;
        }

        /// <inheritdoc/>
        public bool GetPromptPlayerOnCashoutConfigItemValue()
        {
            return promptPlayerOnCashout;
        }

        /// <inheritdoc/>
        /// <remarks>This should hopefully be extended in the future to be configurable in the editor.</remarks>
        public IReadOnlyList<EmulatableButton> GetTheButtonsThatTheEgmRequiresToBeEmulated()
        {
            return emulatableButtons;
        }

        /// <inheritdoc/>
        public bool RegisterForEmulatedServiceButtonEnabledConfigItemChangedEvents()
        {
            return true;
        }

        /// <inheritdoc/>
        public bool UnregisterForEmulatedServiceButtonEnabledConfigItemChangedEvents()
        {
            return true;
        }

        /// <inheritdoc />
        public bool GetPlayerCallAttendantState()
        {
            return playerCallAttendantState;
        }

        /// <inheritdoc />
        public bool RegisterPlayerCallAttendantStateEvent()
        {
            playerCallAttendantStateEventRegistered = true;
            return true;
        }

        /// <inheritdoc />
        public bool UnregisterPlayerCallAttendantStateEvent()
        {
            playerCallAttendantStateEventRegistered = false;
            return true;
        }

        /// <inheritdoc />
        public bool GetPlayerServiceRequestState()
        {
            return playerServiceRequestState;
        }

        /// <inheritdoc />
        public bool RegisterPlayerServiceRequestStateEvent()
        {
            playerServiceRequestStateEventRegistered = true;
            return true;
        }

        /// <inheritdoc />
        public bool UnregisterPlayerServiceRequestStateEvent()
        {
            playerServiceRequestStateEventRegistered = false;
            return true;
        }

        #endregion IService Implementation

        #region Private Methods

        /// <summary>
        /// Toggles the two player service states and raises events if they are registered.
        /// </summary>
        /// <remarks>
        /// In standalone implementation, we do not differentiate the two states.
        /// They are modified at the same time with the same values.
        /// </remarks>
        private void TogglePlayerServiceStates()
        {
            playerCallAttendantState = !playerCallAttendantState;
            playerServiceRequestState = !playerServiceRequestState;

            if(playerCallAttendantStateEventRegistered)
            {
                eventQueue.Enqueue(new PlayerCallAttendantStateChangedEventArgs(playerCallAttendantState));
            }

            if(playerServiceRequestStateEventRegistered)
            {
                eventQueue.Enqueue(new PlayerServiceRequestStateChangedEventArgs(playerServiceRequestState));
            }
        }

        #endregion

        #region Implementation of ICabinetUpdate

        /// <inheritdoc />
        public void Update()
        {
            while(eventQueue.TryDequeue(out var eventArgs))
            {
                if(eventArgs is PlayerCallAttendantStateChangedEventArgs callAttendantEvent)
                {
                    PlayerCallAttendantStateChangedEvent?.Invoke(this, callAttendantEvent);
                }
                else if(eventArgs is PlayerServiceRequestStateChangedEventArgs serviceRequestEvent)
                {
                    PlayerServiceRequestStateChangedEvent?.Invoke(this, serviceRequestEvent);
                }
            }
        }

        #endregion
    }
}