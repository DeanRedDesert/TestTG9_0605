// -----------------------------------------------------------------------
// <copyright file = "CreditMeterDisplayBehaviorMode.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// Type for the behavior the credit meter should exhibit, regarding displaying credits or currency.
    /// </summary>
    public enum CreditMeterDisplayBehaviorMode
    {
        /// <summary>
        /// Invalid credit meter display.
        /// </summary>
        Invalid,

        /// <summary>
        /// Display in credits by default, but allow the player to change it to currency.
        /// </summary>
        PlayerSelectableDefaultCredits,

        /// <summary>
        /// Display in currency by default, but allow the player to change it to credits.
        /// </summary>
        PlayerSelectableDefaultCurrency,

        /// <summary>
        /// Always display currency.
        /// </summary>
        AlwaysCurrency
    }
}