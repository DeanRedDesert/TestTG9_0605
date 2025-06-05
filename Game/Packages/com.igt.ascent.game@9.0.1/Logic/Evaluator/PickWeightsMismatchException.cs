//-----------------------------------------------------------------------
// <copyright file = "PickWeightsMismatchException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception thrown when there is a mismatch in picks weights and available picks.
    /// </summary>
    [Serializable]
    class PickWeightsMismatchException : Exception
    {
        /// <summary>
        /// Construct a PickWeightException.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PickWeightsMismatchException(string message) : base (message)
        {
        }
    }
}
