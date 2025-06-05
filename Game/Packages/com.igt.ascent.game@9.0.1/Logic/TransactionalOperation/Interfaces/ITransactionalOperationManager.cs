// -----------------------------------------------------------------------
// <copyright file = "ITransactionalOperationManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.TransactionalOperation.Interfaces
{
    /// <summary>
    /// This interface defines APIs for managing transactional operations
    /// submitted from multiple threads.
    /// </summary>
    public interface ITransactionalOperationManager
    {
        /// <summary>
        /// Submits a request for executing a transactional operation.
        /// Operation will be synchronized with interface passed in.
        /// </summary>
        /// <param name="transactionalOperation">
        /// Synchronization object to use with transaction request.
        /// </param>
        void SubmitTransactionRequest(ITransactionalOperationSynchronization transactionalOperation);
    }
}