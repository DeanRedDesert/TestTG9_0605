//-----------------------------------------------------------------------
// <copyright file = "ReadyStateChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event indicating that the ready state of a CSI client has changed.
    /// </summary>
    public class ReadyStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The status of the client.
        /// </summary>
        public ReadyStateStatus Status { get; }

        /// <summary>
        /// Construct an instance of the event with the given parameters.
        /// </summary>
        /// <param name="priority">The priority of the client whose state changed.</param>
        /// <param name="identifier">A unique identifier for the client whose state changed.</param>
        /// <param name="state">The new ready state of the client.</param>
        public ReadyStateChangedEventArgs(Priority priority, string identifier, ReadyState state)
        {
            Status = new ReadyStateStatus(priority, identifier, state);
        }
    }
}
