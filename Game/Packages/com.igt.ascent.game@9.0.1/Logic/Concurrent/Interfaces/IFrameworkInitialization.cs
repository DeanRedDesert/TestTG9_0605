// -----------------------------------------------------------------------
// <copyright file = "IFrameworkInitialization.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Game.Core.Communication.Logic.CommServices;
    using Game.Core.Logic.Services;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This interface defines functionality available for a state machine and its states
    /// during the state machine initialization.
    /// The state machine and its states can access less functionality during initialization
    /// than execution.
    /// </summary>
    [SuppressMessage("ReSharper", "EventNeverInvoked.Global")]
    public interface IFrameworkInitialization
    {
        /// <summary>
        /// Gets the service controller to facilitate the state machine initialization.
        /// </summary>
        IServiceController ServiceController { get; }

        /// <summary>
        /// Occurs when the presentation sends a message to the logic over GL2P.
        /// </summary>
        event EventHandler<GameLogicGenericMsg> PresentationEventReceived;

        /// <summary>
        /// Registers an event queue to participate in the event processing loop.
        /// </summary>
        /// <remarks>
        /// The event queue must be unregistered by calling <see cref="UnregisterEventQueue"/>
        /// when the event queue ceases to exist or is disposed, such as in the state machine's
        /// CleanUp method.
        /// 
        /// If two event queues receive events at the same time,
        /// the event queue registered earlier will be processed first.
        /// </remarks>
        /// <param name="eventQueue">
        /// The event queue to register.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void RegisterEventQueue(IEventQueue eventQueue);

        /// <summary>
        /// Unregisters an event queue.
        /// </summary>
        /// <param name="eventQueue">The event queue to unregister.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void UnregisterEventQueue(IEventQueue eventQueue);
    }
}