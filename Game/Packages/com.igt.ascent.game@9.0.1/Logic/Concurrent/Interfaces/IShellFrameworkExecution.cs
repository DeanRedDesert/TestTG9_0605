// -----------------------------------------------------------------------
// <copyright file = "IShellFrameworkExecution.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform.ShellLib.Interfaces;

    /// <inheritdoc/>
    public interface IShellFrameworkExecution : IFrameworkExecution
    {
        /// <summary>
        /// Gets the reference of the interface for communicating with Foundation.
        /// </summary>
        IShellLib ShellLib { get; }

        /// <summary>
        /// Gets the interface used for sending history data to the shell presentation.
        /// </summary>
        /// <remarks>
        /// This is to be only used by shell history state machine maintained by SDK.
        /// </remarks>
        IShellHistoryPresentation HistoryPresentation { get; }

        /// <summary>
        /// Gets the list of selectable themes in the Shell package.
        /// </summary>
        /// <returns>
        /// The list of selectable themes in the Shell package.
        /// </returns>
        IReadOnlyList<ShellThemeInfo> GetSelectableThemes();

        /// <summary>
        /// Gets the list of running co-themes whose presentation
        /// are pending to be loaded.
        /// </summary>
        /// <returns>
        /// The list of presentation keys for the running co-themes.
        /// </returns>
        IReadOnlyList<CothemePresentationKey> GetRunningCothemes();

        /// <summary>
        /// Starts a theme in a new Coplayer.
        /// </summary>
        /// <param name="g2SThemeId">
        /// The g2sThemeId of the theme to start.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the new theme.
        /// </param>
        /// <returns>
        /// True if the new theme is started successfully; False otherwise.
        /// A new theme cannot be started if the number of running cothemes
        /// has reached the limit set by Foundation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="g2SThemeId"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denomination"/> is less than 1.
        /// </exception>
        bool StartNewTheme(string g2SThemeId, long denomination);

        /// <summary>
        /// Starts a theme in a new Coplayer, whose ID is returned in the out parameter.
        /// </summary>
        /// <param name="g2SThemeId">
        /// The g2sThemeId of the theme to start.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the new theme.
        /// </param>
        /// <param name="coplayerId">
        /// Outputs the ID of the new Coplayer started.
        /// The value is undefined if the method returns false.
        /// </param>
        /// <returns>
        /// True if the new theme is started successfully; False otherwise.
        /// A new theme cannot be started if the number of running cothemes
        /// has reached the limit set by Foundation.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="g2SThemeId"/> is null or empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="denomination"/> is less than 1.
        /// </exception>
        bool StartNewTheme(string g2SThemeId, long denomination, out int coplayerId);

        /// <summary>
        /// Switches theme within a given Coplayer.
        /// </summary>
        /// <param name="coplayerId">The Coplayer number to switch on.</param>
        /// <param name="g2SThemeId">The new theme id.</param>
        /// <param name="denomination">The new denomination.</param>
        /// <returns>
        /// True if the Coplayer theme is switched successfully; False otherwise.
        /// The switching can fail if new theme is the same as the existing one.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0
        /// or <paramref name="denomination"/> is less than 1.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="g2SThemeId"/> is null or empty.
        /// </exception>
        bool SwitchCoplayerTheme(int coplayerId, string g2SThemeId, long denomination);

        /// <summary>
        /// Shuts down a running Coplayer and destroys its session.
        /// </summary>
        /// <param name="coplayerId">The Coplayer number to shut down.</param>
        /// <returns>
        /// True if the Coplayer is shut down successfully; False otherwise.
        /// The shut down can fail if the specified Coplayer is not running.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        bool ShutDownCoplayer(int coplayerId);

        /// <summary>
        /// Sends a custom Shell event to a specific Coplayer.
        /// </summary>
        /// <remarks>
        /// The method returns immediately when the event is added to the target queue(s).
        /// It does not wait for the event to be processed.
        /// </remarks>
        /// <param name="eventArgs">
        /// The event to send.
        /// </param>
        /// <param name="coplayerId">
        /// The Coplayer to send the event to.
        /// </param>
        /// <returns>
        /// True if the event has been delivered to the Coplayer; False otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="coplayerId"/> is less than 0.
        /// </exception>
        bool SendEventToCoplayer(CustomShellEventArgs eventArgs, int coplayerId);

        /// <summary>
        /// Sends a custom Shell event to one or more Coplayers.
        /// </summary>
        /// <remarks>
        /// The method returns immediately when the event is added to the target queue(s).
        /// It does not wait for the event to be processed.
        /// </remarks>
        /// <param name="eventArgs">
        /// The event to send.
        /// </param>
        /// <param name="targetCoplayers">
        /// The Coplayers to send the event to.
        /// </param>
        /// <returns>
        /// True if the event has been delivered to all target Coplayers; False otherwise.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="targetCoplayers"/> is null or empty, or when the list contains invalid Coplayer ids.
        /// </exception>
        bool SendEventToCoplayers(CustomShellEventArgs eventArgs, IReadOnlyList<int> targetCoplayers);

        /// <summary>
        /// Sends a custom Shell event to all running Coplayers.
        /// </summary>
        /// <remarks>
        /// The method returns immediately when the event is added to the target queue(s).
        /// It does not wait for the event to be processed.
        /// </remarks>
        /// <param name="eventArgs">
        /// The event to send.
        /// </param>
        /// <returns>
        /// True if the event has been delivered to all running Coplayers;  False otherwise.
        /// </returns>
        bool SendEventToAllCoplayers(CustomShellEventArgs eventArgs);

        /// <summary>
        /// Occurs when a custom Coplayer event has been received.
        /// </summary>
        event EventHandler<CustomCoplayerEventArgs> CoplayerEventReceived;
    }
}