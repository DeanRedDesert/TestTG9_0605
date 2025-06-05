// -----------------------------------------------------------------------
// <copyright file = "ExtensionParcelComm.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2XLinks
{
    using System;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IExtensionParcelComm"/> that uses
    /// F2X to communicate with the Foundation to support parcel communication.
    /// </summary>
    internal class ExtensionParcelComm : ParcelCommBase, IExtensionParcelComm
    {
        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="ExtensionParcelComm"/>.
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
        public ExtensionParcelComm(ITransactionVerification transactionVerifier,
                                   IEventDispatcher transactionalEventDispatcher,
                                   IEventDispatcher nonTransactionalEventDispatcher)
            : base(transactionVerifier, transactionalEventDispatcher, nonTransactionalEventDispatcher)
        {
        }

        #endregion

        #region IExtensionParcelComm Members

        /// <inheritdoc/>
        public ParcelCallStatus SendNonTransactionalParcelCall(string sourceIdentifier, ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            if(string.IsNullOrEmpty(sourceIdentifier))
            {
                throw new ArgumentNullException("sourceIdentifier");
            }

            var sourceEndpoint = new ParcelCommEndpoint(EndpointType.Extension, sourceIdentifier);
            return SendNonTransactionalParcelCall(sourceEndpoint, targetEndpoint, payload);
        }

        /// <inheritdoc/>
        public ParcelCallResult SendTransactionalParcelCall(string sourceIdentifier, ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            if(string.IsNullOrEmpty(sourceIdentifier))
            {
                throw new ArgumentNullException("sourceIdentifier");
            }

            var sourceEndpoint = new ParcelCommEndpoint(EndpointType.Extension, sourceIdentifier);
            return SendTransactionalParcelCall(sourceEndpoint, targetEndpoint, payload);
        }

        #endregion

        #region override Members

        /// <inheritdoc/>
        protected override bool IsValidReceivedTarget(ParcelCommEndpoint target)
        {
            return target.EntityType == EndpointType.Extension;
        }

        #endregion
    }
}
