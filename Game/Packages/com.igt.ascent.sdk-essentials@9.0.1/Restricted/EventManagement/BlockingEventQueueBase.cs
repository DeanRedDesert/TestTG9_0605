// -----------------------------------------------------------------------
// <copyright file = "BlockingEventQueueBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Game.Core.Threading;
    using Interfaces;

    /// <summary>
    /// Base class for the event queue that is capable of handling blocking type of events.
    /// </summary>
    public abstract class BlockingEventQueueBase : EventDispatcherBase, IEventQueue, IEventPoster, IDisposable
    {
        #region Private Fields
        /// <summary>
        /// Objects to control number of blocking events being handled.
        /// </summary>
        private readonly Semaphore blockingSemaphore;

        /// <summary>
        /// The maximum number of blocking events that can be handled.
        /// </summary>
        private readonly int blockingCapacity;

        /// <summary>
        /// Objects to synchronize when the events are processed and removed from the queue.
        /// </summary>
        private readonly ManualResetEvent[] eventProcessedHandles;

        /// <summary>
        /// Object used to abort any blocking thread.
        /// </summary>
        private readonly GenericExceptionMonitor callbackExceptionMonitor = new GenericExceptionMonitor();

        /// <summary>
        /// The index into <see cref="eventProcessedHandles"/> which is used by a blocking event.
        /// </summary>
        private int blockingIndex;

        #endregion

        #region Protected Fields

        /// <summary>
        /// The event queue.
        /// </summary>
        protected readonly Queue<EventQueueItem> ItemQueue = new Queue<EventQueueItem>();

        /// <summary>
        /// Object to synchronize the access to the event queue.
        /// </summary>
        protected readonly object QueueLocker = new object();

        /// <summary>
        /// Object to synchronize when an event is added to the queue.
        /// </summary>
        protected readonly ManualResetEvent EventReceivedHandle = new ManualResetEvent(false);

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool IsDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BlockingEventQueueBase"/>.
        /// </summary>
        /// <param name="blockingEventCapacity">
        /// The maximum number of blocking events this queue can handle at a time.
        /// This parameter is optional.  If not specified, it defaults to 1.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="blockingEventCapacity"/> is less than 1.
        /// </exception>
        protected BlockingEventQueueBase(int blockingEventCapacity = 1)
        {
            if(blockingEventCapacity < 1)
            {
                throw new ArgumentException("The capacity of blocking events cannot be less than 1.",
                                            nameof(blockingEventCapacity));
            }

            blockingSemaphore = new Semaphore(blockingEventCapacity, blockingEventCapacity);

            eventProcessedHandles = new ManualResetEvent[blockingEventCapacity];
            for(var i = 0; i < blockingEventCapacity; i++)
            {
                eventProcessedHandles[i] = new ManualResetEvent(false);
            }

            blockingCapacity = blockingEventCapacity;
            blockingIndex = 0;
        }

        #endregion

        #region IEventQueue Implementation

        /// <inheritdoc/>
        public WaitHandle EventReceived => EventReceivedHandle;

        /// <inheritdoc/>
        public abstract bool ProcessEvents(CheckEventExpectationDelegate checkExpectation = null);

        #endregion

        #region IEventPoster Implementation

        /// <inheritdoc />
        public void PostEvent(EventArgs eventArgs, WaitHandle sigInterrupt = null, Action interruptHandler = null)
        {
            blockingSemaphore.WaitOne();

            EventQueueItem newItem;

            lock(QueueLocker)
            {
                newItem = new EventQueueItem(eventArgs, true, eventProcessedHandles[GetBlockingIndex()]);

                ItemQueue.Enqueue(newItem);

                EventReceivedHandle.Set();

                // Attaching mono debugger to game while it is handling events
                // sends an extra signal to wait handle.
                // This causes the game to crash.
                // Resetting the eventProcessed at this time ensures
                // that the window to attach the debugger which results in crash
                // is reduced significantly.
                newItem.EventProcessed.Reset();
            }

            if(sigInterrupt != null && interruptHandler != null)
            {
                while(!newItem.EventProcessed.WaitOne(0))
                {
                    var waitHandles = new[] { newItem.EventProcessed, sigInterrupt };
                    var signaled = waitHandles.WaitAny(callbackExceptionMonitor);

                    // If the waiting has been interrupted, execute the handler then go back to waiting.
                    if(signaled == sigInterrupt)
                    {
                        interruptHandler();
                    }
                }
            }
            else
            {
                // The event must be processed before returning.
                // Wait for the signal from ProcessEvents method.
                newItem.EventProcessed.WaitOne(callbackExceptionMonitor);
            }

            blockingSemaphore.Release();
        }

        #endregion

        #region IDisposable Implementation

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
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void Dispose(bool disposing)
        {
            if(!IsDisposed)
            {
                if(disposing)
                {
                    // Unblock any waiting thread this waiting in the PostEvent call.
                    callbackExceptionMonitor.ThrowException(
                        new OperationCanceledException("BlockingEventQueue is being disposed."));

                    // Auto and Manual reset events are disposable
                    (EventReceivedHandle as IDisposable).Dispose();

                    foreach(var eventProcessed in eventProcessedHandles)
                    {
                        ((IDisposable)eventProcessed).Dispose();
                    }

                    (blockingSemaphore as IDisposable).Dispose();

                    callbackExceptionMonitor.Dispose();
                }

                IsDisposed = true;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the current index of the event processed handles to use.
        /// </summary>
        /// <returns>The current index of the event processed handles to use.</returns>
        private int GetBlockingIndex()
        {
            var result = blockingIndex;

            if(blockingCapacity > 1)
            {
                blockingIndex = (blockingIndex + 1) % blockingCapacity;
            }

            return result;
        }

        #endregion
    }
}