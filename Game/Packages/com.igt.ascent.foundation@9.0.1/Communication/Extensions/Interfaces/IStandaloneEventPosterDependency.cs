//-----------------------------------------------------------------------
// <copyright file = "IStandaloneEventPosterDependency.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// An interface that provides the functionality to add foundation events
    /// to the main event queue(s) for the standalone interface extension implementations.
    /// </summary>
    public interface IStandaloneEventPosterDependency
    {
        /// <summary>
        /// Adds a transactional event to the foundation event queue.
        /// </summary>
        /// <remarks>
        /// This method returns as soon as the event is enqueued.
        /// </remarks>
        /// <param name="eventArgs">
        /// The event to be added to the queue.
        /// </param>
        /// <devdoc>
        /// This can be modified to take PlatformEventArgs in the future once all events used
        /// by Interface Extensions have been updated to inherit from PlatformEventArgs.
        /// </devdoc>
        void PostTransactionalEvent(EventArgs eventArgs);

        /// <summary>
        /// Adds a non-transactional event to the corresponding foundation event queue.
        /// </summary>
        /// <remarks>
        /// This method returns as soon as the event is enqueued.
        /// </remarks>
        /// <param name="platformEventArgs">
        /// The event to be added to the queue.
        /// </param>
        void PostNonTransactionalEvent(PlatformEventArgs platformEventArgs);
    }
}
