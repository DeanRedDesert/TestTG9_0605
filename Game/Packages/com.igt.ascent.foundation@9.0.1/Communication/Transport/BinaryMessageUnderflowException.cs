//-----------------------------------------------------------------------
// <copyright file = "BinaryMessageUnderflowException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Exception which indicates that a binary message could not be deserialized because the buffer being deserialized
    /// did not contain sufficient data.
    /// </summary>
    public class BinaryMessageUnderflowException : Exception
    {
        /// <summary>
        /// Gets the number of bytes required to deserialize the message.
        /// </summary>
        public int RequiredSize { get; private set; }

        /// <summary>
        /// Gets the number of bytes available in the buffer.
        /// </summary>
        public int AvailableBytes { get; private set; }

        /// <summary>
        /// Gets the type of the message being deserialized.
        /// </summary>
        public Type MessageType { get; private set; }

        /// <summary>
        /// Gets the types of the binary message segments, in order, that make up the message format.
        /// </summary>
        public IList<Type> SegmentTypes { get; private set; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "{0} Type: {1} Bytes Required: {2} Available Bytes: {3}";

        /// <summary>
        /// Create an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="requiredSize">The required number of bytes to deserialize the message.</param>
        /// <param name="availableBytes">The number of bytes which were available for deserialization.</param>
        /// <param name="messageType">The type of message being deserialized.</param>
        public BinaryMessageUnderflowException(string message, int requiredSize, int availableBytes, Type messageType)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, messageType, requiredSize, availableBytes))
        {
            RequiredSize = requiredSize;
            AvailableBytes = availableBytes;
            MessageType = messageType;
        }

        /// <summary>
        /// Create an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="requiredSize">The required number of bytes to deserialize the message.</param>
        /// <param name="availableBytes">The number of bytes which were available for deserialization.</param>
        /// <param name="segmentTypes">The types of the binary message segments, in order.</param>
        public BinaryMessageUnderflowException(string message, int requiredSize, int availableBytes, IList<Type> segmentTypes)
            : base(string.Format(
                CultureInfo.InvariantCulture, 
                MessageFormat, 
                message, 
                segmentTypes == null ? "not provided" : string.Join(", ", segmentTypes.Select(type => type.ToString()).ToArray()), 
                requiredSize, 
                availableBytes))
        {
            RequiredSize = requiredSize;
            AvailableBytes = availableBytes;
            SegmentTypes = segmentTypes;
        }
    }
}