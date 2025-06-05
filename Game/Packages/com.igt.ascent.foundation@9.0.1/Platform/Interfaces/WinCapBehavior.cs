// -----------------------------------------------------------------------
// <copyright file = "WinCapBehavior.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The behavior to use when calculating the win cap.
    /// </summary>
    [Serializable]
    public enum WinCapBehavior
    {
        /// <summary>
        /// Indicates the win cap limit is a fixed amount.
        /// </summary>
        FixedWinCapAmount,

        /// <summary>
        /// Indicates the win cap limit is an amount 
        /// that is calculated by multiplying a value by the current bet 
        /// (e.g. win cap limit = multiplier * current bet).
        /// </summary>
        MultipliedByCurrentBet,

        /// <summary>
        /// Indicates the win cap limit is an amount 
        /// that is calculated by multiplying a value by the max bet 
        /// (e.g. win cap limit = multiplier * max bet).
        /// </summary>
        MultipliedByMaxBet
    }
}