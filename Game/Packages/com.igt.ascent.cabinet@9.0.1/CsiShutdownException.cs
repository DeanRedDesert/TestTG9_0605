//-----------------------------------------------------------------------
// <copyright file = "CsiShutdownException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Exception which indicates that the CSI Manager sent a shutdown.
    /// </summary>
    [Serializable]
    public class CsiShutdownException : Exception
    {
        /// <summary>
        /// Create an instance of the exception.
        /// </summary>
        /// <param name="message">Message associated with the exception.</param>
        public CsiShutdownException(string message)
            : base(message)
        {
        }
    }
}
