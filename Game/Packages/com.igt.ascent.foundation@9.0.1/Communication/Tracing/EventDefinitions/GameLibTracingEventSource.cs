// -----------------------------------------------------------------------
// <copyright file = "GameLibTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Tracing.EventDefinitions
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Core.Tracing.EventDefinitions;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [EventSource(Name = "IGT-Ascent-Core-Communication-Foundation-Tracing-GameLibTracingEventSource")]
    internal sealed partial class GameLibTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Private singleton instance.
        /// </summary>
        private static readonly GameLibTracingEventSource Instance = new GameLibTracingEventSource();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static GameLibTracingEventSource Log
        {
            get { return Instance; }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private GameLibTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        #region Tracing Transactions

        [Event(1, Level = DefaultLevel)]
        public void CreateTransaction(string TransactionName)
        {
            WriteEvent(1, TransactionName);
        }

        [Event(2, Level = DefaultLevel)]
        public void CreateTransactionFailed(string TransactionName, string Error)
        {
            WriteEvent(2, TransactionName, Error);
        }

        [Event(3, Level = DefaultLevel)]
        public void ActionRequestSend(string TransactionName)
        {
            WriteEvent(3, TransactionName);
        }

        [Event(4, Level = DefaultLevel)]
        public void ActionResponseReceive()
        {
            WriteEvent(4);
        }

        [Event(5, Level = DefaultLevel)]
        public void TransactionCreated(string TransactionName)
        {
            WriteEvent(5, TransactionName);
        }

        [Event(6, Level = DefaultLevel)]
        public void TransactionClosed()
        {
            WriteEvent(6);
        }

        #endregion

        #region Tracing Foundation Events

        [Event(7, Level = DefaultLevel)]
        public void ProcessTransactionalEvents()
        {
            WriteEvent(7);
        }

        [Event(8, Level = DefaultLevel)]
        public void ProcessNonTransactionalEvents()
        {
            WriteEvent(8);
        }

        [NonEvent]
        public void TransactionalEventOp(FoundationEventOp foundationEventOp, EventArgs eventArgs)
        {
            if(IsEnabled())
            {
                TransactionalEventOp(foundationEventOp, eventArgs.GetType().Name);
            }
        }

        [Event(9, Level = DefaultLevel)]
        private void TransactionalEventOp(FoundationEventOp FoundationEventOp, string EventType)
        {
            WriteEvent(9, (int)FoundationEventOp, EventType);
        }


        [NonEvent]
        public void NonTransactionalEventOp(FoundationEventOp foundationEventOp, EventArgs eventArgs)
        {
            if(IsEnabled())
            {
                NonTransactionalEventOp(foundationEventOp, eventArgs.GetType().Name);
            }
        }

        [Event(10, Level = DefaultLevel)]
        private void NonTransactionalEventOp(FoundationEventOp FoundationEventOp, string EventType)
        {
            WriteEvent(10, (int)FoundationEventOp, EventType);
        }

        #endregion

        #region Tracing Game Cycle

        [Event(11, Level = DefaultLevel)]
        public void CommitGameCycleSuccess()
        {
            WriteEvent(11);
        }

        [Event(12, Level = DefaultLevel)]
        public void EndGameCycle()
        {
            WriteEvent(12);
        }

        #endregion

        #endregion
    }
}