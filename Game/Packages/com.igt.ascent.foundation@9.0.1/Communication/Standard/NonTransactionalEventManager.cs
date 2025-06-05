// -----------------------------------------------------------------------
// <copyright file = "NonTransactionalEventManager.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2XTransport;
    using Threading;
    using Tracing;

    /// <summary>
    /// This class manages non transactional Foundation events received
    /// on FIN (Foundation Initiated Non-transactional) channel.
    /// </summary>
    internal class NonTransactionalEventManager : IEventSource, INonTransactionalEventCallbacks, IEventDispatcher, IDisposable
    {
        #region Nested Class

        /// <summary>
        /// This class maintains a record for non-transactional events. It includes the event and a flag indicating
        /// if the event must be processed before responding.
        /// </summary>
        private class EventRecord
        {
            /// <summary>
            /// Gets the event added into the queue.
            /// </summary>
            public EventArgs FoundationEvent { get; private set; }

            /// <summary>
            /// Gets the flag indicating whether the event must be processed before replying to the Foundation.
            /// </summary>
            public bool WaitForProcess { get; private set; }

            /// <summary>
            /// Initializes a new instance of <see cref="EventRecord"/>.
            /// </summary>
            /// <param name="foundationEvent">The event to add to the queue.</param>
            /// <param name="waitForProcess">The flag indicating whether the event must be processed before return.</param>
            public EventRecord(EventArgs foundationEvent, bool waitForProcess)
            {
                FoundationEvent = foundationEvent;
                WaitForProcess = waitForProcess;
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Event queue to hold the incoming events posted by the Foundation.
        /// </summary>
        private readonly Queue<EventRecord> eventRecords = new Queue<EventRecord>();

        /// <summary>
        /// Object to synchronize the access the event queues.
        /// </summary>
        private readonly object eventLocker = new object();

        /// <summary>
        /// Object to synchronize the posting and processing of an event.
        /// </summary>
        private readonly ManualResetEvent eventPosted = new ManualResetEvent(false);

        /// <summary>
        /// Object to synchronize the asynchronous processing of an event.
        /// </summary>
        private readonly AutoResetEvent eventProcessed = new AutoResetEvent(false);

        /// <summary>
        /// Object used to abort any blocking callback thread.
        /// </summary>
        private readonly GenericExceptionMonitor callbackExceptionMonitor = new GenericExceptionMonitor();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        #endregion

        #region IEventSource Members

        /// <inheritdoc />
        public WaitHandle EventPosted
        {
            get { return eventPosted; }
        }

        /// <inheritdoc />
        public void ProcessEvents()
        {
            GameLibTracing.Log.ProcessNonTransactionalEvents();

            var handler = EventDispatchedEvent;
            var pendingCount = -1;

            do
            {
                EventRecord pendingRecord = null;

                lock(eventLocker)
                {
                    // Initialize how many events we are going to process.
                    // For each ProcessEvents call, we only process the events that have been received so far.
                    // This is to prevent endless nested send and receive "non-waiting-for-process-events".
                    if(pendingCount == -1)
                    {
                        pendingCount = eventRecords.Count;
                    }

                    if(pendingCount > 0)
                    {
                        pendingRecord = eventRecords.Dequeue();

                        pendingCount--;

                        // If an event is waiting for process, call the event handler inside the lock.
                        // This is to make sure eventProcessed is synced.
                        if(pendingRecord.WaitForProcess)
                        {
                            if(handler != null)
                            {
                                handler(this, new EventDispatchedEventArgs(pendingRecord.FoundationEvent));
                            }
                            eventProcessed.Set();
                        }
                    }
                }

                // If an event is NOT waiting for process, call the event handler outside the lock.
                // This allows the nested calls of "non-waiting-for-process-events", such as non-transactional parcel calls.
                if(pendingRecord != null && !pendingRecord.WaitForProcess && handler != null)
                {
                    handler(this, new EventDispatchedEventArgs(pendingRecord.FoundationEvent));
                }
            } while(pendingCount > 0);

            lock(eventLocker)
            {
                // After processing pendingCount events, check if there is any event pending.
                // Do NOT reset eventPosted unless the queue is empty, otherwise there will be events left unprocessed.
                // When eventPosted is not reset, EventCoordinator will re-call this EventSource's ProcessEvents
                // right after, but only when there is no higher priority event pending.  So we should be fine.
                if(eventRecords.Count == 0)
                {
                    eventPosted.Reset();
                }
            }
        }

        #endregion

        #region INonTransactionalEventCallbacks Members

        /// <inheritdoc />
        public void EnqueueEvent(EventArgs foundationEvent)
        {
            lock(eventLocker)
            {
                eventRecords.Enqueue(new EventRecord(foundationEvent, false));

                GameLibTracing.Log.NonTransactionalEventOp(FoundationEventOp.Queued, foundationEvent);

                eventPosted.Set();
            }
        }

        /// <inheritdoc />
        public void PostEvent(EventArgs foundationEvent)
        {
            lock(eventLocker)
            {
                eventRecords.Enqueue(new EventRecord(foundationEvent, true));

                GameLibTracing.Log.NonTransactionalEventOp(FoundationEventOp.Posted, foundationEvent);

                eventPosted.Set();

                // Attaching mono debugger to game while it is handling events
                // sends an extra signal to wait handle.
                // This causes the game to crash.
                // Resetting the eventProcessed at this time ensures
                // that the window to attach the debugger which results in crash
                // is reduced significantly.
                eventProcessed.Reset();
            }

            // The event must be processed before returning to the Foundation because
            // some events need reply from the event handlers, such as GetThemeBasedGameLevelValues.
            // Wait for the signal from ProcessEvents method.
            eventProcessed.WaitOne(callbackExceptionMonitor);

            GameLibTracing.Log.NonTransactionalEventOp(FoundationEventOp.Processed, foundationEvent);
        }

        #endregion

        #region IEventDispatcher Members

        /// <inheritdoc />
        public event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;

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
                    // Unblock any waiting thread that calls into INonTransactionalEventCallbacks methods.
                    callbackExceptionMonitor.ThrowException(
                        new OperationCanceledException("Non Transactional Event Manager is being disposed."));

                    // Auto and Manual reset events are disposable
                    (eventPosted as IDisposable).Dispose();
                    (eventProcessed as IDisposable).Dispose();

                    callbackExceptionMonitor.Dispose();
                }

                disposed = true;
            }
        }

        #endregion
    }
}