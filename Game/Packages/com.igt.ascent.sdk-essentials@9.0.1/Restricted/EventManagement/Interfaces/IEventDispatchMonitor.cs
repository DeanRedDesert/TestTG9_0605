//-----------------------------------------------------------------------
// <copyright file = "IEventDispatchMonitor.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines APIs for monitoring the dispatching of events from an event queue.
    /// </summary>
    public interface IEventDispatchMonitor
    {
        /// <summary>
        /// Event occurs before an event dispatching starts.
        /// </summary>
        event EventHandler EventDispatchStarting;

        /// <summary>
        /// Event occurs after an event dispatching ends.
        /// </summary>
        event EventHandler EventDispatchEnded;
    }
}
