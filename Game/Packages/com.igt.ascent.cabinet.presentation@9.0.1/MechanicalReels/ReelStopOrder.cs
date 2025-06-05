//-----------------------------------------------------------------------
// <copyright file = "ReelStopOrder.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    /// <summary>
    /// Available stop orders.
    /// </summary>
    public enum ReelStopOrder
    {
        /// <summary>
        /// Clears any set stop order. Reels may still stop in asc. or desc. order, due to other
        /// spin attributes, namely spin duration.
        /// </summary>
        Off,

        /// <summary>
        /// Stop from lowest reel index to highest.
        /// </summary>
        Ascending,

        /// <summary>
        /// Stop from highest reel index to lowest.
        /// </summary>
        Descending
    }
}
