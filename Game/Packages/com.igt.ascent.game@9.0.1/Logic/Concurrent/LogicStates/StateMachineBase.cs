// -----------------------------------------------------------------------
// <copyright file = "StateMachineBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Platform;
    using Interfaces;

    /// <summary>
    /// Generic base class for state machines to implement some common functionalities.
    /// </summary>
    /// <remarks>
    /// This class does implement some interface APIs.  But as the interfaces implemented
    /// are runner specific, this base class does not declare to implement any interface.
    /// The interface declaration is done by the derived classes.
    /// </remarks>
    public abstract class StateMachineBase<TState, TFrameworkInitialization, TFrameworkExecution> : IDisposable
        where TState : ILogicState<TFrameworkInitialization, TFrameworkExecution>
        where TFrameworkInitialization : IFrameworkInitialization
        where TFrameworkExecution : IFrameworkExecution
    {
        #region Protected Fields

        /// <summary>
        /// The list of states in the state machine, keyed by the state names.
        /// </summary>
        protected readonly Dictionary<string, TState> States;

        /// <summary>
        ///  The collection of disposable object held by this class.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly DisposableCollection DisposableCollection;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the state list in the state machine, using a list of states if provided.
        /// If no states are provided, the state list will start as an empty one.
        /// Additional states can be added in derived class constructors by calling <see cref="AddState"/>.
        /// </summary>
        /// <param name="states">
        /// The states to be part of the state machine.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        protected StateMachineBase(IReadOnlyList<TState> states = null)
        {
            DisposableCollection = new DisposableCollection();

            if(states?.Count > 0)
            {
                foreach(var state in states)
                {
                    DisposableCollection.Add(state);
                }

                States = states.ToDictionary(state => state.StateName, state => state);
            }
            else
            {
                States = new Dictionary<string, TState>();
            }
        }

        #endregion

        #region IStateMachine Implementation

        /// <remarks/>
        public string InitialStateName { get; protected set; }

        /// <remarks/>
        public TState GetState(string stateName)
        {
            if(!States.TryGetValue(stateName, out var result))
            {
                throw new ConcurrentLogicException("Could not find the state by name " + stateName);
            }

            return result;
        }

        /// <remarks/>
        public IReadOnlyList<TState> GetAllStates()
        {
            return States.Values.ToList();
        }

        #endregion

        #region IDisposable Implementation

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes resources held by this object.
        /// If <paramref name="disposing"/> is true, dispose both managed
        /// and unmanaged resources.
        /// Otherwise, only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing">True if called from Dispose.</param>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                DisposableCollection.Dispose();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Add a state to the state machine.
        /// </summary>
        /// <param name="state">
        /// The state to add.
        /// </param>
        /// <returns>
        /// The state that was added, same as <paramref name="state"/>.
        /// This is to allow code like:
        /// <code>
        /// var idleState = (IdleState)AddState(new IdleState());
        /// </code>
        /// instead of
        /// <code>
        /// var idleState = new IdleState();
        /// AddState(idleState);
        /// </code>
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="state"/> is null.
        /// </exception>
        /// <exception cref="LogicStateMachineException">
        /// Thrown when the state machine already has a state with the same name.
        /// </exception>
        protected TState AddState(TState state)
        {
            if(state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if(States.ContainsKey(state.StateName))
            {
                throw new LogicStateMachineException(
                    string.Format($"There is already a state named {state.StateName} in the state machine"));
            }

            States.Add(state.StateName, state);
            DisposableCollection.Add(state);

            return state;
        }

        #endregion
    }
}