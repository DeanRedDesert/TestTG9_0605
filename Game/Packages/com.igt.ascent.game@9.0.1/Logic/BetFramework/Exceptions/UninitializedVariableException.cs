//-----------------------------------------------------------------------
// <copyright file = "UninitializedVariableException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;

    /// <summary>
    /// Thrown if a variable is accessed before it is set.
    /// </summary>
    public class UninitializedVariableException : Exception
    {
        private const string MessageFormat = @"Attempt to access uninitialized variable: {0}";

        /// <summary>
        /// Name of the uninitialized variable.
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="variableName">Name of the uninitialized variable.</param>
        public UninitializedVariableException(string variableName)
            : base(string.Format(MessageFormat, variableName))
        {
            VariableName = variableName;
        }
    }
}
