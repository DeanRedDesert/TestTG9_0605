//-----------------------------------------------------------------------
// <copyright file = "InvalidFrameException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;

    /// <summary>
    /// Represents invalid light frame errors.
    /// </summary>
    [Serializable]
    public class InvalidFrameException : Exception
    {
        /// <summary>
        /// Construct a new InvalidFrameException.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidFrameException(string message)
            : this(message, null)
        {

        }

        /// <summary>
        /// Construct a new InvalidFrameException.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidFrameException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
