//-----------------------------------------------------------------------
// <copyright file = "InterfaceExtensionDependencyException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;

    /// <summary>
    /// Exception which indicates that there was an issue handling the dependencies for an extended interface.
    /// </summary>
    [Serializable]
    public class InterfaceExtensionDependencyException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public InterfaceExtensionDependencyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Construct an instance of the exception with the given message and inner exception.
        /// </summary>
        /// <param name="message">Message for the exception.</param>
        /// <param name="innerException">Exception which caused this exception.</param>
        public InterfaceExtensionDependencyException(string message, Exception innerException) : base(message,
                                                                                                      innerException)
        {
        }
    }
}
