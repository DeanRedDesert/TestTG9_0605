// -----------------------------------------------------------------------
// <copyright file = "CabinetCreditDisplayType.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// This enum defines the types of the credit display mode that can be retained on the cabinet.
    /// </summary>
    [Serializable]
    public enum CabinetCreditDisplayType
    {
        /// <summary>
        /// The credit display mode cannot be retained on the cabinet.
        /// </summary>
        NotSupported,

        /// <summary>
        /// The credit display mode is not specified.
        /// </summary>
        NotSet,

        /// <summary>
        /// The credit meter is displayed as credits.
        /// </summary>
        Credit,

        /// <summary>
        /// The credit meter is displayed as currency.
        /// </summary>
        Currency
    }
}
