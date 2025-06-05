//-----------------------------------------------------------------------
// <copyright file = "IEventSource.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System.Threading;

    /// <summary>
    /// This interface defines functionality of an event source,
    /// whose pending events will be processed under the command
    /// of an Event Coordinator. 
    /// </summary>
    public interface IEventSource
    {
        /// <summary>
        /// Gets the synchronization object that will be signaled
        /// when an event is posted to the event source.
        /// It is the event source's responsibility to reset this
        /// wait handle properly after pending events are processed.
        /// </summary>
        WaitHandle EventPosted { get; }

        /// <summary>
        /// Processes all pending events.
        /// This method must reset <see cref="EventPosted"/> at appropriate time.
        /// </summary>
        /// <remarks>
        /// Note that there is no transaction available when events are being processed.
        /// An event source can submit a QueuedOperation with an ITransactionManager
        /// for any transactional operation it would like to execute.
        /// </remarks>
        void ProcessEvents();
    }
}
