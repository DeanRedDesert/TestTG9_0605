// -----------------------------------------------------------------------
// <copyright file = "TransactionVerifier.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;
    using Game.Core.Communication.Foundation;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// An implementation of <see cref="ITransactionWeightVerificationDependency"/> by
    /// monitoring transactional event's transaction weight.
    /// </summary>
    internal sealed class TransactionVerifier : ITransactionWeightVerificationDependency, ITransactionVerification
    {
        #region Private Fields

        private TransactionWeight currentTransactionWeight;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionVerifier"/>.
        /// </summary>
        /// <param name="transactionalEventDispatchMonitor">The object monitors transactional event dispatching.</param>
        internal TransactionVerifier(IEventDispatchMonitor transactionalEventDispatchMonitor)
        {
            if(transactionalEventDispatchMonitor != null)
            {
                transactionalEventDispatchMonitor.EventDispatchStarting += HandleEventDispatchStarting;
                transactionalEventDispatchMonitor.EventDispatchEnded += HandleEventDispatchEnded;
            }
        }

        #endregion

        #region ITransactionWeightVerificationDependency Implementation

        /// <inheritdoc/>
        void ITransactionWeightVerificationDependency.MustHaveOpenTransaction()
        {
            if(currentTransactionWeight == TransactionWeight.None)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        /// <inheritdoc />
        public void MustHaveHeavyweightTransaction()
        {
            if(currentTransactionWeight != TransactionWeight.Heavy)
            {
                var reason = currentTransactionWeight == TransactionWeight.None
                                 ? "currently no transaction is open"
                                 : "current transaction is lightweight";

                throw new InvalidTransactionException($"This operation needs heavyweight transaction but {reason}.");
            }
        }

        #endregion

        #region ITransactionVerification Implementation

        /// <inheritdoc/>
        void ITransactionVerification.MustHaveOpenTransaction()
        {
            if(currentTransactionWeight == TransactionWeight.None)
            {
                throw new InvalidTransactionException("No open transaction is available for this operation.");
            }
        }

        #endregion

        #region Private Methods

        private void HandleEventDispatchStarting(object sender, EventArgs e)
        {
            if(e is PlatformEventArgs platformEventArgs)
            {
                currentTransactionWeight = platformEventArgs.TransactionWeight;
            }
        }

        private void HandleEventDispatchEnded(object sender, EventArgs e)
        {
            currentTransactionWeight = TransactionWeight.None;
        }

        #endregion
    }
}