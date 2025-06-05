//-----------------------------------------------------------------------
// <copyright file = "IVoucherPrintCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Interface to accept callbacks from the voucher print category.
    /// </summary>
    public interface IVoucherPrintCategoryCallbacks
    {
        /// <summary>
        /// Method called when the foundation gives a voucher print notification.
        /// </summary>
        /// <param name="printEvent">The voucher print event type.</param>
        /// <param name="amount">The amount value in base units.</param>
        void ProcessVoucherPrintEvent(VoucherPrintEvent printEvent, long amount);
    }
}
