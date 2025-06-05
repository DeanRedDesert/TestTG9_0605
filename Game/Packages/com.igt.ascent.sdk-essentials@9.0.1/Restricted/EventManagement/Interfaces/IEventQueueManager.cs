// -----------------------------------------------------------------------
// <copyright file = "IEventQueueManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// This interface defines functionality of an event queue manager,
    /// which coordinates the event processing among multiple event queues,
    /// and provides ways to wait for a specific event to occur.
    /// </summary>
    public interface IEventQueueManager
    {
        /// <summary>
        /// Registers an event queue with the event queue manager.
        /// </summary>
        /// <remarks>
        /// The event queue must be unregistered by calling <see cref="UnregisterEventQueue"/>
        /// when the event queue ceases to exist or is disposed.
        /// 
        /// If two event queues receives events at the same time,
        /// the event queue registered earlier will be processed first.
        /// </remarks>
        /// <param name="eventQueue">
        /// The event queue to register.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void RegisterEventQueue(IEventQueue eventQueue);

        /// <summary>
        /// Unregisters an event queue with the event queue manager.
        /// </summary>
        /// <param name="eventQueue">The event queue to unregister.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void UnregisterEventQueue(IEventQueue eventQueue);

        /// <summary>
        /// Process events from all registered event queues.
        /// </summary>
        /// <remarks>
        /// This method blocks and waits for events to come in and processes them as needed.
        /// It returns when either one of <paramref name="returnHandles"/> is signaled, or
        /// the delegate <paramref name="checkExpectation"/> returns true for
        /// the event queues specified by <paramref name="expectedQueues"/>.
        /// </remarks>
        /// <param name="returnHandles">
        /// The synchronization objects that when signaled, would cause this method
        /// to stop waiting for events and return.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <param name="checkExpectation">
        /// The delegate used to check if an expectation is met.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <param name="expectedQueues">
        /// The event queues for which <paramref name="checkExpectation"/> is to run.
        /// If null, <paramref name="checkExpectation"/> will be run for all available event queues.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <returns>
        /// Null if events processed met the given expectation;
        /// One of the <paramref name="returnHandles"/> if it has been signaled
        /// and thus causes this method to return.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="returnHandles"/> is null or empty while
        /// <paramref name="checkExpectation"/> is null as well, or
        /// any item in <paramref name="expectedQueues"/> is not registered with the manager.
        /// </exception>
        WaitHandle ProcessEvents(IList<WaitHandle> returnHandles = null,
                                 CheckEventExpectationDelegate checkExpectation = null,
                                 IList<IEventQueue> expectedQueues = null);
    }
}