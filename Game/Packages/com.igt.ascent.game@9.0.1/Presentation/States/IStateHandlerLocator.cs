//-----------------------------------------------------------------------
// <copyright file = "IStateHandlerLocator.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An interface to use for receiving notifications for game state handlers loaded/unloaded.
    /// </summary>
    public interface IStateHandlerLocator
    {
        /// <summary>
        /// This event is raised once state handlers are loaded.
        /// </summary>
        event EventHandler StateHandlersLoaded;

        /// <summary>
        /// This event is raised once state handlers are unloaded.
        /// </summary>
        event EventHandler StateHandlersUnloaded;

        /// <summary>
        /// Create <see cref="IStateHandler"/> instance.
        /// </summary>
        /// <param name="handlerRegistry">
        /// Create state handler based on the instance of <see cref="StateHandlerRegistry"/>.
        /// </param>
        /// <returns>The <see cref="IStateHandler"/> instance.</returns>
        IStateHandler CreateStateHandler(StateHandlerRegistry handlerRegistry);

        /// <summary>
        /// Try to load state handler of the specified scene.
        /// </summary>
        /// <param name="stateName">Name of the scene.</param>
        /// <returns>If state handlers have been loaded.</returns>
        bool TryLoadStateHandler(string stateName);

        /// <summary>
        /// Check if the specified state could be started.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <returns>If the specified state could be start.</returns>
        bool CanStartState(string stateName);

        /// <summary>
        /// Check if the specified state handler has been loaded or not.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <returns>True if the state handler was loaded; otherwise, false.</returns>
        bool IsStateHandlerLoaded(string stateName);

        /// <summary>
        /// Notify that the state handlers have been refreshed with the latest consumers.
        /// </summary>
        /// <param name="stateHandlers">The state handler list.</param>
        void PostRefreshHandlers(IList<string> stateHandlers);

        /// <summary>
        /// Get registration from specified state.
        /// </summary>
        /// <param name="stateName">Name of the state.</param>
        /// <returns>State handler registry.</returns>
        StateHandlerRegistry GetRegistration(string stateName);
    }
}
