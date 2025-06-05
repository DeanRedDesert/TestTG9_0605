// -----------------------------------------------------------------------
// <copyright file = "WaitStepControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    /// <summary>
    /// The flow control for the <see cref="StateStep.CommittedWait"/> step in a logic state.
    /// </summary>
    public enum WaitStepControl
    {
        /// <summary>
        /// Go to the <see cref="StateStep.CommittedPostWait"/> step.
        /// </summary>
        GoNext,

        /// <summary>
        /// Exit current state.
        /// </summary>
        ExitState,

        /// <summary>
        /// Go back to the <see cref="StateStep.CommittedPreWait"/> step.
        /// </summary>
        BackToPreWait,

        /// <summary>
        /// Repeat current <see cref="StateStep.CommittedWait"/> step.
        /// Use this value to notify the state machine framework to resume waiting if
        /// the waiting has been interrupted by a higher priority event.
        /// </summary>
        RepeatWait,
    }
}