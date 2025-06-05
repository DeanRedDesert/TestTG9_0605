// -----------------------------------------------------------------------
// <copyright file = "StateMachineFrameworkTracing.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.Tracing
{
    using System.Diagnostics;
    using EventDefinitions;

    /// <summary>
    /// This class provides APIs for tracing game logic events occurred at the State Machine Framework level.
    /// </summary>
    public sealed class StateMachineFrameworkTracing
    {
        #region Singleton Implementation

        /// <summary>
        /// Private singleton instance.
        /// </summary>
        private static readonly StateMachineFrameworkTracing Instance = new StateMachineFrameworkTracing();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static StateMachineFrameworkTracing Log
        {
            get { return Instance; }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private StateMachineFrameworkTracing()
        {
        }

        #endregion

        #region Tracing Methods

        /// <summary>
        /// Tracing event indicating the start of the state machine framework construction.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void FrameworkConstructionStart()
        {
            StateMachineFrameworkTracingEventSource.Log.FrameworkConstructionStart();
        }

        /// <summary>
        /// Tracing event indicating the stop of the state machine framework construction.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void FrameworkConstructionStop()
        {
            StateMachineFrameworkTracingEventSource.Log.FrameworkConstructionStop();
        }

        /// <summary>
        /// Tracing event indicating the start of the state machine initialization.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateMachineInitializationStart()
        {
            StateMachineFrameworkTracingEventSource.Log.StateMachineInitializationStart();
        }

        /// <summary>
        /// Tracing event indicating the stop of the state machine initialization.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateMachineInitializationStop()
        {
            StateMachineFrameworkTracingEventSource.Log.StateMachineInitializationStop();
        }

        /// <summary>
        /// Tracing event indicating the start of the state machine framework execution.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void FrameworkExecutionStart()
        {
            StateMachineFrameworkTracingEventSource.Log.FrameworkExecutionStart();
        }

        /// <summary>
        /// Tracing event indicating the stop of the state machine framework execution.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void FrameworkExecutionStop()
        {
            StateMachineFrameworkTracingEventSource.Log.FrameworkExecutionStop();
        }

        /// <summary>
        /// Tracing event indicating the start of reading state configuration.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ReadStateConfigurationStart()
        {
            StateMachineFrameworkTracingEventSource.Log.ReadStateConfigurationStart();
        }

        /// <summary>
        /// Tracing event indicating the stop of reading state configuration.
        /// </summary>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ReadStateConfigurationStop()
        {
            StateMachineFrameworkTracingEventSource.Log.ReadStateConfigurationStop();
        }

        /// <summary>
        /// Tracing event indicating the start of a state's processing stage.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateProcessingStageStart(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateProcessingStageStart(stateName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a state's processing stage.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateProcessingStageStop(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateProcessingStageStop(stateName);
        }

        /// <summary>
        /// Tracing event indicating the start of a state's committed stage.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateCommittedStageStart(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateCommittedStageStart(stateName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a state's committed stage.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateCommittedStageStop(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateCommittedStageStop(stateName);
        }

        /// <summary>
        /// Tracing event indicating the start of executing a queued transaction.
        /// </summary>
        /// <param name="stateName">The name of current state.</param>
        /// <param name="remainingCount">Number of remaining queued transactions before this call.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ExecuteQueuedTransactionStart(string stateName, int remainingCount)
        {
            StateMachineFrameworkTracingEventSource.Log.ExecuteQueuedTransactionStart(stateName, remainingCount);
        }

        /// <summary>
        /// Tracing event indicating the stop of executing a queued transaction.
        /// </summary>
        /// <param name="stateName">The name of current state.</param>
        /// <param name="remainingCount">Number of remaining queued transactions after this call.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void ExecuteQueuedTransactionStop(string stateName, int remainingCount)
        {
            StateMachineFrameworkTracingEventSource.Log.ExecuteQueuedTransactionStop(stateName, remainingCount);
        }

        /// <summary>
        /// Tracing event indicating the start of a state handler's processing stage execution.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateHandlerProcessingStageStart(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateHandlerProcessingStageStart(stateName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a state handler's processing stage execution.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateHandlerProcessingStageStop(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateHandlerProcessingStageStop(stateName);
        }

        /// <summary>
        /// Tracing event indicating the start of a state handler's committed stage execution.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateHandlerCommittedStageStart(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateHandlerCommittedStageStart(stateName);
        }

        /// <summary>
        /// Tracing event indicating the stop of a state handler's committed stage execution.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void StateHandlerCommittedStageStop(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.StateHandlerCommittedStageStop(stateName);
        }

        /// <summary>
        /// Tracing event indicating the start of writing cached history.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryStart(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryStart(stateName);
        }

        /// <summary>
        /// Tracing event indicating the stop of writing cached history.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryStop(string stateName)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryStop(stateName);
        }

        /// <summary>
        /// Tracing event indicating the start of creating the common history block.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryCreateCommonHistoryBlockStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryCreateCommonHistoryBlockStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of creating the common history block.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryCreateCommonHistoryBlockStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryCreateCommonHistoryBlockStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of merging the serialized data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryMergeSerializedStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryMergeSerializedStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of merging the serialized data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryMergeSerializedStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryMergeSerializedStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of writing critical data with async data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryWriteCriticalDataAsyncStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryWriteCriticalDataAsyncStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of writing critical data with async data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryWriteCriticalDataAsyncStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryWriteCriticalDataAsyncStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of writing critical data without async data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryWriteCriticalDataNoAsyncStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryWriteCriticalDataNoAsyncStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of writing critical data without async data.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryWriteCriticalDataNoAsyncStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryWriteCriticalDataNoAsyncStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of updating the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryUpdateHistoryListStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryUpdateHistoryListStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of updating the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void WriteCachedHistoryUpdateHistoryListStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.WriteCachedHistoryUpdateHistoryListStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of reading the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListReadHistoryListStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListReadHistoryListStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of reading the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListReadHistoryListStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListReadHistoryListStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of processing the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListProcessHistoryListStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListProcessHistoryListStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of processing the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListProcessHistoryListStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListProcessHistoryListStop(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the start of writing the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListWriteHistoryListStart(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListWriteHistoryListStart(stateName, historyStepNumber);
        }

        /// <summary>
        /// Tracing event indicating the stop of writing the history list.
        /// </summary>
        /// <param name="stateName">The state name.</param>
        /// <param name="historyStepNumber">The history step number.</param>
        [Conditional("TRACING_FOR_RELEASE_TEST")]
        public void UpdateHistoryListWriteHistoryListStop(string stateName, int historyStepNumber)
        {
            StateMachineFrameworkTracingEventSource.Log.UpdateHistoryListWriteHistoryListStop(stateName, historyStepNumber);
        }

        #endregion
    }
}