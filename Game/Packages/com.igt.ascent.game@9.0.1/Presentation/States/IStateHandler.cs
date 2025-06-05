//-----------------------------------------------------------------------
// <copyright file = "IStateHandler.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Reflection;
    using Communication.CommunicationLib;

    /// <summary>
    /// The IStateHandler interface provides functions which will be called by the State Manager for
    /// state transitions and for data updates during a state.
    /// </summary>
    public interface IStateHandler
    {
        /// <summary>
        /// The event occurs when the state handler is being destroyed.
        /// </summary>
        event EventHandler BeingDestroyed;

        /// <summary>
        /// Get or Set the StateManager object that owns this state.
        /// </summary>
        IPresentationStateManager StateManager { get; set; }

        /// <summary>
        /// Allow the state handler to run without the initial scenes being fully loaded.
        /// This should never be set to true if the state references any scene data outside of its own scene.
        /// For example, if a state references dashboard or DPP scene objects, CanRunWithoutFullLoad must be false.
        /// </summary>
        bool CanRunWithoutFullLoad { get; set; }

        /// <summary>
        /// OnEnter is called when the StartState has been called for the handler's state.
        /// </summary>
        /// <remarks>
        /// The OnEnter function is where a state handler should configure everything it needs to execute properly.
        /// This includes registering event handlers, such as a spin button pressed event.
        ///
        /// Anything that is setup in this method should be taken down / cleaned up when the state is finished
        /// executing (i.e. when the presentation complete event is sent).
        /// </remarks>
        /// <param name="stateData">Data associated with the state being started.</param>
        void OnEnter(DataItems stateData);

        /// <summary>
        /// OnExit is called when the handler's state is exited (i.e. when the presentation complete event is sent)
        /// or aborted (i.e. the game is switched to a new game mode, and hence a new presentation state without
        /// completing the old one).
        /// </summary>
        /// <remarks>
        /// The OnExit function is where a state handler should do the clean up job.
        /// Anything that is setup in OnEnter should be taken down / cleaned up in this method.
        ///
        /// This includes unregistering event handlers.  For instance the idle state may unregister the
        /// spin button handler so that additional spin messages are not received.
        /// </remarks>
        void OnExit();

        /// <summary>
        /// Called to clean up after OnExit()
        /// </summary>
        /// <remarks>
        /// Intended to be separated from the OnExit implementation.
        /// </remarks>
        void PostExit();

        /// <summary>
        ///    OnDataUpdate will be called when the UpdateAsynchData has been called for the handler's state.
        /// </summary>
        /// <param name = "updatedData">Data items that have been updated.</param>
        void OnDataUpdate(DataItems updatedData);

        /// <summary>
        /// OnPresentationComplete will be called when any state handler calls PresentationComplete.
        /// </summary>
        void OnPresentationComplete();

        /// <summary>
        /// Add a game service consumer to this state.
        /// </summary>
        /// <param name="consumerProperty">The property that consumes the service data.</param>
        /// <param name="consumer">The object which has the consumer property.</param>
        /// <param name="serviceProvider">The name of the service provider.</param>
        /// <param name="serviceLocation">Service to access within the provider.</param>
        void AddConsumerProperty(PropertyInfo consumerProperty, object consumer, string serviceProvider,
                                 ServiceAccessor serviceLocation);

        /// <summary>
        /// Clear all the consumer properties.
        /// </summary>
        void ClearConsumerProperties();
    }
}
