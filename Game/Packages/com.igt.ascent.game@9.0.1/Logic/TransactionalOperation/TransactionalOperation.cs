// -----------------------------------------------------------------------
// <copyright file = "TransactionalOperation.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.TransactionalOperation
{
    using System;
    using System.Threading;
    using Game.Core.Threading;
    using Interfaces;

    /// <summary>
    /// This class can be used to track individual functions that need to operate in a transaction.
    /// Note that this class invokes the function on the caller's thread, so it is more suitable for
    /// operations that requires thread safety.
    /// </summary>
    public sealed class TransactionalOperation : ITransactionalOperationSynchronization, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Object that manages transactional operations.
        /// </summary>
        private readonly ITransactionalOperationManager transactionalOperationManager;

        /// <summary>
        /// Auto Reset Event used to signal when a transaction is open.
        /// </summary>
        private readonly AutoResetEvent transactionOpened = new AutoResetEvent(false);

        /// <summary>
        /// Auto Reset Event used to signal when a transaction is ready to be closed.
        /// </summary>
        private readonly AutoResetEvent transactionReadyToBeClosed = new AutoResetEvent(false);

        /// <summary>
        /// Auto Reset Event used to signal when a transaction request has been canceled.
        /// </summary>
        private readonly AutoResetEvent transactionCanceled = new AutoResetEvent(false);

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionalOperation"/>.
        /// </summary>
        /// <param name="transactionalOperationManager">
        /// Object that manages transactional operations.
        /// </param>
        /// <param name="operationName">
        /// Name of the queued operation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionalOperationManager"/> is null.
        /// </exception>
        public TransactionalOperation(ITransactionalOperationManager transactionalOperationManager,
                                      string operationName)
        {
            this.transactionalOperationManager = transactionalOperationManager ??
                                                 throw new ArgumentNullException(nameof(transactionalOperationManager));
            Name = operationName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Invokes an action function within a transaction.
        /// Blocks the caller's thread until a transaction is opened and the delegate is fired.
        /// </summary>
        /// <param name="action">
        /// Function to execute within the transaction.
        /// </param>
        /// <returns>
        /// True if function was successfully executed within the transaction.
        /// False if a transaction couldn't be opened.
        /// </returns>
        /// <remarks>
        /// Currently the transactional operation name is only used for debugging purposes
        /// and no validation is checked.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="action"/> is null.
        /// </exception>
        public bool Invoke(Action action)
        {
            if(action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var result = false;

            transactionalOperationManager.SubmitTransactionRequest(this);

            var waitHandles = new WaitHandle[]
                                  {
                                      transactionOpened,
                                      transactionCanceled,
                                  };

            var setter = waitHandles.WaitAny();

            if(setter == transactionOpened)
            {
                try
                {
                    action();
                }
                finally
                {
                    transactionReadyToBeClosed.Set();
                }

                result = true;
            }

            return result;
        }

        #endregion

        #region ITransactionalOperationSynchronization Implementation

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        void ITransactionalOperationSynchronization.FireDelayedTransaction()
        {
            transactionOpened.Set();
            transactionReadyToBeClosed.WaitOne();
        }

        /// <inheritdoc/>
        void ITransactionalOperationSynchronization.CancelTransaction()
        {
            transactionCanceled.Set();
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if(isDisposed)
            {
                return;
            }

            if(disposing)
            {
                // Auto and Manual reset events are disposable
                (transactionOpened as IDisposable).Dispose();
                (transactionReadyToBeClosed as IDisposable).Dispose();
                (transactionCanceled as IDisposable).Dispose();
            }

            isDisposed = true;
        }

        #endregion
    }
}