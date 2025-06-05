// -----------------------------------------------------------------------
// <copyright file = "BaseIdlePresentationAction.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// Presentation actions supported by <see cref="BaseIdleState"/>.
    /// </summary>
    public enum BaseIdlePresentationAction
    {
        /// <summary>
        /// Commits a bet amount and then start the game cycle.
        /// </summary>
        CommitStart,
    }
}