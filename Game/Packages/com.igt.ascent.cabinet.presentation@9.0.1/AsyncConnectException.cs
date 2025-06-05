// -----------------------------------------------------------------------
// <copyright file = "AsyncConnectException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation
{
    using System;

    /// <summary>
    /// Exception thrown when AsyncConnect or PostConnect failed.
    /// </summary>
    [Serializable]
    public class AsyncConnectException : Exception
    {
        /// <summary>
        /// Create a new exception with the given message.
        /// </summary>
        /// <param name="message">The error message related to Async connect.</param>
        public AsyncConnectException(string message) : base(message)
        {
        }
    }
}
