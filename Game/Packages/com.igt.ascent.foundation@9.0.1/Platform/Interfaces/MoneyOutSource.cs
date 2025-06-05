//-----------------------------------------------------------------------
// <copyright file = "MoneyOutSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the type of source through which the money leaves the EGM.
    /// </summary>
    [Serializable]
    public enum MoneyOutSource
    {
        /// <summary>
        /// Indicates the money out source is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Indicates the money has left the EGM through the other source.
        /// </summary>
        OtherSource,

        /// <summary>
        /// Indicates the money has left the EGM through the bill acceptor.
        /// </summary>
        Bill,

        /// <summary>
        /// Indicates the money has left the EGM through the hopper.
        /// </summary>
        Coin,

        /// <summary>
        /// Indicates the money has left the EGM through the voucher.
        /// </summary>
        Ticket,

        /// <summary>
        /// Indicates the money has left the EGM through the host or EFT.
        /// </summary>
        FundsTransfer,

        /// <summary>
        /// Indicates the money has left the EGM through the handpay.
        /// </summary>
        Handpay
    }
}
