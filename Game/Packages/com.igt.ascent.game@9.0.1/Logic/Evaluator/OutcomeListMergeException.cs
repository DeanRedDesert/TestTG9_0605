//-----------------------------------------------------------------------
// <copyright file = "OutcomeListMergeException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception thrown when an error occurs merging an OutcomeList into a WinOutcome.
    /// </summary>
    public class OutcomeListMergeException : Exception
    {
        /// <summary>
        /// Create a OutcomeListMergeException with the given message.
        /// </summary>
        /// <param name="message">Message indicating the reason for the exception.</param>
        public OutcomeListMergeException(string message) : base(message)
        {
        }
    }
}
