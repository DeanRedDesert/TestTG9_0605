// -----------------------------------------------------------------------
// <copyright file = "MaxBetButtonBehavior.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// The behavior of the max bet button in regards to what bets it will allow to be committed.
    /// </summary>
    public enum MaxBetButtonBehavior
    {
        /// <summary>
        /// If the max bet button will allow the max bet to be committed. 
        /// </summary>
        BetMaxCreditsOnly,

        /// <summary>
        /// If the max bet button will allow the last remaining credits to be committed.
        /// </summary>
        BetAvailableCredits
    }
}