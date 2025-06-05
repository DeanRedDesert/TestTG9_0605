//-----------------------------------------------------------------------
// <copyright file = "InvalidTransactionException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This exception indicates that a method was invoked on an
    /// invalid transaction. This most likely indicates that a
    /// transactional function was called without a transaction being opened.
    /// </summary>
    [Serializable]
    public class InvalidTransactionException : Exception
    {
        /// <summary>
        /// Construct an InvalidTransactionException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InvalidTransactionException(string message)
            : base(message)
        {
        }
    }
}
