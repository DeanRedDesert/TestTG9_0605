//-----------------------------------------------------------------------
// <copyright file = "BettableTransferDirection.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the direction of money being transferred to or from the player bettable meter.
    /// </summary>
    [Serializable]
    public enum BettableTransferDirection
    {
        /// <summary>
        /// Indicates the direction of money transfer is uninitialized or invalid.
        /// </summary>
        Invalid,

        /// <summary>
        /// Indicates the money has been transfered from player bettable to an unknown/unspecified target.
        /// </summary>
        ToUnknownSource,

        /// <summary>
        /// Indicates the money has been transfered from an unknown source to player bettable.
        /// </summary>
        FromUnknownSource,

        /// <summary>
        /// Indicates the money has been transfered from player transferable to player bettable.
        /// </summary>
        FromTransferable,

        /// <summary>
        /// Indicates the money has been transfered from player bettable to player transferable.
        /// </summary>
        ToTransferable,

        /// <summary>
        /// Indicates the money has been transfered from escrow to player bettable.
        /// </summary>
        FromCashEscrow
    }
}
