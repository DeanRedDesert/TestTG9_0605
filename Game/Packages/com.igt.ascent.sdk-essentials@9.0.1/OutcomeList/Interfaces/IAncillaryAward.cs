//-----------------------------------------------------------------------
// <copyright file = "IAncillaryAward.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.OutcomeList.Interfaces
{
    /// <summary>
    /// Type used to communicate ancillary/double up awards.
    /// </summary>
    /// <remarks>
    /// Only created by the game and may only be sent to the
    /// Foundation for evaluation during the ancillary play phase.
    /// </remarks>
    public interface IAncillaryAward : IAward
    {
        /// <summary>
        /// Gets the win type for this Ancillary Award.
        /// </summary>
        AncillaryAwardWinType WinType { get; }

        /// <summary>
        /// Gets the amount risked by the player.
        /// </summary>
        long RiskAmountValue { get; }
    }
}