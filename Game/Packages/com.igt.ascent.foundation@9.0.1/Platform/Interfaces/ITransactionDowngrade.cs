// -----------------------------------------------------------------------
// <copyright file = "ITransactionDowngrade.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines the APIs to downgrade a Foundation event's transaction weight.
    /// </summary>
    /// <typeparamref name="TPlatformEventArgs">
    /// The type of the Foundation event to downgrade.
    /// </typeparamref>
    public interface ITransactionDowngrade<out TPlatformEventArgs> where TPlatformEventArgs : PlatformEventArgs
    {
        /// <summary>
        /// Downgrade the event's transaction weight to the specified value.
        /// </summary>
        /// <param name="newTransactionWeight">
        /// The new transaction weight of the event.
        /// </param>
        /// <returns>
        /// A new event instance that has the exactly the same fields as
        /// before downgrading except the transaction weight.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="newTransactionWeight"/> is no less than the existing one.
        /// </exception>
        TPlatformEventArgs Downgrade(TransactionWeight newTransactionWeight);
    }
}