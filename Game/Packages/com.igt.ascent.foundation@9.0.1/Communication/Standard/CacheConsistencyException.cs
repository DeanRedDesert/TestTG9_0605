//-----------------------------------------------------------------------
// <copyright file = "CacheConsistencyException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;

    /// <summary>
    /// Exception thrown when there is a consistency issue with the critical data cache.
    /// </summary>
    [Serializable]
    public class CacheConsistencyException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public CacheConsistencyException(string message) : base(message)
        {
        }
    }
}
