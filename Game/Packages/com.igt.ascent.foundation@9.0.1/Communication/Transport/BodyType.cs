//-----------------------------------------------------------------------
// <copyright file = "BodyType.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    /// <summary>
    /// Enumeration indicating the body type of a foundation transport message.
    /// </summary>
    public enum BodyType
    {
        /// <summary>
        /// The body contains a connection request.
        /// </summary>
        ConnectionRequested = 0,

        /// <summary>
        /// The body indicates that a connection was accepted.
        /// </summary>
        ConnectionAccepted = 1,

        /// <summary>
        /// The message contains transport information.
        /// </summary>
        TransportInformation = 2
    }
}