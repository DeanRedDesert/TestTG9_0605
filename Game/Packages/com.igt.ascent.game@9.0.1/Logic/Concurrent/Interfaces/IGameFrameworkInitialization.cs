// -----------------------------------------------------------------------
// <copyright file = "IGameFrameworkInitialization.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;

    /// <inheritdoc/>
    public interface IGameFrameworkInitialization : IFrameworkInitialization
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
        /// Gets the collection of observables that could provide source data for game services.
        /// </summary>
        ICoplayerObservableCollection ObservableCollection { get; }

        /// <summary>
        /// Gets the reference of the interface for sending and receiving parcel calls
        /// through the Shell.
        /// </summary>
        /// <remarks>
        /// Parcel Comm calls are limited to between the Shell and the Foundation.
        /// Coplayers have to go through the Shell for sending and receiving parcel calls.
        /// Since Coplayers and Shell are on different sessions, they cannot share the transactions.
        /// Therefore, all parcel calls at the Coplayer level are technically non-transactional.
        /// The names of "Transactional" and "NonTransactional" in this interface only mean that
        /// the former can set a return value (that does not need transactional operations) while
        /// the latter cannot.
        /// </remarks>
        IGameParcelComm GameParcelComm { get; }

        /// <summary>
        /// Occurs when a custom Shell event has been received.
        /// </summary>
        event EventHandler<CustomShellEventArgs> ShellEventReceived;
    }
}