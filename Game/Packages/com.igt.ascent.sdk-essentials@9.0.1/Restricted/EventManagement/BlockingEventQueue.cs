// -----------------------------------------------------------------------
// <copyright file = "BlockingEventQueue.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Restricted.EventManagement
{
    using System;
    using Interfaces;

    /// <summary>
    /// An event queue that is capable of handling only blocking type of events.
    /// </summary>
    public class BlockingEventQueue : BlockingEventQueueBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="BlockingEventQueue"/>.
        /// </summary>
        /// <param name="blockingEventCapacity">
        /// The maximum number of blocking events this queue can handle at a time.
        /// This parameter is optional.  If not specified, it defaults to 1.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="blockingEventCapacity"/> is less than 1.
        /// </exception>
        public BlockingEventQueue(int blockingEventCapacity = 1) : base(blockingEventCapacity)
        {
        }

        #endregion

        #region BlockingEventQueueBase Overrides

        /// <inheritdoc/>
        public override bool ProcessEvents(CheckEventExpectationDelegate checkExpectation = null)
        {
            var result = false;

            lock(QueueLocker)
            {
                foreach(var pendingItem in ItemQueue)
                {
                    result |= DispatchEventForProcessing(pendingItem.EventArgs, checkExpectation);
                    pendingItem.EventProcessed.Set();
                }

                ItemQueue.Clear();
                EventReceivedHandle.Reset();
            }

            return result;
        }

        #endregion
    }
}