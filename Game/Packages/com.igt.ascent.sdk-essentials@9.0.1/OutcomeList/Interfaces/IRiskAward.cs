//-----------------------------------------------------------------------
// <copyright file = "IRiskAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Type used to communicate generic risk play awards,
    /// such as the outcome of a Round Wager Up Playoff feature.
    /// </summary>
    public interface IRiskAward : IAward
    {
        /// <summary>
        /// Gets the amount risked by the player to start the risk play.
        /// </summary>
        long RiskAmountValue { get; }

        /// <summary>
        /// The type of risk play that was awarded.
        /// </summary>
        RiskAwardType AwardType { get; }
    }
}