// -----------------------------------------------------------------------
// <copyright file = "BaseEndGameCycleState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.Interfaces;
    using Interfaces;

    /// <summary>
    /// A base implementation of an EndGameCycle game state.
    /// </summary>
    /// <remarks>
    /// This class can also be used as reference on how to transition
    /// game cycle states using the <see cref="ICoplayerLib"/> APIs.
    /// </remarks>
    public class BaseEndGameCycleState : GameStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "EndGameCycleState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when game cycle ends without idle pending.
        /// </summary>
        public IGameState NextOnComplete { protected get; set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseEndGameCycleState(string stateName = DefaultName) : base(stateName)
        {
            InitialStep = StateStep.Processing;
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
                case StateStep.Processing:
                {
                    // Processing could update some critical data, which only needs a Light transaction.
                    result = TransactionWeight.Light;
                    break;
                }
                case StateStep.CommittedPreWait:
                {
                    // EndGameCycle call needs a Heavy transaction.
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
        public override ProcessingStepControl Processing(IGameFrameworkExecution framework)
        {
            // Update some critical data.

            return ProcessingStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
        {
            framework.CoplayerLib.GameCyclePlay.EndGameCycle(framework.HistoryStepCount);

            SetNextState(framework, NextOnComplete, "CommittedPreWait nextState");

            return PreWaitStepControl.ExitState;
        }

        #endregion
    }
}