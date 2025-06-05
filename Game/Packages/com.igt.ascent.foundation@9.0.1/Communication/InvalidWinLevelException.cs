//-----------------------------------------------------------------------
// <copyright file = "InvalidWinLevelException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This exception indicates there was a problem with a win level
    /// the game tried to use.
    /// </summary>
    [Serializable]
    public class InvalidWinLevelException : Exception
    {
        /// <summary>
        /// Construct a new InvalidWinLevelException with a message and inner exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidWinLevelException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Construct a new InvalidWinLevelException with a message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidWinLevelException(string message)
            : this(message, null)
        {
        }
    }
}
