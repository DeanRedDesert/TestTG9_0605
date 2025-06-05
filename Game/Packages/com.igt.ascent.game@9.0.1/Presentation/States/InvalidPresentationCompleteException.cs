//-----------------------------------------------------------------------
// <copyright file = "InvalidPresentationCompleteException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;

    /// <summary>
    /// Exception that is thrown when the presentation complete call is not valid.
    /// </summary>
    [Serializable]
    public class InvalidPresentationCompleteException : Exception
    {
        /// <summary>
        /// Create a new InvalidPresentationCompleteException.
        /// </summary>
        /// <param name="message">The reason this exception is being thrown.</param>
        public InvalidPresentationCompleteException(string message):
            this(message, null)
        {

        }

        /// <summary>
        /// Create a new InvalidPresentationCompleteException.
        /// </summary>
        /// <param name="message">The reason this exception is being thrown.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidPresentationCompleteException(string message, Exception innerException):
            base(message, innerException)
        {

        }
    }
}
