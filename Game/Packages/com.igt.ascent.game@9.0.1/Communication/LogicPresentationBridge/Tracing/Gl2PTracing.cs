// -----------------------------------------------------------------------
// <copyright file = "Gl2PTracing.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge.Tracing
{
    using System.Diagnostics;
    using Cloneable;
    using CompactSerialization;
    using EventDefinitions;

    /// <summary>
    /// This class is used to trace the GL2P communication performance metrics.
    /// </summary>
    public class Gl2PTracing
    {
        #region Private Fields

        /// <summary>
        /// The source used to write the tracing events into underlying tracing system.
        /// </summary>
        private readonly Gl2PTracingEventSource tracingSource = new Gl2PTracingEventSource();

        #endregion

        #region Singleton Implementation

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static readonly Gl2PTracing Log = new Gl2PTracing();

        /// <summary>
        /// Private constructor.
        /// </summary>
        private Gl2PTracing()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Tracing the event that the given service data type does not implement the interface
        /// <see cref="IDeepCloneable"/>.
        /// </summary>
        /// <param name="type">
        /// The service data type name which does not implement the interface <see cref="IDeepCloneable"/>.
        /// </param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void MissingIDeepCloneable(string type)
        {
            tracingSource.MissingIDeepCloneableInfo(type);
        }

        /// <summary>
        /// Tracing the event that the given service data type does not implement the interface
        /// <see cref="ICompactSerializable"/>.
        /// </summary>
        /// <param name="type">
        /// The service data type name which does not implement the interface <see cref="ICompactSerializable"/>.
        /// </param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void MissingICompactSerializable(string type)
        {
            tracingSource.MissingICompactSerializableInfo(type);
        }

        /// <summary>
        /// Tracing the event that the message is about to being constructed.
        /// </summary>
        /// <param name="messageType">The type of message to be constructed.</param>
        /// <param name="stateName">The current state name when the message constructed.</param>
        /// <param name="identifier">A unique identifier for this message.</param>
        /// <devdoc>
        /// A test shows that if certain pattern of arguments, such as (int, string, string, int, string), were passed
        /// to method WriteEvent, an unexpected exception would be thrown by the code underlying.
        /// So we will only report <paramref name="messageType"/> and <paramref name="stateName"/> once and use
        /// <paramref name="identifier"/> to identify the messages later.
        /// </devdoc>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void Gl2PMessageConstructionStart(string messageType, string stateName, int identifier)
        {
            tracingSource.Gl2PMessageConstructionStart(messageType, stateName, identifier);
        }

        /// <summary>
        /// Reports the message enqueued event.
        /// </summary>
        /// <param name="identifier">The identifier of the enqueued message.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void Gl2PMessageEnqueued(int identifier)
        {
            tracingSource.Gl2PMessageEnqueued(identifier);
        }

        /// <summary>
        /// Reports the message dequeued event.
        /// </summary>
        /// <param name="identifier">The identifier of the dequeued message.</param>
        /// <param name="serializationSize">
        /// The data size in bytes that is serialized during the GL2P communication.
        /// </param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void Gl2PMessageDequeued(int identifier, int serializationSize)
        {
            tracingSource.Gl2PMessageDequeued(identifier, serializationSize);
        }

        /// <summary>
        /// Reports the data size in bytes that is serialized during the GL2P communication per service.
        /// </summary>
        /// <param name="provider">The provider that the service belongs to.</param>
        /// <param name="serviceHash">The hash of the service.</param>
        /// <param name="serializationSize">The serialization size of this service data in bytes.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        internal void PerServiceSerializationSize(string provider, int serviceHash, int serializationSize)
        {
            tracingSource.PerServiceSerializationSizeInfo(provider, serviceHash, serializationSize);
        }

        /// <summary>
        /// Tracing event indicating the start of notifying a consumer of an update.
        /// </summary>
        /// <param name="identifier">Identifier of the consumer binding.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void HandleConsumersUpdatedStart(string identifier)
        {
            tracingSource.HandleConsumersUpdatedStart(identifier);
        }

        /// <summary>
        /// Tracing event indicating the stop of notifying a consumer of an update.
        /// </summary>
        /// <param name="identifier">Identifier of the consumer binding.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void HandleConsumersUpdatedStop(string identifier)
        {
            tracingSource.HandleConsumersUpdatedStop(identifier);
        }

        #endregion
    }
}