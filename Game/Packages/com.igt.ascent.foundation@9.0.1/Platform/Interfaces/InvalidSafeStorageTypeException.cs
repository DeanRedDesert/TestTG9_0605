//-----------------------------------------------------------------------
// <copyright file = "InvalidSafeStorageTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This exception indicates that an attempt to write a non-safe store
    /// type to safe storage was made. This would normally indicate an
    /// attempt to write a non-serializable type.
    /// </summary>
    [Serializable]
    public class InvalidSafeStorageTypeException : Exception
    {
        /// <summary>
        /// Construct an InvalidSafeStorageTypeException with message and
        /// internal exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="innerException">
        /// The exception which caused this exception to be raised.
        /// </param>
        public InvalidSafeStorageTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
