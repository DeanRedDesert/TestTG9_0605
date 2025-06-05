//-----------------------------------------------------------------------
// <copyright file = "CoplayerHistoryStateMachine.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using IGT.Game.Core.Communication.CommunicationLib;
    using IGT.Game.Core.Communication.Logic.CommServices;
    using Interfaces;
    using ServiceProviders;

    /// <summary>
    /// A state machine used to drive coplayer's history display directly, and the shell's history display indirectly.
    /// </summary>
    /// <remarks>
    /// Each step of coplayer history has a corresponding step of shell history. These are stored together in the 
    /// coplayer's history store. The coplayer is responsible for reading both and sending them to the presentation
    /// via the <see cref="IHistoryPresentationControl"/> interface.
    /// </remarks>
    internal class CoplayerHistoryStateMachine : IGameStateMachine
    {
        #region History State

        /// <summary>
        /// Each time this state is executed it advances the history to the next step, updates the presentation, and waits
        /// for the presentation to complete.
        /// </summary>
        private class HistoryStateMoveNext : IGameState
        {
            #region Private Fields

            private readonly DataItems coplayerData = new DataItems();

            private CoplayerHistoryStateMachine historyStateMachine;
            private HistoryReader historyReader;

            #endregion

            #region IGameState Implementation

            /// <inheritdoc/>
            public string StateName => "MoveNextHistoryState";

            /// <inheritdoc/>
            public StateStep InitialStep => StateStep.CommittedPreWait;

            /// <inheritdoc/>
            public void Initialize(IGameFrameworkInitialization framework, object stateMachine)
            {
                historyStateMachine = (CoplayerHistoryStateMachine)stateMachine;
                historyReader = historyStateMachine.historyReader;
            }

            /// <inheritdoc/>
            public void CleanUp(IGameFrameworkInitialization framework)
            {
                historyStateMachine = null;
            }

            /// <inheritdoc/>
            public TransactionWeight GetTransactionWeight(StateStep step)
            {
                TransactionWeight result;

                switch(step)
                {
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
            public ProcessingStepControl Processing(IGameFrameworkExecution framework)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public PreWaitStepControl CommittedPreWait(IGameFrameworkExecution framework)
            {
                // Read history data from critical data
                if(historyReader.MoveNext())
                {
                    var totalSteps = historyReader.TotalHistorySteps;

                    // Attention!!!
                    // When writing history, the HistoryRecord.StepNumber is 1-based.
                    // However, when displaying history, the presentation side, specifically HistoryMenuController.
                    // uses 0-based currentStep.  Therefore, we have to modify the value in HistoryMetaData accordingly.

                    // Send shell record to shell
                    var shellRecord = historyReader.Current.ShellRecord;
                    shellRecord.HistoryData.HistoryData = HistoryMetaData.Create(totalSteps, shellRecord.StepNumber - 1);
                    framework.HistoryControl.SetShellHistoryData(new CothemePresentationKey(framework.CoplayerLib.Context.CoplayerId,
                                                                                            framework.CoplayerLib.Context.G2SThemeId),
                                                                 shellRecord.StepNumber,
                                                                 shellRecord.StateName,
                                                                 shellRecord.HistoryData);

                    // Process coplayer record and start the history presentation state
                    var coplayerRecord = historyReader.Current.CoplayerRecord;
                    coplayerRecord.HistoryData.HistoryData = HistoryMetaData.Create(totalSteps, coplayerRecord.StepNumber - 1);
                    coplayerData.Merge(coplayerRecord.HistoryData); // The coplayer history data saved was a diff
                    historyStateMachine.waitForPresentationState = coplayerRecord.StateName;
                    framework.HistoryControl.SetCoplayerHistoryData(historyStateMachine.waitForPresentationState,
                                                                    coplayerData);
                }

                historyStateMachine.presentationCompleteMessage = null;
                framework.SetNextState(StateName);
                return PreWaitStepControl.GoNext;
            }

            /// <inheritdoc/>
            public WaitStepControl CommittedWait(IGameFrameworkExecution framework)
            {
                var presentationComplete = framework.WaitForNonTransactionalCondition(() => historyStateMachine.HistoryPresentationComplete);

                if(presentationComplete)
                {
                    // In history, the presentation complete message may contain the information on
                    // whether to "restart" or "next".
                    switch(historyStateMachine.presentationCompleteMessage.ActionRequest)
                    {
                        case "FirstStep":
                        {
                            // Restart the steps.
                            historyReader.Reset();
                            break;
                        }
                    }
                }

                return presentationComplete ? WaitStepControl.ExitState : WaitStepControl.RepeatWait;
            }

            /// <inheritdoc/>
            public PostWaitStepControl CommittedPostWait(IGameFrameworkExecution framework)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region State Machine

        private readonly HistoryStateMoveNext historyState = new HistoryStateMoveNext();
        private HistoryReader historyReader;
        private GameLogicPresentationStateCompleteMsg presentationCompleteMessage;
        private string waitForPresentationState;

        #region IStateMachine Implementation

        /// <inheritdoc/>
        public string InitialStateName => historyState.StateName;

        /// <inheritdoc/>
        public void Initialize(IGameFrameworkInitialization framework)
        {
            historyReader = new HistoryReader(framework.CoplayerLib.HistoryStore);
            framework.PresentationEventReceived += HandlePresentationEventReceived;

            var gameContextModeProvider = new GameContextModeProvider(framework.CoplayerLib.Context.GameMode);
            framework.ServiceController.AddProvider(gameContextModeProvider, gameContextModeProvider.Name);

            var displayControlStateProvider = new DisplayControlStateProvider(framework.ObservableCollection.ObservableDisplayControlState);
            framework.ServiceController.AddProvider(displayControlStateProvider, displayControlStateProvider.Name);
        }

        /// <inheritdoc/>
        public void ReadConfiguration(ICoplayerLib libInterface)
        {
            System.Diagnostics.Debug.Assert(historyReader != null, "Call Initialize before ReadConfiguration.");
            historyReader.ReadConfiguration();
        }

        /// <inheritdoc/>
        public IGameState GetState(string stateName)
        {
            return historyState;
        }

        /// <inheritdoc/>
        public IReadOnlyList<IGameState> GetAllStates()
        {
            return new IGameState[] { historyState };
        }

        /// <inheritdoc/>
        public void CleanUp(IGameFrameworkInitialization framework)
        {
            historyReader = null;
            framework.PresentationEventReceived -= HandlePresentationEventReceived;
        }

        #endregion

        private void HandlePresentationEventReceived(object sender, GameLogicGenericMsg args)
        {
            presentationCompleteMessage = args as GameLogicPresentationStateCompleteMsg;
        }

        /// <summary>
        /// Returns true if the state machine has received a presentation complete message with the expected state name.
        /// </summary>
        private bool HistoryPresentationComplete => presentationCompleteMessage != null &&
                                                    string.Equals(presentationCompleteMessage.StateName,
                                                                  waitForPresentationState,
                                                                  StringComparison.Ordinal);

        #endregion
    }
}
