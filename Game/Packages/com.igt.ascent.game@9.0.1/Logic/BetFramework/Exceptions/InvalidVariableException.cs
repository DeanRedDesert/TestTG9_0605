//-----------------------------------------------------------------------
// <copyright file = "InvalidVariableException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;

    /// <summary>
    /// Thrown if a requested variable doesn't exist.
    /// </summary>
    public class InvalidVariableException : Exception
    {
        private const string MessageFormat = @"Invalid variable: {0}";

        /// <summary>
        /// Name of the invalid variable.
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="variableName">Name of the invalid variable.</param>
        public InvalidVariableException(string variableName)
            : base(string.Format(MessageFormat, variableName))
        {
            VariableName = variableName;
        }
    }
}
