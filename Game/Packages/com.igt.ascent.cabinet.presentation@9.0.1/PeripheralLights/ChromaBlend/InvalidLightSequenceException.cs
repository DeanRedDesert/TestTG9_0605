//-----------------------------------------------------------------------
// <copyright file = "InvalidLightSequenceException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;

    /// <summary>
    /// An exception for when a light sequence is invalid.
    /// </summary>
    [Serializable]
    public class InvalidLightSequenceException : Exception
    {
        /// <summary>
        /// Construct a new exception given a message.
        /// </summary>
        /// <param name="message">The exception error message.</param>
        public InvalidLightSequenceException(string message)
            : this(message, null)
        {

        }

        /// <summary>
        /// Construct a new exception given a message and another exception.
        /// </summary>
        /// <param name="message">The exception error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidLightSequenceException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
