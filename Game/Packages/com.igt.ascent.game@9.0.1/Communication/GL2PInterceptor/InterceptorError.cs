//-----------------------------------------------------------------------
// <copyright file = "InterceptorError.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    /// <summary>
    /// Enumeration of interceptor errors.
    /// </summary>
    public enum InterceptorError
    {
        /// <summary>
        /// A connection error has occurred.
        /// </summary>
        ConnectionError,

        /// <summary>
        /// A unknown message type has been received or cannot be processed.
        /// </summary>
        UnknownMessageReceived,
    }
}
