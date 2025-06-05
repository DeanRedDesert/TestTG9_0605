//-----------------------------------------------------------------------
// <copyright file = "UnexpectedResponseException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown when a response is received and there isn't a pending request.
    /// </summary>
    public class UnexpectedResponseException : Exception
    {
        /// <summary>
        /// Format string for the exception message.
        /// </summary>
        private const string MessageFormat = "Received response with no pending request. Type: {0}";

        /// <summary>
        /// The type of the message.
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// Construct an instance of the exception.
        /// </summary>
        /// <param name="type">The type of the message.</param>
        public UnexpectedResponseException(Type type)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, type))
        {
            MessageType = type;
        }

        /// <summary>
        /// Construct an instance of the exception for an unknown type.
        /// </summary>
        public UnexpectedResponseException()
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, "Unknown Type"))
        {
        }
    }
}
