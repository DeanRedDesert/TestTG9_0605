// -----------------------------------------------------------------------
// <copyright file = "DummyTransactionVerifier.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using Game.Core.Communication.Foundation;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;

    /// <summary>
    /// A dummy implementation of <see cref="ITransactionWeightVerificationDependency"/>.
    /// </summary>
    /// <devdoc>
    /// This is temporary until we figure out whether we still need this interface
    /// to verify transaction open.
    /// Hopefully the transport object would be able to verify transaction weight
    /// for each message as defined by the schema.
    /// If not, the libs can know the transaction weight from the events being handled.
    /// </devdoc>
    internal sealed class DummyTransactionVerifier : ITransactionWeightVerificationDependency, ITransactionVerification
    {
        #region ITransactionWeightVerificationDependency Implementation

        /// <inheritdoc/>
        void ITransactionWeightVerificationDependency.MustHaveOpenTransaction()
        {
        }

        /// <inheritdoc />
        public void MustHaveHeavyweightTransaction()
        {
        }

        #endregion

        #region ITransactionVerification Implementation

        /// <inheritdoc/>
        void ITransactionVerification.MustHaveOpenTransaction()
        {
        }

        #endregion
    }
}