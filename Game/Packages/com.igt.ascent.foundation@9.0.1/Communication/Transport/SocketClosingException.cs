//-----------------------------------------------------------------------
// <copyright file = "SocketClosingException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;

    /// <summary>
    /// Exception thrown when a read is completed while the socket is in the process
    /// of being closed.
    /// </summary>
    [Serializable]
    public class SocketClosingException : Exception
    {
        /// <summary>
        /// Create a new <see cref="SocketClosingException"/> with an internal exception.
        /// </summary>
        /// <param name="message">A message to include about the exception.</param>
        /// <param name="innerException">The inner exception that triggered the SocketClosingException.</param>
        public SocketClosingException(string message, Exception innerException) : base(BuildMessage(message, innerException), innerException)
        {
        }

        /// <summary>
        /// Build the message for the exception. Output information on the inner exceptions.
        /// </summary>
        /// <param name="message">A message to include about the exception.</param>
        /// <param name="innerException">The original exception.</param>
        /// <returns>The message for the exception, including information on the inner exception, if one exists.</returns>
        private static string BuildMessage(string message, Exception innerException)
        {
            var outMessage = message;
            if(innerException != null)
            {
                outMessage += " -->" + Environment.NewLine + innerException.Message;
            }

            return outMessage;
        }
    }
}
