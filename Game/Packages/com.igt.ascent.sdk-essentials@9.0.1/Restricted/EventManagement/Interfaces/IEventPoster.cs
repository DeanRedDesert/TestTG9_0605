// -----------------------------------------------------------------------
// <copyright file = "IEventPoster.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;
    using System.Threading;

    /// <summary>
    /// This interface defines methods to add an event to the event queue and
    /// wait for the event to be processed before return.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface IEventPoster
    {
        /// <summary>
        /// Adds an event to the event queue and waits for the event to be processed before return.
        /// </summary>
        /// <param name="eventArgs">
        /// The event to post and process.
        /// </param>
        /// <param name="sigInterrupt">
        /// The wait handle that, when signaled, will interrupt the waiting for the event being processed.
        /// </param>
        /// <param name="interruptHandler">
        /// The action to perform when <paramref name="sigInterrupt"/> is signaled.
        /// </param>
        void PostEvent(EventArgs eventArgs, WaitHandle sigInterrupt = null, Action interruptHandler = null);
    }
}