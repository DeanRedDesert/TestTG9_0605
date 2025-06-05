//-----------------------------------------------------------------------
// <copyright file = "UnexpectedReplyTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which is thrown when an unexpected reply type is received.
    /// </summary>
    public class UnexpectedReplyTypeException : Exception
    {
        /// <summary>
        /// The type of reply which was expected.
        /// </summary>
        public Type ExpectedType { private set; get; }

        /// <summary>
        /// The type of reply which was received.
        /// </summary>
        public Type ReceivedType { private set; get; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat = "{0} + Received Type: {1} Expected Type: {2}";

        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        /// <param name="expected">The message type which was expected.</param>
        /// <param name="received">The message type which was received.</param>
        public UnexpectedReplyTypeException(string message, Type expected, Type received)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, message, received, expected))
        {
            ExpectedType = expected;
            ReceivedType = received;
        }
    }
}