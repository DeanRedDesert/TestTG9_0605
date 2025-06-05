//-----------------------------------------------------------------------
// <copyright file = "InvalidMessageException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// Exception which indicates that an invalid message type was received.
    /// </summary>
    public class InvalidMessageException : Exception
    {
        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        public InvalidMessageException(string message)
            : base(message)
        {
        }
    }
}