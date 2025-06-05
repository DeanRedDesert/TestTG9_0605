//-----------------------------------------------------------------------
// <copyright file = "IEventCoordinator.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This interface defines functionality of an event coordinator,
    /// who coordinates the event processing among multiple event sources.
    /// </summary>
    public interface IEventCoordinator
    {
        /// <summary>
        /// Registers an event source with the event coordinator.
        /// </summary>
        /// <remarks>
        /// The event source must be unregistered by calling <see cref="UnregisterEventSource"/>
        /// when the event source ceases to exist or is disposed.
        /// </remarks>
        /// <param name="eventSource">The event source to register.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventSource"/> is null.
        /// </exception>
        void RegisterEventSource(IEventSource eventSource);

        /// <summary>
        /// Unregisters an event source with the event coordinator.
        /// </summary>
        /// <param name="eventSource">The event source to unregister.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventSource"/> is null.
        /// </exception>
        void UnregisterEventSource(IEventSource eventSource);
    }
}
