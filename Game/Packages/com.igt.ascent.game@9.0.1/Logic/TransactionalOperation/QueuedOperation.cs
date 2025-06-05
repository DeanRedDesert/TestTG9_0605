// -----------------------------------------------------------------------
// <copyright file = "QueuedOperation.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.TransactionalOperation
{
    using System;
    using Interfaces;

    /// <summary>
    /// This class can be used to track individual functions that need to operate in a transaction.
    /// Note that this class invokes the function on the Transactional Operation Manager's thread
    /// rather than the caller's thread, therefore it is more suitable for "fire and forget" and
    /// thread-safe operations.
    /// </summary>
    public class QueuedOperation : ITransactionalOperationSynchronization
    {
        #region Private Fields

        /// <summary>
        /// Object that manages transactional operations.
        /// </summary>
        private readonly ITransactionalOperationManager transactionalOperationManager;

        /// <summary>
        /// Action to execute within the transaction.
        /// </summary>
        private Action transactionalAction;

        /// <summary>
        /// Object for synchronizing the access to the transactional action.
        /// </summary>
        private readonly object actionLocker = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="QueuedOperation"/>.
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
        public QueuedOperation(ITransactionalOperationManager transactionalOperationManager,
                               string operationName)
        {
            this.transactionalOperationManager = transactionalOperationManager ??
                                                 throw new ArgumentNullException(nameof(transactionalOperationManager));
            Name = operationName;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Submits an action function to run within a transaction when available later.
        /// This method is non blocking.
        /// </summary>
        /// <param name="action">
        /// Function to execute within the transaction.
        /// </param>
        /// <returns>
        /// True if submission succeeds, false otherwise.
        /// A submission could fail if a function submitted before has not been executed yet.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="action"/> is null.
        /// </exception>
        /// <remarks>
        /// Currently the transactional operation name is only used for debugging purposes
        /// and no validation is checked.
        /// </remarks>
        public bool Submit(Action action)
        {
            if(action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var canSubmit = false;
            lock(actionLocker)
            {
                if(transactionalAction == null)
                {
                    canSubmit = true;
                    transactionalAction = action;
                }
            }

            if(canSubmit)
            {
                transactionalOperationManager.SubmitTransactionRequest(this);
            }

            return canSubmit;
        }

        #endregion

        #region ITransactionalOperationSynchronization Members

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Transactional Action is invalid when
        /// the delayed transaction is fired.
        /// </exception>
        void ITransactionalOperationSynchronization.FireDelayedTransaction()
        {
            Action action;

            lock(actionLocker)
            {
                action = transactionalAction;
                transactionalAction = null;
            }

            // Usually the function would not be null since Fire would not be
            // called by the manager if Submit has not been called first.
            if(action == null)
            {
                throw new InvalidOperationException("Transactional Action is null when delayed transaction is fired.");
            }

            action();
        }

        /// <inheritdoc/>
        void ITransactionalOperationSynchronization.CancelTransaction()
        {
            lock(actionLocker)
            {
                transactionalAction = null;
            }
        }

        #endregion
    }
}