// -----------------------------------------------------------------------
// <copyright file = "ISessionManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport.Sessions
{
    /// <summary>
    /// The Session Manager creates new sessions and tracks which sessions are active. Each <see cref="ISession"/> 
    /// represents a completely distinct conversation over the shared transport.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Creates a new session with the given identifier.
        /// </summary>
        /// <param name="sessionId">The identifier for the session to create.</param>
        /// <returns>The <see cref="ISession"/> instance for the new session.</returns>
        /// <precondition>The session identifier is available.</precondition>
        /// <postcondition>The identifier for the newly created session is no longer available.</postcondition>
        /// <exception cref="SessionNotAvailableException">
        /// Thrown if <paramref name="sessionId"/> is not available (e.g. it is already in use.)
        /// </exception>
        ISession CreateSession(int sessionId);

        /// <summary>
        /// Destroys the given session and returns its identifier to the pool of available identifiers.
        /// </summary>
        /// <param name="session">The session to destroy.</param>
        /// <postcondition>The session object is no longer usable.</postcondition>
        /// <postcondition>The session identifier can be used to create a new session.</postcondition>
        void DestroySession(ISession session);
    }
}