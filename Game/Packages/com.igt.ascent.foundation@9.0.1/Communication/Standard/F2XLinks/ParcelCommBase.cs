// -----------------------------------------------------------------------
// <copyright file = "ParcelCommBase.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using F2XCallbacks;
    using F2XRequestParcel = F2X.Schemas.Internal.ParcelComm.RequestParcel;

    /// <summary>
    /// Base implementation class of the <see cref="IExtensionParcelComm"/> that uses
    /// F2X to communicate with the Foundation to support parcel communication.
    /// </summary>
    internal abstract class ParcelCommBase
    {
        /// <summary>
        /// The interface for verifying the transaction.
        /// </summary>
        private readonly ITransactionVerification transactionVerifier;

        /// <summary>
        /// The interface for parcel comm category.
        /// </summary>
        private IParcelCommCategory parcelCommCategory;

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="ParcelCommBase"/>.
        /// </summary>
        /// <param name="transactionVerifier">
        /// The interface for verifying that a transaction is open.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <param name="nonTransactionalEventDispatcher">
        /// Interface for processing a non-transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if any argument is null.
        /// </exception>
        protected ParcelCommBase(ITransactionVerification transactionVerifier,
                                 IEventDispatcher transactionalEventDispatcher,
                                 IEventDispatcher nonTransactionalEventDispatcher)
        {
            if(transactionVerifier == null)
            {
                throw new ArgumentNullException("transactionVerifier");
            }

            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException("transactionalEventDispatcher");
            }

            if(nonTransactionalEventDispatcher == null)
            {
                throw new ArgumentNullException("nonTransactionalEventDispatcher");
            }

            this.transactionVerifier = transactionVerifier;

            transactionalEventDispatcher.EventDispatchedEvent += HandleTransactionalParcelCallReceivedEvent;
            nonTransactionalEventDispatcher.EventDispatchedEvent += HandleNonTransactionalParcelCallReceivedEvent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes class members whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IParcelCommCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException("category");
            }

            parcelCommCategory = category;
        }

        #endregion

        /// <summary>
        /// Invokes a non transactional parcel call from the specified source to the specified target.
        /// </summary>
        /// <param name="sourceEndpoint">The source entity of the parcel call.</param>
        /// <param name="targetEndpoint">The target entity of the parcel call.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The acceptance status of the parcel call.</returns>
        protected ParcelCallStatus SendNonTransactionalParcelCall(ParcelCommEndpoint sourceEndpoint, ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            CheckInitialization();

            var parcel = new F2XRequestParcel
            {
                Source = sourceEndpoint.ToInternal(),
                Target = targetEndpoint.ToInternal(),
                Payload = payload
            };
            var reply = parcelCommCategory.ParcelCallTx(parcel);

            // Simply use integer to convert the status.
            // The public type and the internal schema type should always have the same enumeration values.
            return (ParcelCallStatus)(int)reply.Status;
        }

        /// <summary>
        /// Invokes a transactional parcel call from the specified source to the specified target.
        /// </summary>
        /// <param name="sourceEndpoint">The source entity of the parcel call.</param>
        /// <param name="targetEndpoint">The target entity of the parcel call.</param>
        /// <param name="payload">Binary payload of the parcel call.</param>
        /// <returns>The result of the parcel call.</returns>
        protected ParcelCallResult SendTransactionalParcelCall(ParcelCommEndpoint sourceEndpoint, ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            CheckInitialization();
            transactionVerifier.MustHaveOpenTransaction();

            var parcel = new F2XRequestParcel
            {
                Source = sourceEndpoint.ToInternal(),
                Target = targetEndpoint.ToInternal(),
                Payload = payload
            };
            var reply = parcelCommCategory.TransParcelCallTx(parcel);

            // Simply use integer to convert the status.
            // The public type and the internal schema type should always have the same enumeration values.
            return new ParcelCallResult((ParcelCallStatus)(int)reply.Status, reply.Payload);
        }

        /// <summary>
        /// Event occurs when a non-transactional parcel call is received by the extension.
        /// </summary>
        public event EventHandler<NonTransactionalParcelCallReceivedEventArgs> NonTransactionalParcelCallReceivedEvent;

        /// <summary>
        /// Event occurs when a non-transactional parcel call is received by the extension.
        /// </summary>
        public event EventHandler<TransactionalParcelCallReceivedEventArgs> TransactionalParcelCallReceivedEvent;

        #region Private Methods

        /// <summary>
        /// Checks if this object has been initialized correctly before being used.
        /// </summary>
        /// <exception cref="CommunicationInterfaceUninitializedException">
        /// Thrown when any API is called before Initialize is called.
        /// </exception>
        private void CheckInitialization()
        {
            if(parcelCommCategory == null)
            {
                throw new CommunicationInterfaceUninitializedException(
                    "ParcelComm cannot be used without calling its Initialize method first.");
            }
        }

        /// <summary>
        /// Handles the dispatched event if the dispatched event is NonTransactionalParcelCallReceivedEvent event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandleNonTransactionalParcelCallReceivedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(NonTransactionalParcelCallReceivedEventArgs))
            {
                var eventArgs = dispatchedEventArgs.DispatchedEvent as NonTransactionalParcelCallReceivedEventArgs;

                // Make sure that it's a valid target!
                var handler = NonTransactionalParcelCallReceivedEvent;
                if(eventArgs != null && IsValidReceivedTarget(eventArgs.Target) && handler != null)
                {
                    handler(this, eventArgs);

                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Handles the dispatched event if the dispatched event is TransactionalParcelCallReceivedEvent event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandleTransactionalParcelCallReceivedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(TransactionalParcelCallReceivedEventArgs))
            {
                var eventArgs = dispatchedEventArgs.DispatchedEvent as TransactionalParcelCallReceivedEventArgs;

                // Make sure that it's a valid target!
                if(eventArgs != null && IsValidReceivedTarget(eventArgs.Target) && TransactionalParcelCallReceivedEvent != null)
                {
                    TransactionalParcelCallReceivedEvent(this, eventArgs);

                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Verifies if it is a valid target at handling a received parcel call event.
        /// </summary>
        /// <param name="target">
        /// The target endpoint of the parcel call.
        /// </param>
        /// <returns>
        /// True if it is a valid target; false otherwise.
        /// </returns>
        protected abstract bool IsValidReceivedTarget(ParcelCommEndpoint target);

        #endregion
    }
}
