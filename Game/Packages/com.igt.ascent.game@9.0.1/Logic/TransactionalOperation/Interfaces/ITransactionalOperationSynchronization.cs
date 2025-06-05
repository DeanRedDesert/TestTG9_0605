// -----------------------------------------------------------------------
// <copyright file = "ITransactionalOperationSynchronization.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.TransactionalOperation.Interfaces
{
    /// <summary>
    /// This interface defines APIs to synchronize a heavyweight transactional operation,
    /// such as when to execute, and when to cancel.
    /// </summary>
    /// <devdoc>
    /// It is assumed that the transactional operation to be synchronized is always a heavyweight one.
    /// For shell/coplayers, there is no need to synchronize lightweight transactions.
    /// For other use cases such as support for communal framework, we'll revisit it when the time comes.
    /// </devdoc>
    public interface ITransactionalOperationSynchronization
    {
        /// <summary>
        /// Gets the name of transactional operation.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Triggers delayed transaction function to execute.
        /// Wait for it to finish to close the transaction.
        /// </summary>
        void FireDelayedTransaction();

        /// <summary>
        /// Cancels a submitted transactional operation.
        /// </summary>
        void CancelTransaction();
    }
}