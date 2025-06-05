//-----------------------------------------------------------------------
// <copyright file = "InvalidHistoryRecordException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;

    ///<summary>
    /// Used to notify that there was an invalid history record.
    ///</summary>
    public class InvalidHistoryRecordException : Exception
    {
        /// <summary>
        /// Create an InvalidHistoryRecordException with the given message.
        /// </summary>
        /// <param name="message">A message describing why the exception occurred.</param>
        public InvalidHistoryRecordException(string message)
            : base(message)
        {
        }
    }
}