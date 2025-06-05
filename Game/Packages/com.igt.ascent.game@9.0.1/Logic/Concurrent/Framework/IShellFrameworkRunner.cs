// -----------------------------------------------------------------------
// <copyright file = "IShellFrameworkRunner.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// This interface defines functionalities provided by the shell runner
    /// to support the shell state machine framework.
    /// </summary>
    internal interface IShellFrameworkRunner : IFrameworkRunner
    {
        #region Managing Coplayers and Cothemes

        /// <summary>
        /// Gets the list of selectable themes in the shell package.
        /// </summary>
        /// <returns>
        /// The list of selectable themes in the shell package.
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
        /// Starts a theme in a new coplayer, whose ID is returned in the out parameter.
        /// </summary>
        /// <param name="g2SThemeId">
        /// The g2sThemeId of the theme to start.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the new theme.
        /// </param>
        /// <param name="coplayerId">
        /// Outputs the ID of the new coplayer started.
        /// The value is undefined if the method returns false.
        /// </param>
        /// <returns>
        /// True if the new theme is started successfully; False otherwise.
        /// A new theme cannot be started if the number of running cothemes
        /// has reached the limit set by Foundation.
        /// </returns>
        bool StartNewTheme(string g2SThemeId, long denomination, out int coplayerId);

        /// <summary>
        /// Switches theme within a given coplayer.
        /// </summary>
        /// <param name="coplayerId">The coplayer number to switch on.</param>
        /// <param name="g2SThemeId">The new theme id.</param>
        /// <param name="denomination">The new denomination.</param>
        /// <returns>
        /// True if the coplayer theme is switched successfully; False otherwise.
        /// The switching can fail if new theme is the same as the existing one.
        /// </returns>
        bool SwitchCoplayerTheme(int coplayerId, string g2SThemeId, long denomination);

        /// <summary>
        /// Shuts down a running coplayer and destroys its session.
        /// </summary>
        /// <param name="coplayerId">The coplayer number to shut down.</param>
        /// <returns>
        /// True if the coplayer is shut down successfully; False otherwise.
        /// The shut down can fail if the specified coplayer is not running.
        /// </returns>
        bool ShutDownCoplayer(int coplayerId);

        #endregion

        #region Events to/from Coplayers

        /// <summary>
        /// Occurs when an event from a coplayer has been received.
        /// </summary>
        event EventHandler<EventDispatchedEventArgs> CoplayerEventReceived;

        /// <summary>
        /// Posts a <see cref="PlatformEventArgs"/> to target coplayers, one by one.
        /// This function blocks until the event is processed by all the coplayers.
        /// </summary>
        /// <remarks>
        /// When routing events to coplayer, the event must be non-transactional.
        /// </remarks>
        /// <param name="platformEventArgs">
        /// The event to post.  Its transaction weight must be None.
        /// </param>
        /// <param name="isHeavyweightTransaction">
        /// The flag indicating whether this function is called within a heavyweight transaction.
        /// </param>
        /// <param name="targetCoplayers">
        /// The list of target coplayers.  Null or empty list means routing to all coplayers.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <exception cref="FrameworkRunnerException">
        /// Throws when <paramref name="platformEventArgs"/> is transactional.
        /// </exception>
        void PostEventToCoplayers(PlatformEventArgs platformEventArgs, bool isHeavyweightTransaction, IReadOnlyList<int> targetCoplayers = null);

        /// <summary>
        /// Enqueues a <see cref="PlatformEventArgs"/> to target coplayers, one by one.
        /// This function does not block.  It returns as soon as the event has been added to the coplayer's event queue.
        /// </summary>
        /// <remarks>
        /// When routing events to coplayer, the event must be non-transactional.
        /// </remarks>
        /// <param name="platformEventArgs">
        /// The event to enqueue.  Its transaction weight must be None.
        /// </param>
        /// <param name="targetCoplayers">
        /// The list of target coplayers.  Null or empty list means routing to all coplayers.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <exception cref="FrameworkRunnerException">
        /// Throws when <paramref name="platformEventArgs"/> is transactional.
        /// </exception>
        void EnqueueEventToCoplayers(PlatformEventArgs platformEventArgs, IReadOnlyList<int> targetCoplayers = null);

        #endregion
    }
}