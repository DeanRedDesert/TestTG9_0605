// -----------------------------------------------------------------------
// <copyright file = "SessionNotAvailableException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport.Sessions
{
    using System;

    /// <summary>
    /// An exception which is thrown when a session with the requested identifier is not available.
    /// </summary>
    public class SessionNotAvailableException : Exception
    {
        /// <summary>
        /// The session identifier that was requested.
        /// </summary>
        public int RequestedSessionIdentifier { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionNotAvailableException"/> class.
        /// </summary>
        /// <param name="requestedSessionIdentifier">The session identifier that was requested.</param>
        public SessionNotAvailableException(int requestedSessionIdentifier) : 
            base($"Session {requestedSessionIdentifier} is not available.")
        {
            RequestedSessionIdentifier = requestedSessionIdentifier;
        }
    }
}