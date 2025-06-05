//-----------------------------------------------------------------------
// <copyright file = "InvalidGameModeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// Exception which is thrown when the game mode from the foundation is invalid.
    /// </summary>
    public class InvalidGameModeException : Exception
    {
        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message indicating the reason for the exception.</param>
        public InvalidGameModeException(string message)
            : base(message)
        {
        }
    }
}
