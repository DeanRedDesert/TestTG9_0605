//-----------------------------------------------------------------------
// <copyright file = "IConsolidateAsynchronousServiceUpdate.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    /// <summary>
    /// This interface is used to consolidate multiple asynchronous service data update
    /// notifications during a event processing cycle into one GL2P message.
    /// </summary>
    public interface IConsolidateAsynchronousServiceUpdate
    {
        /// <summary>
        /// Sends the cached asynchronous services data to the presentation.
        /// </summary>
        void UpdateAsynchronousData(IStateMachineFramework framework);

        /// <summary>
        /// Clears the cached asynchronous services data.
        /// </summary>
        void ClearAsynchronousData();
    }
}
