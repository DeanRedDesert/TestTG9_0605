// -----------------------------------------------------------------------
// <copyright file = "F2XSerializationTracing.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Tracing.EventDefinitions;

    /// <summary>
    /// Defines the trace events that are emitted by F2X transport xml serializations.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [EventSource(Name = "IGT-Ascent-Core-Communication-Foundation-F2XTransport-F2XSerializationTracing")]
    public partial class F2XSerializationTracing : EventSourceBase
    {
        /// <summary>
        /// Gets the instance to use for logging trace events.
        /// </summary>
        public static readonly F2XSerializationTracing Log = new F2XSerializationTracing();

        /// <summary>
        /// Hide constructor for Singleton pattern.
        /// </summary>
        private F2XSerializationTracing()
        {
        }

        /// <summary>
        /// Marks the start of serializing a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// 
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(1)]
        public void SerializeMessageStart(MessageCategory category)
        {
            WriteEvent(1, (int)category);
        }

        /// <summary>
        /// Marks the end of serializing a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="payloadSize">The size of the payload, in bytes after serialization.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(2)]
        public void SerializeMessageStop(MessageCategory category, int payloadSize)
        {
            WriteEvent(2, (int)category, payloadSize);
        }

        /// <summary>
        /// Marks the start of deserializing a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="payloadSize">The size of the payload, in bytes before deserialization.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(3)]
        public void DeserializeMessageStart(MessageCategory category, int payloadSize)
        { 
            WriteEvent(3, (int)category, payloadSize);
        }

        /// <summary>
        /// Marks the end of deserializing a message payload.
        /// </summary>
        /// <param name="category">The <see cref="MessageCategory"/> of the message.</param>
        /// <param name="success"><b>true</b> if the payload was deserialized successfully, <b>false</b> otherwise.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(4)]
        public void DeserializeMessageStop(MessageCategory category, bool success)
        {
            WriteEventIntBool(4, (int)category, success);
        }
    }
}