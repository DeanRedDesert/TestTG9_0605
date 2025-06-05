//-----------------------------------------------------------------------
// <copyright file = "ReadyStateStatus.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas;

    /// <summary>
    /// Class that represents the ready status of a client.
    /// </summary>
    public class ReadyStateStatus
    {
        #region Properties

        /// <summary>
        /// The priority of the client.
        /// </summary>
        public Priority Priority { get; }

        /// <summary>
        /// The identifier for the client.
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// The ready state of the client.
        /// </summary>
        public ReadyState ReadyState { get; }

        #endregion

        /// <summary>
        /// Construct an instance of the class with the given parameters.
        /// </summary>
        /// <param name="priority">The priority of the client.</param>
        /// <param name="identifier">A unique identifier for the client.</param>
        /// <param name="state">The ready state of the client.</param>
        public ReadyStateStatus(Priority priority, string identifier, ReadyState state)
        {
            Priority = priority;
            Identifier = identifier;
            ReadyState = state;
        }
    }
}
