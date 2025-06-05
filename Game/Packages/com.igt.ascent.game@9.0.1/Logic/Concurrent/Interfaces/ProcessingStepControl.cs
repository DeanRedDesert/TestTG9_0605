// -----------------------------------------------------------------------
// <copyright file = "ProcessingStepControl.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    /// <summary>
    /// The flow control for the <see cref="StateStep.Processing"/> step in a logic state.
    /// </summary>
    public enum ProcessingStepControl
    {
        /// <summary>
        /// Go to the <see cref="StateStep.CommittedPreWait"/> step.
        /// </summary>
        GoNext,

        /// <summary>
        /// Exit current state.
        /// </summary>
        ExitState,

        /// <summary>
        /// Skip <see cref="StateStep.CommittedPreWait"/> and go to the <see cref="StateStep.CommittedWait"/> step.
        /// </summary>
        SkipToWait,
    }
}