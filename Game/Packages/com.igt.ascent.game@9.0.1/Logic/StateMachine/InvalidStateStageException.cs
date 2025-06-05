//-----------------------------------------------------------------------
// <copyright file = "InvalidStateStageException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Globalization;

    /// <summary>
    /// An InvalidStateStageException is used to indicate that a function was called
    /// during a stage in which it is invalid.
    /// </summary>
    public class InvalidStateStageException : Exception
    {
        /// <summary>
        /// The function which was called during an invalid stage.
        /// </summary>
        public string FunctionName
        {
            get;
            private set;
        }

        /// <summary>
        /// The stage during which the call was invalid.
        /// </summary>
        public StateStage Stage
        {
            get;
            private set;
        }

        /// <summary>
        /// Format string used for format the message provided to the base constructor.
        /// </summary>
        private const string MessageFormat = "Function: {0} State Stage: {1} Reason: {2}";

        /// <summary>
        /// Create an InvalidStateStageException.
        /// </summary>
        /// <param name="functionName">The name of the function which was called.</param>
        /// <param name="stage">The state stage in which the function was called.</param>
        /// <param name="message">Message indicating the nature of the exception.</param>
        public InvalidStateStageException(string functionName, StateStage stage, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, functionName, stage, message))
        {
            FunctionName = functionName;
            Stage = stage;
        }
    }
}
