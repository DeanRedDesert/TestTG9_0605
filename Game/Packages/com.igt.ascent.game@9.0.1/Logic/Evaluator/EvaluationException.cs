//-----------------------------------------------------------------------
// <copyright file = "EvaluationException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception to be thrown when there is a problem with evaluation which is not based on the
    /// configuration.
    /// </summary>
    public class EvaluationException : Exception
    {
        /// <summary>
        /// Initialize an instance of EvaluationException.
        /// </summary>
        /// <param name="message">Reason for this exception being thrown.</param>
        public EvaluationException(string message)
            : base(message)
        {
        }
    }
}

