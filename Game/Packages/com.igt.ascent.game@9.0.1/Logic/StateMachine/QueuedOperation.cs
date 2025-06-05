//-----------------------------------------------------------------------
// <copyright file = "QueuedOperation.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;

    /// <summary>
    /// Class used to track individual functions that need to operate in a transaction.
    /// This class invokes the function on the Transaction Manager's thread.
    /// </summary>
    public class QueuedOperation : ITransactionalOperationSynchronization
    {
        /// <summary>
        /// Object in charge of handling transactions.
        /// </summary>
        private readonly ITransactionManager transactionManager;

        /// <summary>
        /// Function to execute inside of transaction.
        /// </summary>
        private TransactionalFunction transactionalFunction;

        /// <summary>
        /// Object for synchronizing the access to the transactional function.
        /// </summary>
        private readonly object funcLocker = new object();

        /// <summary>
        /// Creates an entry for executing a delegate inside of a transaction.
        /// </summary>
        /// <param name="transactionManager">Object that manages transactions.</param>
        /// <param name="transactionName">Name of transaction.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="transactionManager"/> is null.
        /// </exception>
        public QueuedOperation(ITransactionManager transactionManager, string transactionName)
        {
            if(transactionManager == null)
            {
                throw new ArgumentNullException("transactionManager");
            }

            this.transactionManager = transactionManager;
            Name = transactionName;
        }

        #region Public Methods

        /// <summary>
        /// Used to submit a function to run inside of a critical data transaction later.
        /// This method is non blocking.
        /// </summary>
        /// <param name="function">Function to execute inside of transaction.</param>
        /// <returns>
        /// True if submission succeeds, false otherwise.  A submission will fail
        /// if a function submitted before has not been executed yet.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="function"/> is null.
        /// </exception>
        /// <remarks>
        /// Currently transaction name is only used for debugging purposes and no validation is checked.
        /// </remarks>
        public bool Submit(TransactionalFunction function)
        {
            if(function == null)
            {
                throw new ArgumentNullException("function");
            }

            var canSubmit = false;
            lock(funcLocker)
            {
                if(transactionalFunction == null)
                {
                    canSubmit = true;
                    transactionalFunction = function;
                }
            }

            if(canSubmit)
            {
                transactionManager.SubmitTransactionRequest(this);
            }

            return canSubmit;
        }

        #endregion

        #region ITransactionalOperationSynchronization Members

        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <inheritdoc/>
        void ITransactionalOperationSynchronization.CancelTransaction()
        {
            lock(funcLocker)
            {
                transactionalFunction = null;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the Transactional Function is invalid when
        /// the delayed transaction is fired.
        /// </exception>
        void ITransactionalOperationSynchronization.FireDelayedTransaction()
        {
            TransactionalFunction func;

            lock(funcLocker)
            {
                func = transactionalFunction;
                transactionalFunction = null;
            }

            // Usually the function would not be null since Fire would not be
            // called by the transaction manager without calling Submit first.
            if(func == null)
            {
                throw new InvalidOperationException(string.Format("Transactional Function {0} is null when delayed transaction is fired. Make sure the transaction is submitted first.", Name));
            }

            func(transactionManager.GameLib);
        }

        #endregion
    }
}