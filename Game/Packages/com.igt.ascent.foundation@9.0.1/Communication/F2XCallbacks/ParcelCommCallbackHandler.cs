//-----------------------------------------------------------------------
// <copyright file = "ParcelCommCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.ParcelComm;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the unified F2X
    /// Parcel Communication API category.
    /// </summary>
    internal class ParcelCommCallbackHandler : IParcelCommCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// The callback interface for handling non transactional events.
        /// </summary>
        private readonly INonTransactionalEventCallbacks nonTransactionalEventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="ParcelCommCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <param name="nonTransactionalEventCallbacks">The callback interface for handling non transactional events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> or <paramref name="nonTransactionalEventCallbacks"/> is null.
        /// </exception>
        public ParcelCommCallbackHandler(IEventCallbacks eventCallbacks,
                                         INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
            this.nonTransactionalEventCallbacks = nonTransactionalEventCallbacks ?? throw new ArgumentNullException(nameof(nonTransactionalEventCallbacks));
        }

        #region IParcelCommCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessParcelCallRx(RequestParcel parcel, out ParcelCallRxReplyContent callbackResult)
        {
            var source = parcel.Source.ToPublic();
            var target = parcel.Target.ToPublic();

            string errorMessage = null;
            if(source == null || target == null)
            {
                errorMessage = "Failed to process the non-transactional parcel call: source or target is empty";
            }
            else
            {
                if(target.EntityType == EndpointType.Theme ||
                   target.EntityType == EndpointType.Shell)
                {
                    // We don't want to expose the theme/shell identifier to the game, just wipe it out for this case.
                    target = new ParcelCommEndpoint(target.EntityType, null);
                }

                // Non transactional parcel call is simply queued and return without waiting to be processed.
                nonTransactionalEventCallbacks.EnqueueEvent(
                    new NonTransactionalParcelCallReceivedEventArgs(source, target, parcel.Payload));
            }

            callbackResult = new ParcelCallRxReplyContent
                                 {
                                     Accepted = errorMessage == null,
                                     Payload = null
                                 };

            return errorMessage;
        }

        /// <inheritdoc/>
        public string ProcessTransParcelCallRx(RequestParcel parcel, out TransParcelCallRxReplyContent callbackResult)
        {
            var source = parcel.Source.ToPublic();
            var target = parcel.Target.ToPublic();

            string errorMessage = null;
            if(source == null || target == null)
            {
                errorMessage = "Failed to process the transactional parcel call: source or target is empty";
                callbackResult = new TransParcelCallRxReplyContent
                                     {
                                         Accepted = false,
                                         Payload = null
                                     };
            }
            else
            {
                if(target.EntityType == EndpointType.Theme ||
                   target.EntityType == EndpointType.Shell)
                {
                    // We don't want to expose the theme/shell identifier to the game, just wipe it out for this case.
                    target = new ParcelCommEndpoint(target.EntityType, null);
                }

                var eventArgs = new TransactionalParcelCallReceivedEventArgs(source, target, parcel.Payload);

                eventCallbacks.PostEvent(eventArgs);
                var callResult = eventArgs.CallResult;
                callbackResult = new TransParcelCallRxReplyContent
                                     {
                                         Accepted = callResult != null && callResult.Status == ParcelCallStatus.Success,
                                         Payload = callResult?.Payload
                                     };
            }

            return errorMessage;
        }

        #endregion
    }
}
