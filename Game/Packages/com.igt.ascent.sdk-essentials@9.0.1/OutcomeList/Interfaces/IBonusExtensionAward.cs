//-----------------------------------------------------------------------
// <copyright file = "IBonusExtensionAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Bonus class extension awards declared during the game-cycle extended bonus states.
    /// </summary>
    /// <remarks>
    /// Used for reporting awards of a bonus extension, such as a Jackpot Bonus.
    /// Can only be declared during game cycle states related to Bonus play,
    /// such as BonusPlaying and BonusEvaluatePending.
    /// </remarks>
    public interface IBonusExtensionAward : IAward
    {
        /// <summary>
        /// Gets the bonus identifier provided by the foundation, if it exists.
        /// </summary>
        long? BonusIdentifier { get; }
    }
}