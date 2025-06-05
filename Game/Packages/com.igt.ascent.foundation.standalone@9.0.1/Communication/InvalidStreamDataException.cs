//-----------------------------------------------------------------------
// <copyright file = "InvalidStreamDataException.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Standalone
{
    using System;

    /// <summary>
    /// This exception indicates that a data stream is in an invalid format.
    /// This is to replace the <see langword="InvalidDataException"/> which
    /// is not supported in Mono.
    /// </summary>
    [Serializable]
    public class InvalidStreamDataException : Exception
    {
        /// <summary>
        /// Construct an InvalidStreamDataException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidStreamDataException(string message)
            : base(message)
        {
        }
    }
}
