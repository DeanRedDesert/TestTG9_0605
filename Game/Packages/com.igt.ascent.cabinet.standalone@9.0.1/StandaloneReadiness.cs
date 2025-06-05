//-----------------------------------------------------------------------
// <copyright file = "StandaloneReadiness.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas;

    /// <summary>
    /// Provide a virtual implementation of the readiness category.
    /// </summary>
    class StandaloneReadiness : IReadiness, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// Unique identifier for the client provided on connection.
        /// </summary>
        private string virtualClientToken;

        /// <summary>
        /// The priority of the client.
        /// </summary>
        private Priority virtualClientPriority;

        /// <summary>
        /// Collection of clients and their ready states.
        /// </summary>
        private readonly Dictionary<Priority, Dictionary<string, ReadyState>> readyStates =
            new Dictionary<Priority, Dictionary<string, ReadyState>>();

        /// <summary>
        /// Event to fire when the ready state of a client changes.
        /// </summary>
        private event EventHandler<ReadyStateChangedEventArgs> ReadyStateChangedEvent;

        /// <summary>
        /// A list of clients for which ready state subscriptions are active.
        /// </summary>
        private readonly List<Priority> readyStateSubscriptions = new List<Priority>();

        /// <summary>
        /// A list of pending notification events.
        /// </summary>
        private readonly List<ReadyStateChangedEventArgs> pendingEvents = new List<ReadyStateChangedEventArgs>();

        /// <summary>
        /// Flag indicating if the connection has been configured.
        /// </summary>
        private bool connectionSet;

        #endregion

        #region IReadiness Implementation

        /// <inheritdoc/>
        /// <devdoc>Explicit interface implementations require specification of add/remove methods.</devdoc>
        event EventHandler<ReadyStateChangedEventArgs> IReadiness.ReadyStateChangedEvent
        {
            add => ReadyStateChangedEvent += value;
            remove => ReadyStateChangedEvent -= value;
        }

        /// <inheritdoc/>
        ICollection<ReadyStateStatus> IReadiness.GetReadyState(Priority clientPriority)
        {
            CheckConnection();

            var responses = new List<ReadyStateStatus>();

            if(readyStates.ContainsKey(clientPriority))
            {
                var matchedClients = readyStates[clientPriority];
                responses.AddRange(
                    matchedClients.Select(
                        matchedClient => new ReadyStateStatus(clientPriority, matchedClient.Key, matchedClient.Value)));
            }

            return responses;
        }

        /// <inheritdoc/>
        ReadyStateStatus IReadiness.GetReadyState(Priority clientPriority, string clientIdentifier)
        {
            CheckConnection();

            if(clientIdentifier == null)
            {
                throw new ArgumentNullException(nameof(clientIdentifier), "The client identifier may not be null.");
            }

            ReadyStateStatus status = null;
            if(readyStates.ContainsKey(clientPriority))
            {
                var clients = readyStates[clientPriority];
                if(clients.ContainsKey(clientIdentifier))
                {
                    var clientStatus = clients[clientIdentifier];
                    status = new ReadyStateStatus(clientPriority, clientIdentifier, clientStatus);
                }
            }

            return status;
        }

        /// <inheritdoc/>
        void IReadiness.SetReadyState(ReadyState newReadyState)
        {
            CheckConnection();
            UpdateVirtualClient(virtualClientToken, virtualClientPriority, newReadyState);
        }

        /// <inheritdoc/>
        void IReadiness.SubscribeToReadyNotifications(Priority clientPriority, bool subscribe)
        {
            CheckConnection();

            if(!subscribe)
            {
                readyStateSubscriptions.Remove(clientPriority);
            }
            else if(!readyStateSubscriptions.Contains(clientPriority))
            {
                readyStateSubscriptions.Add(clientPriority);
            }
        }

        #endregion

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        void ICabinetUpdate.Update()
        {
            foreach(var readyStateChangedEventArgs in pendingEvents)
            {
                ReadyStateChangedEvent?.Invoke(this, readyStateChangedEventArgs);
            }
            pendingEvents.Clear();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Set the token for the client.
        /// </summary>
        /// <param name="newToken">The token for the client.</param>
        /// <param name="newPriority">The priority for the client.</param>
        /// <exception cref="ArgumentNullException">Thrown if the new token is null.</exception>
        internal void SetConnectionInfo(string newToken, Priority newPriority)
        {
            virtualClientToken = newToken ?? throw new ArgumentNullException(nameof(newToken), "The token may not be null.");
            virtualClientPriority = newPriority;
            UpdateVirtualClient(virtualClientToken, virtualClientPriority, ReadyState.NotReadyForDisplay);
            connectionSet = true;
        }

        /// <summary>
        /// Add or update virtual clients.
        /// </summary>
        /// <param name="token">The token associated with the client.</param>
        /// <param name="priority">The priority associated with the client.</param>
        /// <param name="state">The ready state of the client.</param>
        internal void UpdateVirtualClient(string token, Priority priority, ReadyState state)
        {
            if(!readyStates.ContainsKey(priority))
            {
                readyStates[priority] = new Dictionary<string, ReadyState>();
            }

            var lastState = ReadyState.NotReadyForDisplay;
            if(readyStates[priority].ContainsKey(token))
            {
                lastState = readyStates[priority][token];
            }

            readyStates[priority][token] = state;

            if(lastState != state && readyStateSubscriptions.Contains(priority))
            {
                pendingEvents.Add(new ReadyStateChangedEventArgs(priority, token, state));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method for ensuring the standalone readiness implementation is in a valid state.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the client connection information has not been set.
        /// </exception>
        private void CheckConnection()
        {
            if(!connectionSet)
            {
                throw new InvalidOperationException("Standalone readiness support has not been properly configured.");
            }
        }

        #endregion
    }
}
