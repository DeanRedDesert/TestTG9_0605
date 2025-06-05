// -----------------------------------------------------------------------
// <copyright file = "BaseEnrollState.cs" company = "IGT">
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
    /// A base implementation of an Enroll game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    public class BaseEnrollState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "EnrollState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when enrollment has succeeded.
        /// </summary>
        public IGameState NextOnEnrollSuccess { protected get; set; }

        /// <summary>
        /// The state to go when enrollment has failed.
        /// </summary>
        public IGameState NextOnEnrollFailure { protected get; set; }

        #endregion

        #region Private Fields

        /// <summary>
        /// The enroll response ready event received after PreWait step.
        /// </summary>
        private EnrollResponseReadyEventArgs enrollResponseReadyEventArgs;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseEnrollState(string stateName = DefaultName) : base(stateName)
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

            framework.CoplayerLib.GameCyclePlay.EnrollResponseReadyEvent += HandleEnrollResponseReady;
        }

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            framework.CoplayerLib.GameCyclePlay.EnrollResponseReadyEvent -= HandleEnrollResponseReady;
        }

        /// <inheritdoc/>
        public override TransactionWeight GetTransactionWeight(StateStep step)
        {
            TransactionWeight result;

            switch(step)
            {
                case StateStep.CommittedPreWait:
                case StateStep.CommittedPostWait:
                {
                    // Processing enroll response needs heavyweight transactions.
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
            PreWaitStepControl result;

            // Clear cached data.
            // If the event has been received prior to this moment, it is still okay to clear,
            // since the latest data is retrieved by the "get response data" call below.
            enrollResponseReadyEventArgs = null;

            framework.StartPresentationState();

            var enrollResponseData = framework.CoplayerLib.GameCyclePlay.GetEnrollResponseData();

            // If enroll response is ready, proceed to next state.
            if(enrollResponseData.IsReady && enrollResponseData.EnrollSuccess != null)
            {
                // This implementation ignores the enrollment data.
                ProcessEnrollResponse(enrollResponseData.EnrollSuccess.Value, framework);

                if(enrollResponseData.EnrollSuccess.Value)
                {
                    SetNextState(framework, NextOnEnrollSuccess, "NextOnEnrollSuccess");
                }
                else
                {
                    SetNextState(framework, NextOnEnrollFailure, "NextOnEnrollFailure");
                }

                result = PreWaitStepControl.ExitState;
            }
            // Otherwise, move on to Wait
            else
            {
                result = PreWaitStepControl.GoNext;
            }

            return result;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            // Wait for enroll response ready event.
            var eventArrived = framework.WaitForNonTransactionalCondition(() => enrollResponseReadyEventArgs != null);

            return eventArrived ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
        }

        /// <inheritdoc/>
        public override PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
        {
            // This should not happen.  At this moment, the response ready event should have been received.
            if(enrollResponseReadyEventArgs == null)
            {
                throw new LogicStateException(
                    string.Format($"In state {StateName}, CommittedPostWait was executed before enroll response ready event is received."));
            }

            // This implementation ignores the enrollment data.
            ProcessEnrollResponse(enrollResponseReadyEventArgs.Success, framework);

            if(enrollResponseReadyEventArgs.Success)
            {
                SetNextState(framework, NextOnEnrollSuccess, "NextOnEnrollSuccess");
            }
            else
            {
                SetNextState(framework, NextOnEnrollFailure, "NextOnEnrollFailure");
            }

            return PostWaitStepControl.ExitState;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles enroll response ready event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the event.
        /// </param>
        /// <param name="eventArgs">
        /// The event arguments.
        /// </param>
        protected virtual void HandleEnrollResponseReady(object sender, EnrollResponseReadyEventArgs eventArgs)
        {
            enrollResponseReadyEventArgs = eventArgs;
        }

        /// <summary>
        /// Process the enroll response information.
        /// </summary>
        /// <param name="enrollSuccess">
        /// Whether or not the enrollment was successful.
        /// </param>
        /// <param name="framework">
        /// Game state machine framework.
        /// </param>
        protected virtual void ProcessEnrollResponse(bool enrollSuccess, IGameFrameworkExecution framework)
        {
            if(enrollSuccess)
            {
                framework.CoplayerLib.GameCycleBetting.PlaceStartingBet(IsCommittedMaxBet());
                framework.CoplayerLib.GameCyclePlay.StartPlaying();
            }
            else
            {
                SafeUnenroll(framework.CoplayerLib);
            }
        }

        /// <summary>
        /// Determine is the max bet is committed.
        /// </summary>
        /// <returns>
        /// False.
        /// </returns>
        protected virtual bool IsCommittedMaxBet()
        {
            return false;
        }

        /// <summary>
        /// Safely rewinds game cycle state back to <see cref="GameCycleState.Idle"/>
        /// by calling proper Coplayer Lib functions.
        /// </summary>
        /// <param name="coplayerLib">
        /// The reference of coplayer lib.
        /// </param>
        protected virtual void SafeUnenroll(ICoplayerLib coplayerLib)
        {
            coplayerLib.GameCycleBetting.UncommitBet();
            coplayerLib.GameCyclePlay.UnenrollGameCycle();
        }

        #endregion
    }
}