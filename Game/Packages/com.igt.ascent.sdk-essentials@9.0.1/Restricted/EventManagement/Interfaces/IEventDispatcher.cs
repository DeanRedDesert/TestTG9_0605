//-----------------------------------------------------------------------
// <copyright file = "IEventDispatcher.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for dispatching events from an event queue.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Event occurs when an event is retrieved from a queue
        /// and dispatched for handling.
        /// </summary>
        event EventHandler<EventDispatchedEventArgs> EventDispatchedEvent;
    }
}
