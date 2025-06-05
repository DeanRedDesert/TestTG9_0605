// -----------------------------------------------------------------------
// <copyright file = "NegativeNumberFormat.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    /// <summary>
    /// Enumeration that indicates how to format negative numbers.
    /// </summary>
    public enum NegativeNumberFormat
    {
        /// <summary>
        /// A negative sign is shown as the first symbol, whether currency symbol leads or follows.
        /// Examples: "-€127.54", "-127,54 F"
        /// </summary>
        IsFirstSymbol = 0,

        /// <summary>
        /// A negative sign is shown before numerics.
        /// Examples: "€-127.54", "kr-127,54"
        /// </summary>
        BeforeNumerics = 1,

        /// <summary>
        /// A negative sign is shown after numerics.
        /// Example: "€ 127,54-"
        /// </summary>
        AfterNumerics = 2,

        /// <summary>
        /// Negative numbers are represented by being contained in parentheses.
        /// Example: "($127.54)"
        /// </summary>
        InParentheses = 3
    }
}