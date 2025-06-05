//-----------------------------------------------------------------------
// <copyright file = "PickWeightException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception thrown when there is a problem with the weight of a pick.
    /// </summary>
    [Serializable]
    public class PickWeightException : Exception
    {
        /// <summary>
        /// Construct a PickWeightException.
        /// </summary>
        /// <param name="message">The error message.</param>
        public PickWeightException(string message)
            : base(message)
        {

        }
    }
}
