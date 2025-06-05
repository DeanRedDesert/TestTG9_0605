//-----------------------------------------------------------------------
// <copyright file = "StateMachineForcedExitException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;

    /// <summary>
    /// Exception used to indicate that the state machine has been forced to exit. This exception
    /// should be thrown when the state machine is shutdown in a state which was not designed for shutdown.
    /// </summary>
    /// <devdoc>This exception is used to shutdown the state machine when running in the Unity editor.</devdoc>
    public class StateMachineForcedExitException : Exception
    {
        /// <summary>
        /// Construct a new StateMachineForcedExitException.
        /// </summary>
        /// <param name="message">
        /// Message to associate with the exception. This should be the reason for the forced exit.
        /// </param>
        public StateMachineForcedExitException(string message) : base(message)
        {
        }
    }
}
