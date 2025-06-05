//-----------------------------------------------------------------------
// <copyright file = "EvaluatorConfigurationException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception to be thrown when there is a problem with the evaluator configuration.
    /// </summary>
    public class EvaluatorConfigurationException : Exception
    {
        /// <summary>
        /// Initialize an instance of EvaluatorConfigurationException.
        /// </summary>
        /// <param name="message">Reason for this exception being thrown.</param>
        public EvaluatorConfigurationException(string message) : base(message)
        {
        }
    }
}
