// -----------------------------------------------------------------------
// <copyright file = "CreditDisplayMode.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Money
{
    /// <summary>
    /// Represents modes for formatting to credits.
    /// </summary>
    public enum CreditDisplayMode
    {
        /// <summary>
        /// Displays credits with partial values.
        /// Example: 1.3 credits is shown as "1.3"
        /// </summary>
        Credit,

        /// <summary>
        /// Truncates partial credits.
        /// Example: 1.3 credits is shown as "1"
        /// </summary>
        CreditNoPartial
    }
}