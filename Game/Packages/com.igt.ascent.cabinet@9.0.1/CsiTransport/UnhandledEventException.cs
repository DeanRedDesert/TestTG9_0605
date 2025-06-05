//-----------------------------------------------------------------------
// <copyright file = "UnhandledEventException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.CsiTransport
{
    using System;

    /// <summary>
    /// Exception which indicates that an unhandled event was received.
    /// </summary>
    [Serializable]
    public class UnhandledEventException : Exception
    {
        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        public UnhandledEventException(string message)
            : base(message)
        {
        }
    }
}