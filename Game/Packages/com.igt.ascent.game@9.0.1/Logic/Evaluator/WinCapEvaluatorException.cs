//-----------------------------------------------------------------------
// <copyright file = "WinCapEvaluatorException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception thrown when an error occurs evaluating the win cap.
    /// </summary>
    public class WinCapEvaluatorException : Exception
    {
        /// <summary>
        /// Create a WinCapEvaluatorException with the given message.
        /// </summary>
        /// <param name="message">Message indicating the reason for the exception.</param>
        public WinCapEvaluatorException(string message) : base(message)
        {
        }
    }
}
