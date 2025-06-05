//-----------------------------------------------------------------------
// <copyright file = "IPresentationServiceHost.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Presentation.CommServices
{
    using System;

    /// <summary>
    /// Interface to use for hosts of the IPresentation.
    /// </summary>
    public interface IPresentationServiceHost
    {
        /// <summary>
        /// Dequeues and returns the next message object in the message queue. Returns null if the message queue is empty.
        /// This method is blocked if queue is occupied by another thread.
        /// </summary>
        /// <returns>The next <see cref="PresentationGenericMsg"/>, or null if queue is empty.</returns>
        PresentationGenericMsg GetNextMessage();

        /// <summary>
        /// Attempts to dequeue and return the next message object in the message queue.
        /// Returns null if the message queue is empty or blocked.
        /// </summary>
        /// <returns>The next <see cref="PresentationGenericMsg"/>, or null if queue is empty or blocked.</returns>
        PresentationGenericMsg TryGetNextMessage();

        /// <summary>
        /// Determines if there are any message objects in the message queue.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        /// Returns the next message object in the message queue, does
        /// not remove the message object from the queue, returns null 
        /// if the message queue is empty.
        /// </summary>
        PresentationGenericMsg PeekNextMessage();

        /// <summary>
        /// Sets the function used to tell whether a <see cref="PresentationTransition"/> is a presentation reset message.
        /// </summary>
        /// <param name="presentationResetTransitionFilterFunc">
        /// A function used to tell if a given presentation message is a message which will trigger the presentation to reset.
        /// </param>
        /// <remarks>
        /// The PresentationServiceHost should use this to tell whether a message is a presentation reset message
        /// when enqueueing, dequeuing, or peeking at messages, and to update its presentation reset message count.
        /// </remarks>
        void SetPresentationResetTransitionFilter(Func<PresentationTransition, bool> presentationResetTransitionFilterFunc);

        /// <summary>
        /// Get the number of presentation reset messages currently in the queue.
        /// </summary>
        /// <returns>The number of presentation reset messages currently in the queue.</returns>
        int PendingPresentationResetMsgCount { get; }
    }
}
