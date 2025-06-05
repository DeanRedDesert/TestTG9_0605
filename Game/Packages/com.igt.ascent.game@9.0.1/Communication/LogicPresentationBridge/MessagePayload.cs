// -----------------------------------------------------------------------
// <copyright file = "MessagePayload.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;

    /// <summary>
    /// This data structure is used to hold the GL2P message and its info.
    /// </summary>
    /// <remarks>Use struct to avoid extra allocations on heap.</remarks>
    internal struct MessagePayload
    {
        /// <summary>
        /// The GL2P message object.
        /// </summary>
        public readonly object Message;

        /// <summary>
        /// The info.
        /// </summary>
        public readonly MessageInfo Info;

        /// <summary>
        /// Construct the instance with the GL2P message and the info.
        /// </summary>
        /// <param name="message">The GL2P message.</param>
        /// <param name="info">The info relating to <paramref name="message"/>.</param>
        public MessagePayload(object message, MessageInfo info)
        {
            Message = message ?? throw new ArgumentNullException("message");
            Info = info ?? throw new ArgumentNullException("info");
        }
    }
}