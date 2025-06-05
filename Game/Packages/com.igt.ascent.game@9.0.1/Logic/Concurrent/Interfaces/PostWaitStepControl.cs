// -----------------------------------------------------------------------
// <copyright file = "PostWaitStepControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    /// <summary>
    /// The flow control for the <see cref="StateStep.CommittedPostWait"/> step in a logic state.
    /// </summary>
    public enum PostWaitStepControl
    {
        /// <summary>
        /// Exit current state.
        /// </summary>
        ExitState,

        /// <summary>
        /// Go back to the <see cref="StateStep.CommittedPreWait"/> step.
        /// </summary>
        BackToPreWait,

        /// <summary>
        /// Go back to the <see cref="StateStep.CommittedWait"/> step.
        /// </summary>
        BackToWait,
    }
}