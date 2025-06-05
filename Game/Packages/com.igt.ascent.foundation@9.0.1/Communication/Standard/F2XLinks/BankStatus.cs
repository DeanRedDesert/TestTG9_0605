//-----------------------------------------------------------------------
// <copyright file = "BankStatus.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using F2XCallbacks;
    using Foundation;
    using InternalType = F2X.Schemas.Internal.BankStatus;

    /// <summary>
    /// Implementation of the <see cref="IBankStatus"/> interface that is backed by the F2X.
    /// </summary>
    internal class BankStatus : IBankStatus
    {
        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="BankStatus"/>.
        /// </summary>
        /// <param name="transactionVerification">
        /// Reference to <see cref="ITransactionVerification"/> to verify transactions.
        /// </param>
        /// <param name="eventDispatcher">Interface for processing a transactional event.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="transactionVerification"/> or <paramref name="eventDispatcher"/> are null.
        /// </exception>
        public BankStatus(ITransactionVerification transactionVerification,
                          IEventDispatcher eventDispatcher)
        {
            if(eventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(eventDispatcher));
            }

            this.transactionVerification = transactionVerification ?? throw new ArgumentNullException(nameof(transactionVerification));
            eventDispatcher.EventDispatchedEvent += HandleBankStatusEvent;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(category == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "BankStatus cannot be used without calling its Initialize method first.");
            }
        }

        /// <summary>
        /// Handles the bank status changed event.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="dispatchedEventArgs">The arguments related to the bank status event.</param>
        private void HandleBankStatusEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs != null)
            {
                if(dispatchedEventArgs.DispatchedEventType == typeof(BankMeterEventArgs))
                {
                    var handler = BankMeterEvent;
                    if(dispatchedEventArgs.DispatchedEvent is BankMeterEventArgs eventArgs && handler != null)
                    {
                        handler(this, eventArgs);
                        dispatchedEventArgs.IsHandled = true;
                    }
                }
            }
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Object that references the bank status events category.
        /// </summary>
        private IBankStatusCategory category;

        /// <summary>
        /// Reference to <see cref="ITransactionVerification"/> to verify transactions.
        /// </summary>
        private readonly ITransactionVerification transactionVerification;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="bankStatusCategory">
        /// The interface for communicating with the Foundation.
        /// </param>
        public void Initialize(IBankStatusCategory bankStatusCategory)
        {
            category = bankStatusCategory;
        }

        #endregion

        #region  IBankStatus

        /// <inheritdoc/>
        public event EventHandler<BankMeterEventArgs> BankMeterEvent;

        /// <inheritdoc/>
        public void RegisterBankMeterEvents(IList<BankMeterEventType> eventTypes)
        {
            CheckInitialization();

            if(eventTypes == null)
            {
                throw new ArgumentNullException(nameof(eventTypes));
            }

            if(!eventTypes.Any())
            {
                throw new ArgumentException(nameof(eventTypes));
            }

            transactionVerification.MustHaveOpenTransaction();
            var convertedEvents =
                eventTypes.Select(bankMeterEventType => (InternalType.BankEventType)bankMeterEventType);
            category.SetBankEventRegistration(convertedEvents);
        }

        /// <inheritdoc/>
        public void ClearBankMeterEventRegistration()
        {
            CheckInitialization();
            transactionVerification.MustHaveOpenTransaction();
            category.SetBankEventRegistration(new List<InternalType.BankEventType>());
        }

        /// <inheritdoc/>
        public IPlayerMeters GetPlayerMeters()
        {
            CheckInitialization();

            transactionVerification.MustHaveOpenTransaction();
            var categoryPlayMeters = category.GetPlayerMeters();
            return categoryPlayMeters.ToPublic();
        }

        #endregion
    }
}