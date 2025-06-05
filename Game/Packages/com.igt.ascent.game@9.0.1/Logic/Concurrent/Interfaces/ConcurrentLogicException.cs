// -----------------------------------------------------------------------
// <copyright file = "ConcurrentLogicException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error occurs with
    /// the concurrent logic.
    /// </summary>
    public class ConcurrentLogicException : Exception
    {
        #region Constructors

        /// <inheritdoc/>
        public ConcurrentLogicException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public ConcurrentLogicException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}