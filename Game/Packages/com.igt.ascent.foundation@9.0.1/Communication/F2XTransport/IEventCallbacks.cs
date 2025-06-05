//-----------------------------------------------------------------------
// <copyright file = "IEventCallbacks.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// This interface defines callback methods related to events.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface IEventCallbacks
    {
        /// <summary>
        /// Post a Foundation event to the event queue.
        /// </summary>
        /// <param name="foundationEvent">The event to post.</param>
        void PostEvent(EventArgs foundationEvent);
    }
}
