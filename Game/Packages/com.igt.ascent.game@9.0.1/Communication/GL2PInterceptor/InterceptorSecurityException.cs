//-----------------------------------------------------------------------
// <copyright file = "InterceptorSecurityException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// Exception for security related errors in the interceptor.
    /// </summary>
    [Serializable]
    public class InterceptorSecurityException : Exception
    {
        /// <summary>
        /// Construct a new InterceptorSecurityException.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InterceptorSecurityException(string message):
            this(message, null)
        {

        }

        /// <summary>
        /// Construct a new InterceptorSecurityException.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception to pass.</param>
        public InterceptorSecurityException(string message, Exception innerException):
            base(message, innerException)
        {

        }
    }
}
