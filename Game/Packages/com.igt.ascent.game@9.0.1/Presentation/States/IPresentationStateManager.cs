//-----------------------------------------------------------------------
// <copyright file = "IPresentationStateManager.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Presentation state manager interface. This interface is used by presentation state handlers to access
    /// state manager functionality.
    /// </summary>
    public interface IPresentationStateManager
    {
        /// <summary>
        /// Presentation state complete is used to notify the logic that a presentation state has finished executing.
        /// </summary>
        /// <remarks>
        /// This function should be called by a state handler to notify that it is finished executing.
        /// </remarks>
        /// <param name="stateName">The name of the presentation state that is completing.</param>
        /// <param name="actionRequest">An action that the presentation state is attempting to initiate.</param>
        /// <param name="genericData">
        /// List of data that is not associated with he logic but should be safe stored.
        /// </param>
        void PresentationStateComplete(string stateName, string actionRequest,
                                       Dictionary<string, object> genericData = null);

        /// <summary>
        /// Get a list of the registered states.
        /// </summary>
        /// <devdoc>
        /// This information is needed by the Logic Data Selector so that it
        /// may provide the user a list of available states.
        /// </devdoc>
        ICollection<string> States{ get; }

        /// <summary>
        /// Get the state with the specified name.
        /// </summary>
        /// <param name="stateName">The name of the state to get.</param>
        /// <returns>The state handler for the specified state name.</returns>
        /// <exception cref="UnknownPresentationStateException">
        /// This exception is thrown when an attempt is made to get a state which does not exist.
        /// </exception>
        IStateHandler GetStateHandler(string stateName);

        /// <summary>
        /// Clean up and exit the current context.
        /// </summary>
        void ExitContext();

        /// <summary>
        /// Tells the state manager to clear any indication that the 
        /// first state has been entered as to re-raise the 
        /// <see cref="OnFirstStateEntered"/> during the next state OnEnter.
        /// </summary>
        void ResetFirstStateEnteredStatus();

        /// <summary>
        /// Raised when the OnEnter has been completed for the first state.
        /// </summary>
        event EventHandler OnFirstStateEntered;
    }
}
