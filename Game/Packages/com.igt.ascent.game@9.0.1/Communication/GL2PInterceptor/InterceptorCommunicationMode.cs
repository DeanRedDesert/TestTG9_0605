//-----------------------------------------------------------------------
// <copyright file = "InterceptorCommunicationMode.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    /// <summary>
    /// Enumeration for the different types of possible interceptor communication modes.
    /// </summary>
    public enum InterceptorCommunicationMode
    {
        /// <summary>
        /// In Passive Communication Mode, communication is processed normally
        /// between the Game Logic and Presentation. Interceptor Clients receive
        /// a copy of all normal communication but they do not have access to
        /// modify or add standard communication messages. In this mode the
        /// Interceptor Clients are restricted in interacting with the Presentation
        /// and Game Logic to a set of narrowly defined interactions.
        /// </summary>
        PassiveCommunication,

        /// <summary>
        /// In Intercept Communication Mode incoming communication is no longer
        /// processed normally, all communication is forwarded to connected Interceptor
        /// Clients. The Interceptor Clients have full access to modify, add, or respond
        /// to messages in any manner it sees fit. It is the responsibility of the Interceptor
        /// Clients to ensure that communications are handled in a proper manner.
        /// </summary>
        InterceptCommunication,
    }
}
