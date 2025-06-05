// -----------------------------------------------------------------------
// <copyright file = "StateStep.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    /// <summary>
    /// The steps of a logic state.
    /// </summary>
    public enum StateStep
    {
        /// <summary>
        /// Processing stage
        /// </summary>
        Processing,

        /// <summary>
        /// Committed stage, before waiting for a condition to occur
        /// </summary>
        CommittedPreWait,

        /// <summary>
        /// Committed state, waiting for a condition to occur
        /// </summary>
        CommittedWait,

        /// <summary>
        /// Committed state, after a condition has occurred
        /// </summary>
        CommittedPostWait,
    }
}