//-----------------------------------------------------------------------
// <copyright file = "VariableTypeException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework.Exceptions
{
    using System;

    /// <summary>
    /// Thrown if a bet variable is accessed as the wrong type.
    /// </summary>
    public class VariableTypeException : Exception
    {
        private const string MessageFormat = @"Invalid variable type: requested type {0} on variable {1} (type {2}).";

        /// <summary>
        /// Type of the variable.
        /// </summary>
        public Type ActualType { get; private set; }

        /// <summary>
        /// Type used to access the variable.
        /// </summary>
        public Type RequestedType { get; private set; }

        /// <summary>
        /// Name of the variable.
        /// </summary>
        public string VariableName { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="requestedType">Type used to access the variable.</param>
        /// <param name="actualType">Type of the variable.</param>
        public VariableTypeException(string variableName, Type requestedType, Type actualType)
            : base(string.Format(MessageFormat, actualType, requestedType, variableName))
        {
            VariableName = variableName;
            RequestedType = requestedType;
            ActualType = actualType;
        }
    }
}
