// -----------------------------------------------------------------------
// <copyright file = "FrameworkRunnerException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error occurs within
    /// a concurrent framework and/or its runner.
    /// </summary>
    public class FrameworkRunnerException : Exception
    {
        #region Constructors

        /// <inheritdoc/>
        public FrameworkRunnerException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public FrameworkRunnerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}