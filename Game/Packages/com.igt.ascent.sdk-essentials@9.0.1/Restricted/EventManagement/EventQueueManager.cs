// -----------------------------------------------------------------------
// <copyright file = "EventQueueManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Game.Core.Threading;
    using Interfaces;

    /// <summary>
    /// This class manages multiple event queues, and coordinates the event processing among the queues.
    /// </summary>
    public sealed class EventQueueManager : IEventQueueManager
    {
        #region Private Fields

        private readonly List<IEventQueue> eventQueues;

        private readonly IExceptionMonitor exceptionMonitor;

        private WaitHandle[] eventQueueHandles;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="EventQueueManager"/>.
        /// </summary>
        /// <param name="exceptionMonitor">
        /// The object used to abort the blocking wait.
        /// </param>
        public EventQueueManager(IExceptionMonitor exceptionMonitor)
        {
            this.exceptionMonitor = exceptionMonitor;

            eventQueues = new List<IEventQueue>();
            UpdateEventQueueHandles();
        }

        #endregion

        #region IEventQueueManager Implementation

        /// <inheritdoc/>
        public void RegisterEventQueue(IEventQueue eventQueue)
        {
            if(eventQueue == null)
            {
                throw new ArgumentNullException(nameof(eventQueue));
            }

            if(!eventQueues.Contains(eventQueue))
            {
                eventQueues.Add(eventQueue);
                UpdateEventQueueHandles();
            }
        }

        /// <inheritdoc/>
        public void UnregisterEventQueue(IEventQueue eventQueue)
        {
            if(eventQueue == null)
            {
                throw new ArgumentNullException(nameof(eventQueue));
            }

            eventQueues.Remove(eventQueue);
            UpdateEventQueueHandles();
        }

        /// <inheritdoc/>
        public WaitHandle ProcessEvents(IList<WaitHandle> returnHandles = null,
                                        CheckEventExpectationDelegate checkExpectation = null,
                                        IList<IEventQueue> expectedQueues = null)
        {
            if(returnHandles?.Any() != true &&
               checkExpectation == null)
            {
                throw new ArgumentException("There must be a return condition specified, either a return handle" +
                                            " or a delegate to check event expectation.");
            }

            if(expectedQueues != null)
            {
                if(!expectedQueues.Any())
                {
                    throw new ArgumentException("The list of expected event queues is empty.", nameof(expectedQueues));
                }

                if(expectedQueues.Any(queue => !eventQueues.Contains(queue)))
                {
                    throw new ArgumentException("The list of expected event queues contains unregistered event queues.",
                                                nameof(expectedQueues));
                }
            }

            // Check for exceptions which may have occurred.
            var exception = exceptionMonitor?.CheckException();

            if(exception != null)
            {
                throw new RelayedException(exception);
            }

            // Process pending events and/or wait for new events.
            WaitHandle signaledReturnHandle = null;
            var expectationMet = false;
            var waiting = true;

            // Wait and process events until being told to return, having received the expected event,
            // or running into an exception.
            while(waiting)
            {
                // The priorities are: return handle (highest), followed by event queues in order of registration.
                // After processing an event, items could have been added/removed from the returnHandles list,
                // event queues registered or unregistered, handles disposed etc.  Therefore, combinedHandles must
                // be re-generated each loop to prevent ObjectDisposedException etc. from being thrown.
                var combinedHandles = returnHandles != null
                                          ? returnHandles.Concat(eventQueueHandles).ToArray()
                                          : eventQueueHandles;

                var signaled = exceptionMonitor != null
                   ? combinedHandles.WaitAny(exceptionMonitor)
                   : combinedHandles.WaitAny();

                // If return handle is signaled, return.
                if(returnHandles?.Contains(signaled) == true)
                {
                    signaledReturnHandle = signaled;
                    waiting = false;
                }
                else
                {
                    var eventQueue = eventQueues.First(queue => queue.EventReceived == signaled);

                    // Process events and if applicable, check whether expectation has been met.
                    if(checkExpectation != null && expectedQueues?.Contains(eventQueue) != false)
                    {
                        expectationMet = eventQueue.ProcessEvents(checkExpectation);
                    }
                    else
                    {
                        eventQueue.ProcessEvents();
                    }

                    waiting = !expectationMet;
                }
            }

            return signaledReturnHandle;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update <see cref="eventQueueHandles"/> based on the latest <see cref="eventQueues"/>.
        /// </summary>
        private void UpdateEventQueueHandles()
        {
            eventQueueHandles = eventQueues.Select(eventQueue => eventQueue.EventReceived).ToArray();
        }

        #endregion
    }
}