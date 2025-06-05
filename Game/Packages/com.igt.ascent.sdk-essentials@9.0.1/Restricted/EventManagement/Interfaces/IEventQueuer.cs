// -----------------------------------------------------------------------
// <copyright file = "IEventQueuer.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines methods to add an event to the event queue and
    /// return immediately without waiting for the event to be processed.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface IEventQueuer
    {
        /// <summary>
        /// Adds an event to the event queue and returns immediately without waiting for the event to be processed.
        /// The event will be processed at a later time.
        /// </summary>
        /// <param name="eventArgs">The event to post.</param>
        void EnqueueEvent(EventArgs eventArgs);
    }
}