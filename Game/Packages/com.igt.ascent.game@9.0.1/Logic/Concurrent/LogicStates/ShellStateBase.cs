// -----------------------------------------------------------------------
// <copyright file = "ShellStateBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;
    using Communication.Platform.Interfaces;
    using Interfaces;
    using ServiceProviders;

    /// <inheritdoc/>
    /// <summary>
    /// Base implementation of <see cref="IShellState"/>.
    /// </summary>
    public abstract class ShellStateBase : IShellState
    {
        #region Protected Fields

        /// <summary>
        /// Provides game services for querying information on the
        /// selectable themes for starting a new Coplayer in a Shell.
        /// </summary>
        protected SelectableThemesProvider SelectableThemesProvider;

        /// <summary>
        /// Provides game services for querying information on the running Cothemes in a Shell.
        /// </summary>
        protected RunningCothemesProvider RunningCothemesProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of shell state with the specified name.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throw when <paramref name="stateName"/> is null or empty.
        /// </exception>
        protected ShellStateBase(string stateName)
        {
            if(string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(stateName));
            }

            StateName = stateName;
        }

        #endregion

        #region IShellState Implementation

        /// <inheritdoc/>
        public string StateName { get; }

        /// <inheritdoc/>
        public StateStep InitialStep { get; protected set; }

        /// <inheritdoc/>
        public virtual void Initialize(IShellFrameworkInitialization framework, object stateMachine)
        {
            if(stateMachine is ShellStateMachineBase shellStateMachine)
            {
                SelectableThemesProvider = shellStateMachine.SelectableThemesProvider;
                RunningCothemesProvider = shellStateMachine.RunningCothemesProvider;
            }
        }

        /// <inheritdoc/>
        public abstract TransactionWeight GetTransactionWeight(StateStep step);

        /// <inheritdoc/>
        public virtual ProcessingStepControl Processing(IShellFrameworkExecution framework)
        {
            return ProcessingStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual PreWaitStepControl CommittedPreWait(IShellFrameworkExecution framework)
        {
            return PreWaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual WaitStepControl CommittedWait(IShellFrameworkExecution framework)
        {
            return WaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual PostWaitStepControl CommittedPostWait(IShellFrameworkExecution framework)
        {
            return PostWaitStepControl.ExitState;
        }

        /// <inheritdoc/>
        public abstract void CleanUp(IShellFrameworkInitialization framework);

        #endregion

        #region Protected Methods

        /// <summary>
        /// Tells the framework to set next state for execution.
        /// Validates the state before making the call.
        /// </summary>
        /// <param name="framework">
        /// The framework to use.
        /// </param>
        /// <param name="nextState">
        /// Next shell state to set.
        /// </param>
        /// <param name="description">
        /// The description of <paramref name="nextState"/>. Used for generating the error message.
        /// </param>
        protected void SetNextState(IShellFrameworkExecution framework, IShellState nextState, string description)
        {
            if(nextState == null)
            {
                throw new LogicStateException(string.Format($"In state {StateName}, {description} is null."));
            }

            framework.SetNextState(nextState.StateName);
        }

        #endregion
    }
}