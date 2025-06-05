// -----------------------------------------------------------------------
// <copyright file = "IShellFrameworkInitialization.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.ShellLib.Interfaces;

    /// <inheritdoc/>
    public interface IShellFrameworkInitialization : IFrameworkInitialization
    {
        /// <summary>
        /// Gets the reference of the interface for communicating with Foundation.
        /// </summary>
        IShellLib ShellLib { get; }

        /// <summary>
        /// Gets the collection of observables that could provide source data for game services.
        /// </summary>
        IShellObservableCollection ObservableCollection { get; }

        /// <summary>
        /// Occurs when a custom Coplayer event has been received.
        /// </summary>
        event EventHandler<CustomCoplayerEventArgs> CoplayerEventReceived;
    }
}