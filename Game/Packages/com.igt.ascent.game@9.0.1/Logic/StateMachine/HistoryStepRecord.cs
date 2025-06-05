// -----------------------------------------------------------------------
// <copyright file = "HistoryStepRecord.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Logic.StateMachine
{
    /// <summary>
    /// This class holds the history data of a history step.
    /// </summary>
    class HistoryStepRecord
    {
        /// <summary>
        /// The priority of the state that determines whether this state should be deleted 
        /// when there are more than "MaxHistorySteps" steps in history list.
        /// A lower unsigned integer value means a lower priority history step.
        /// </summary>
        public uint Priority;

        /// <summary>
        /// Start state history data which may be overwritten by customized write/update history 
        /// handler if any.
        /// </summary>
        public byte[] StartStateData;

        /// <summary>
        /// Cache of serialized asynchronous update data which should be merged to <see cref="StartStateData"/> 
        /// and written to critical data by the last transaction of the presentation state. 
        /// </summary>
        public StateMachineBase.SerializedDataItems AsynchronousData;
    }
}