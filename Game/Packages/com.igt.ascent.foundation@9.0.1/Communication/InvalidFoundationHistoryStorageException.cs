//-----------------------------------------------------------------------
// <copyright file = "InvalidFoundationHistoryStorageException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This exception indicates that there are errors in the
    /// Foundation owned history storage.
    /// </summary>
    [Serializable]
    public class InvalidFoundationHistoryStorageException : Exception
    {
        /// <summary>
        /// Construct an InvalidFoundationHistoryStorageException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidFoundationHistoryStorageException(string message)
            : base(message)
        {
        }
    }
}
