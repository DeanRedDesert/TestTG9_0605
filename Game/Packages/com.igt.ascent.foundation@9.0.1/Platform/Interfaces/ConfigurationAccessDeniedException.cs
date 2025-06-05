//-----------------------------------------------------------------------
// <copyright file = "ConfigurationAccessDeniedException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This exception indicates that an access to the configuration data is denied.
    /// </summary>
    [Serializable]
    public class ConfigurationAccessDeniedException : Exception
    {
        /// <summary>
        /// Construct an ConfigurationAccessDeniedException with a message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public ConfigurationAccessDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Construct an ConfigurationAccessDeniedException with a message,
        /// and an inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="ex">The inner exception.</param>
        public ConfigurationAccessDeniedException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
