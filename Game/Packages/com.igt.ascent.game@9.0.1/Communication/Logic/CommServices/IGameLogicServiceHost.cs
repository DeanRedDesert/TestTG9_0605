//-----------------------------------------------------------------------
// <copyright file = "IGameLogicServiceHost.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Logic.CommServices
{
    using System.Threading;

    /// <summary>
    /// Interface to use for hosts of IGameLogic.
    /// </summary>
    public interface IGameLogicServiceHost
    {
        /// <summary>
        /// Handle which is signaled when an event is pending from the presentation
        /// </summary>
        WaitHandle MessageReceivedHandle { get; }

        /// <summary>
        ///    Dequeue's and returns the next message object in the
        ///    message queue, returns null if the message queue is
        ///    empty.
        /// </summary>
        GameLogicGenericMsg GetNextMessage();

        /// <summary>
        ///    Determines if there are any message objects in the message queue.
        /// </summary>
        bool IsMessagePending { get; }

        /// <summary>
        ///    Returns the next message object in the message queue, does
        ///    not remove the message object from the queue, returns null 
        ///    if the message queue is empty.
        /// </summary>
        GameLogicGenericMsg PeekNextMessage();

        /// <summary>
        /// Clears any queued messages and allows new messages to be queued.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops any new messages from being queued until <see cref="Start"/> is called.
        /// </summary>
        void Stop();
    }
}
