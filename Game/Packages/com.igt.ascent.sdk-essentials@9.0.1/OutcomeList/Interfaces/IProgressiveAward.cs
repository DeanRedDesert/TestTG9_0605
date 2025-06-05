//-----------------------------------------------------------------------
// <copyright file = "IProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Contains progressive win level data.  Used for progressive hits and validation.
    /// </summary>
    public interface IProgressiveAward : IAward
    {
        /// <summary>
        /// Gets the hit state of this progressive award.
        /// </summary>
        ProgressiveAwardHitState HitState { get; }

        /// <summary>
        /// Gets the progressive level declared in payvar registry.
        /// </summary>
        uint? GameLevel { get; }
    }
}