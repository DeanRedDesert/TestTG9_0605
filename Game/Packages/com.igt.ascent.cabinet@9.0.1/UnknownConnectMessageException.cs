//-----------------------------------------------------------------------
// <copyright file = "UnknownConnectMessageException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Exception which indicates that an unknown connect message was received.
    /// </summary>
    [Serializable]
    public class UnknownConnectMessageException : Exception
    {
        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        public UnknownConnectMessageException(string message)
            : base(message)
        {
        }
    }
}
