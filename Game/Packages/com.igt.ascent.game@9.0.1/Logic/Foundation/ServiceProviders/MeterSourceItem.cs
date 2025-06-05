//-----------------------------------------------------------------------
// <copyright file = "MeterSourceItem.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;

    /// <summary>
    /// This enumeration is used to indicate the type of meter source is updated.
    /// </summary>
    [Serializable]
    public enum MeterSourceItem
    {
        /// <summary>
        /// Credit Meter.
        /// </summary>
        CreditMeter,

        /// <summary>
        /// Banked Credits Support - Wagerable Meter.
        /// </summary>
        WagerableMeter,

        /// <summary>
        /// Paid Meter.
        /// </summary>
        PaidMeter,

        /// <summary>
        /// Win is pending or not.
        /// </summary>
        IsWinPending,
    }
}
