// -----------------------------------------------------------------------
// <copyright file = "CashDisplayMode.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    /// <summary>
    /// Represents modes for formatting to cash in dealing with whole (ex. dollars) and base (ex. cents) values.
    /// </summary>
    public enum CashDisplayMode
    {
        /// <summary>
        /// Base value is not shown if it is zero.
        /// Examples: $1, $1.23, $0.23
        /// </summary>
        Whole,

        /// <summary>
        /// Whole and base values are always shown even when either is zero.
        /// Examples: $1.00, $1.23, $0.23
        /// </summary>
        WholePlusBase,

        /// <summary>
        /// Base value is not shown if it is zero. If the whole value is zero,
        /// it is not shown and the cent symbol is used.
        /// Examples: $1, $1.23, 23¢
        /// </summary>
        Base
    };
}