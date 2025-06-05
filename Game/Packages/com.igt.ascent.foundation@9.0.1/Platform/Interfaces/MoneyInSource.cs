//-----------------------------------------------------------------------
// <copyright file = "MoneyInSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the type of source from which the money is received.
    /// </summary>
    [Serializable]
    public enum MoneyInSource
    {
        /// <summary>
        /// Indicates the money in source is invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Indicates the money has been received from the other source.
        /// </summary>
        OtherSource,

        /// <summary>
        /// Indicates the money has been received from the bill acceptor.
        /// </summary>
        Bill,

        /// <summary>
        /// Indicates the money has been received from the coin acceptor.
        /// </summary>
        Coin,

        /// <summary>
        /// Indicates the money has been received from the voucher.
        /// </summary>
        Ticket,

        /// <summary>
        /// Indicates the money has been received from the host or EFT.
        /// </summary>
        FundsTransfer
    }
}
