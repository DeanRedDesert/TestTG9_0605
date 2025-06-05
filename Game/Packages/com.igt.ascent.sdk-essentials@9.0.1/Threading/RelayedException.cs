// -----------------------------------------------------------------------
// <copyright file = "RelayedException.cs" company = "IGT">
//     Copyright (c) 2023 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Threading
{
    using System;

    /// <summary>
    /// Represents a relayed exception thrown from another thread.
    /// The purpose of this exception is to retain the callstacks where the original exception was thrown. 
    /// </summary>
    public class RelayedException : Exception
    {
        /// <summary>
        /// Construct the relayed exception with the original exception instance.
        /// </summary>
        /// <param name="relayedException">The original exception instance.</param>
        public RelayedException(Exception relayedException)
            : this(null, relayedException)
        {
        }
        
        /// <summary>
        /// Construct the relayed exception with the error message and the original exception instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="relayedException">The original exception instance.</param>
        public RelayedException(string message, Exception relayedException)
            : base(message, relayedException)
        {
        }
    }
}