//-----------------------------------------------------------------------
// <copyright file = "GameRegistryException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;

    /// <summary>
    /// This exception indicates that an error occurs while processing
    /// game registries.
    /// </summary>
    [Serializable]
    public class GameRegistryException : Exception
    {
        /// <summary>
        /// Construct an GameRegistryException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public GameRegistryException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Construct an GameRegistryException with a message and the inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">An inner exception that causes this exception.</param>
        public GameRegistryException(string message, GameRegistryException innerException)
            : base(innerException != null
                ? $"{message}{innerException.Message}"
                : message,
                innerException)
        {
        }
    }
}
