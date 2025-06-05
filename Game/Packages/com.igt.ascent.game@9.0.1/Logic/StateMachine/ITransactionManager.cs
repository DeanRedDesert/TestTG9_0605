//-----------------------------------------------------------------------
// <copyright file = "ITransactionManager.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Manages transaction requests from multiple threads.
    /// </summary>
    public interface ITransactionManager
    {
        /// <summary>
        /// Submit a request for a transaction.  Operation will be synchronized with interface passed in.
        /// </summary>
        /// <param name="transactionData">Synchronization object to use with transaction request.</param>
        void SubmitTransactionRequest(ITransactionalOperationSynchronization transactionData);

        /// <summary>
        /// Game Lib for use with transactions.
        /// </summary>
        IGameLib GameLib { get; }
    }
}