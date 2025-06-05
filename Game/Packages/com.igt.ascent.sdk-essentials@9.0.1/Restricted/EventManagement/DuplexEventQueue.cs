// -----------------------------------------------------------------------
// <copyright file = "DuplexEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using Interfaces;

    /// <summary>
    /// An event queue that is capable of handling both non-blocking and blocking types of events.
    /// </summary>
    public class DuplexEventQueue : BlockingEventQueueBase, IEventQueuer
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="DuplexEventQueue"/>.
        /// </summary>
        /// <param name="blockingEventCapacity">
        /// The maximum number of blocking events this queue can handle at a time.
        /// This parameter is optional.  If not specified, it defaults to 1.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="blockingEventCapacity"/> is less than 1.
        /// </exception>
        public DuplexEventQueue(int blockingEventCapacity = 1) : base(blockingEventCapacity)
        {
        }

        #endregion

        #region BlockingEventQueueBase Overrides

        /// <inheritdoc/>
        public override bool ProcessEvents(CheckEventExpectationDelegate checkExpectation = null)
        {
            var result = false;

            var pendingCount = -1;

            do
            {
                EventQueueItem pendingItem = null;

                lock(QueueLocker)
                {
                    // Initialize how many events we are going to process.
                    // For each ProcessEvents call, we only process the events that have been received so far.
                    // This is to prevent endless nested send and receive "no-waiting-for-process-events".
                    if(pendingCount == -1)
                    {
                        pendingCount = ItemQueue.Count;
                    }

                    if(pendingCount > 0)
                    {
                        pendingItem = ItemQueue.Dequeue();

                        pendingCount--;

                        // If an event is waiting for process, call the event handler inside the lock.
                        // This is to make sure eventProcessed is synced.
                        if(pendingItem.WaitForProcess)
                        {
                            result |= DispatchEventForProcessing(pendingItem.EventArgs, checkExpectation);
                            pendingItem.EventProcessed.Set();
                        }
                    }
                }

                // If an event is NOT waiting for process, call the event handler outside the lock.
                // This allows the nested calls of "no-waiting-for-process-events", such as non-transactional parcel calls.
                if(pendingItem?.WaitForProcess == false)
                {
                    result |= DispatchEventForProcessing(pendingItem.EventArgs, checkExpectation);
                }
            } while(pendingCount > 0);

            lock(QueueLocker)
            {
                // After processing pendingCount events, check if there is any event pending.
                // Do NOT reset eventPosted unless the queue is empty, otherwise there will be events left unprocessed.
                // When eventPosted is not reset, EventCoordinator will re-call this EventSource's ProcessEvents
                // right after, but only when there is no higher priority event pending.  So we should be fine.
                if(ItemQueue.Count == 0)
                {
                    EventReceivedHandle.Reset();
                }
            }

            return result;
        }

        #endregion

        #region IEventQueuer Implementation

        /// <inheritdoc/>
        public void EnqueueEvent(EventArgs eventArgs)
        {
            lock(QueueLocker)
            {
                ItemQueue.Enqueue(new EventQueueItem(eventArgs, false, null));

                EventReceivedHandle.Set();
            }
        }

        #endregion
    }
}