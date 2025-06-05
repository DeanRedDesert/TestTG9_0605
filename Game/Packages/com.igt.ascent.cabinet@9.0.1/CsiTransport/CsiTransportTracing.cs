// -----------------------------------------------------------------------
// <copyright file = "CsiTransportTracing.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using CSI.Schemas.Internal;
    using Tracing.EventDefinitions;

    /// <summary>
    /// Defines the trace events that are emitted by the F2X transport.
    /// </summary>
    [EventSource(Name = "IGT-Ascent-Core-Communication-Cabinet-CsiTransport-CsiTransportTracing")]
    public sealed class CsiTransportTracing : EventSourceBase
    {
        /// <summary>
        /// Gets the instance to use for logging trace events.
        /// </summary>
        public static readonly CsiTransportTracing Log = new CsiTransportTracing();

        /// <summary>
        /// Hide constructor for Singleton pattern.
        /// </summary>
        private CsiTransportTracing()
        {
        }

        /// <summary>
        /// Marks the start of a message being sent.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(1)]
        public void SendMessageStart(ulong messageNumber, Category category)
        {
            WriteEventLongInt(1, (long)messageNumber, (int)category);
        }

        /// <summary>
        /// Marks the end of a message being sent.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="messageLength">The message length in characters.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(2)]
        public void SendMessageStop(ulong messageNumber, int messageLength)
        {
            WriteEventLongInt(2, (long)messageNumber, messageLength);
        }

        /// <summary>
        /// Marks the start of handling an incoming message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(3)]
        public void HandleMessageStart(ulong messageNumber, Category category)
        {
            WriteEventLongInt(3, (long)messageNumber, (int)category);
        }

        /// <summary>
        /// Marks the end of handling an incoming message.
        /// </summary>
        /// <param name="messageNumber">The message number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(4)]
        public void HandleMessageStop(ulong messageNumber)
        {
            WriteEvent(4, (long)messageNumber);
        }

        /// <summary>
        /// Marks the start of encoding a message payload.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(5)]
        public void EncodeMessageStart()
        {
           WriteEvent(5);
        }

        /// <summary>
        /// Marks the end of encoding a message payload.
        /// </summary>
        /// <param name="payloadSize">The payload size, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(6)]
        public void EncodeMessageStop(int payloadSize)
        {
            WriteEvent(6, payloadSize);
        }

        /// <summary>
        /// Marks the start of decoding a message payload.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(7)]
        public void DecodeMessageStart()
        {
            WriteEvent(7);
        }

        /// <summary>
        /// Marks the end of decoding a message payload.
        /// </summary>
        /// <param name="payloadSize">The size of the payload, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(8)]
        public void DecodeMessageStop(int payloadSize)
        {
            WriteEvent(8, payloadSize);
        }

        /// <summary>
        /// Marks the start of sending the connect request message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(9)]
        public void SendConnectRequestStart()
        {
            WriteEvent(9);
        }

        /// <summary>
        /// Marks the end of sending the connect request message.
        /// </summary>
        /// <param name="messageLength">The message length in characters.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(10)]
        public void SendConnectRequestStop(int messageLength)
        {
            WriteEvent(10, messageLength);
        }
        
        /// <summary>
        /// Marks the start of sending the shutdown request message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(11)]
        public void SendShutdownRequestStart()
        {
            WriteEvent(11);
        }
        
        /// <summary>
        /// Marks the end of sending the shutdown request message.
        /// </summary>
        /// <param name="messageLength">The message length in characters.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(12)]
        public void SendShutdownRequestStop(int messageLength)
        {
            WriteEvent(12, messageLength);
        }

        /// <summary>
        /// Marks the start of handling a connect response message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(13)]
        public void HandleConnectResponseStart()
        {
            WriteEvent(13);
        }

        /// <summary>
        /// Marks the end of handling a connect response message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(14)]
        public void HandleConnectResponseStop()
        {
            WriteEvent(14);
        }

        /// <summary>
        /// Writes an event by using the provided event identifier and payloads which are ordered in (long, int).
        /// </summary>
        /// <param name="eventId">The event identifier.  This value should be between 0 and 65535.</param>
        /// <param name="arg1">A 64 bit integer argument.</param>
        /// <param name="arg2">A 32 bit integer argument.</param>
        /// <remarks>The payload of this event is not popular, thus stays private.</remarks>
        [NonEvent]
        private unsafe void WriteEventLongInt(int eventId, long arg1, int arg2)
        {
            if(!IsEnabled())
            {
                return;
            }

            var eventData = stackalloc EventData[2];

            eventData[0].DataPointer = (IntPtr)(&arg1);
            eventData[0].Size = sizeof(long);

            eventData[1].DataPointer = (IntPtr)(&arg2);
            eventData[1].Size = sizeof(int);

            WriteEventCore(eventId, 2, eventData);
        }
    }
}