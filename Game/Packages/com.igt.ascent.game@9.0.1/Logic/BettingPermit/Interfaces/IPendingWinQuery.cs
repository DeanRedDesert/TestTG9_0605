// -----------------------------------------------------------------------
// <copyright file = "IPendingWinQuery.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.BettingPermit.Interfaces
{
    /// <summary>
    /// This interface defines APIs to query the pending wins of current game cycle.
    /// </summary>
    public interface IPendingWinQuery
    {
        /// <summary>
        /// Gets the pending win of current game cycle, in base units.
        /// </summary>
        /// <returns>
        /// The pending win of current game cycle, in base units.
        /// </returns>
        long GetPendingWin();
    }
}