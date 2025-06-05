//-----------------------------------------------------------------------
// <copyright file = "IReadiness.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Interface for Readiness related functionality of the cabinet.
    /// </summary>
    public interface IReadiness
    {
        /// <summary>
        /// Event which is fired upon a change in client readiness.
        /// </summary>
        event EventHandler<ReadyStateChangedEventArgs> ReadyStateChangedEvent;

        /// <summary>
        /// Get the current ready state of clients with the specified type. There can be more than one client for
        /// certain client types.
        /// </summary>
        /// <param name="clientPriority">The priority to get the state for.</param>
        /// <returns>A list of states of that priority along with identifying information.
        ///  The return could include no entries.
        ///  </returns>
        ICollection<ReadyStateStatus> GetReadyState(Priority clientPriority);

        /// <summary>
        /// Get the current ready state of the specified client.
        /// </summary>
        /// <param name="clientPriority">The priority to get the state for.</param>
        /// <param name="clientIdentifier">The identifier for the client.</param>
        /// <returns>The state of the requested client. Null if the client is not present.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the client identifier is null.</exception>
        ReadyStateStatus GetReadyState(Priority clientPriority, string clientIdentifier);

        /// <summary>
        /// Set the ready state for the current client connection.
        /// </summary>
        /// <param name="newReadyState">The new ready state.</param>
        void SetReadyState(ReadyState newReadyState);

        /// <summary>
        /// Subscribe and unsubscribe to ready state notifications for the specified client priority.
        /// </summary>
        /// <param name="clientPriority">The priority to subscribe or unsubscribe to.</param>
        /// <param name="subscribe">
        /// Flag that if true indicates that the client wants to subscribe to notifications. 
        /// </param>
        void SubscribeToReadyNotifications(Priority clientPriority, bool subscribe);
    }
}
