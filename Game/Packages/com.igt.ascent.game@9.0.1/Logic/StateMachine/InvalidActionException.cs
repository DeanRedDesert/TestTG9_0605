//-----------------------------------------------------------------------
// <copyright file = "InvalidActionException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;

    ///<summary>
    /// Used to notify that there was an invalid action.
    ///</summary>
    public class InvalidActionException : Exception
    {
        /// <summary>
        /// Create an InvalidActionException with the given message.
        /// </summary>
        /// <param name="message">A message describing why the exception occurred.</param>
        public InvalidActionException(string message)
            : base(message)
        {
        }
    }
}