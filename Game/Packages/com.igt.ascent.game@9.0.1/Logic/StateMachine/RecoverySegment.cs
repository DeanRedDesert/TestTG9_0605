//-----------------------------------------------------------------------
// <copyright file = "RecoverySegment.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.Collections.Generic;
    using Communication.CommunicationLib;
    using Communication.Logic.CommServices;
    using Foundation.ServiceProviders;
    using IGT.Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// The state machine segment for replaying presentation states during power hit recovery and history.
    /// </summary>
    public class RecoverySegment : StateMachineSegment
    {
        /// <summary>
        /// Identifies the function of the recovery segment.
        /// </summary>
        public enum RecoverySegmentFunction
        {
            /// <summary>Indicates that the <see cref="RecoverySegment"/> is performing history functions.</summary>
            History,

            /// <summary>Indicates that the <see cref="RecoverySegment"/> is performing power hit recovery functions.</summary>
            Recovery
        };

        #region Constants

        /// <summary>
        /// Name of state when performing power hit recovery functions.
        /// </summary>
        private const string RecoveryStateName = "RecoveryState";

        /// <summary>
        /// Name of state when performing history functions.
        /// </summary>
        private const string HistoryStateName = "History";

        /// <summary>
        /// Name of the history mode accessor.
        /// </summary>
        private const string HistoryModeAccessorName = "HistoryMode";

        /// <summary>
        /// Name of the recovery mode accessor.
        /// </summary>
        private const string RecoveryModeAccessorName = "RecoveryMode";

        /// <summary>
        /// Name of the display control state accessor.
        /// </summary>
        private const string DisplayControlStateAccessorName = "DisplayControlState";

        /// <summary>
        /// Name of the current history step accessor.
        /// </summary>
        private const string CurrentHistoryStepAccessorName = "CurrentHistoryStep";

        /// <summary>
        /// Name of the total history steps accessor.
        /// </summary>
        private const string TotalHistoryStepAccessorName = "TotalHistorySteps";

        /// <summary>
        /// The name of <see cref="DisplayControlStateProvider"/> stored in the service controller.
        /// </summary>
        private const string DisplayControlStateProviderName = "DisplayControlStateProvider";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The list of history step numbers and history step priorities, 
        /// where the Key is history step number and the Value is priority of that step.
        /// </summary>
        private List<KeyValuePair<int, uint>> recoveryHistoryList;

        /// <summary>
        /// Current History step to send to presentation.
        /// </summary>
        private int currentHistoryStepIndex;

        /// <summary>
        /// A flag indicating whether RecoverySegment is performing power hit recovery.
        /// </summary>
        public bool IsRecovering { get; private set; }

        /// <summary>
        /// The name of the presentation state that the RecoverySegment is currently executing.
        /// </summary>
        /// <remarks>
        /// This is only valid when executing the Committed stage of the logic state, in RecoveryStateCommitted().
        /// </remarks>
        public string PresentationStateName { get; private set; }

        #endregion Fields

        /// <summary>
        /// Creates an instance of the Recovery Segment.
        /// </summary>
        /// <param name="stateMachine">The base game state machine.</param>
        /// <param name="segmentFunction">
        /// Identifies the current function of this <see cref="RecoverySegment"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="segmentFunction"/> is invalid.</exception>
        public RecoverySegment(StateMachineBase stateMachine, RecoverySegmentFunction segmentFunction) : base(stateMachine)
        {
            switch(segmentFunction)
            {
                case RecoverySegmentFunction.History:
                    CreateState(HistoryStateName, StateMachineBase.EmptyStateStage, RecoveryStateCommitted, false);
                    InitialState = HistoryStateName;
                    break;
                case RecoverySegmentFunction.Recovery:
                    CreateState(RecoveryStateName, StateMachineBase.EmptyStateStage, RecoveryStateCommitted, false);
                    InitialState = RecoveryStateName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("segmentFunction");
            }

            currentHistoryStepIndex = 0;
        }

        /// <summary>
        /// Load Recovery Data and notify Presentation.
        /// </summary>
        /// <devDoc>
        /// The current assumption is that the presentation is in charge of the history menu for the current game.
        /// </devDoc>
        /// <param name="framework">Framework to use for sending data and reading critical data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="framework"/> is null.
        /// </exception>
        /// <exception cref="InvalidHistoryRecordException">
        /// Thrown when the History list is invalid.
        /// </exception>
        /// <exception cref="InvalidActionException">
        /// Thrown when unable to change history steps.
        /// </exception>
        private void RecoveryStateCommitted(IStateMachineFramework framework)
        {
            // Set IsRecovering to true in Play mode but false in History mode.
            IsRecovering = framework.GameLib.GameContextMode == GameMode.Play;

            if(framework == null)
            {
                throw new ArgumentNullException("framework");
            }

            // History list should not change while history state machine is in use so only read once.
            if(recoveryHistoryList == null)
            {
                recoveryHistoryList = StateMachineBase.ReadHistoryList(framework);
            }

            if(recoveryHistoryList == null || recoveryHistoryList.Count == 0)
            {
                throw new InvalidHistoryRecordException("Invalid History List.");
            }

            // Attention!!!
            // In the next line of code, recoveryHistoryList[0].Key equals to "1"
            // That is, when writing history, the stepNumber/recordNumber is 1-based.
            // However, when displaying history, currentHistoryStepIndex is 0-based, therefore,
            // from this point on, the data sent to presentation will use 0-based currentStep,
            // which is to be consumed by HistoryMenuController.

            // Read the history block from the safe storage.
            var historyBlock = GameStateMachine.ReadCommonHistoryBlock(framework, recoveryHistoryList[currentHistoryStepIndex].Key);
            PresentationStateName = historyBlock.StateName;

            // Amend history block with step info and async volatile service data.
            AmendHistoryBlock(framework, historyBlock, currentHistoryStepIndex, recoveryHistoryList.Count);

            // Start presentation display.
            StartRecoveryPresentation(framework, historyBlock);

            // Wait for presentation state complete message.
            var completeMessage = framework.GetPresentationEvent<GameLogicPresentationStateCompleteMsg>();

            // Process step navigation request.
            switch(completeMessage.ActionRequest)
            {
                case "NextStep":
                    {
                        if(currentHistoryStepIndex < recoveryHistoryList.Count - 1)
                        {
                            currentHistoryStepIndex++;
                        }
                        else
                        {
                            IsRecovering = false;
                            recoveryHistoryList = null;
                        }
                    }
                    break;
                case "FirstStep":
                    {
                        if(currentHistoryStepIndex > 0)
                        {
                            currentHistoryStepIndex = 0;
                        }
                    }
                    break;
                default:
                    throw new InvalidActionException("Cannot change History record based on current Action" +
                                                     completeMessage.ActionRequest);
            }

            // Exit presentation display.
            ExitRecoveryPresentation(framework, historyBlock);

            // Set next state to this state again.
            framework.SetNextState(framework.CurrentState);
        }

        /// <summary>
        /// Start the recovery and history presentation using the given history data block.
        /// </summary>
        /// <remarks>
        /// The base class just starts the presentation state specified in <paramref name="historyBlock"/>.
        /// 
        /// Derived classes should override this method if it needs to do extra work for history presentation,
        /// e.g. notify a bonus extension to display something for history.
        /// </remarks>
        /// <param name="framework">
        /// The framework used for communications, with both the Foundation and the presentation.
        /// </param>
        /// <param name="historyBlock">
        /// The data block used for presentation.
        /// </param>
        private void StartRecoveryPresentation(IStateMachineFramework framework, CommonHistoryBlock historyBlock)
        {
            framework.Presentation.StartState(historyBlock.StateName, historyBlock.Data);
        }

        /// <summary>
        /// Exit the history presentation, using the given history data block as needed.
        /// </summary>
        /// <remarks>
        /// The base class does nothing.
        /// 
        /// Derived classes should override this method if it needs to do extra work for history presentation,
        /// e.g. notify a bonus extension to change or hide the history display.
        /// </remarks>
        /// <param name="framework">
        /// The framework used for communications, with both the Foundation and the presentation.
        /// </param>
        /// <param name="historyBlock">
        /// The data block used for presentation.
        /// </param>
        private void ExitRecoveryPresentation(IStateMachineFramework framework, CommonHistoryBlock historyBlock)
        {
            // Base class does nothing.
        }

        /// <summary>
        /// Update the history block for history and recovery mode.
        /// </summary>
        /// <param name="framework">A state machine framework for negotiating data.</param>
        /// <param name="historyBlock">The <see cref="CommonHistoryBlock"/> to update.</param>
        /// <param name="stepIndex">The history step index of the history block.</param>
        /// <param name="totalHistorySteps">The total number of history steps in the game.</param>
        private void AmendHistoryBlock(IStateMachineFramework framework, CommonHistoryBlock historyBlock, int stepIndex, int totalHistorySteps)
        {
            // Set history mode to true so presentation can know if its in history.
            // This will allow for adding the game side menu and do any other special history cases.
            if(historyBlock.Data.HistoryData == null)
            {
                historyBlock.Data.HistoryData = new Dictionary<int, object>();
            }

            var historyModeAccessor = new ServiceAccessor(HistoryModeAccessorName);
            var recoveryModeAccessor = new ServiceAccessor(RecoveryModeAccessorName);
            var displayControlStateAccessor = new ServiceAccessor(DisplayControlStateAccessorName);
            var currentHistoryStepAccessor = new ServiceAccessor(CurrentHistoryStepAccessorName);
            var totalHistoryStepAccessor = new ServiceAccessor(TotalHistoryStepAccessorName);

            // Add step information.
            historyBlock.Data.HistoryData[currentHistoryStepAccessor.Identifier] = stepIndex;
            historyBlock.Data.HistoryData[totalHistoryStepAccessor.Identifier] = totalHistorySteps;
            historyBlock.Data.HistoryData[historyModeAccessor.Identifier] = true;
            historyBlock.Data.HistoryData[recoveryModeAccessor.Identifier] = IsRecovering;

            // TODO:This is a workaround of the display control state issue in history mode and subject to change.
            // In history mode, the display control state should always be normal.
            if(historyBlock.Data.ContainsKey(DisplayControlStateProviderName) &&
               historyBlock.Data[DisplayControlStateProviderName]
                   .ContainsKey(displayControlStateAccessor.Identifier))
            {
                historyBlock.Data[DisplayControlStateProviderName][displayControlStateAccessor.Identifier]
                    = DisplayControlState.DisplayAsNormal;
            }

            // Merge volatile async service data.
            var volatileData = GameStateMachine.GetHistoryStateData(framework, historyBlock.StateName);
            historyBlock.Data.Merge(volatileData);
        }

        /// <summary>
        /// Perform an asynchronous update of the history data for the service specified in the asyncEventArgs. 
        /// </summary>
        /// <param name="currentState">The name of the state which is currently being executed.</param>
        /// <param name="framework">Framework to use for performing the update.</param>
        /// <param name="asyncData">History data items, cached and retrieved by <see cref="StateMachineBase"/>.</param>
        public void UpdateAsyncData(string currentState, IStateMachineFramework framework, DataItems asyncData)
        {
            if(string.IsNullOrEmpty(currentState) || asyncData == null)
            {
                return;
            }

            if(asyncData.HistoryData == null)
            {
                asyncData.HistoryData = new Dictionary<int, object>();
            }

            var historyModeAccessor = new ServiceAccessor(HistoryModeAccessorName);
            var recoveryModeAccessor = new ServiceAccessor(RecoveryModeAccessorName);

            asyncData.HistoryData[historyModeAccessor.Identifier] = true;
            asyncData.HistoryData[recoveryModeAccessor.Identifier] = IsRecovering;
            framework.Presentation.UpdateAsynchData(currentState, asyncData);
        }
    }
}
