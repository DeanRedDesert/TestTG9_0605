//-----------------------------------------------------------------------
// <copyright file = "TriggerException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator
{
    using System;

    /// <summary>
    /// Exception to be thrown when there is a problem with a trigger.
    /// </summary>
    public class TriggerException : Exception
    {
        /// <summary>
        /// Initialize an instance of TriggerException.
        /// </summary>
        /// <param name="message">Reason for this exception being thrown.</param>
        public TriggerException(string message)
            : base(message)
        {
        }
    }
}
