// -----------------------------------------------------------------------
// <copyright file = "BaseShellIdleState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.Logic.CommServices;
    using Interfaces;

    /// <inheritdoc/>
    /// <summary>
    /// A base implementation of a Shell Idle state.
    /// </summary>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class BaseShellIdleState : ShellStateBase
    {
        #region Constants

        /// <summary>
        /// The default name of the state.
        /// </summary>
        private const string DefaultName = "ShellIdleState";

        #endregion

        #region State Transitions

        /// <summary>
        /// The state to go for loading cotheme presentations.
        /// </summary>
        public IShellState NextOnLoading { protected get; set; }

        #endregion

        #region Protected Fields

        /// <summary>
        /// The presentation state complete message saved after the PreWait step.
        /// </summary>
        protected GameLogicPresentationStateCompleteMsg PresentationStateCompleteMsg;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <param name="stateName">
        /// The name of the state.
        /// This parameter is optional.  If not specified, the state name
        /// will be set to <see cref="DefaultName"/>.
        /// </param>
        public BaseShellIdleState(string stateName = DefaultName) : base(stateName)
        {
            // No processing is needed.
            InitialStep = StateStep.CommittedPreWait;
        }

        #endregion

        #region IShellState Overrides

        /// <inheritdoc/>
        public override void Initialize(IShellFrameworkInitialization framework, object stateMachine)
        {
            base.Initialize(framework, stateMachine);

            framework.PresentationEventReceived += HandlePresentationEvent;
        }

        /// <inheritdoc/>
        public override void CleanUp(IShellFrameworkInitialization framework)
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
                    // Starting or switching themes needs Heavy transactions.
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
        public override PreWaitStepControl CommittedPreWait(IShellFrameworkExecution framework)
        {
            PreWaitStepControl result;

            // Clear cached data.
            // No presentation event should have arrived before the presentation state starts.
            PresentationStateCompleteMsg = null;

            SelectableThemesProvider.SelectableThemes = framework.GetSelectableThemes();
            RunningCothemesProvider.MaxNumCoplayers = framework.ShellLib.MaxNumCoplayers;

            var runningCothemes = framework.GetRunningCothemes();

            var goLoadingState = false;

            // If there is no running cothemes (which usually indicates a cold boot up)...
            if(runningCothemes.Count == 0)
            {
                // Check if we want to auto-start any themes.
                var startupThemes = GetStartupThemes(framework);

                if(startupThemes != null)
                {
                    // Ask Foundation to start these themes.  Only load the number allowed by the foundation.
                    // This could take a while to finish.
                    foreach(var startupTheme in startupThemes.Take(framework.ShellLib.MaxNumCoplayers))
                    {
                        framework.StartNewTheme(startupTheme.Key, startupTheme.Value);
                    }

                    goLoadingState = true;
                }
            }

            // If the running cothemes are different from those recorded in the provider
            // (which usually indicates a warm power up)...
            if(!runningCothemes.SequenceEqual(RunningCothemesProvider.RunningCothemes))
            {
                goLoadingState = true;
            }

            if(goLoadingState)
            {
                // Go to the theme loading state right a way.
                SetNextState(framework, NextOnLoading, "NextOnLoading");

                result = PreWaitStepControl.ExitState;
            }

            // If there is no running cothemes, and no start up theme is specified, OR
            // if the running cothemes are the same as those recorded in the provider
            // (which usually indicates a coming back from the theme loading state)...
            else
            {
                // Start the presentation state and wait for user input.
                framework.StartPresentationState();

                result = PreWaitStepControl.GoNext;
            }

            return result;
        }

        /// <inheritdoc/>
        public override WaitStepControl CommittedWait(IShellFrameworkExecution framework)
        {
            var eventArrived = framework.WaitForNonTransactionalCondition(() => PresentationStateCompleteMsg != null);

            return eventArrived ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
        }

        /// <inheritdoc/>
        public override PostWaitStepControl CommittedPostWait(IShellFrameworkExecution framework)
        {
            IShellState nextState;
            PostWaitStepControl result;

            if(PresentationStateCompleteMsg != null)
            {
                result = ProcessPresentationStateComplete(PresentationStateCompleteMsg,
                                                          framework,
                                                          out nextState);

                // Make sure to reset after processing.
                PresentationStateCompleteMsg = null;
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
        /// Gets a list of themes to start in case of a cold boot up.
        /// </summary>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine execution.
        /// </param>
        /// <returns>
        /// A list of [G2SThemeId, Denomination] pairs.  Null or empty list means DO NOT load any theme at start up.
        /// IList (rather than IDictionary) allows starting multiple copies for a theme and denomination pair.
        /// </returns>
        /// <remarks>
        /// If there are more startup themes than allowed cothemes by the foundation,
        /// only part of them will be loaded.
        /// 
        /// The base class implementation simply returns null.
        /// Derived class should override this method if it would like to load some themes at start up.
        /// </remarks>
        protected virtual IList<KeyValuePair<string, long>> GetStartupThemes(IShellFrameworkExecution framework)
        {
            return null;
        }

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
        }

        /// <summary>
        /// Handles a <see cref="GameLogicPresentationStateCompleteMsg"/>.
        /// </summary>
        /// <param name="presentationStateComplete">
        /// The event to handle.
        /// </param>
        /// <param name="framework">
        /// <see cref="IShellFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="nextState">
        /// Outputs the next state to transition to.
        /// </param>
        /// <returns>
        /// Which step to go next.
        /// </returns>
        protected virtual PostWaitStepControl ProcessPresentationStateComplete(GameLogicPresentationStateCompleteMsg presentationStateComplete,
                                                                               IShellFrameworkExecution framework,
                                                                               out IShellState nextState)
        {
            // By default, stay in Shell Idle state.
            nextState = this;

            if(TryParse(presentationStateComplete.ActionRequest, out var presentationAction))
            {
                var genericData = presentationStateComplete.GenericData.FirstOrDefault().Value;

                if(genericData == null)
                {
                    throw new LogicStateException("No generic data is found for action " + presentationAction);
                }

                switch(presentationAction)
                {
                    case BaseShellIdlePresentationAction.StartNewTheme:
                    {
                        if(!(genericData is StartNewThemeActionParam actionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(StartNewThemeActionParam)}");
                        }

                        nextState = StartNewTheme(framework, actionParam.G2SThemeId, actionParam.Denomination);

                        break;
                    }
                    case BaseShellIdlePresentationAction.SwitchCoplayerTheme:
                    {
                        if(!(genericData is SwitchCoplayerThemeActionParam actionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(SwitchCoplayerThemeActionParam)}");
                        }

                        nextState = SwitchCoplayerTheme(framework, actionParam.CoplayerId, actionParam.G2SThemeId,
                            actionParam.Denomination);

                        break;
                    }
                    case BaseShellIdlePresentationAction.ShutDownCoplayer:
                    {
                        if(!(genericData is ShutDownCoplayerActionParam actionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(ShutDownCoplayerActionParam)}");
                        }

                        nextState = ShutDownCoplayer(framework, actionParam.CoplayerId);

                        break;
                    }
                    case BaseShellIdlePresentationAction.RequestChooser:
                    {
                        if(!(genericData is RequestChooserActionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(RequestChooserActionParam)}");
                        }

                        framework.ShellLib.ChooserServices.RequestChooser();

                        nextState = this;

                        break;
                    }
                    case BaseShellIdlePresentationAction.AddCredits:
                    {
                        if(!(genericData is AddCreditsActionParam actionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(AddCreditsActionParam)}");
                        }

                        framework.ShellLib.ShowDemo.AddMoney(actionParam.CreditAmountToAdd);

                        nextState = this;

                        break;
                    }
                    case BaseShellIdlePresentationAction.RequestCashout:
                    {
                        if(!(genericData is RequestCashoutActionParam))
                        {
                            throw new LogicStateException(
                                $"Failed to cast generic data to {nameof(RequestCashoutActionParam)}");
                        }

                        framework.ShellLib.BankPlay.RequestCashout();

                        nextState = this;
                        break;
                    }
                }
            }

            return PostWaitStepControl.ExitState;
        }

        /// <summary>
        /// Asks the state machine framework to start a new theme.
        /// Derived class could override this method to execute some customized operation.
        /// </summary>
        /// <param name="framework">
        /// The <see cref="IShellFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="g2SThemeId">
        /// The G2S Theme Id of the new theme started.
        /// </param>
        /// <param name="denomination">
        /// The denomination for the new theme.
        /// </param>
        /// <returns>
        /// The next state to transition to.
        /// </returns>
        protected virtual IShellState StartNewTheme(IShellFrameworkExecution framework, string g2SThemeId, long denomination)
        {
            var success = framework.StartNewTheme(g2SThemeId, denomination);

            return success ? NextOnLoading : this;
        }

        /// <summary>
        /// Asks the state machine framework to switch the theme for a coplayer.
        /// Derived class could override this method to execute some customized operation.
        /// </summary>
        /// <param name="framework">
        /// The <see cref="IShellFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="coplayerId">
        /// The ID of the coplayer whose theme to be switched.
        /// </param>
        /// <param name="g2SThemeId">
        /// The G2S Theme Id of the new theme.
        /// </param>
        /// <param name="denomination">
        /// The denomination for the new theme.
        /// </param>
        /// <returns>
        /// The next state to transition to.
        /// </returns>
        protected virtual IShellState SwitchCoplayerTheme(IShellFrameworkExecution framework, int coplayerId, string g2SThemeId, long denomination)
        {
            var success = framework.SwitchCoplayerTheme(coplayerId, g2SThemeId, denomination);

            return success ? NextOnLoading : this;
        }

        /// <summary>
        /// Asks the state machine framework to shut down a coplayer.
        /// Derived class could override this method to execute some customized operation.
        /// </summary>
        /// <param name="framework">
        /// The <see cref="IShellFrameworkExecution"/> used when handling requests.
        /// </param>
        /// <param name="coplayerId">
        /// The ID of the coplayer to shut down.
        /// </param>
        /// <returns>
        /// The next state to transition to.
        /// </returns>
        protected virtual IShellState ShutDownCoplayer(IShellFrameworkExecution framework, int coplayerId)
        {
            var success = framework.ShutDownCoplayer(coplayerId);

            return success ? NextOnLoading : this;
        }

        #endregion

        #region Private Methods

        private static readonly Dictionary<string, BaseShellIdlePresentationAction> EnumValues =
            Enum.GetValues(typeof(BaseShellIdlePresentationAction))
                .Cast<BaseShellIdlePresentationAction>()
                .ToDictionary(ev => ev.ToString(), ev => ev);

        /// <devdoc>
        /// Enum.TryParse is only available on .NET Framework 4.0+.
        /// </devdoc>
        private static bool TryParse(string stringValue, out BaseShellIdlePresentationAction presentationAction)
        {
            return EnumValues.TryGetValue(stringValue, out presentationAction);
        }

        #endregion
    }
}