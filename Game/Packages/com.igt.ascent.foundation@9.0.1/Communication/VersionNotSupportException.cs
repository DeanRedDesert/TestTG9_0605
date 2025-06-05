//-----------------------------------------------------------------------
// <copyright file = "VersionNotSupportException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// Exception thrown when something is not supported by the effective version of F2L/F2X.
    /// </summary>
    [Serializable]
    public class VersionNotSupportException : Exception
    {
        /// <summary>
        /// Construct an instance of the exception with the given message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        public VersionNotSupportException(string message)
            : base(message)
        {
        }
    }
}
