// -----------------------------------------------------------------------
// <copyright file = "BasePlayState.cs" company = "IGT">
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
    /// A base implementation of a Play game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class BasePlayState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "PlayState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when main play is complete.
        /// </summary>
        public IGameState NextOnMainPlayComplete { protected get; set; }

        /// <summary>
        /// The state to go when a bonus is triggered.
        /// </summary>
        public IGameState NextOnBonusTriggered { protected get; set; }

        /// <summary>
        /// The state to go when an abort is being requested.
        /// </summary>
        public IGameState NextOnAbort { protected get; set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BasePlayState(string stateName = DefaultName) : base(stateName)
        {
            // No processing is needed.
            InitialStep = StateStep.CommittedPreWait;
        }

        #endregion

        #region IGameState Overrides

        /// <inheritdoc/>
        public override void CleanUp(IGameFrameworkInitialization framework)
        {
            // Unregister event handlers if any is subscribed.
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
                    // Accessing critical data only needs Light transactions.
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
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            framework.StartPresentationState();

            return PreWaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IGameFrameworkExecution framework)
        {
            var eventArrived = framework.WaitForPresentationStateComplete();

            return eventArrived ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
        }

        /// <inheritdoc/>
        public override PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
        {
            var nextState = CheckPlayStatus(framework);

            SetNextState(framework, nextState, "CommittedPostWait nextState");

            return PostWaitStepControl.ExitState;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Checks the play results and decides what state to transition to.
        /// </summary>
        /// <param name="framework">The game execution framework.</param>
        /// <returns>The next state to transition to.</returns>
        protected virtual IGameState CheckPlayStatus(IGameFrameworkExecution framework)
        {
            // Check for win results and see if any bonus is triggered
            // Losing game goes to Finalize state.
            return NextOnMainPlayComplete;
        }

        #endregion
    }
}