// -----------------------------------------------------------------------
// <copyright file = "IStateMachine.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System.Collections.Generic;
    using Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines the APIs needed by the state machine execution.
    /// </summary>
    /// <typeparamref name="TState">
    /// The type of logic state contained in the state machine.
    /// </typeparamref>
    /// <typeparamref name="TFrameworkInitialization">
    /// The type of interface to access the functionalities available for
    /// a state machine and its states during the state machine initialization.
    /// </typeparamref>
    /// <typeparamref name="TFrameworkExecution">
    /// The type of interface to access the functionalities available for
    /// a state machine and its states during the state machine execution.
    /// </typeparamref>
    /// <typeparamref name="TLibInterface">
    /// The type of lib interface used to communicate with Foundation.
    /// </typeparamref>
    public interface IStateMachine<TState, TFrameworkInitialization, TFrameworkExecution, TLibInterface>
        where TState : ILogicState<TFrameworkInitialization, TFrameworkExecution>
        where TFrameworkInitialization : IFrameworkInitialization
        where TFrameworkExecution : IFrameworkExecution
        where TLibInterface : IAppLib
    {
        /// <summary>
        /// Gets the name of the first state to run.
        /// </summary>
        string InitialStateName { get; }

        /// <summary>
        /// Initializes the state machine.
        /// Providers should be created and initialized in this method.
        /// </summary>
        /// <remarks>
        /// This method is run without a transaction.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine initialization.
        /// </param>
        void Initialize(TFrameworkInitialization framework);

        /// <summary>
        /// Reads configuration information from Foundation and applies additional configurations
        /// before states begin to execute.
        /// </summary>
        /// <remarks>
        /// This method is run within a transaction.
        /// 
        /// It is executed each time that a state machine is started, after the <see cref="Initialize"/> method.
        /// Providers should not be created during this method call, they should be created earlier in
        /// <see cref="Initialize"/>.  This method should only be used for reading and configuring data.
        /// </remarks>
        /// <param name="libInterface">
        /// The reference of the lib interface to communicate with Foundation.
        /// </param>
        void ReadConfiguration(TLibInterface libInterface);

        /// <summary>
        /// Gets the state object by the given name.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state to get.
        /// </param>
        /// <returns>
        /// The state object of the given state name.
        /// </returns>
        /// <exception cref="ConcurrentLogicException">
        /// Thrown when no state by the given name is found.
        /// </exception>
        TState GetState(string stateName);

        /// <summary>
        /// Gets all the states available in the state machine.
        /// </summary>
        /// <returns>
        /// A list of logic states available in the state machine.
        /// </returns>
        IReadOnlyList<TState> GetAllStates();

        /// <summary>
        /// Clean up the resources held by the state machine.  Called by the framework
        /// when the state machine is done executing.
        /// The clean up work should include, but not limited to:
        /// <list type="bullet">
        ///     <item>Unregister all event handlers from the framework and the LibInterface.</item>
        /// </list>
        /// </summary>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine initialization.
        /// </param>
        void CleanUp(TFrameworkInitialization framework);
    }
}