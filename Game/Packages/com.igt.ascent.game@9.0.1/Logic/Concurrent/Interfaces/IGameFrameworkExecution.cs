// -----------------------------------------------------------------------
// <copyright file = "IGameFrameworkExecution.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;

    /// <inheritdoc/>
    public interface IGameFrameworkExecution : IFrameworkExecution
    {
        /// <summary>
        /// Gets the reference of the interface for communicating with Foundation.
        /// </summary>
        ICoplayerLib CoplayerLib { get; }

        /// <summary>
        /// Gets the reference of the interface for receiving notifications from the Shell
        /// and querying config data and properties values that are manged by the Shell.
        /// </summary>
        IShellExposition ShellExposition { get; }

        /// <summary>
        /// Gets the reference of the interface for sending and receiving parcel calls
        /// through the Shell.
        /// </summary>
        /// <remarks>
        /// Parcel Comm calls are limited between the Shell and the Foundation.
        /// Coplayers have to go through the Shell for sending and receiving parcel calls.
        /// Since Coplayers and Shell are on different sessions, they cannot share the transactions.
        /// Therefore, all parcel calls at the Coplayer level are technically non-transactional.
        /// The names of "Transactional" and "NonTransactional" in this interface only mean that
        /// the former can set a return value (that does not need transactional operations) while
        /// the latter cannot.
        /// </remarks>
        IGameParcelComm GameParcelComm { get; }

        /// <summary>
        /// Gets the interface used to display which history data is being displayed for the coplayer.
        /// </summary>
        IHistoryPresentationControl HistoryControl { get; }

        /// <summary>
        /// Gets the interface used to interact with tilts.
        /// </summary>
        ICoplayerTiltController TiltController { get; }

        /// <summary>
        /// Gets the number of history steps that the game framework has written for the current game cycle.
        /// </summary>
        int HistoryStepCount { get; }

        /// <summary>
        /// Sends a custom Coplayer event to the Shell.
        /// </summary>
        /// <remarks>
        /// The method returns immediately when the event is added to the target queue.
        /// It does not wait for the event to be processed.
        /// </remarks>
        /// <param name="eventArgs">
        /// The event to send.
        /// </param>
        /// <returns>
        /// True if the event has been delivered to the Shell; False otherwise.
        /// </returns>
        bool SendEventToShell(CustomCoplayerEventArgs eventArgs);

        /// <summary>
        /// Occurs when a custom Shell event has been received.
        /// </summary>
        event EventHandler<CustomShellEventArgs> ShellEventReceived;
    }
}