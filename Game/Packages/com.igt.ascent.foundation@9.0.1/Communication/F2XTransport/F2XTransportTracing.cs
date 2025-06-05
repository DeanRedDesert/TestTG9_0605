//-----------------------------------------------------------------------
// <copyright file = "F2XTransportTracing.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Tracing.EventDefinitions;

    /// <summary>
    /// Defines the trace events that are emitted by the F2X transport.
    /// </summary>
    /// <remarks>
    /// Ordering of events:
    /// 
    ///   Sending Messages:
    /// 
    ///   1. SendMessageStart
    ///   2. BuildMessageStart
    ///   3. EncodeMessageStart
    ///   4. EncodeMessageStop
    ///   5. BuildMessageStop
    ///   6. SendMessageStop
    /// 
    ///   Receiving Messages:
    ///   1. ProcessHeaderStart
    ///   2. ProcessHeaderStop
    ///   3. HandleMessageStart
    ///   4. DecodeMessageStart
    ///   5. DecodeMessageStop
    ///   6. HandleMessageStop
    /// 
    /// Clients of this class perform the XML serialization, so this work is not reflected in any of these events.
    /// </remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [EventSource(Name = "IGT-Ascent-Core-Communication-Foundation-F2XTransport-F2XTransportTracing")]
    internal sealed partial class F2XTransportTracing : EventSourceBase
    {
        #region Nested Payload Types

        /// <summary>
        /// The type that is emitted to the ETW manifest in place of <see cref="Channel"/>
        /// for back-compatibility purpose.
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private enum ChannelUInt32 : uint
        {
            /// <summary>
            /// Value corresponding to Channel.Foundation.
            /// </summary>
            Foundation = Channel.Foundation,

            /// <summary>
            /// Value corresponding to Channel.Game.
            /// </summary>
            Game = Channel.Game,

            /// <summary>
            /// Value corresponding to Channel.FoundationNonTransactional.
            /// </summary>
            FoundationNonTransactional = Channel.FoundationNonTransactional,
        }

        #endregion
        
        /// <summary>
        /// Gets the instance to use for logging trace events.
        /// </summary>
        internal static readonly F2XTransportTracing Log = new F2XTransportTracing();

        /// <summary>
        /// Hide constructor for Singleton pattern.
        /// </summary>
        private F2XTransportTracing()
        {
        }

        /// <summary>
        /// Marks the start of a message being sent.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="channel">The <see cref="Channel"> of the message.</see></param>
        /// <param name="transactionId">The transaction ID of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), NonEvent]
        public void SendMessageStart(uint messageNumber, MessageCategory category, Channel channel, uint transactionId)
        {
            SendMessageStart(messageNumber, category, (ChannelUInt32)channel, transactionId);
        }
        
        /// <summary>
        /// Marks the start of a message being sent.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="channel">The <see cref="Channel"> of the message.</see></param>
        /// <param name="transactionId">The transaction ID of the message.</param>
        /// <remarks>
        /// The method signature will be dumped into the ETW manifest file. So we should make the types
        /// of the parameters to what we want to present in the ETW logs.
        /// </remarks>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(1)]
        private void SendMessageStart(uint messageNumber, MessageCategory category, ChannelUInt32 channel, 
            uint transactionId)
        {
            WriteEventIntx4(1, (int)messageNumber, (int)category, (int)channel,
                (int)transactionId);
        }

        /// <summary>
        /// Marks the end of a message being sent.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="messageSize">The message size, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(2)]
        public void SendMessageStop(uint messageNumber, int messageSize)
        {
            WriteEvent(2, (int)messageNumber, messageSize);
        }

        /// <summary>
        /// Marks the start of handling an incoming message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="channel">The <see cref="Channel"> of the message.</see></param>
        /// <param name="transactionId">The transaction ID of the message.</param>
        /// <param name="messageSize">The size of the message, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), NonEvent]
        public void HandleMessageStart(uint messageNumber, MessageCategory category, Channel channel,
            uint transactionId, int messageSize)
        {
            HandleMessageStart(messageNumber, category, (ChannelUInt32)channel, transactionId, messageSize);
        }
        
        /// <summary>
        /// Marks the start of handling an incoming message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="channel">The <see cref="Channel"> of the message.</see></param>
        /// <param name="transactionId">The transaction ID of the message.</param>
        /// <param name="messageSize">The size of the message, in bytes.</param>
        /// <remarks>
        /// The method signature will be dumped into the ETW manifest file. So we should make the types
        /// of the parameters to what we want to present in the ETW logs.
        /// </remarks>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(3)]
        private void HandleMessageStart(uint messageNumber, MessageCategory category, ChannelUInt32 channel,
            uint transactionId, int messageSize)
        {
            WriteEventIntx5(3, (int)messageNumber, (int)category, (int)channel, (int)transactionId,
                messageSize);
        }

        /// <summary>
        /// Marks the end of handling an incoming message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="success"><b>true</b> if the message was successfully handled, <b>false</b> otherwise.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(4)]
        public void HandleMessageStop(uint messageNumber, bool success)
        {
            WriteEventIntBool(4, (int)messageNumber, success);
        }

        /// <summary>
        /// Marks the start of reading the message header and handling bookkeeping.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(5)]
        public void ProcessHeaderStart()
        {
            WriteEvent(5);
        }

        /// <summary>
        /// Marks the end of reading the message header and handling bookkeeping.
        /// </summary>
        /// <param name="success"><b>true</b> if the header was completely processed, <b>false</b> otherwise.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(6)]
        public void ProcessHeaderStop(bool success)
        {
            WriteEventBool(6, success);
        }

        /// <summary>
        /// Marks the start of building a message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(7)]
        public void BuildMessageStart(uint messageNumber)
        {
            WriteEvent(7, (int)messageNumber);
        }

        /// <summary>
        /// Marks the end of building a message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="messageSize">The size of the message, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(8)]
        public void BuildMessageStop(uint messageNumber, int messageSize)
        {
            WriteEvent(8, (int)messageNumber, messageSize);
        }

        /// <summary>
        /// Marks the start of encoding a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(9)]
        public void EncodeMessageStart(MessageCategory category)
        {
            WriteEvent(9, (int)category);
        }

        /// <summary>
        /// Marks the end of encoding a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="payloadSize">The payload size, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(10)]
        public void EncodeMessageStop(MessageCategory category, int payloadSize)
        {
            WriteEvent(10, (int)category, payloadSize);
        }

        /// <summary>
        /// Marks the start of decoding a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(11)]
        public void DecodeMessageStart(MessageCategory category)
        {
            WriteEvent(11, (int)category);
        }

        /// <summary>
        /// Marks the end of decoding a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="payloadSize">The size of the payload, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(12)]
        public void DecodeMessageStop(MessageCategory category, int payloadSize)
        {
            WriteEvent(12, (int)category, payloadSize);
        }
    }
}