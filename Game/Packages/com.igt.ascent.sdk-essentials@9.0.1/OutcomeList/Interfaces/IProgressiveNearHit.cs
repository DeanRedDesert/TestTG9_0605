//-----------------------------------------------------------------------
// <copyright file = "IProgressiveNearHit.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Used to communicate progressive near-hit outcomes.
    /// </summary>
    /// <remarks>
    /// Typically, this is done when the correct symbols/conditions appear 
    /// but the bet does not qualify for the jackpot.
    /// </remarks>
    public interface IProgressiveNearHit : IOutcome
    {
        /// <summary>
        /// Gets the numeric progressive level declared in the payvar registry.
        /// </summary>
        uint? GameLevel { get; }
    }
}