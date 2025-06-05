//-----------------------------------------------------------------------
// <copyright file = "InvalidPendingStateException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The InvalidPendingStateException is used to indicate that
    /// no state has been set to execute after the currently
    /// executing state. The State property should contain
    /// the name of the state during which the next state was
    /// not set.
    /// </summary>
    public class InvalidPendingStateException : Exception
    {
        /// <summary>
        /// Get the state during which the exception occurred.
        /// </summary>
        public string StateName
        {
            get;
            private set;
        }

        /// <summary>
        /// Format string used for format the message provided to the base constructor.
        /// </summary>
        private const string MessageFormat = "State Name: {0} Reason: {1}";

        /// <summary>
        /// Create a new InvalidPendingStateException with the given
        /// state and message.
        /// </summary>
        /// <param name="state">The state the exception occurred in.</param>
        /// <param name="message">Message explaining why the exception occurred.</param>
        public InvalidPendingStateException(string state, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, state, message))
        {
            StateName = state;
        }
    }
}
