// -----------------------------------------------------------------------
// <copyright file = "IGameFrameworkRunner.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Interfaces;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// This interface defines functionalities provided by the coplayer runner
    /// to support the game state machine framework.
    /// </summary>
    internal interface IGameFrameworkRunner : IFrameworkRunner
    {
        /// <summary>
        /// Occurs when an event from the shell has been received.
        /// </summary>
        event EventHandler<EventDispatchedEventArgs> ShellEventReceived;

        /// <summary>
        /// Gets the interface for the shell to send the parcel calls on behalf of coplayers.
        /// </summary>
        IShellParcelCallSender ParcelCallSender { get; }

        /// <summary>
        /// Gets the interface used to obtain history information from the shell.
        /// </summary>
        IShellHistoryQuery ShellHistoryQuery { get; }

        /// <summary>
        /// Gets the interface used for the shell to send tilts on behalf of coplayers.
        /// </summary>
        IShellTiltSender ShellTiltSender { get; }

        /// <summary>
        /// Sends a custom coplayer event to the shell.
        /// </summary>
        /// <param name="eventArgs">
        /// The event to send.
        /// </param>
        /// <returns>
        /// True if the event has been delivered to the shell; False otherwise.
        /// </returns>
        bool SendEventToShell(CustomCoplayerEventArgs eventArgs);
    }
}