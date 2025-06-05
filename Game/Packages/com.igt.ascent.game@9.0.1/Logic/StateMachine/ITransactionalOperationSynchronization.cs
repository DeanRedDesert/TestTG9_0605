//-----------------------------------------------------------------------
// <copyright file = "ITransactionalOperationSynchronization.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    /// <summary>
    /// Interface that handles synchronizing when to execute the transactional operation.
    /// </summary>
    public interface ITransactionalOperationSynchronization
    {
        /// <summary>
        /// Cancels a Transaction.
        /// </summary>
        void CancelTransaction();

        /// <summary>
        /// Trigger delayed transaction function to execute.
        /// Wait for it to finish to close the transaction.
        /// </summary>
        void FireDelayedTransaction();

        /// <summary>
        /// Name of transaction to open.
        /// </summary>
        string Name { get; }
    }
}