//-----------------------------------------------------------------------
// <copyright file = "StandaloneBankStatus.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Standalone implementation of the <see cref="IBankStatus"/> interface.
    /// </summary>
    internal class StandaloneBankStatus : IBankStatus
    {
        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneBankStatus"/>.
        /// </summary>
        /// <param name="verifier">
        /// Reference to <see cref="ITransactionVerification"/> to verify transactions.
        /// </param>
        /// <param name="dispatcher">Interface for processing a transactional event.</param>
        public StandaloneBankStatus(ITransactionVerification verifier, IEventDispatcher dispatcher)
        {
            if(dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            transactionVerification = verifier ?? throw new ArgumentNullException(nameof(verifier));
            dispatcher.EventDispatchedEvent += HandleBankStatusEvent;
            BankMeterEvent += SavePlayerMeters;
            registeredEvents = new HashSet<BankMeterEventType>();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Store the player meters that were sent by an event.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs">The arguments related to the bank status event.</param>
        private void SavePlayerMeters(object sender, BankMeterEventArgs eventArgs)
        {
            currentPlayMeters = eventArgs.PlayerMeters;
        }

        /// <summary>
        /// Handles the bank status changed event.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="dispatchedEventArgs">The arguments related to the bank status event.</param>
        private void HandleBankStatusEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(BankMeterEventArgs))
            {
                var handler = BankMeterEvent;
                if(dispatchedEventArgs.DispatchedEvent is BankMeterEventArgs eventArgs && handler != null && registeredEvents.Contains(eventArgs.EventType))
                {
                    handler(this, eventArgs);
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Set of registered event types.
        /// </summary>
        private readonly HashSet<BankMeterEventType> registeredEvents;

        /// <summary>
        /// Store the current play meters locally in standalone version.
        /// </summary>
        private IPlayerMeters currentPlayMeters = new PlayerMeters();

        /// <summary>
        /// Reference to <see cref="ITransactionVerification"/> to verify transactions.
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        #endregion

        #region IBankStatus

        /// <inheritdoc/>
        public event EventHandler<BankMeterEventArgs> BankMeterEvent;

        /// <inheritdoc/>
        public IPlayerMeters GetPlayerMeters()
        {
            transactionVerification.MustHaveOpenTransaction();
            return currentPlayMeters;
        }

        /// <inheritdoc/>
        public void RegisterBankMeterEvents(IList<BankMeterEventType> eventTypes)
        {
            if(eventTypes == null)
            {
                throw new ArgumentNullException(nameof(eventTypes));
            }

            if(!eventTypes.Any())
            {
                throw new ArgumentException("eventTypes");
            }

            transactionVerification.MustHaveOpenTransaction();
            registeredEvents.Clear();
            foreach(var type in eventTypes)
            {
                registeredEvents.Add(type);
            }
        }

        /// <inheritdoc/>
        public void ClearBankMeterEventRegistration()
        {
            transactionVerification.MustHaveOpenTransaction();
            registeredEvents.Clear();
        }

        #endregion
    }
}