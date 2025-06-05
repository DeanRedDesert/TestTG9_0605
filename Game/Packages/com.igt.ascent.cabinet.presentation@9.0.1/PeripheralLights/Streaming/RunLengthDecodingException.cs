//-----------------------------------------------------------------------
// <copyright file = "RunLengthDecodingException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;

    /// <summary>
    /// Represents errors that occur during run length decoding.
    /// </summary>
    [Serializable]
    public class RunLengthDecodingException : Exception
    {
        /// <summary>
        /// Construct a new RunLengthDecodingException.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RunLengthDecodingException(string message)
            : this(message, null)
        {

        }

        /// <summary>
        /// Construct a new RunLengthDecodingException.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public RunLengthDecodingException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
