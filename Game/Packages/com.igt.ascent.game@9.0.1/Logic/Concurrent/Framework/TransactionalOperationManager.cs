// -----------------------------------------------------------------------
// <copyright file = "TransactionalOperationManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using TransactionalOperation.Interfaces;

    /// <summary>
    /// This class manages transactional operations submitted from multiple threads.
    /// </summary>
    internal sealed class TransactionalOperationManager : ITransactionalOperationManager, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Object to synchronize the access to the event queue.
        /// </summary>
        private readonly object queueLocker = new object();

        /// <summary>
        /// Backend field for the wait handle indicating whether a transactional operation has been requested.
        /// </summary>
        private readonly ManualResetEvent transactionalOperationRequested = new ManualResetEvent(false);

        /// <summary>
        /// The flag indicating whether any transactional operation is being executed.
        /// </summary>
        private volatile bool executionInProgress;

        /// <summary>
        /// The event queue.
        /// </summary>
        private Queue<ITransactionalOperationSynchronization> operationQueue = new Queue<ITransactionalOperationSynchronization> ();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the wait handle indicating whether a transactional operation has been requested.
        /// </summary>
        public WaitHandle TransactionalOperationRequested => transactionalOperationRequested;

        #endregion

        #region Public Methods

        /// <summary>
        /// Fires all pending operations in the queue.
        /// This is called when the caller has a heavyweight transaction open.
        /// </summary>
        public void FireQueuedOperations()
        {
            // Do not check the transactionalOperationRequested signal here, as ExecuteQueuedOperations method
            // could have reset the signal and is waiting for a transaction.

            Queue<ITransactionalOperationSynchronization> pendingOperations = null;

            lock(queueLocker)
            {
                if(operationQueue.Count > 0)
                {
                    pendingOperations = operationQueue;
                    operationQueue = new Queue<ITransactionalOperationSynchronization>();

                    transactionalOperationRequested.Reset();
                }
            }

            if(pendingOperations != null)
            {
                foreach(var pendingOperation in pendingOperations)
                {
                    pendingOperation.FireDelayedTransaction();
                }
            }
        }

        /// <summary>
        /// Executes all pending operations in the queue by requesting a transaction from Foundation.
        /// This is called when the caller is sure that there are pending operations, but has no
        /// heavyweight transaction open at the time.
        /// </summary>
        /// <devdoc>
        /// IFrameworkRunner is a little over kill.  All we need is the OnNextTransaction function.
        /// </devdoc>
        /// <param name="runner">
        /// The framework runner in charge of transactions.
        /// </param>
        public void ExecuteQueuedOperations(IFrameworkRunner runner)
        {
            string transactionName;

            lock(queueLocker)
            {
                executionInProgress = true;

                // This must be reset first, otherwise when waiting for ActionResponse,
                // event processing will return early because of this handle.
                transactionalOperationRequested.Reset();

                var opNames = string.Join(", ", operationQueue.Take(3).Select(op => op.Name).ToArray());
                transactionName = $"Executing {operationQueue.Count} Queued Operations: {opNames} etc.";
            }

            // Open a transaction in order to fire all pending operations.
            runner.OnNextTransaction(transactionName,
                                     () => FireQueuedOperations());

            // Now check if any operation has been queued during the execution.
            lock(queueLocker)
            {
                executionInProgress = false;

                if(operationQueue.Any())
                {
                    transactionalOperationRequested.Set();
                }
            }
        }

        /// <summary>
        /// Cancel all pending operations in the queue.
        /// </summary>
        public void CancelQueuedOperations()
        {
            Queue<ITransactionalOperationSynchronization> pendingOperations;

            lock(queueLocker)
            {
                pendingOperations = operationQueue;
                operationQueue = new Queue<ITransactionalOperationSynchronization>();
                transactionalOperationRequested.Reset();
            }

            foreach(var pendingOperation in pendingOperations)
            {
                pendingOperation.CancelTransaction();
            }
        }

        #endregion

        #region ITransactionalOperationManager Implementation

        /// <inheritdoc/>
        public void SubmitTransactionRequest(ITransactionalOperationSynchronization transactionalOperation)
        {
            lock(queueLocker)
            {
                operationQueue.Enqueue(transactionalOperation);

                if(!executionInProgress)
                {
                    transactionalOperationRequested.Set();
                }
            }
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose resources held by this object.
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
                (transactionalOperationRequested as IDisposable).Dispose();
            }

            isDisposed = true;
        }

        #endregion
    }
}