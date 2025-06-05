//-----------------------------------------------------------------------
// <copyright file = "CriticalDataAccessDeniedException.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This exception indicates that an access to the critical data is denied.
    /// </summary>
    [Serializable]
    public class CriticalDataAccessDeniedException : Exception
    {
        /// <summary>
        /// Construct an CriticalDataAccessDeniedException with a message..
        /// </summary>
        public CriticalDataAccessDeniedException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Construct an CriticalDataAccessDeniedException with a message,
        /// and an inner exception.
        /// </summary>
        public CriticalDataAccessDeniedException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
