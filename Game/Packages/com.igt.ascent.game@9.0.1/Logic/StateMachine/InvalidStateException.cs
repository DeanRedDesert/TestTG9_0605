//-----------------------------------------------------------------------
// <copyright file = "InvalidStateException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Globalization;

    /// <summary>
    /// The InvalidStateException is used to indicate that
    /// a state value was not correct. Usually it indicates
    /// that the specified state was not configured, or that
    /// the state was null.
    /// </summary>
    public class InvalidStateException : Exception
    {
        /// <summary>
        /// Get the state associated with the exception.
        /// </summary>
        public string StateName
        {
            private set;
            get;
        }

        /// <summary>
        /// Format string used for format the message provided to the base constructor.
        /// </summary>
        private const string MessageFormat = "State Name: {0} Reason: {1}";

        /// <summary>
        /// Create an InvalidStateException with the given state and message.
        /// </summary>
        /// <param name="stateName">Invalid state which caused the exception.</param>
        /// <param name="message">A message describing why the exception occurred.</param>
        public InvalidStateException(string stateName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message))
        {
            StateName = stateName;
        }
    }

}
