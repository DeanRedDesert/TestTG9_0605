// -----------------------------------------------------------------------
// <copyright file = "BaseShellLoadThemeState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using Communication.Platform.Interfaces;
    using Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// A base implementation of a Shell Load Theme state.
    /// </summary>
    public class BaseShellThemeLoadingState : ShellStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "ShellThemeLoadingState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go when cotheme presentations loading is complete.
        /// </summary>
        public IShellState NextOnLoadingComplete { protected get; set; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseShellThemeLoadingState(string stateName = DefaultName) : base(stateName)
        {
            // No processing is needed.
            InitialStep = StateStep.CommittedPreWait;
        }

        #endregion

        #region IShellState Overrides

        /// <inheritdoc/>
        public override void CleanUp(IShellFrameworkInitialization framework)
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
                {
                    // Accessing critical data only needs Light transaction.
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
        public override PreWaitStepControl CommittedPreWait(IShellFrameworkExecution framework)
        {
            // Update the provider before starting the presentation state.
            SelectableThemesProvider.SelectableThemes = framework.GetSelectableThemes();
            RunningCothemesProvider.RunningCothemes = framework.GetRunningCothemes();
            RunningCothemesProvider.MaxNumCoplayers = framework.ShellLib.MaxNumCoplayers;

            framework.StartPresentationState();

            return PreWaitStepControl.GoNext;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IShellFrameworkExecution framework)
        {
            WaitStepControl result;

            var eventArrived = framework.WaitForPresentationStateComplete();

            if(eventArrived)
            {
                // Go to the theme loading state right a way.
                SetNextState(framework, NextOnLoadingComplete, "NextOnLoadingComplete");

                result = WaitStepControl.ExitState;
            }
            else
            {
                result = WaitStepControl.RepeatWait;
            }

            return result;
        }

        #endregion
    }
}