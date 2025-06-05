// -----------------------------------------------------------------------
// <copyright file = "PaytableType.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.PaytableLoader.Interfaces
{
    /// <summary>
    /// Enumerations of paytable types that are supported.
    /// </summary>
    public enum PaytableType
    {
        /// <summary>
        /// The paytable type is invalid or not supported.
        /// </summary>
        Unknown,

        /// <summary>
        /// Legacy Ascent paytable files.
        /// </summary>
        Xpaytable,

        /// <summary>
        /// MPT 64-bit binary paytable files.
        /// </summary>
        Mpt,
    }
}
