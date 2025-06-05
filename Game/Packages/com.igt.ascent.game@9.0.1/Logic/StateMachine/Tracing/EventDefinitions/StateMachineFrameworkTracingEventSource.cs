// -----------------------------------------------------------------------
// <copyright file = "StateMachineFrameworkTracingEventSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.Tracing.EventDefinitions
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Tracing;
    using Core.Tracing.EventDefinitions;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [EventSource(Name = "IGT-Ascent-Core-Logic-StateMachine-Tracing-StateMachineFrameworkTracingEventSource")]
    internal sealed partial class StateMachineFrameworkTracingEventSource : EventSourceBase
    {
        #region Constants

        private const EventLevel DefaultLevel = EventLevel.Informational;

        #endregion

        #region Singleton

        /// <summary>
        /// Private singleton instance.
        /// </summary>
        private static readonly StateMachineFrameworkTracingEventSource Instance = new StateMachineFrameworkTracingEventSource();

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static StateMachineFrameworkTracingEventSource Log
        {
            get { return Instance; }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private StateMachineFrameworkTracingEventSource()
        {
        }

        #endregion

        #region Event Definitions

        [Event(1, Level = DefaultLevel)]
        public void FrameworkConstructionStart()
        {
            WriteEvent(1);
        }

        [Event(2, Level = DefaultLevel)]
        public void FrameworkConstructionStop()
        {
            WriteEvent(2);
        }

        [Event(3, Level = DefaultLevel)]
        public void StateMachineInitializationStart()
        {
            WriteEvent(3);
        }

        [Event(4, Level = DefaultLevel)]
        public void StateMachineInitializationStop()
        {
            WriteEvent(4);
        }

        [Event(5, Level = DefaultLevel)]
        public void FrameworkExecutionStart()
        {
            WriteEvent(5);
        }

        [Event(6, Level = DefaultLevel)]
        public void FrameworkExecutionStop()
        {
            WriteEvent(6);
        }

        [Event(7, Level = DefaultLevel)]
        public void ReadStateConfigurationStart()
        {
            WriteEvent(7);
        }

        [Event(8, Level = DefaultLevel)]
        public void ReadStateConfigurationStop()
        {
            WriteEvent(8);
        }

        [Event(9, Level = DefaultLevel)]
        public void StateProcessingStageStart(string StateName)
        {
            WriteEvent(9, StateName);
        }

        [Event(10, Level = DefaultLevel)]
        public void StateProcessingStageStop(string StateName)
        {
            WriteEvent(10, StateName);
        }

        [Event(11, Level = DefaultLevel)]
        public void StateCommittedStageStart(string StateName)
        {
            WriteEvent(11, StateName);
        }

        [Event(12, Level = DefaultLevel)]
        public void StateCommittedStageStop(string StateName)
        {
            WriteEvent(12, StateName);
        }

        [Event(13, Level = DefaultLevel)]
        public void ExecuteQueuedTransactionStart(string StateName, int RemainingCount)
        {
            WriteEvent(13, StateName, RemainingCount);
        }

        [Event(14, Level = DefaultLevel)]
        public void ExecuteQueuedTransactionStop(string StateName, int RemainingCount)
        {
            WriteEvent(14, StateName, RemainingCount);
        }

        [Event(15, Level = DefaultLevel)]
        public void StateHandlerProcessingStageStart(string StateName)
        {
            WriteEvent(15, StateName);
        }
        
        [Event(16, Level = DefaultLevel)]
        public void StateHandlerProcessingStageStop(string StateName)
        {
            WriteEvent(16, StateName);
        }
        
        [Event(17, Level = DefaultLevel)]
        public void StateHandlerCommittedStageStart(string StateName)
        {
            WriteEvent(17, StateName);
        }
        
        [Event(18, Level = DefaultLevel)]
        public void StateHandlerCommittedStageStop(string StateName)
        {
            WriteEvent(18, StateName);
        }

        [Event(19, Level = DefaultLevel)]
        public void WriteCachedHistoryStart(string StateName)
        {
            WriteEvent(19, StateName);
        }

        [Event(20, Level = DefaultLevel)]
        public void WriteCachedHistoryStop(string StateName)
        {
            WriteEvent(20, StateName);
        }
        
        [Event(21, Level = DefaultLevel)]
        public void WriteCachedHistoryCreateCommonHistoryBlockStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(21, StateName, HistoryStepNumber);
        }
        
        [Event(22, Level = DefaultLevel)]
        public void WriteCachedHistoryCreateCommonHistoryBlockStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(22, StateName, HistoryStepNumber);
        }
        
        [Event(23, Level = DefaultLevel)]
        public void WriteCachedHistoryMergeSerializedStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(23, StateName, HistoryStepNumber);
        }
        
        [Event(24, Level = DefaultLevel)]
        public void WriteCachedHistoryMergeSerializedStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(24, StateName, HistoryStepNumber);
        }
        
        [Event(25, Level = DefaultLevel)]
        public void WriteCachedHistoryWriteCriticalDataAsyncStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(25, StateName, HistoryStepNumber);
        }
        
        [Event(26, Level = DefaultLevel)]
        public void WriteCachedHistoryWriteCriticalDataAsyncStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(26, StateName, HistoryStepNumber);
        }
        
        [Event(27, Level = DefaultLevel)]
        public void WriteCachedHistoryWriteCriticalDataNoAsyncStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(27, StateName, HistoryStepNumber);
        }
        
        [Event(28, Level = DefaultLevel)]
        public void WriteCachedHistoryWriteCriticalDataNoAsyncStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(28, StateName, HistoryStepNumber);
        }
        
        [Event(29, Level = DefaultLevel)]
        public void WriteCachedHistoryUpdateHistoryListStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(29, StateName, HistoryStepNumber);
        }
        
        [Event(30, Level = DefaultLevel)]
        public void WriteCachedHistoryUpdateHistoryListStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(30, StateName, HistoryStepNumber);
        }
        
        [Event(31, Level = DefaultLevel)]
        public void UpdateHistoryListReadHistoryListStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(31, StateName, HistoryStepNumber);
        }
        
        [Event(32, Level = DefaultLevel)]
        public void UpdateHistoryListReadHistoryListStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(32, StateName, HistoryStepNumber);
        }

        [Event(33, Level = DefaultLevel)]
        public void UpdateHistoryListProcessHistoryListStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(33, StateName, HistoryStepNumber);
        }
        
        [Event(34, Level = DefaultLevel)]
        public void UpdateHistoryListProcessHistoryListStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(34, StateName, HistoryStepNumber);
        }
        
        [Event(35, Level = DefaultLevel)]
        public void UpdateHistoryListWriteHistoryListStart(string StateName, int HistoryStepNumber)
        {
            WriteEvent(35, StateName, HistoryStepNumber);
        }
        
        [Event(36, Level = DefaultLevel)]
        public void UpdateHistoryListWriteHistoryListStop(string StateName, int HistoryStepNumber)
        {
            WriteEvent(36, StateName, HistoryStepNumber);
        }

        #endregion
    }
}