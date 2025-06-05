//-----------------------------------------------------------------------
// <copyright file = "GameParcelComm.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using Game.Core.Communication.Foundation.F2X;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;
    using F2XExtensionIdentifier = Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types.ExtensionIdentifier;
    using F2XParcelCommEntity = Game.Core.Communication.Foundation.F2X.Schemas.Internal.ParcelComm.ParcelCommEntity;
    using F2XRequestParcel = Game.Core.Communication.Foundation.F2X.Schemas.Internal.ParcelComm.RequestParcel;

    /// <summary>
    /// Implementation of the <see cref="IGameParcelComm"/> that uses
    /// F2X to communicate with the Foundation to support parcel communication.
    /// </summary>
    internal class GameParcelComm : IGameParcelComm
    {
        #region Private Fields
        
        /// <summary>
        /// The interface for the parcel comm category.
        /// </summary>
        private readonly CategoryInitializer<IParcelCommCategory> parcelCommCategory = new CategoryInitializer<IParcelCommCategory>();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="GameParcelComm"/>.
        /// </summary>
        /// <param name="eventSender">
        /// The object to put as the sender of all events raised by this class.
        /// If null, this instance will be put as the sender.
        /// 
        /// This is so that the event handlers can cast sender to
        /// IShellLib if needed, e.g. writing critical data.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <param name="nonTransactionalEventDispatcher">
        /// Interface for processing a non-transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments except <paramref name="eventSender"/> is null.
        /// </exception>
        public GameParcelComm(object eventSender,
                              IEventDispatcher transactionalEventDispatcher,
                              IEventDispatcher nonTransactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            if(nonTransactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(nonTransactionalEventDispatcher));
            }

            var actualSender = eventSender ?? this;

            transactionalEventDispatcher.EventDispatchedEvent +=
                (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(actualSender, TransactionalParcelCallReceivedEvent);
            nonTransactionalEventDispatcher.EventDispatchedEvent +=
                (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(actualSender, NonTransactionalParcelCallReceivedEvent);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="GameParcelComm"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IParcelCommCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            parcelCommCategory.Initialize(category);
        }

        #endregion

        #region IGameParcelComm Members

        /// <inheritdoc/>
        public ParcelCallStatus SendNonTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            var parcel = new F2XRequestParcel
                             {
                                 // Source is left null intentionally as the game does not know its own identifier.
                                 Target = ToInternal(targetEndpoint),
                                 Payload = payload
                             };
            var reply = parcelCommCategory.Instance.ParcelCallTx(parcel);

            // Simply use integer to convert the status.
            // The public type and the internal schema type should always have the same enumeration values.
            return (ParcelCallStatus)(int)reply.Status;
        }

        /// <inheritdoc/>
        public ParcelCallResult SendTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            var parcel = new F2XRequestParcel
                             {
                                 // Source is left null intentionally as the game does not know its own identifier.
                                 Target = ToInternal(targetEndpoint),
                                 Payload = payload
                             };
            var reply = parcelCommCategory.Instance.TransParcelCallTx(parcel);

            // Simply use integer to convert the status.
            // The public type and the internal schema type should always have the same enumeration values.
            return new ParcelCallResult((ParcelCallStatus)(int)reply.Status,
                                        reply.Payload);
        }

        /// <inheritdoc/>
        public event EventHandler<NonTransactionalParcelCallReceivedEventArgs> NonTransactionalParcelCallReceivedEvent;

        /// <inheritdoc/>
        public event EventHandler<TransactionalParcelCallReceivedEventArgs> TransactionalParcelCallReceivedEvent;

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a public endpoint to an F2X parcel comm entity.
        /// </summary>
        /// <param name="endpoint">The public endpoint to convert.</param>
        /// <returns>The converted endpoint.</returns>
        private F2XParcelCommEntity ToInternal(ParcelCommEndpoint endpoint)
        {
            if(endpoint.EntityType != EndpointType.Extension)
            {
                throw new InvalidOperationException("Target endpoint of game parcel call must be Extension!");
            }

            return new F2XParcelCommEntity
                       {
                           Item = new F2XExtensionIdentifier { Value = endpoint.EntityIdentifier }
                       };
        }
        
        #endregion
    }
}
