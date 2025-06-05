// -----------------------------------------------------------------------
// <copyright file = "BaseFinalizeState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Interfaces;

    /// <summary>
    /// A base implementation of a Finalize game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    public class BaseFinalizeState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "FinalizeState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when finalize outcome response is received.
        /// </summary>
        public IGameState NextOnComplete { protected get; set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The finalize outcome event received after PreWait step.
        /// </summary>
        private FinalizeOutcomeEventArgs finalizeOutcomeEventArgs;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseFinalizeState(string stateName = DefaultName) : base(stateName)
        {
            InitialStep = StateStep.Processing;
        }

        #endregion

        #region IGameState Overrides

        /// <inheritdoc/>
        public override void Initialize(IGameFrameworkInitialization framework, object stateMachine)
        {
            base.Initialize(framework, stateMachine);

            framework.CoplayerLib.GameCyclePlay.FinalizeOutcomeEvent += HandleFinalizeOutcome;
        }

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            framework.CoplayerLib.GameCyclePlay.FinalizeOutcomeEvent -= HandleFinalizeOutcome;
        }

        /// <inheritdoc/>
        public override TransactionWeight GetTransactionWeight(StateStep step)
        {
            TransactionWeight result;

            switch(step)
            {
                case StateStep.Processing:
                case StateStep.CommittedPreWait:
                {
                    // Finalizing awards needs only Light transactions.
                    result = TransactionWeight.Light;
                    break;
                }
                default:
                {
                    result = TransactionWeight.None;
                    break;
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public override ProcessingStepControl Processing(IGameFrameworkExecution framework)
        {
            framework.CoplayerLib.GameCyclePlay.FinalizeOutcome();

            return ProcessingStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            PreWaitStepControl result;

            // Clear cached data.
            // If the event has been received prior to this moment, it is still okay to clear,
            // since the game cycle state check below takes care of it.
            finalizeOutcomeEventArgs = null;

            var gameCycleState = framework.CoplayerLib.GameCyclePlay.GameCycleState;

            // If we are still in pending, then go to Wait.  Otherwise move on to next state.
            // If the FinalizeOutcomeEvent comes right before a power hit, after recovering,
            // we might already have left the FinalizeAwardPending state, and the FinalizeOutcomeEvent will
            // not be sent again.
            if(gameCycleState == GameCycleState.FinalizeAwardPending)
            {
                framework.StartPresentationState();

                result = PreWaitStepControl.GoNext;
            }
            else
            {
                result = PreWaitStepControl.ExitState;
            }

            SetNextState(framework, NextOnComplete, "NextOnComplete");

            return result;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            var eventArrived = framework.WaitForNonTransactionalCondition(() => finalizeOutcomeEventArgs != null);

            return eventArrived ? WaitStepControl.ExitState : WaitStepControl.RepeatWait;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handle the FinalizeOutcome message from the foundation.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected virtual void HandleFinalizeOutcome(object sender, FinalizeOutcomeEventArgs eventArgs)
        {
            finalizeOutcomeEventArgs = eventArgs;
        }

        #endregion
    }
}