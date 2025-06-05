// -----------------------------------------------------------------------
// <copyright file = "TransactionWeight.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    /// <summary>
    /// This enumeration defines the weight of a transaction.
    /// </summary>
    public enum TransactionWeight
    {
        /// <summary>
        /// There is no transaction available.
        /// </summary>
        None,

        /// <summary>
        /// There is a lightweight transaction available, which can be used
        /// to access local state data.
        /// </summary>
        Light,

        /// <summary>
        /// There is a heavyweight transaction available, which can be used
        /// to access machine-wide state data.
        /// </summary>
        Heavy
    }
}