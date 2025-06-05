//-----------------------------------------------------------------------
// <copyright file = "CompactSerializationException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.CompactSerialization
{
    using System;

    /// <summary>
    /// This exception indicates that an error occurs during serialization
    /// or deserialization by Compact Serializer.
    /// </summary>
    [Serializable]
    public class CompactSerializationException : Exception
    {
        /// <summary>
        /// Construct a CompactSerializationException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public CompactSerializationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Construct a CompactSerializationException with a message
        /// and an inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public CompactSerializationException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
