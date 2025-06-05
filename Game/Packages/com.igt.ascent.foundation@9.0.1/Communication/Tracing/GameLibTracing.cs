// -----------------------------------------------------------------------
// <copyright file = "GameLibTracing.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Tracing
{
    using System;
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// This class provides APIs for tracing game logic events occurred at the Game Lib level.
    /// </summary>
    public sealed class GameLibTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Private singleton instance.
        /// </summary>
        private static readonly GameLibTracing Instance = new GameLibTracing();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLibTracing Log
        {
            get { return Instance; }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLibTracing()
        {
        }

        #endregion

        #region Tracing Methods

        #region Tracing Transactions

        /// <summary>
        /// Tracing event indicating that an attempt to create transaction is made.
        /// </summary>
        /// <param name="transactionName">The transaction name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CreateTransaction(string transactionName)
        {
            GameLibTracingEventSource.Log.CreateTransaction(transactionName);
        }

        /// <summary>
        /// Tracing event indicating that an attempt to create transaction is failed.
        /// </summary>
        /// <param name="transactionName">The transaction name.</param>
        /// <param name="error">The description of the error.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CreateTransactionFailed(string transactionName, string error)
        {
            GameLibTracingEventSource.Log.CreateTransactionFailed(transactionName, error);
        }

        /// <summary>
        /// Tracing event indicating that an F2X ActionRequest message is sent out.
        /// </summary>
        /// <param name="transactionName">The transaction name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ActionRequestSend(string transactionName)
        {
            GameLibTracingEventSource.Log.ActionRequestSend(transactionName);
        }

        /// <summary>
        /// Tracing event indicating that an F2X ActionResponse message is received.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ActionResponseReceive()
        {
            GameLibTracingEventSource.Log.ActionResponseReceive();
        }

        /// <summary>
        /// Tracing event indicating that a transaction is created successfully.
        /// </summary>
        /// <param name="transactionName">The transaction name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void TransactionCreated(string transactionName)
        {
            GameLibTracingEventSource.Log.TransactionCreated(transactionName);
        }

        /// <summary>
        /// Tracing event indicating that a transaction is closed.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void TransactionClosed()
        {
            GameLibTracingEventSource.Log.TransactionClosed();
        }

        #endregion

        #region Tracing Foundation Events

        /// <summary>
        /// Tracing event indicating that transactional events are being processed.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ProcessTransactionalEvents()
        {
            GameLibTracingEventSource.Log.ProcessTransactionalEvents();
        }

        /// <summary>
        /// Tracing event indicating that non-transactional events are being processed.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ProcessNonTransactionalEvents()
        {
            GameLibTracingEventSource.Log.ProcessNonTransactionalEvents();
        }

        /// <summary>
        /// Tracing event indicating an operation of a transactional event.
        /// </summary>
        /// <param name="foundationEventOp">The operation performed on the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void TransactionalEventOp(FoundationEventOp foundationEventOp, EventArgs eventArgs)
        {
            GameLibTracingEventSource.Log.TransactionalEventOp(foundationEventOp, eventArgs);
        }

        /// <summary>
        /// Tracing event indicating an operation of a non-transactional event.
        /// </summary>
        /// <param name="foundationEventOp">The operation performed on the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void NonTransactionalEventOp(FoundationEventOp foundationEventOp, EventArgs eventArgs)
        {
            GameLibTracingEventSource.Log.NonTransactionalEventOp(foundationEventOp, eventArgs);
        }

        #endregion

        #region Tracing Game Cycle

        /// <summary>
        /// Tracing event indicating that a game cycle is successfully committed.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void CommitGameCycleSuccess()
        {
            GameLibTracingEventSource.Log.CommitGameCycleSuccess();
        }

        /// <summary>
        /// Tracing event indicating that a game cycle is ended.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void EndGameCycle()
        {
            GameLibTracingEventSource.Log.EndGameCycle();
        }

        #endregion

        #endregion
    }
}