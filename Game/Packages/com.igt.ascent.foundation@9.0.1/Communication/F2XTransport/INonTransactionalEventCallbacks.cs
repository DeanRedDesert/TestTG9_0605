//-----------------------------------------------------------------------
// <copyright file = "INonTransactionalEventCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// This interface defines callback methods related to non transactional events
    /// that are received on FIN (Foundation Initiated Non-transactional) channel.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface INonTransactionalEventCallbacks
    {
        /// <summary>
        /// Add a Foundation Non Transactional event to the event queue
        /// and wait for the event to be processed before return.
        /// </summary>
        /// <param name="foundationEvent">The event to post and process.</param>
        /// <devdoc>
        /// This method is named so to be consistent with the <see cref="IEventCallbacks.PostEvent(EventArgs)"/>
        /// method which has the same blocking behavior.
        /// </devdoc>
        void PostEvent(EventArgs foundationEvent);

        /// <summary>
        /// Add a Foundation Non Transactional event to the event queue
        /// and return immediately without waiting for the event to be processed.
        /// </summary>
        /// <param name="foundationEvent">The event to post.</param>
        void EnqueueEvent(EventArgs foundationEvent);
    }
}
