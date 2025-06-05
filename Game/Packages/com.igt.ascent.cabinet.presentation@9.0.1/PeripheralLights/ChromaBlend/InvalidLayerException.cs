//-----------------------------------------------------------------------
// <copyright file = "InvalidLayerException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;

    /// <summary>
    /// An exception for when a layer is invalid.
    /// </summary>
    [Serializable]
    public class InvalidLayerException : Exception
    {
        /// <summary>
        /// Construct a new exception given a message.
        /// </summary>
        /// <param name="message">The exception error message.</param>
        public InvalidLayerException(string message)
            : this(message, null)
        {

        }

        /// <summary>
        /// Construct a new exception given a message and another exception.
        /// </summary>
        /// <param name="message">The exception error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidLayerException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
