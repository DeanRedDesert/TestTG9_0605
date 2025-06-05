// -----------------------------------------------------------------------
// <copyright file = "CreditSeparatorMode.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    /// <summary>
    /// Modes for formatting credits with a digit group separator.
    /// </summary>
    public enum CreditSeparatorMode
    {
        /// <summary>
        /// Pick the mode automatically based on <see cref="CreditFormatter.UseCreditSeparator"/>.
        /// <see cref="SeparateAsCash"/> if <see cref="CreditFormatter.UseCreditSeparator"/> is true,
        /// <see cref="NoSeparator"/> if it is false.
        /// </summary>
        Auto,

        /// <summary>
        /// Do not include a separator.
        /// This mode ignores the value of <see cref="CreditFormatter.UseCreditSeparator"/>.
        /// </summary>
        NoSeparator,

        /// <summary>
        /// Separate the same as cash.
        /// This mode ignores the value of <see cref="CreditFormatter.UseCreditSeparator"/>.
        /// </summary>
        SeparateAsCash,

        /// <summary>
        /// Only separate if the number is greater than four digits.
        /// The Separator will sill appear after the third digit (10,000 1000).
        /// This mode ignores the value of <see cref="CreditFormatter.UseCreditSeparator"/>.
        /// </summary>
        SeparateAfterFourDigits
    }
}