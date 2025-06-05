//-----------------------------------------------------------------------
// <copyright file = "TransactionalOperation.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System.Threading;
    using Threading;

    /// <summary>
    /// Class used to track individual functions that need to operate in a transaction.
    /// </summary>
    public class TransactionalOperation : ITransactionalOperationSynchronization
    {
        /// <inheritdoc/>
        public string Name { get; private set; }

        /// <summary>
        /// Object in charge of handling transactions.
        /// </summary>
        public ITransactionManager TransactionManager { get; private set; }

        /// <summary>
        /// Auto Reset Event used to signal when a transaction is open.
        /// </summary>
        private readonly AutoResetEvent transactionOpened;

        /// <summary>
        /// Auto Reset Event used to signal when a transaction is ready to be closed.
        /// </summary>
        private readonly AutoResetEvent transactionReadyToBeClosed;

        /// <summary>
        /// Auto Reset Event used to signal when a transaction request has been canceled.
        /// </summary>
        private readonly AutoResetEvent transactionCanceled;

        /// <summary>
        /// Cancels a Transaction.
        /// </summary>
        void ITransactionalOperationSynchronization.CancelTransaction()
        {
            transactionCanceled.Set();
        }

        /// <summary>
        /// Creates an entry for executing a delegate inside of a transaction.
        /// </summary>
        /// <param name="transactionManager">Object that managers transactions</param>
        /// <param name="transactionName">Name of transaction.</param>
        public TransactionalOperation(ITransactionManager transactionManager, string transactionName)
        {
            TransactionManager = transactionManager;
            transactionOpened = new AutoResetEvent(false);
            transactionReadyToBeClosed = new AutoResetEvent(false);
            transactionCanceled = new AutoResetEvent(false);
            Name = transactionName;
        }

        /// <summary>
        /// Triggers delayed transaction function to execute.
        /// Wait for it to finish to close the transaction.
        /// </summary>
        void ITransactionalOperationSynchronization.FireDelayedTransaction()
        {
            transactionOpened.Set();
            transactionReadyToBeClosed.WaitOne();
        }

        /// <summary>
        /// Used to invoke a function inside of a critical data transaction.
        /// Blocks thread until a transaction is opened and delegate is fired.
        /// </summary>
        /// <param name="func">Function to execute inside of transaction.</param>
        /// <returns>
        /// Returns true if function was successfully executed inside of a transaction. 
        /// False if a transaction couldn't be opened.
        /// </returns>
        /// <remarks>Currently transaction name is only used for debugging purposes and no validation is checked.</remarks>
        public bool Invoke(TransactionalFunction func)
        {
            var result = false;

            TransactionManager.SubmitTransactionRequest(this);
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
                    func(TransactionManager.GameLib);
                }
                finally
                {
                    transactionReadyToBeClosed.Set();
                }

                result = true;
            }

            return result;
        }
    }
}