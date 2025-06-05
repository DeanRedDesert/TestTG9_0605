//-----------------------------------------------------------------------
// <copyright file = "SocketTransportTracing.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Tracing.EventDefinitions;

    /// <summary>
    /// Defines the trace events that can be emitted from a socket transport.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [EventSource(Name = "IGT-Ascent-Core-Communication-Foundation-Transport-SocketTransportTracing")]
    internal sealed partial class SocketTransportTracing : EventSourceBase
    {
        internal static readonly SocketTransportTracing Log = new SocketTransportTracing();

        /// <summary>
        /// Hide the constructor for Singleton pattern.
        /// </summary>
        private SocketTransportTracing()
        {
        }

        /// <summary>
        /// Marks the start of a message handler being invoked.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(1)]
        public void InvokeHandlerStart()
        {
            WriteEvent(1);
        }

        /// <summary>
        /// Marks the end of a message handler being invoked.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(2)]
        public void InvokeHandlerStop()
        {
            WriteEvent(2);
        }

        /// <summary>
        /// Marks the start of a message send.
        /// </summary>
        /// <param name="messageLength">The length of the message, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(3)]
        public void SendMessageStart(int messageLength)
        {
            WriteEvent(3, messageLength);
        }

        /// <summary>
        /// Marks the end of a message send, and indicates if it was successful or not.
        /// </summary>
        /// <param name="success"><b>true</b> if the send succeeded, <b>false</b> otherwise.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(4)]
        public void SendMessageStop(bool success)
        {
            WriteEvent(4, success ? 1 : 0);
        }

        /// <summary>
        /// Marks the start of a message receive.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(5)]
        public void ReceiveStart()
        {
            WriteEvent(5);
        }

        /// <summary>
        /// Marks the end of a message receive, and indicates if it was successful or not.
        /// </summary>
        /// <param name="success"><b>true</b> if the receive succeeded, <b>false</b> otherwise.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(6)]
        public void ReceiveStop(bool success)
        {
            WriteEvent(6, success ? 1 : 0);
        }

        /// <summary>
        /// Indicates that a complete message of the given length was received.
        /// </summary>
        /// <param name="messageLength">The length of the message, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(7)]
        public void CompleteMessageReceived(int messageLength)
        {
            WriteEvent(7, messageLength);
        }
    }
}