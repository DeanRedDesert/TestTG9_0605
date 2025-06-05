//-----------------------------------------------------------------------
// <copyright file = "FundsTransferAccountSource.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the type of account from which the funds are transferred.
    /// Used in association with <see cref="Platform.Interfaces.MoneyInSource.FundsTransfer"/>.
    /// </summary>
    [Serializable]
    public enum FundsTransferAccountSource
    {
        /// <summary>
        /// Indicates the account source is other than the mentioned options.
        /// </summary>
        OtherAccount,

        /// <summary>
        /// Indicates the account source is from a Wagering Account Transfer (WAT) account.
        /// </summary>
        CasinoAccount,

        /// <summary>
        /// Indicates the account source is from a financial institution.
        /// </summary>
        BankAccount,

        /// <summary>
        /// Indicates the account source is a bonus to be paid according to normal win rules.
        /// </summary>
        BonusSystem,

        /// <summary>
        /// Indicates the account source is a bonus not metered as win.
        /// </summary>
        NonWinBonusSystem
    }
}
