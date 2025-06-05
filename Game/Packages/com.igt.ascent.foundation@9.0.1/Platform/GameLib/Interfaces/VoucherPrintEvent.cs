// -----------------------------------------------------------------------
// <copyright file = "VoucherPrintEvent.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    /// <summary>
    /// Enumeration indicating the type of a voucher printing notification.
    /// </summary>
    public enum VoucherPrintEvent
    {
        /// <summary>
        /// Voucher printing has been initiated.
        /// </summary>
        VoucherPrintingInitiated,

        /// <summary>
        /// Voucher printing is complete.
        /// </summary>
        VoucherPrintingComplete,

        /// <summary>
        /// Voucher is waiting to be removed from the printer.
        /// </summary>
        VoucherAwaitingRemoval,

        /// <summary>
        /// Voucher has been removed from the printer.
        /// </summary>
        VoucherRemoved,
    }
}