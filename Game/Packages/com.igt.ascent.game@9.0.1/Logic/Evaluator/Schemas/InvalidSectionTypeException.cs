//-----------------------------------------------------------------------
// <copyright file = "InvalidSectionTypeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;

    /// <summary>
    /// Exception which indicates that a section was not the requested type.
    /// </summary>
    public class InvalidSectionTypeException : Exception
    {
        /// <summary>
        /// Create a new InvalidSectionTypeException with the specified message.
        /// </summary>
        /// <param name="message">Message indicating why the exception occurred.</param>
        public InvalidSectionTypeException(string message)
            : base(message)
        {

        }
    }
}
