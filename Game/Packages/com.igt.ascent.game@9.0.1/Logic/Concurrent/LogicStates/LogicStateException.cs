// -----------------------------------------------------------------------
// <copyright file = "LogicStateException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error occurs within a logic state.
    /// </summary>
    public class LogicStateException : Exception
    {
        #region Constructors

        /// <inheritdoc/>
        public LogicStateException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public LogicStateException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}