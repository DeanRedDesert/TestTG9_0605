// -----------------------------------------------------------------------
// <copyright file = "GameStateBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;
    using Communication.Platform.Interfaces;
    using Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// Base implementation of <see cref="IGameState"/>.
    /// </summary>
    public abstract class GameStateBase : IGameState
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of game state with the specified name.
        /// </summary>
        /// <param name="stateName">
        /// The name of the state.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Throw when <paramref name="stateName"/> is null or empty.
        /// </exception>
        protected GameStateBase(string stateName)
        {
            if(string.IsNullOrEmpty(stateName))
            {
                throw new ArgumentException("State name cannot be null or empty.", nameof(stateName));
            }

            StateName = stateName;
        }

        #endregion

        #region IGameState Implementation

        /// <inheritdoc/>
        public string StateName { get; }

        /// <inheritdoc/>
        public StateStep InitialStep { get; protected set; }

        /// <inheritdoc/>
        public virtual void Initialize(IGameFrameworkInitialization framework, object stateMachine)
        {
        }

        /// <inheritdoc/>
        public abstract TransactionWeight GetTransactionWeight(StateStep step);

        /// <inheritdoc/>
        public virtual ProcessingStepControl Processing(IGameFrameworkExecution framework)
        {
            return ProcessingStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            return PreWaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            return WaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public virtual PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
        {
            return PostWaitStepControl.ExitState;
        }

        /// <inheritdoc/>
        public abstract void CleanUp(IGameFrameworkInitialization framework);

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
        /// Next game state to set.
        /// </param>
        /// <param name="description">
        /// The description of <paramref name="nextState"/>. Used for generating the error message.
        /// </param>
        protected void SetNextState(IGameFrameworkExecution framework, IGameState nextState, string description)
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