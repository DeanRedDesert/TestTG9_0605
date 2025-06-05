// -----------------------------------------------------------------------
// <copyright file = "CurrencySymbolPosition.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    /// <summary>
    /// Enumeration that indicates if the currency symbol would be to the right or left of the numeric value.
    /// </summary>
    public enum CurrencySymbolPosition
    {
        /// <summary>
        /// Place the currency symbol to the left of the numeric value without a space between the symbol and
        /// numeric amount.
        /// Example: "€127.54"
        /// </summary>
        Left = 0,

        /// <summary>
        /// Place the currency symbol to the right of the numeric value without a space between the symbol and
        /// numeric amount.
        /// Example: "127,54F"
        /// </summary>
        Right = 1,

        /// <summary>
        /// Place the currency symbol to the left of the numeric value with a space between the symbol and
        /// numeric amount.
        /// Example: "€ 127,54"
        /// </summary>
        LeftWithSpace = 2,

        /// <summary>
        /// Place the currency symbol to the right of the numeric value with a space between the symbol and
        /// numeric amount.
        /// Example: "127,54 F"
        /// </summary>
        RightWithSpace = 3,
    }
}