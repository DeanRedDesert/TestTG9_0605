//-----------------------------------------------------------------------
// <copyright file = "UnhandledReplyTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that a received reply type does not have a handler.
    /// </summary>
    public class UnhandledReplyTypeException : Exception
    {
        /// <summary>
        /// The type of the unhandled reply.
        /// </summary>
        public Type ReplyType { private set; get; }

        /// <summary>
        /// Message format for the exception.
        /// </summary>
        private const string MessageFormat = "{0} Type: {1}";

        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="replyType">The reply type which was unexpected.</param>
        public UnhandledReplyTypeException(string message, Type replyType)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, replyType))
        {
            ReplyType = replyType;
        }
    }
}