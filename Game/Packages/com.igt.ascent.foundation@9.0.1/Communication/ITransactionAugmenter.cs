//-----------------------------------------------------------------------
// <copyright file = "ITransactionAugmenter.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// This interface defines APIs that allow the user to piggy back
    /// on each transaction created.
    /// </summary>
    public interface ITransactionAugmenter
    {
        /// <summary>
        /// Event occurs when a transaction, either initiated by
        /// the game or the Foundation, is about to close.
        /// </summary>
        event EventHandler TransactionClosingEvent;
    }
}
