//-----------------------------------------------------------------------
// <copyright file = "EventCoordinator.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Threading;
    using Timing;

    /// <summary>
    /// This class coordinates the event processing among multiple event sources.
    /// </summary>
    /// <remarks>
    /// Currently it only coordinates between one blocking main event source
    /// and other non-blocking event sources.  In the future, it may be expanded
    /// to handle more generic scenarios where a blocking main event source
    /// needs not present.
    /// </remarks>
    public class EventCoordinator : IEventCoordinator, IEventProcessing
    {
        #region Fields

        /// <summary>
        /// The object used to abort the blocking wait.
        /// </summary>
        private readonly IExceptionMonitor exceptionMonitor;

        /// <summary>
        /// The main event source which would block and wait for new event.
        /// </summary>
        private readonly IEventSource mainEventSource;

        /// <summary>
        /// The list of non-blocking event sources.
        /// </summary>
        private readonly List<IEventSource> eventSources = new List<IEventSource>();

        /// <summary>
        /// The array that holds the EventPosted wait handle of the main event source.
        /// This is to speed up the array concatenation in ProcessEvents.
        /// </summary>
        private readonly WaitHandle[] mainSourceHandle;

        /// <summary>
        /// The array of EventPosted wait handles of all event sources.
        /// </summary>
        private WaitHandle[] eventSourceHandles = new WaitHandle[0];

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="EventCoordinator"/> that has
        /// a main event source for which the event coordinator will block
        /// the thread and wait for its events.
        /// </summary>
        /// <param name="mainEventSource">
        /// The main event source for which the event coordinator would
        /// block and wait for its new event.
        /// </param>
        /// <param name="exceptionMonitor">
        /// The object used to abort the blocking wait.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="mainEventSource"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="mainEventSource"/> has invalid
        /// <see cref="IEventSource.EventPosted"/> wait handle.
        /// </exception>
        public EventCoordinator(IEventSource mainEventSource, IExceptionMonitor exceptionMonitor)
        {
            if(mainEventSource == null)
            {
                throw new ArgumentNullException("mainEventSource");
            }

            if(mainEventSource.EventPosted == null)
            {
                throw new ArgumentException(
                    "Main event source must have a valid wait handle for event posted signal.");
            }

            this.mainEventSource = mainEventSource;
            this.exceptionMonitor = exceptionMonitor;

            mainSourceHandle = new [] { mainEventSource.EventPosted };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Processes pending events from all event sources, or wait for an event
        /// to be posted to the main blocking event source and then process it.
        /// If any of the passed waitHandles are signaled, the function will unblock.
        /// </summary>
        /// <param name="timeout">
        /// If no events are available on the main event source, then this specifies
        /// the amount of time for the event coordinator to wait for an event.
        /// If the timeout is 0, then the function will return immediately after
        /// processing any pending events.
        /// If the timeout is Timeout.Infinite, then the function will not return until
        /// an event has been processed.
        /// </param>
        /// <param name="waitHandles">
        /// An array of additional wait handles that will unblock the function.
        /// </param>
        /// <returns>
        /// The supplied wait handle that unblocked the function, or <see langword="null"/>
        /// if the function was unblocked for another reason.
        /// </returns>
        public WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles)
        {
            // Check for exceptions which may have occurred.
            var exception = exceptionMonitor?.CheckException();

            if(exception != null)
            {
                throw new RelayedException(exception);
            }

            // Process the pending non blocking events first.
            // This ensures that non blocking events get processed
            // at least once in this method.
            var nonBlockedEventSignalled = false;
            if(eventSourceHandles.WaitAny(0) != null)
            {
                nonBlockedEventSignalled = true;
                foreach(var eventSource in eventSources)
                {
                    eventSource.ProcessEvents();
                }
            }

            if(nonBlockedEventSignalled)
            {
                RaiseEventPostProcessed();
            }

            // If there is event already posted to the main event source, process it and return.
            if(mainEventSource.EventPosted.WaitOne(0))
            {
                mainEventSource.ProcessEvents();
                RaiseEventPostProcessed();
                return null;
            }

            // Add event sources' wait handles to the passed in array.
            // The priorities are: main event source > passed in handles > non blocking event sources.
            var combinedHandles = waitHandles != null
                                      ? mainSourceHandle.Concat(waitHandles).Concat(eventSourceHandles).ToArray()
                                      : mainSourceHandle.Concat(eventSourceHandles).ToArray();

            var startTime = TimeSpanWatch.Now;
            var waitTime = timeout;

            WaitHandle result = null;
            var waiting = true;

            while(waiting)
            {
                // Block and wait.
                var signaledHandle = exceptionMonitor != null
                                         ? combinedHandles.WaitAny(exceptionMonitor, waitTime)
                                         : combinedHandles.WaitAny(waitTime);

                // Null signal means time out.
                if(signaledHandle == null)
                {
                    waiting = false;
                }
                // The main event source is signaled.
                else if(signaledHandle == mainEventSource.EventPosted)
                {
                    mainEventSource.ProcessEvents();
                    RaiseEventPostProcessed();
                    waiting = false;
                }
                // One of the passed in wait handles is signaled.
                else if(!eventSourceHandles.Contains(signaledHandle))
                {
                    result = signaledHandle;
                    waiting = false;
                }
                // One of the event sources is signaled.
                else
                {
                    var signaledSource = eventSources.First(source => source.EventPosted == signaledHandle);

                    // Calculate the remaining time to wait.
                    if(waitTime != Timeout.Infinite && waitTime != 0)
                    {
                        var elapsedMilliseconds = (int)(TimeSpanWatch.Now - startTime).TotalMilliseconds;
                        waitTime = Math.Max(timeout - elapsedMilliseconds, 0);
                    }

                    // No more waiting if time is up.
                    waiting = waitTime != 0;

                    // Process the pending events of the event source.
                    // Do this last so that the calculation of waitTime could be more accurate.
                    signaledSource.ProcessEvents();

                    RaiseEventPostProcessed();
                }
            }

            return result;
        }

        #endregion

        #region IEventCoordinator Members

        /// <inheritdoc/>
        public void RegisterEventSource(IEventSource eventSource)
        {
            if(eventSource == null)
            {
                throw new ArgumentNullException("eventSource");
            }

            if(!eventSources.Contains(eventSource))
            {
                eventSources.Add(eventSource);
                if(eventSource.EventPosted != null)
                {
                    eventSourceHandles = eventSourceHandles.Concat(new [] { eventSource.EventPosted }).ToArray();
                }
            }
        }

        /// <inheritdoc/>
        public void UnregisterEventSource(IEventSource eventSource)
        {
            if(eventSource == null)
            {
                throw new ArgumentNullException("eventSource");
            }

            eventSources.Remove(eventSource);
            eventSourceHandles = eventSourceHandles.Where(handle => handle != eventSource.EventPosted).ToArray();
        }

        #endregion

        #region Implementation of IEventProcessing

        /// <inheritdoc />
        public event EventHandler EventProcessed;

        #endregion

        #region Private Methods

        /// <summary>
        /// Raise the event NonBlockingEventPostProcessed.
        /// </summary>
        private void RaiseEventPostProcessed()
        {
            var handler = EventProcessed;
            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
