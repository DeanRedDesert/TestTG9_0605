// -----------------------------------------------------------------------
// <copyright file = "LogicStateMachineException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;

    /// <summary>
    /// The exception that is thrown when an error occurs within a logic state machine.
    /// </summary>
    public class LogicStateMachineException : Exception
    {
        #region Constructors

        /// <inheritdoc/>
        public LogicStateMachineException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public LogicStateMachineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}