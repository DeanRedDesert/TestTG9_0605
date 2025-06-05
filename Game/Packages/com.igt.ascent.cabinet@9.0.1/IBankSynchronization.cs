//-----------------------------------------------------------------------
// <copyright file = "IBankSynchronization.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Interface for synchronizing game attract behaviors across a bank of EGMs.
    /// </summary>
    public interface IBankSynchronization
    {
        /// <summary>
        /// Gets the current bank synchronization status.
        /// </summary>
        /// <returns>The bank synchronization status and information.</returns>
        BankSynchronizationInformation GetSynchronizationStatus();

        /// <summary>
        /// Gets whether game events have been enabled or not.
        /// </summary>
        bool GameEventsEnabled { get; }

        /// <summary>
        /// Request registration for game events.
        /// </summary>
        /// <returns>True if registration succeeded.</returns>
        bool RegisterForGameEvents();

        /// <summary>
        /// Unregister for game events.
        /// </summary>
        void UnregisterForGameEvents();

        /// <summary>
        /// Send a game event message.
        /// </summary>
        /// <param name="messagePayload">The serialized message to send.</param>
        void SendGameEvent(string messagePayload);

        /// <summary>
        /// Clean up data used by game events.
        /// </summary>
        void CleanUpGameEvents();

        /// <summary>
        /// Raised when a new game message event is received.
        /// </summary>
        event EventHandler<GameMessageReceivedEventArgs> GameMessageReceivedEvent;
    }
}
