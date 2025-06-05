// -----------------------------------------------------------------------
// <copyright file = "MessageInfo.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System.Threading;

    /// <summary>
    /// This class stores the Id and data size for a GL2P message to be processed.
    /// </summary>
    internal sealed class MessageInfo
    {
        /// <summary>
        /// Size of the message(in bytes).
        /// </summary>
        public long DataSize { get; set; }

        /// <summary>
        /// A unique identifier for current message.
        /// </summary>
        public int Identifier { get; }

        /// <summary>
        /// This is used to trace down every gl2p message.
        /// </summary>
        /// <remarks>
        /// Integer overflows should not be a problem as we can take negative number as an identifier and only
        /// a small amount of messages are processed at certain interval.
        /// </remarks>
        private static int messageTracingSeed;

        /// <summary>
        /// The private constructor to prevent this class being constructed outside.
        /// </summary>
        /// <param name="identifier">The unique identifier of current message.</param>
        private MessageInfo(int identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// Start tracking the message at the beginning of construction.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="MessageInfo"/> with a unique Identifier for this session.
        /// </returns>
        public static MessageInfo NewMessageTracking() => 
            new MessageInfo(Interlocked.Increment(ref messageTracingSeed));

        /// <summary>
        /// The empty info instance will be used by messages that do not need the message data size.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="MessageInfo"/> with an Identifier of 0.
        /// </returns>
        public static MessageInfo NewEmptyInfo() =>
            new MessageInfo(0);
    }
}