//-----------------------------------------------------------------------
// <copyright file = "ErrorCode.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The Error Code enumeration is used to represent
    /// the error messages returned by Foundation methods.
    /// </summary>
    [Serializable]
    public enum ErrorCode
    {
        /// <summary>
        /// The function returns with no error.
        /// </summary>
        NoError,

        /// <summary>
        /// Error with no specific reason.
        /// </summary>
        GeneralError,

        /// <summary>
        /// Error code returned by CreateTransaction, indicating
        /// that an open transaction already existed.
        /// </summary>
        OpenTransactionExisted,

        /// <summary>
        /// Error code returned by CreateTransaction, indicating
        /// that a transaction cannot be initiated by the game
        /// right now, since there are events waiting to be processed,
        /// which means a Foundation initiated transaction is open.
        /// </summary>
        EventWaitingForProcess,

        /// <summary>
        /// Error code returned by CloseTransaction, indicating
        /// that no open transaction is available to be closed.
        /// </summary>
        NoTransactionOpen
    }
}
