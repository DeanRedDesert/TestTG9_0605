// -----------------------------------------------------------------------
// <copyright file = "Gl2PTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge.Tracing.EventDefinitions
{
    using System.Diagnostics.Tracing;
    using Cloneable;
    using CompactSerialization;
    using Core.Tracing.EventDefinitions;

    /// <summary>
    /// This class is used to trace the GL2P metrics.
    /// </summary>
    [EventSource(Name = "IGT-Ascent-Core-Communication-LogicPresentationBridge-Tracing-EventDefinitions")]
    internal partial class Gl2PTracingEventSource : EventSourceBase
    {
        #region Event Definitions

        /// <summary>
        /// Reports the service data type name which does not implement the interface
        /// <see cref="IDeepCloneable"/>.
        /// </summary>
        /// <param name="type">
        /// The service data type name which does not implement the interface <see cref="IDeepCloneable"/>.
        /// </param>
        [Event(1, Level = EventLevel.Warning)]
        public void MissingIDeepCloneableInfo(string type)
        {
            WriteEvent(1, type);
        }

        /// <summary>
        /// Reports the service data type name which does not implement the interface
        /// <see cref="ICompactSerializable"/>.
        /// </summary>
        /// <param name="type">
        /// The service data type name which does not implement the interface <see cref="ICompactSerializable"/>.
        /// </param>
        [Event(2, Level = EventLevel.Warning)]
        public void MissingICompactSerializableInfo(string type)
        {
            WriteEvent(2, type);
        }

        /// <summary>
        /// Reports the message constructing event.
        /// </summary>
        /// <param name="messageType">The type of message to be constructed.</param>
        /// <param name="stateName">The current state name when the message constructed.</param>
        /// <param name="identifier">A unique identifier for this message.</param>
        /// <devdoc>
        /// A test shows that if certain patterns of arguments, such as (int, string, string, int, string), were passed
        /// to method WriteEvent, an unexpected exception would be thrown by the code underlying.
        /// So we will only report <paramref name="messageType"/> and <paramref name="stateName"/> once and use
        /// <paramref name="identifier"/> to identify the messages later.
        /// </devdoc>
        [Event(3, Level = EventLevel.Informational)]
        public void Gl2PMessageConstructionStart(string messageType, string stateName, int identifier)
        {
            WriteEventStringx2Int(3, messageType, stateName, identifier);
        }

        /// <summary>
        /// Reports the message enqueued event.
        /// </summary>
        /// <param name="identifier">The identifier of the enqueued message.</param>
        [Event(4, Level = EventLevel.Informational)]
        public void Gl2PMessageEnqueued(int identifier)
        {
            WriteEvent(4, identifier);
        }

        /// <summary>
        /// Reports the message dequeued event.
        /// </summary>
        /// <param name="identifier">The identifier of the dequeued message.</param>
        /// <param name="serializationSize">
        /// The data size in bytes that is serialized during the GL2P communication.
        /// </param>
        [Event(5, Level = EventLevel.Informational)]
        public void Gl2PMessageDequeued(int identifier, int serializationSize)
        {
            WriteEvent(5, identifier, serializationSize);
        }

        /// <summary>
        /// Reports the serialization scale per service.
        /// </summary>
        /// <param name="provider">The provider that the service belongs to.</param>
        /// <param name="serviceHash">The hash of the service.</param>
        /// <param name="serializationSize">The serialized data size of the service.</param>
        [Event(6, Level = EventLevel.Verbose)]
        public void PerServiceSerializationSizeInfo(string provider, int serviceHash, int serializationSize)
        {
            WriteEvent(6, provider, serviceHash, serializationSize);
        }

        /// <summary>
        /// Tracing event indicating the start of notifying a consumer of an update.
        /// </summary>
        /// <param name="identifier">Identifier of the consumer binding.</param>
        [Event(7, Level = EventLevel.Verbose)]
        public void HandleConsumersUpdatedStart(string identifier)
        {
            WriteEvent(7, identifier);
        }

        /// <summary>
        /// Tracing event indicating the stop of notifying a consumer of an update.
        /// </summary>
        /// <param name="identifier">Identifier of the consumer binding.</param>
        [Event(8, Level = EventLevel.Verbose)]
        public void HandleConsumersUpdatedStop(string identifier)
        {
            WriteEvent(8, identifier);
        }

        #endregion
    }
}