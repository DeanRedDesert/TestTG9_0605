// -----------------------------------------------------------------------
// <copyright file = "F2XSerializationTracing.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System.Diagnostics;
    using System.Diagnostics.Tracing;
    using CSI.Schemas.Internal;
    using Tracing.EventDefinitions;

    /// <summary>
    /// Defines the trace events that are emitted by CSI transport xml serializations.
    /// </summary>
    [EventSource(Name = "IGT-Ascent-Core-Communication-Cabinet-CsiTransport-CsiSerializationTracing")]
    public sealed class CsiSerializationTracing : EventSourceBase
    {
        /// <summary>
        /// Gets the instance to use for logging trace events.
        /// </summary>
        public static readonly CsiSerializationTracing Log = new CsiSerializationTracing();

        /// <summary>
        /// Hide constructor for Singleton pattern.
        /// </summary>
        private CsiSerializationTracing()
        {
        }

        /// <summary>
        /// Marks the start of serializing a CSI message payload.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(1)]
        public void SerializeInnerMessageStart(Category category)
        {
            WriteEvent(1, (int)category);
        }

        /// <summary>
        /// Marks the end of serializing a CSI message payload.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(2)]
        public void SerializeInnerMessageStop(Category category)
        {
            WriteEvent(2, (int)category);
        }

        /// <summary>
        /// Marks the start of serializing a CSI message container.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(3)]
        public void SerializeCsiContainerStart(Category category)
        {
            WriteEvent(3, (int)category);
        }

        /// <summary>
        /// Marks the end of serializing a CSI message container.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        /// <param name="payloadSize">The size of the payload, in bytes after serialization.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(4)]
        public void SerializeCsiContainerStop(Category category, int payloadSize)
        {
            WriteEvent(4, (int)category, payloadSize);
        }

        /// <summary>
        /// Marks the start of deserializing a CSI message payload.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(5)]
        public void DeserializeInnerMessageStart(Category category)
        { 
            WriteEvent(5, (int)category);
        }

        /// <summary>
        /// Marks the end of deserializing a CSI message payload.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(6)]
        public void DeserializeInnerMessageStop(Category category)
        {
            WriteEvent(6, (int)category);
        }

        /// <summary>
        /// Marks the start of deserializing a CSI message container.
        /// </summary>
        /// <param name="payloadSize">The size of the payload, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(7)]
        public void DeserializeCsiContainerStart(int payloadSize)
        { 
            WriteEvent(7, payloadSize);
        }

        /// <summary>
        /// Marks the end of deserializing a CSI message container.
        /// </summary>
        /// <param name="category">The <see cref="Category"/> of the message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(8)]
        public void DeserializeCsiContainerStop(Category category)
        {
            WriteEvent(8, (int)category);
        }

        /// <summary>
        /// Marks the start of serializing a CSI connect message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(9)]
        public void SerializeCsiConnectMessageStart()
        {
            WriteEvent(9);
        }

        /// <summary>
        /// Marks the end of serializing a CSI connect message.
        /// </summary>
        /// <param name="payloadSize">The size of the payload, in bytes after serialization.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(10)]
        public void SerializeCsiConnectMessageStop(int payloadSize)
        {
            WriteEvent(10, payloadSize);
        }

        /// <summary>
        /// Marks the start of deserializing a CSI connect message.
        /// </summary>
        /// <param name="payloadSize">The size of the payload, in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(11)]
        public void DeserializeCsiConnectStart(int payloadSize)
        { 
            WriteEvent(11, payloadSize);
        }

        /// <summary>
        /// Marks the end of deserializing a CSI connect message.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST"), Event(12)]
        public void DeserializeCsiConnectStop()
        {
            WriteEvent(12);
        }
    }
}