// -----------------------------------------------------------------------
// <copyright file = "BaseIdleState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.Logic.CommServices;
    using Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// A base implementation of an Idle game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib" /> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class BaseIdleState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "IdleState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when game cycle has been committed and enrolled.
        /// </summary>
        public IGameState NextOnCommitted { protected get; set; }

        #endregion

        #region Protected Fields

        /// <summary>
        /// The presentation state complete message saved after the PreWait step.
        /// </summary>
        protected GameLogicPresentationStateCompleteMsg PresentationStateCompleteMsg;

        /// <summary>
        /// The player session parameters reset message saved after the PreWait step.
        /// </summary>
        protected GameLogicPlayerSessionParametersResetMsg PlayerSessionParamsResetMsg;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseIdleState(string stateName = DefaultName) : base(stateName)
        {
            // No processing is needed.
            InitialStep = StateStep.CommittedPreWait;
        }

        #endregion

        #region IGameState Overrides

        /// <inheritdoc/>
        public override void Initialize(IGameFrameworkInitialization framework, object stateMachine)
        {
            base.Initialize(framework, stateMachine);

            framework.PresentationEventReceived += HandlePresentationEvent;
        }

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            framework.PresentationEventReceived -= HandlePresentationEvent;
        }

        /// <inheritdoc/>
        public override TransactionWeight GetTransactionWeight(StateStep step)
        {
            TransactionWeight result;

            switch(step)
            {
                case StateStep.CommittedPreWait:
                {
                    // Accessing critical data only needs Light transactions.
                    result = TransactionWeight.Light;
                    break;
                }
                case StateStep.CommittedPostWait:
                {
                    // EnrollGameCycle call needs Heavy transactions.
                    result = TransactionWeight.Heavy;
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
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            // Clear cached data.
            // No presentation event should have arrived before the presentation state starts.
            PresentationStateCompleteMsg = null;
            PlayerSessionParamsResetMsg = null;

            framework.StartPresentationState();

            return PreWaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            var eventArrived = framework.WaitForNonTransactionalCondition(() => PresentationStateCompleteMsg != null ||
                                                                                PlayerSessionParamsResetMsg != null);

            return eventArrived ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
        }

        /// <inheritdoc/>
        public override PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
        {
            IGameState nextState;
            PostWaitStepControl result;

            if(PresentationStateCompleteMsg != null)
            {
                result = ProcessPresentationStateComplete(PresentationStateCompleteMsg,
                                                          framework,
                                                          out nextState);
            }
            else if(PlayerSessionParamsResetMsg != null)
            {
                result = ProcessPlayerSessionParamsReset(PlayerSessionParamsResetMsg,
                                                         framework,
                                                         out nextState);
            }
            else
            {
                // This should not happen.
                throw new LogicStateException(
                    string.Format($"In state {StateName}, CommittedPostWait was executed without having received any expected events."));
            }

            SetNextState(framework, nextState, "CommittedPostWait nextState");

            return result;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles a presentation event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected virtual void HandlePresentationEvent(object sender, GameLogicGenericMsg eventArgs)
        {
            if(eventArgs is GameLogicPresentationStateCompleteMsg presentationStateComplete)
            {
                PresentationStateCompleteMsg = presentationStateComplete;
            }
            else
            {
                // TODO EGT: Is the player session parameter reset message sent to Shell or Coplayer??
                if(eventArgs is GameLogicPlayerSessionParametersResetMsg playerSessionParamsReset)
                {
                    PlayerSessionParamsResetMsg = playerSessionParamsReset;
                }
            }
        }

        /// <summary>
        /// Handles a <see cref="GameLogicPresentationStateCompleteMsg"/>.
        /// </summary>
        /// <param name="presentationStateComplete">
        /// The event to handle.
        /// </param>
        /// <param name="framework">
        /// The <see cref="IGameFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="nextSate">
        /// Outputs the next state to transition to.
        /// </param>
        /// <returns>
        /// Which step to go next.
        /// </returns>
        protected virtual PostWaitStepControl ProcessPresentationStateComplete(GameLogicPresentationStateCompleteMsg presentationStateComplete,
                                                                               IGameFrameworkExecution framework,
                                                                               out IGameState nextSate)
        {
            // By default, stay in Idle state.
            nextSate = this;

            if(TryParse(presentationStateComplete.ActionRequest, out var presentationAction))
            {
                var genericData = presentationStateComplete.GenericData.FirstOrDefault().Value;

                if(genericData == null)
                {
                    throw new LogicStateException("No generic data is found for action " + presentationAction);
                }

                switch(presentationAction)
                {
                    case BaseIdlePresentationAction.CommitStart:
                    {
                        if(!(genericData is CommitStartActionParam actionParam))
                        {
                            throw new LogicStateException("Failed to cast generic data to CommitStartActionParam");
                        }

                        var committed = SafeCommitEnroll(framework.CoplayerLib,
                                                         actionParam.BetCredits,
                                                         framework.CoplayerLib.Context.Denomination);

                        if(committed)
                        {
                            nextSate = NextOnCommitted;
                        }

                        break;
                    }
                }
            }

            return PostWaitStepControl.ExitState;
        }

        /// <summary>
        /// Handles a <see cref="GameLogicPlayerSessionParametersResetMsg"/>.
        /// </summary>
        /// <param name="playerSessionParamsReset">
        /// The event to handle.
        /// </param>
        /// <param name="framework">
        /// The <see cref="IGameFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="nextState">
        /// Outputs the next state to transition to.
        /// </param>
        /// <returns>
        /// Which step to go next.
        /// </returns>
        protected virtual PostWaitStepControl ProcessPlayerSessionParamsReset(GameLogicPlayerSessionParametersResetMsg playerSessionParamsReset,
                                                                              IGameFrameworkExecution framework,
                                                                              out IGameState nextState)
        {
            // Stay in current state.
            nextState = this;

            // TODO ETG: if denomsReset == true, wait for denom changed event, then we should return RepeatWait here,

            return PostWaitStepControl.BackToPreWait;
        }

        /// <summary>
        /// Safely commit and enroll a game cycle with the given bet and enrollment data.
        /// In case of any of the committing calls failed, rewinds the game cycle state
        /// back to <see cref="GameCycleState.Idle"/> by calling proper Coplayer Lib functions.
        /// </summary>
        /// <param name="coplayerLib">
        /// The reference of coplayer lib.
        /// </param>
        /// <param name="betCredits">
        /// The bet amount in units of <paramref name="denomination"/>.
        /// </param>
        /// <param name="denomination">
        /// The denomination of the bet.
        /// </param>
        /// <param name="enrollmentData">
        /// The binary enrollment data to send to Foundation.
        /// This parameter is optional.  If not specified, it defaults to null.
        /// </param>
        /// <returns>
        /// The flag indicating if the game cycle has been successfully committed and enrolled.
        /// </returns>
        protected virtual bool SafeCommitEnroll(ICoplayerLib coplayerLib,
                                                long betCredits,
                                                long denomination,
                                                byte[] enrollmentData = null)
        {
            if(betCredits < 0 || denomination <= 0)
            {
                return false;
            }

            var result = false;

            if(coplayerLib.GameCyclePlay.CommitGameCycle())
            {
                if(coplayerLib.GameCycleBetting.CommitBet(betCredits, denomination))
                {
                    coplayerLib.GameCyclePlay.EnrollGameCycle(enrollmentData);
                    result = true;
                }
                else
                {
                    coplayerLib.GameCyclePlay.UncommitGameCycle();
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        private static readonly Dictionary<string, BaseIdlePresentationAction> EnumValues =
            Enum.GetValues(typeof(BaseIdlePresentationAction))
                .Cast<BaseIdlePresentationAction>()
                .ToDictionary(ev => ev.ToString(), ev => ev);

        /// <devdoc>
        /// Enum.TryParse is only available on .NET Framework 4.0+.
        /// </devdoc>
        private static bool TryParse(string stringValue, out BaseIdlePresentationAction presentationAction)
        {
            return EnumValues.TryGetValue(stringValue, out presentationAction);
        }

        #endregion
    }
}