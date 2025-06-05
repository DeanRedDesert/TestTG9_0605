//-----------------------------------------------------------------------
// <copyright file = "ShellHistoryStateMachine.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using IGT.Ascent.Communication.Platform.ShellLib.Interfaces;
    using IGT.Game.Core.Communication.CommunicationLib;
    using IGT.Game.Core.Communication.Logic.CommServices;
    using Interfaces;
    using ServiceProviders;

    /// <summary>
    /// An implementation of a shell state machine that drives the display of game history.
    /// </summary>
    internal class ShellHistoryStateMachine : IShellStateMachine
    {
        #region State Base

        /// <summary>
        /// Abstract base class for history states.
        /// </summary>
        private abstract class HistoryState : IShellState
        {
            /// <summary>
            /// Gets the state machine reference.
            /// </summary>
            protected ShellHistoryStateMachine StateMachine { get; private set; }

            /// <inheritdoc/>
            public abstract string StateName { get; }

            /// <inheritdoc/>
            public abstract StateStep InitialStep { get; }

            /// <inheritdoc/>
            public virtual void Initialize(IShellFrameworkInitialization framework, object stateMachine)
            {
                StateMachine = (ShellHistoryStateMachine)stateMachine;
            }

            /// <inheritdoc/>
            public virtual void CleanUp(IShellFrameworkInitialization framework)
            {
                StateMachine = null;
            }

            /// <inheritdoc/>
            public virtual TransactionWeight GetTransactionWeight(StateStep step)
            {
                return TransactionWeight.None;
            }

            /// <inheritdoc/>
            public virtual ProcessingStepControl Processing(IShellFrameworkExecution framework)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public virtual PreWaitStepControl CommittedPreWait(IShellFrameworkExecution framework)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public abstract WaitStepControl CommittedWait(IShellFrameworkExecution framework);

            /// <inheritdoc/>
            public virtual PostWaitStepControl CommittedPostWait(IShellFrameworkExecution framework)
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region StartPresentation State

        /// <summary>
        /// The history state that starts the history presentation when data arrives from the history coplayer.
        /// </summary>
        private class HistoryStateStartPresentation : HistoryState
        {
            #region HistoryData Accessors

            private const string RunningCothemesProviderName = "RunningCothemesProvider";
            private readonly ServiceAccessor runningCothemesAccessor = new ServiceAccessor("RunningCothemes");
            private readonly ServiceAccessor cothemePresentationKeyAccessor = new ServiceAccessor("CothemePresentationKey");

            #endregion

            #region Private Fields

            private readonly DataItems shellData = new DataItems();

            private ShellHistoryReader historyReader;
            private ShellHistoryUpdateEventArgs historyUpdate;

            #endregion

            #region IShellState implementation

            /// <inheritdoc/>
            public override string StateName => "StartPresentationHistoryState";

            /// <inheritdoc/>
            public override StateStep InitialStep => StateStep.CommittedWait;

            /// <inheritdoc/>
            public override void Initialize(IShellFrameworkInitialization framework, object stateMachine)
            {
                base.Initialize(framework, stateMachine);
                framework.CoplayerEventReceived += HandleCoplayerEventReceived;

                // A little bit hacky, but we don't want to expose IShellHistoryControl to regular shell state machines.
                if(framework.ShellLib is IShellLibRestricted shellLibRestricted)
                {
                    historyReader = new ShellHistoryReader(shellLibRestricted.ShellHistoryControl);
                }
            }

            /// <inheritdoc/>
            public override void CleanUp(IShellFrameworkInitialization framework)
            {
                base.CleanUp(framework);
                framework.CoplayerEventReceived -= HandleCoplayerEventReceived;
            }

            /// <inheritdoc/>
            public override TransactionWeight GetTransactionWeight(StateStep step)
            {
                TransactionWeight result;

                switch(step)
                {
                    case StateStep.CommittedPostWait:
                    {
                        result = historyReader.Initialized ? TransactionWeight.None : TransactionWeight.Heavy;
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
            public override WaitStepControl CommittedWait(IShellFrameworkExecution framework)
            {
                var received = framework.WaitForNonTransactionalCondition(() => historyUpdate != null);

                return received ? WaitStepControl.GoNext : WaitStepControl.RepeatWait;
            }

            /// <inheritdoc/>
            public override PostWaitStepControl CommittedPostWait(IShellFrameworkExecution framework)
            {
                // Read the last step data from critical data if has not done so
                if(!historyReader.Initialized)
                {
                    historyReader.ReadLastStep();
                }

                // Check if current step is the last step; If yes, merge the data
                string shellStateName;
                if(historyUpdate.ShellRecord.StepNumber == historyReader.LastStepRecord.StepNumber)
                {
                    historyUpdate.ShellRecord.HistoryData.Merge(historyReader.LastStepRecord.HistoryData);
                    shellStateName = historyReader.LastStepRecord.StateName;
                }
                else
                {
                    shellStateName = historyUpdate.ShellRecord.StateName;
                }

                // Process history data
                AddCothemePresentationKey(historyUpdate.CoplayerPresentationKey, historyUpdate.ShellRecord.HistoryData);
                OverrideRunningCothemes(historyUpdate.CoplayerPresentationKey, historyUpdate.ShellRecord.HistoryData);

                // Merge the data into the master copy...This is needed once we calculate the diffs for shell history data
                // See the TODO item in HistoryManager
                shellData.Merge(historyUpdate.ShellRecord.HistoryData);

                // Prepare to wait for a new presentation complete message.
                StateMachine.presentationCompleteMessage = null;
                StateMachine.waitForPresentationState = shellStateName;

                // Start the shell history presentation state
                framework.HistoryPresentation.StartHistoryState(StateMachine.waitForPresentationState, shellData);

                // Ready to exit
                historyUpdate = null;
                framework.SetNextState("WaitForPresentationHistoryState");
                return PostWaitStepControl.ExitState;
            }

            #endregion

            /// <summary>
            /// Handles the events sent from the coplayers.
            /// </summary>
            /// <param name="sender">The event sender.</param>
            /// <param name="eventArgs">The event data.</param>
            private void HandleCoplayerEventReceived(object sender, CustomCoplayerEventArgs eventArgs)
            {
                if(eventArgs is ShellHistoryUpdateEventArgs historyUpdateEventArgs)
                {
                    historyUpdate = historyUpdateEventArgs;
                }
            }

            private void AddCothemePresentationKey(CothemePresentationKey cothemePresentationKey, DataItems data)
            {
                data.HistoryData[cothemePresentationKeyAccessor.Identifier] = cothemePresentationKey;
            }

            /// <summary>
            /// Overrides the running cothemes in the give data.
            /// </summary>
            /// <param name="runningCotheme">The running cotheme in current history display.</param>
            /// <param name="data">The data to override.</param>
            private void OverrideRunningCothemes(CothemePresentationKey runningCotheme, DataItems data)
            {
                if(data.TryGetValue(RunningCothemesProviderName, out var provider) &&
                   provider.ContainsKey(runningCothemesAccessor.Identifier))
                {
                    provider[runningCothemesAccessor.Identifier] = new List<CothemePresentationKey>
                                                                       {
                                                                           runningCotheme
                                                                       };
                }
            }
        }

        #endregion

        #region WaitForPresentation State

        /// <summary>
        /// The history state that waits for a presentation complete message.
        /// </summary>
        private class HistoryStateWaitForPresentation : HistoryState
        {
            /// <inheritdoc/>
            public override string StateName => "WaitForPresentationHistoryState";

            /// <inheritdoc/>
            public override StateStep InitialStep => StateStep.CommittedWait;

            /// <inheritdoc/>
            public override WaitStepControl CommittedWait(IShellFrameworkExecution framework)
            {
                WaitStepControl result;

                var presentationComplete = framework.WaitForNonTransactionalCondition(() => StateMachine.HistoryPresentationComplete);

                if(presentationComplete)
                {
                    framework.SetNextState("StartPresentationHistoryState");
                    result = WaitStepControl.ExitState;
                }
                else
                {
                    result = WaitStepControl.RepeatWait;
                }

                return result;
            }
        }

        #endregion

        #region State Machine

        private readonly IShellState[] states = { new HistoryStateStartPresentation(), new HistoryStateWaitForPresentation() };
        private readonly Dictionary<string, IShellState> stateMap;
        private GameLogicPresentationStateCompleteMsg presentationCompleteMessage;
        private string waitForPresentationState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellHistoryStateMachine"/> class.
        /// </summary>
        internal ShellHistoryStateMachine()
        {
            stateMap = states.ToDictionary(state => state.StateName, state => state);
        }

        #region IShellStateMachine implementation

        /// <inheritdoc/>
        public string InitialStateName => states[0].StateName;

        /// <inheritdoc/>
        public void Initialize(IShellFrameworkInitialization framework)
        {
            var gameContextModeProvider = new GameContextModeProvider(framework.ShellLib.Context.GameMode);
            framework.ServiceController.AddProvider(gameContextModeProvider, gameContextModeProvider.Name);

            var displayControlStateProvider = new DisplayControlStateProvider(framework.ObservableCollection.ObservableDisplayControlState);
            framework.ServiceController.AddProvider(displayControlStateProvider, displayControlStateProvider.Name);

            framework.PresentationEventReceived += HandlePresentationEventReceived;
        }

        /// <inheritdoc/>
        public void ReadConfiguration(IShellLib libInterface)
        {
        }

        /// <inheritdoc/>
        public IShellState GetState(string stateName)
        {
            if(stateMap.TryGetValue(stateName, out var state))
            {
                return state;
            }

            var message = $"Could not find state with name {stateName}.";
            throw new ConcurrentLogicException(message);
        }

        /// <inheritdoc/>
        public IReadOnlyList<IShellState> GetAllStates()
        {
            return new ReadOnlyCollection<IShellState>(states);
        }

        /// <inheritdoc/>
        public void CleanUp(IShellFrameworkInitialization framework)
        {
            framework.PresentationEventReceived -= HandlePresentationEventReceived;
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}
