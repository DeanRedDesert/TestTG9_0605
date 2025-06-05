// -----------------------------------------------------------------------
// <copyright file = "BaseAbortState.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Interfaces;

    /// <summary>
    /// A base implementation of an Abort game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    public class BaseAbortState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "AbortState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when the abort complete response is received.
        /// </summary>
        public IGameState NextOnAbortComplete { protected get; set; }

        /// <summary>
        /// The state to go when about the abort request is rejected.
        /// </summary>
        public IGameState NextOnAbortRejected { protected get; set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The abort complete event received after PreWait step.
        /// </summary>
        private AbortCompleteEventArgs abortCompleteEventArgs;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional. If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseAbortState(string stateName = DefaultName) : base(stateName)
        {
            InitialStep = StateStep.Processing;
        }

        #endregion

        #region IGameState Overrides

        /// <inheritdoc/>
        public override void Initialize(IGameFrameworkInitialization framework, object stateMachine)
        {
            base.Initialize(framework, stateMachine);

            framework.CoplayerLib.GameCyclePlay.AbortCompleteEvent += HandleAbortComplete;
        }

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            framework.CoplayerLib.GameCyclePlay.AbortCompleteEvent -= HandleAbortComplete;
        }

        /// <inheritdoc/>
        public override TransactionWeight GetTransactionWeight(StateStep step)
        {
            TransactionWeight result;

            switch(step)
            {
                case StateStep.Processing:
                {
                    result = TransactionWeight.Heavy;
                    break;
                }
                case StateStep.CommittedPreWait:
                {
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
            ProcessingStepControl result;

            var abortAccepted = framework.CoplayerLib.GameCyclePlay.AbortGameCycle();

            if(abortAccepted)
            {
                result = ProcessingStepControl.GoNext;
            }
            else
            {
                SetNextState(framework, NextOnAbortRejected, "NextOnAbortRejected");
                result = ProcessingStepControl.ExitState;
            }

            return result;
        }

        /// <inheritdoc/>
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            PreWaitStepControl result;

            // Clear cached data.
            // If the event has been received prior to this moment, it is still okay to clear,
            // since the game cycle state check below takes care of it.
            abortCompleteEventArgs = null;

            var gameCycleState = framework.CoplayerLib.GameCyclePlay.GameCycleState;

            // If we are still in pending, then go to Wait.  Otherwise move on to next state.
            // If the AbortCompleteEvent comes right before a power hit, after recovering,
            // we might already have left the AbortPending state, and the AbortCompleteEvent will
            // not be sent again.
            if(gameCycleState == GameCycleState.AbortPending)
            {
                framework.StartPresentationState();
                result = PreWaitStepControl.GoNext;
            }
            else
            {
                // If the game cycle is not AbortPending, the game has either already advanced, or the request was rejected
                // and the state machine should go backwards to the state it came from before the abort request.
                result = PreWaitStepControl.ExitState;
            }

            SetNextState(framework, NextOnAbortComplete, "NextOnAbortComplete");

            return result;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            var eventArrived = framework.WaitForNonTransactionalCondition(() => abortCompleteEventArgs != null);
            return eventArrived ? WaitStepControl.ExitState : WaitStepControl.RepeatWait;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handle the AbortComplete message from the foundation.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected virtual void HandleAbortComplete(object sender, AbortCompleteEventArgs eventArgs)
        {
            abortCompleteEventArgs = eventArgs;
        }

        #endregion
    }
}