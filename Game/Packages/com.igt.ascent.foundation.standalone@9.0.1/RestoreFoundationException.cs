//-----------------------------------------------------------------------
// <copyright file = "RestoreFoundationException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    using System;

    /// <summary>
    /// An exception to throw when there is an error with the <see cref="RestoreFoundationException"/> data.
    /// </summary>
    [Serializable]
    internal class RestoreFoundationException : Exception
    {
        private const string MessageFormatString = "Error recovering from critical data. " +
                                                   "Message: {0}\nInput: {1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreFoundationException"/> class.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="input">The input that caused the error.</param>
        public RestoreFoundationException(string message, string input)
            : base(string.Format(MessageFormatString, message, input))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestoreFoundationException"/> class.
        /// </summary>
        /// <param name="message">A message describing the error.</param>
        /// <param name="innerException">Exception that caused the error.</param>
        public RestoreFoundationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
