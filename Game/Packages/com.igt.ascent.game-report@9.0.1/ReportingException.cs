// -----------------------------------------------------------------------
// <copyright file = "ReportingException.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport
{
    using System;

    /// <summary>
    /// Exception to be thrown when there's a problem in reporting object.
    /// </summary>
    public class ReportingException : Exception
    {
        /// <summary>
        /// Initializes an instance of <see cref="ReportingException"/>
        /// with the message of the exception.
        /// </summary>
        /// <param name="message">
        /// The message of the exception was thrown.
        /// </param>
        public ReportingException(string message)
            : base(message)
        {
        }
    }
}
