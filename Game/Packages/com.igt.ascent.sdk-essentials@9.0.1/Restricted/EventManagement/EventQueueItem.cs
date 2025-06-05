// -----------------------------------------------------------------------
// <copyright file = "EventQueueItem.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using System.Threading;

    /// <summary>
    /// This class maintains a record for an eventArgs in the event queue.
    /// It includes the event and a flag indicating if the event must be processed before responding.
    /// </summary>
    public class EventQueueItem
    {
        #region Properties

        /// <summary>
        /// Gets the event added into the queue.
        /// </summary>
        public EventArgs EventArgs { get; }

        /// <summary>
        /// Gets the flag indicating whether the event must be processed before replying to the Foundation.
        /// </summary>
        public bool WaitForProcess { get; }

        /// <summary>
        /// Gets the wait handle that signals when the event has been processed.
        /// </summary>
        public ManualResetEvent EventProcessed { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="EventQueueItem"/>.
        /// </summary>
        /// <devdoc>
        /// This internal method does not validate arguments.
        /// </devdoc>
        /// <param name="eventArgs">The event to add to the queue.</param>
        /// <param name="waitForProcess">The flag indicating whether the event must be processed before return.</param>
        /// <param name="eventProcessed">The wait handle that signals when the event has been processed</param>
        protected internal EventQueueItem(EventArgs eventArgs, bool waitForProcess, ManualResetEvent eventProcessed)
        {
            EventArgs = eventArgs;
            WaitForProcess = waitForProcess;
            EventProcessed = eventProcessed;
        }

        #endregion
    }
}