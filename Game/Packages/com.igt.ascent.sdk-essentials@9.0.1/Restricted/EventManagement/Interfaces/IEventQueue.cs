// -----------------------------------------------------------------------
// <copyright file = "IEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;
    using System.Threading;

    /// <summary>
    /// Delegate used to verify if an event being processed meets the expectation.
    /// </summary>
    /// <param name="eventArgs">
    /// The event being processed.
    /// </param>
    /// <returns>
    /// A flag indicating if the event being processed meets the expectation.
    /// </returns>
    public delegate bool CheckEventExpectationDelegate(EventArgs eventArgs);

    /// <summary>
    /// This interface defines functionality of an event queue,
    /// whose pending events will be processed under the command
    /// of the event queue manager.
    /// </summary>
    public interface IEventQueue
    {
        /// <summary>
        /// Gets the synchronization object that will be signaled
        /// when an event is added to the event queue.
        /// It is the event queue's responsibility to reset this
        /// wait handle properly after pending events are processed.
        /// </summary>
        WaitHandle EventReceived { get; }

        /// <summary>
        /// Processes all pending events.
        /// This method must reset <see cref="EventReceived"/> at the appropriate time.
        /// </summary>
        /// <param name="checkExpectation">
        /// The delegate used to check if an expectation is met.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <returns>
        /// True if <paramref name="checkExpectation"/> is specified, and the events processed met the expectation;
        /// False otherwise.
        /// </returns>
        bool ProcessEvents(CheckEventExpectationDelegate checkExpectation = null);
    }
}