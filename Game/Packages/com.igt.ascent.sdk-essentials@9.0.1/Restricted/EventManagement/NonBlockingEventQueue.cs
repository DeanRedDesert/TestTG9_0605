// -----------------------------------------------------------------------
// <copyright file = "NonBlockingEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Interfaces;

    /// <summary>
    /// An event queue that is capable of handling only non-blocking type of events.
    /// </summary>
    public class NonBlockingEventQueue : EventDispatcherBase, IEventQueue, IEventQueuer, IDisposable
    {
        #region Private Fields

        /// <summary>
        /// Object to synchronize the access to the event queue.
        /// </summary>
        private readonly object queueLocker = new object();

        /// <summary>
        /// Object to synchronize when an event is added to the queue.
        /// </summary>
        private readonly ManualResetEvent eventReceived = new ManualResetEvent(false);

        /// <summary>
        /// The event queue.
        /// </summary>
        private Queue<EventArgs> eventQueue = new Queue<EventArgs>();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool isDisposed;

        #endregion

        #region IEventQueue Implementation

        /// <inheritdoc/>
        // ReSharper disable once InconsistentlySynchronizedField
        public WaitHandle EventReceived => eventReceived;

        /// <inheritdoc/>
        public bool ProcessEvents(CheckEventExpectationDelegate checkExpectation = null)
        {
            var result = false;

            Queue<EventArgs> pendingEvents;

            lock(queueLocker)
            {
                pendingEvents = eventQueue;
                eventQueue = new Queue<EventArgs>();
                eventReceived.Reset();
            }

            foreach(var pendingEvent in pendingEvents)
            {
                result |= DispatchEventForProcessing(pendingEvent, checkExpectation);
            }

            return result;
        }

        #endregion

        #region IEventQueuer Implementation

        /// <inheritdoc/>
        public void EnqueueEvent(EventArgs eventArgs)
        {
            lock(queueLocker)
            {
                eventQueue.Enqueue(eventArgs);

                eventReceived.Set();
            }
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
        private void Dispose(bool disposing)
        {
            if(!isDisposed)
            {
                if(disposing)
                {
                    // Auto and Manual reset events are disposable
                    // ReSharper disable once InconsistentlySynchronizedField
                    (eventReceived as IDisposable).Dispose();
                }

                isDisposed = true;
            }
        }

        #endregion
    }
}