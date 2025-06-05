// -----------------------------------------------------------------------
// <copyright file = "ISession.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport.Sessions
{
    /// <summary>
    /// A Session is a conversation between two participants on a given transport.
    /// </summary>
    /// <remarks>
    /// In order to access the underlying transport a session is needed. Each session on a transport represents an
    /// exchange of messages between two participants that is totally separate from all other such exchanges. This
    /// allows a single transport to support multiple conversations that do not interfere with one another.
    /// </remarks>
    public interface ISession : ITransport
    {
        /// <summary>
        /// An identifier which is unique across all sessions created by a given session manager.
        /// </summary>
        /// <remarks>
        /// The session identifier is the same on both sides of the connection.
        /// </remarks>
        int SessionIdentifier { get; }
    }
}