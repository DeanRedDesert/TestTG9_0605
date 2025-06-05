//-----------------------------------------------------------------------
// <copyright file = "DuplicateStateNameException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that an attempt was made to add a
    /// state with the same name as an existing state.
    /// </summary>
    public class DuplicateStateNameException : Exception
    {
        private readonly string stateName;

        /// <summary>
        /// Get the name of the state.
        /// </summary>
        public string StateName
        {
            get { return stateName; }
        }

        /// <summary>
        /// Format string used for format the message provided to the base constructor.
        /// </summary>
        private const string MessageFormat = "State Name: {0} Reason: {1}";

        /// <summary>
        /// Create a DuplicateStateNameException with the given information.
        /// </summary>
        /// <param name="stateName">The duplicate state name.</param>
        /// <param name="message">A message explaining the exception.</param>
        public DuplicateStateNameException(string stateName, string message)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, stateName, message))
        {
            this.stateName = stateName;
        }
    }
}
