// -----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardPayType.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    /// <summary>
    /// Defines the possible progressive award pay types.
    /// </summary>
    public enum ProgressiveAwardPayType
    {
        /// <summary>
        /// Transfer the progressive amount to the credit meter.
        /// </summary>
        CreditMeter,

        /// <summary>
        /// Transfer the progressive amount to the win meter.
        /// </summary>
        WinMeter,

        /// <summary>
        /// Call an attendant to award the progressive.
        /// </summary>
        Attendant,

        /// <summary>
        /// Transfer the progressive amount to Cashless.
        /// </summary>
        Cashless,
    }
}
