// -----------------------------------------------------------------------
// <copyright file = "PreWaitStepControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    /// <summary>
    /// The flow control for the <see cref="StateStep.CommittedPreWait"/> step in a logic state.
    /// </summary>
    public enum PreWaitStepControl
    {
        /// <summary>
        /// Go to the <see cref="StateStep.CommittedWait"/> step.
        /// </summary>
        GoNext,

        /// <summary>
        /// Exit current state.
        /// </summary>
        ExitState,

        /// <summary>
        /// Repeat current <see cref="StateStep.CommittedPreWait"/> step.
        /// </summary>
        RepeatPreWait,
    }
}