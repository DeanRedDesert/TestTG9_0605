//-----------------------------------------------------------------------
// <copyright file = "TransactionManager.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Threading;
    using Tracing;

    /// <summary>
    /// This class manages both game initiated transactions and
    /// Foundation initiated transactions (i.e. Foundation events).
    /// </summary>
    internal class TransactionManager : IEventSource, IEventDispatcher, ITransactionAugmenter, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// The link to the Foundation that communicates transactions and events.
        /// </summary>
        private readonly ITransactionEventLink transactionEventLink;

        /// <summary>
        /// Event queue to hold the incoming events posted by the Foundation.
        /// </summary>
        /// <remarks>Guarded by eventLocker.</remarks>
        /// <devdoc>
        /// There may not be a way for there to be multiple events
        /// but for now the lib will assume support for it.
        /// </devdoc>
        private readonly List<EventArgs> foundationEvents = new List<EventArgs>();

        /// <summary>
        /// Synchronization object to support creating a transaction asynchronously.
        /// </summary>
        private readonly AutoResetEvent createTransaction = new AutoResetEvent(false);

        /// <summary>
        /// Synchronization object to support closing a transaction asynchronously.
        /// </summary>
        private readonly AutoResetEvent closeTransaction = new AutoResetEvent(false);

        /// <summary>
        /// Reset event used to indicate if an event has been received while
        /// an attempt to open a transaction is in progress.
        /// </summary>
        /// <remarks>
        /// If a Foundation event is received while attempting to open a transaction,
        /// then the client must process the event first. This reset event allows
        /// CreateTransaction to return to the client code so that it may process the event.
        /// </remarks>
        private readonly ManualResetEvent eventPostedCancelCreate = new ManualResetEvent(false);

        /// <summary>
        /// Reset event used to indicate that an event has been posted and is available to process.
        /// </summary>
        private readonly AutoResetEvent foundationEventPosted = new AutoResetEvent(false);

        /// <summary>
        /// Synchronization object to support processing an event asynchronously.
        /// </summary>
        private readonly AutoResetEvent foundationEventProcessed = new AutoResetEvent(false);

        /// <summary>
        /// Object used to abort any blocking transport thread.
        /// </summary>
        private readonly IExceptionMonitor transportExceptionMonitor;

        /// <summary>
        /// Object used to abort any blocking callback thread.
        /// </summary>
        private readonly GenericExceptionMonitor callbackExceptionMonitor = new GenericExceptionMonitor();

        /// <summary>
        /// Flag indicating if there is a game initiated transaction pending.
        /// </summary>
        private bool gameTransactionPending;

        /// <summary>
        /// Flag indicating if a Foundation event is pending for process.
        /// Set to true by event posting.
        /// Set to false at the end of event processing.
        /// </summary>
        private volatile bool foundationEventPending;

        /// <summary>
        /// Flag indicating if a Foundation initiated transaction is open.
        /// This is true when the pending Foundation event has a transaction id.
        /// </summary>
        private volatile bool foundationTransactionOpen;

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region Properties

        /// <summary>
        /// Get the flag indicating whether a foundation event is pending for process.
        /// This is for internal use only for now.
        /// </summary>
        private bool FoundationEventPending
        {
            // For best thread safety, we guard the getter with lock,
            // since the value is always set in the lock, along with
            // other event operations.
            get
            {
                lock(foundationEvents)
                {
                    return foundationEventPending;
                }
            }
            set
            {
                lock(foundationEvents)
                {
                    foundationEventPending = value;
                }
            }
        }

        /// <summary>
        /// Get the flag indicating whether a foundation initiated transaction is open.
        /// This is for internal use only for now.
        /// </summary>
        private bool FoundationTransactionOpen
        {
            // For best thread safety, we guard the getter with lock,
            // since the value is always set in the lock, along with
            // other event operations.
            get
            {
                lock(foundationEvents)
                {
                    return foundationTransactionOpen;
                }
            }
            set
            {
                lock(foundationEvents)
                {
                    foundationTransactionOpen = value;
                }
            }
        }

        /// <summary>
        /// Get the flag indicating whether a game initiated transaction is open.
        /// </summary>
        /// <DevDoc>
        /// Made this virtual for testing only.
        /// </DevDoc>
        public virtual bool GameTransactionOpen { get; private set; }

        /// <summary>
        /// Get the flag indicating whether a transaction, initiated by
        /// either the game or the Foundation, is open.
        /// </summary>
        public bool TransactionOpen
        {
            get
            {
                return GameTransactionOpen || FoundationTransactionOpen;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an instance of <see cref="TransactionManager"/>.
        /// </summary>
        /// <param name="transactionEventLink">
        /// The link to the Foundation for communicating transactions and events.
        /// </param>
        /// <param name="transportExceptionMonitor">
        /// The object used to abort any blocking transport thread.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="transactionEventLink"/> or
        /// <paramref name="transportExceptionMonitor"/> is null.
        /// </exception>
        internal TransactionManager(ITransactionEventLink transactionEventLink, IExceptionMonitor transportExceptionMonitor)
        {
            if(transactionEventLink == null)
            {
                throw new ArgumentNullException("transactionEventLink");
            }

            if(transportExceptionMonitor == null)
            {
                throw new ArgumentNullException("transportExceptionMonitor");
            }

            this.transactionEventLink = transactionEventLink;
            this.transportExceptionMonitor = transportExceptionMonitor;

            transactionEventLink.PostingEvent += ProcessPostingEvent;
            transactionEventLink.ActionResponseEvent += ProcessActionResponse;
        }

        #endregion

        #region Events

        /// <summary>
        /// Event occurs when a transaction, either initiated by
        /// the game or the Foundation, needs to be finalized.
        /// </summary>
        /// <remarks>
        /// This is to separate the action of finalizing a transaction,
        /// such as committing pending critical data writes, from the
        /// TransactionClosingEvent, where the event handlers could
        /// write some critical data.
        /// Usually no one other than Game Lib should register for this event.
        /// </remarks>
        public event EventHandler FinalizeTransactionEvent;

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a game initiated transaction with the Foundation with a name.
        /// </summary>
        /// <param name="name">Name of the transaction to be put into the request payload.</param>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if a game initiated transaction is created successfully.
        /// <see cref="ErrorCode.OpenTransactionExisted"/>
        /// if a game initiated transaction is already open.
        /// <see cref="ErrorCode.EventWaitingForProcess"/>
        /// if there are events waiting in queue to be processed.
        /// The game should go process the events when receiving
        /// this error code.
        /// </returns>
        public ErrorCode CreateTransaction(string name)
        {
            ErrorCode result;

            GameLibTracing.Log.CreateTransaction(name);

            if(FoundationEventPending)
            {
                result = ErrorCode.EventWaitingForProcess;

                GameLibTracing.Log.CreateTransactionFailed(name, "EventWaitingForProcess");
            }
            else if(GameTransactionOpen)
            {
                result = ErrorCode.OpenTransactionExisted;

                GameLibTracing.Log.CreateTransactionFailed(name, "OpenTransactionExisted");
            }
            else
            {
                // If a request has already been made, then do not make a new one.
                if(!gameTransactionPending)
                {
                    var nameInBytes = string.IsNullOrEmpty(name) ? new byte[0] : Encoding.ASCII.GetBytes(name);

                    var sendSuccess = transactionEventLink.ActionRequest(nameInBytes);

                    GameLibTracing.Log.ActionRequestSend(name);

                    if(!sendSuccess)
                    {
                        GameLibTracing.Log.CreateTransactionFailed(name, "GeneralError");
                        return ErrorCode.GeneralError;
                    }
                }

                // Wait for an Action Response from Foundation, which signals
                // the transaction being opened, or abort waiting when an event
                // has arrived.
                var waitHandles = new WaitHandle[]
                                      {
                                          createTransaction,
                                          eventPostedCancelCreate
                                      };

                var signaled = waitHandles.WaitAny(transportExceptionMonitor);

                if(signaled == createTransaction)
                {
                    result = ErrorCode.NoError;
                    gameTransactionPending = false;
                    GameTransactionOpen = true;

                    GameLibTracing.Log.TransactionCreated(name);
                }
                else if(signaled == eventPostedCancelCreate)
                {
                    result = ErrorCode.EventWaitingForProcess;
                    gameTransactionPending = true;
                    GameTransactionOpen = false;

                    GameLibTracing.Log.CreateTransactionFailed(name, "EventPostedCancelCreate");
                }
                else
                {
                    // Should never reach here.
                    result = ErrorCode.GeneralError;

                    GameLibTracing.Log.CreateTransactionFailed(name, "ShouldNeverHappen");
                }
            }

            return result;
        }

        /// <summary>
        /// Close an open game initiated transaction with the foundation.
        /// </summary>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if a game initiated transaction is closed successfully.
        /// <see cref="ErrorCode.NoTransactionOpen"/>
        /// if no open game initiated transaction is available to be closed.
        /// </returns>
        public ErrorCode CloseTransaction()
        {
            ErrorCode result;

            if(GameTransactionOpen)
            {
                RaiseEvent(TransactionClosingEvent, EventArgs.Empty);

                RaiseEvent(FinalizeTransactionEvent, EventArgs.Empty);

                GameTransactionOpen = false;

                // Signal that the transaction can be closed now.
                closeTransaction.Set();

                result = ErrorCode.NoError;
            }
            else
            {
                result = ErrorCode.NoTransactionOpen;
            }

            return result;
        }

        /// <summary>
        /// Clear the pending game initiated transaction.
        /// </summary>
        public void ClearPendingTransaction()
        {
            gameTransactionPending = false;
        }

        #endregion

        #region IEventSource Members

        /// <inheritdoc />
        public WaitHandle EventPosted
        {
            get { return foundationEventPosted; }
        }

        /// <inheritdoc />
        public void ProcessEvents()
        {
            // If there is a game initiated transaction open, there would be
            // no event in the queue to be processed.
            if(!GameTransactionOpen)
            {
                lock(foundationEvents)
                {
                    GameLibTracing.Log.ProcessTransactionalEvents();

                    if(foundationEvents.Count > 0)
                    {
                        foreach(var foundationEvent in foundationEvents)
                        {
                            var handler = EventDispatchedEvent;

                            if(handler != null)
                            {
                                handler(this, new EventDispatchedEventArgs(foundationEvent));
                            }
                        }
                    }

                    foundationEvents.Clear();

                    RaiseEvent(TransactionClosingEvent, EventArgs.Empty);

                    RaiseEvent(FinalizeTransactionEvent, EventArgs.Empty);

                    // End of processing event means the closing of the Foundation initiated transaction.
                    FoundationEventPending = false;
                    FoundationTransactionOpen = false;

                    // Signal post event that the previously posted event has been processed.
                    foundationEventProcessed.Set();

                    // Events have been processed, so currently there is not one pending.
                    // Client code can attempt to open a transaction.
                    eventPostedCancelCreate.Reset();
                }
            }
        }

        #endregion

        #region IEventDispatcher Members

        /// <inheritdoc/>
        public event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;

        #endregion

        #region ITransactionAugmenter Members

        /// <inheritdoc />
        public event EventHandler TransactionClosingEvent;

        #endregion

        #region IDisposable Members

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
            if(!disposed)
            {
                if(disposing)
                {
                    // Unblock any waiting thread that fires ITransactionEventLink callback event.
                    callbackExceptionMonitor.ThrowException(
                        new OperationCanceledException("Transaction Manager is being disposed."));

                    // Auto and Manual reset events are disposable
                    (createTransaction as IDisposable).Dispose();
                    (closeTransaction as IDisposable).Dispose();
                    (eventPostedCancelCreate as IDisposable).Dispose();
                    (foundationEventProcessed as IDisposable).Dispose();
                    (foundationEventPosted as IDisposable).Dispose();

                    callbackExceptionMonitor.Dispose();
                }

                disposed = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Post an event received from the Foundation to the event queue.
        /// </summary>
        /// <param name="sender">The event sender.  Not used.</param>
        /// <param name="foundationEvent">The event to post.</param>
        private void ProcessPostingEvent(object sender, EventArgs foundationEvent)
        {
            lock(foundationEvents)
            {
                // An event being posted means a Foundation initiated transaction
                // is in process.
                FoundationEventPending = true;

                // The events implemented prior to PlatformEventArgs are all considered Heavyweight.
                // The events derived from PlatformEventArgs should be checked for TransactionWeight.
                // TransactionManager only handles Heavyweight transactions.
                var platformEvent = foundationEvent as PlatformEventArgs;
                FoundationTransactionOpen = platformEvent == null || platformEvent.IsHeavyweight;

                foundationEvents.Add(foundationEvent);

                GameLibTracing.Log.TransactionalEventOp(FoundationEventOp.Posted, foundationEvent);

                // The reset events are inside the lock to keep ProcessEvents from
                // resetting eventPostedCancelCreate before it is set here.

                // If a CreateTransaction call is pending, tell it to return immediately.
                eventPostedCancelCreate.Set();

                //Signal process events that an event is available.
                foundationEventPosted.Set();

                //Attaching mono debugger to game while it is handling events
                //sends an extra signal to wait handle.
                //This causes the game to crash.
                //Resetting the foundationEventProcessed at this time ensures
                //that the window to attach the debugger which results in crash
                //is reduced significantly.
                //TO-DO : Investigate more in future the reason why Mono sends in
                //extra waitHandle when attached to the game.
                foundationEventProcessed.Reset();
            }

            // The event must be processed before returning to the Foundation.
            // Wait for the signal from ProcessEvents method.
            foundationEventProcessed.WaitOne(callbackExceptionMonitor);

            GameLibTracing.Log.TransactionalEventOp(FoundationEventOp.Processed, foundationEvent);
        }

        /// <summary>
        /// Process an Action Response received from the Foundation.
        /// </summary>
        /// <param name="sender">The event sender.  Not used.</param>
        /// <param name="eventArgs">The event data.  Not used.</param>
        private void ProcessActionResponse(object sender, EventArgs eventArgs)
        {
            GameLibTracing.Log.ActionResponseReceive();

            // Signal that a new transaction has been opened.
            createTransaction.Set();

            // Wait for the signal from the CloseTransaction method.
            closeTransaction.WaitOne(callbackExceptionMonitor);

            GameLibTracing.Log.TransactionClosed();
        }

        /// <summary>
        /// Raise an event.
        /// </summary>
        /// <param name="handler">The event to raise.</param>
        /// <param name="eventArgs">The event data.</param>
        private void RaiseEvent(EventHandler handler, EventArgs eventArgs)
        {
            if(handler != null)
            {
                handler(this, eventArgs);
            }
        }

        #endregion
    }
}
