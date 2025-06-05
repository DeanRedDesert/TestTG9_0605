// -----------------------------------------------------------------------
// <copyright file = "ShellParcelCallSender.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform.Interfaces;
    using TransactionalOperation;
    using TransactionalOperation.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IShellParcelCallSender"/>.
    /// </summary>
    internal sealed class ShellParcelCallSender : IShellParcelCallSender
    {
        private readonly IGameParcelComm gameParcelComm;
        private readonly ITransactionalOperationManager transactionalOperationManager;

        #region Constructors

        /// <devdoc>
        /// Internal class skips argument check.  The caller is responsible for passing in valid argument.
        /// </devdoc>
        internal ShellParcelCallSender(IGameParcelComm gameParcelComm,
                                       ITransactionalOperationManager transactionalOperationManager)
        {
            this.gameParcelComm = gameParcelComm;
            this.transactionalOperationManager = transactionalOperationManager;
        }

        #endregion

        #region IShellParcelCallSender Implementation

        // This interface is to be used by coplayer runners.
        // Therefore, its implementation must be thread safe.

        /// <inheritdoc/>
        public ParcelCallStatus SendNonTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            // Thread safety is provided down in F2XTransport.AcquireChannel.
            return gameParcelComm.SendNonTransactionalParcelCall(targetEndpoint, payload);
        }

        /// <inheritdoc/>
        public ParcelCallResult SendTransactionalParcelCall(ParcelCommEndpoint targetEndpoint, byte[] payload)
        {
            ParcelCallResult result = null;

            var transactionalOperation = new TransactionalOperation(transactionalOperationManager,
                                                                    "ShellParcelCallSender.SendTransactionalParcelCall");

            var invoked = transactionalOperation.Invoke(
                () => result = gameParcelComm.SendTransactionalParcelCall(targetEndpoint, payload));

            return invoked ? result : new ParcelCallResult(ParcelCallStatus.Unavailable, null);
        }

        #endregion
    }
}