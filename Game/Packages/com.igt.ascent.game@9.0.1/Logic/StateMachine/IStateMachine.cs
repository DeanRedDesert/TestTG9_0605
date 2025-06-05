//-----------------------------------------------------------------------
// <copyright file = "IStateMachine.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication.CommunicationLib;

    /// <summary>
    /// Delegate to use for specifying a state stage callback.
    /// </summary>
    /// <param name="framework">State machine framework which the callback may utilize.</param>
    public delegate void StateStageHandler(IStateMachineFramework framework);

    /// <summary>
    /// Delegate used for custom writing of history for StartState messages.
    /// </summary>
    /// <param name="stateName">The name of the state data is being written for.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="framework">Framework from which to get safe storage functions.</param>
    /// <returns>Not null if data was written during the handler.</returns>
    public delegate CommonHistoryBlock WriteStartStateHistoryHandler(string stateName, DataItems data,
                                                                     IStateMachineFramework framework);

    /// <summary>
    /// Delegate used for custom writing of history for UpdateAsynchData messages.
    /// </summary>
    /// <param name="stateName">The name of the state data is being written for.</param>
    /// <param name="data">The data to write.</param>
    /// <param name="framework">Framework from which to get safe storage functions.</param>
    /// <returns>Not null if data was written during the handler.</returns>
    public delegate CommonHistoryBlock WriteUpdateDataHistoryHandler(string stateName, DataItems data,
                                                                     IStateMachineFramework framework);

    /// <summary>
    /// This interface is used for state machines designed to
    /// execute within a state machine framework.
    /// </summary>
    public interface IStateMachine
    {
        /// <summary>
        /// The context information that was used to create the state machine.
        /// This property returns null if the state machine was constructed without a context.
        /// </summary>
        IStateMachineContext Context { get; }

        /// <summary>
        /// Property containing the currently configured states for
        /// the state machine.
        /// </summary>
        ICollection<string> States { get; }

        /// <summary>
        /// The initial state to execute for this state machine.
        /// This should be used the first time the state machine is
        /// executed. Subsequent executions should use safe stored
        /// States variables.
        /// </summary>
        /// <exception cref="InvalidStateException">
        /// If an attempt is made to set the initial state to one which
        /// does not exist in the currently configured states, then
        /// this exception will be thrown.
        /// </exception>
        string InitialState { get; }

        /// <summary>
        /// Tells whether power hit recovery functionality is enabled.
        /// If true, during power hit recovery, the state machine will replay
        /// all previously played game presentation states for the current game.
        /// </summary>
        bool RecoveryEnabled { get; }

        /// <summary>
        /// The state to execute when recovering from a power hit.
        /// </summary>
        string RecoveryState { get; }

        /// <summary>
        /// Tells whether the StateMachine is recovering from a power hit.
        /// </summary>
        bool IsRecovering { get; }

        /// <summary>
        /// Execute the specified state stage on the specified state.
        /// </summary>
        /// <param name="state">The state for which to execute a state stage.</param>
        /// <param name="stage">The state stage to execute.</param>
        /// <param name="framework">A state machine framework which the state can access.</param>
        void ExecuteStateStage(string state, StateStage stage, IStateMachineFramework framework);

        /// <summary>
        /// Check if a state is a history state.
        /// </summary>
        /// <param name="state">The name of the state to check.</param>
        /// <returns>True if the state is to be saved for history.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the state parameter is null, then this exception will be thrown.
        /// </exception>
        /// <exception cref="InvalidStateException">
        /// If the specified state is not configured, then this exception will be thrown.
        /// </exception>
        bool IsHistoryState(string state);

        /// <summary>
        /// Instruct the specified state to write history for a start state message.
        /// </summary>
        /// <param name="state">The state to write history for.</param>
        /// <param name="data">The data to write to history.</param>
        /// <param name="historyStep">Critical data path to write the record to.</param>
        /// <param name="framework">State machine framework to use for critical data access.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown if the passed state, framework, or data is null.
        /// </exception>
        /// <exception cref="InvalidStateException">
        /// This exception is thrown if the specified state is not configured for the state machine.
        /// </exception>
        void WriteStartStateHistory(string state, DataItems data, int historyStep,
                                    IStateMachineFramework framework);

        /// <summary>
        /// Instruct the specified state to write history for an asynchronous update.
        /// </summary>
        /// <param name="state">The state to write history for.</param>
        /// <param name="data">The data to write to history.</param>
        /// <param name="historyStep">Critical data path to write the record to.</param>
        /// <param name="framework">State machine framework to use for critical data access.</param>
        /// <exception cref="ArgumentNullException">
        /// This exception is thrown if the passed state, framework, or data is null.
        /// </exception>
        /// <exception cref="InvalidStateException">
        /// This exception is thrown if the specified state is not configured for the state machine.
        /// </exception>
        bool WriteUpdateHistory(string state, DataItems data, int historyStep,
                                IStateMachineFramework framework);

        /// <summary>
        /// Initialize the StateMachine. Providers should be created and initialized in this function.
        /// </summary>
        /// <param name="framework">State machine framework which hosts a service controller.</param>
        void Initialize(IStateMachineFramework framework);

        /// <summary>
        /// Replaces the current cache of required data requests with the one provided.
        /// </summary>
        /// <param name="requiredDataCache">
        /// A dictionary containing state names for keys and the <see cref="ServiceRequestData"/> objects for
        /// the corresponding states.
        /// </param>
        void UpdateRequiredDataCache(IEnumerable<KeyValuePair<string, ServiceRequestData>> requiredDataCache);

        /// <summary>
        /// Checks if the <paramref name="state"/> is a history state that has a asynchronous update handler.
        /// </summary>
        /// <param name="state">The name of the state.</param>
        /// <returns>True if the <paramref name="state"/> is a history state that has a asynchronous
        /// state handler, and false otherwise.</returns>
        bool IsAsynchronousHistoryState(string state);

        /// <summary>
        /// Method for reading configuration information before states begin to execute. This method will be executed
        /// each time that a state machine is started. It will occur after the <see cref="Initialize"/> method is
        /// called. Providers should not be created during this method call, they should be created earlier in
        /// <see cref="Initialize"/> and may have additional configuration applied during this method. This method
        /// should only be used for reading and configuring data. A transaction will be available for the duration of
        /// the method's execution.
        /// </summary>
        /// <param name="gameLib">
        /// An <see cref="IGameLib"/> instance that may be used for accessing configuration information.
        /// </param>
        void ReadConfiguration(IGameLib gameLib);

        /// <summary>
        /// Determines if the specified stage in a state has a handler function or not.
        /// </summary>
        /// <param name="state">The name of the state.</param>
        /// <param name="stage">The stage in the state.</param>
        /// <returns>True if there is a handler.</returns>
        bool ContainsStateHandler(string state, StateStage stage);

        /// <summary>
        /// Clean up the framework-related resource.
        /// </summary>
        /// <param name="framework">The state machine framework.</param>
        void CleanUp(IStateMachineFramework framework);
    }
}
