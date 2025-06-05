// -----------------------------------------------------------------------
// <copyright file = "BaseEvaluationState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Interfaces;
    using OutcomeList;
    using OutcomeList.Interfaces;

    /// <summary>
    /// A base implementation of an Evaluation game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class BaseEvaluationState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "EvaluationState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when evaluation is complete.
        /// </summary>
        public IGameState NextOnComplete { protected get; set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The outcome response ready event received after PreWait step.
        /// </summary>
        private OutcomeResponseReadyEventArgs outcomeResponseReadyEventArgs;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseEvaluationState(string stateName = DefaultName) : base(stateName)
        {
            InitialStep = StateStep.Processing;
        }

        #endregion

        #region IGameState Overrides

        /// <inheritdoc/>
        public override void Initialize(IGameFrameworkInitialization framework, object stateMachine)
        {
            base.Initialize(framework, stateMachine);

            framework.CoplayerLib.GameCyclePlay.OutcomeResponseReadyEvent += HandleOutcomeResponseReady;
        }

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            framework.CoplayerLib.GameCyclePlay.OutcomeResponseReadyEvent -= HandleOutcomeResponseReady;
        }

        /// <inheritdoc/>
        public override TransactionWeight GetTransactionWeight(StateStep step)
        {
            TransactionWeight result;

            switch(step)
            {
                case StateStep.Processing:
                case StateStep.CommittedPreWait:
                case StateStep.CommittedPostWait:
                {
                    // Outcome evaluation only needs Light transactions.
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
            var isLast = GetOutcomeList(framework, out var outcomeList);

            if(isLast)
            {
                framework.CoplayerLib.GameCyclePlay.AdjustLastOutcome(outcomeList, GetWagerCategoryOutcomes(framework));
            }
            else
            {
                framework.CoplayerLib.GameCyclePlay.AdjustOutcome(outcomeList);
            }

            return ProcessingStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            PreWaitStepControl result;

            // Clear cached data.
            // If the event has been received prior to this moment, it is still okay to clear,
            // since the latest data is retrieved by the "get response data" call below.
            outcomeResponseReadyEventArgs = null;

            var responseData = framework.CoplayerLib.GameCyclePlay.GetOutcomeResponseData();

            // If enroll response is ready, proceed to next state.
            if(responseData.IsReady)
            {
                ProcessOutcomeResponse(framework, responseData.OutcomeList, responseData.IsLastOutcome);

                result = PreWaitStepControl.ExitState;
            }
            // Otherwise, move on to Wait
            else
            {
                result = PreWaitStepControl.GoNext;
            }

            SetNextState(framework, NextOnComplete, "NextOnComplete");

            return result;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            // Wait for outcome response ready event.
            var eventArrived = framework.WaitForNonTransactionalCondition(() => outcomeResponseReadyEventArgs != null);

            return eventArrived ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
        }

        /// <inheritdoc/>
        public override PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
        {
            var responseData = framework.CoplayerLib.GameCyclePlay.GetOutcomeResponseData();

            // This should not happen.  At this moment, the response data should have been ready.
            if(!responseData.IsReady)
            {
                throw new LogicStateException(
                    string.Format($"In state {StateName}, CommittedPostWait was executed before outcome response data is ready."));
            }

            ProcessOutcomeResponse(framework, responseData.OutcomeList, responseData.IsLastOutcome);

            return PostWaitStepControl.ExitState;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// An event fired when the outcome response has been evaluated and potentially adjusted.
        /// </summary>
        /// <param name="sender">The object which raised this event.</param>
        /// <param name="eventArgs">The arguments associated with the updated outcome response.</param>
        protected virtual void HandleOutcomeResponseReady(object sender, OutcomeResponseReadyEventArgs eventArgs)
        {
            outcomeResponseReadyEventArgs = eventArgs;
        }

        /// <summary>
        /// Processes the most recent outcome list after it is returned from the foundation.
        /// </summary>
        /// <param name="framework">
        /// The game execution framework.
        /// </param>
        /// <param name="outcomeList">
        /// The outcome list returned by the foundation.
        /// </param>
        /// <param name="isLastOutcome">
        /// A flag indicating whether this outcome list concludes play. False if a bonus was triggered.
        /// </param>
        protected virtual void ProcessOutcomeResponse(IGameFrameworkExecution framework,
                                                      IOutcomeList outcomeList,
                                                      bool isLastOutcome)
        {
        }

        /// <summary>
        /// Gets the most recent outcome list after game play has occurred.
        /// Sets the passed in <see cref="OutcomeList"/>.
        /// </summary>
        /// <param name="framework">The game execution framework.</param>
        /// <param name="outcomeList">The outcome list to update with new data.</param>
        /// <returns></returns>
        protected virtual bool GetOutcomeList(IGameFrameworkExecution framework, out IOutcomeList outcomeList)
        {
            outcomeList = new OutcomeList();

            return true;
        }

        /// <summary>
        /// Gets the list containing information on wager category outcomes, their amounts and denominations.
        /// </summary>
        /// <param name="framework">The game execution framework.</param>
        /// <returns>A list of <see cref="WagerCategoryOutcome"/> objects.</returns>
        protected virtual IList<WagerCategoryOutcome> GetWagerCategoryOutcomes(IGameFrameworkExecution framework)
        {
            return new List<WagerCategoryOutcome>
                       {
                           new WagerCategoryOutcome(0, 1, 1),
                       };
        }

        #endregion
    }
}