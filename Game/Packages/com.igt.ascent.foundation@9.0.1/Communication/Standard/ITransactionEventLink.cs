//-----------------------------------------------------------------------
// <copyright file = "ITransactionEventLink.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard
{
    using System;

    /// <summary>
    /// This interface defines functionality of a Foundation link
    /// that communicates transactions and events.
    /// </summary>
    internal interface ITransactionEventLink
    {
        /// <summary>
        /// Send request to Foundation to open a transaction.
        /// </summary>
        /// <param name="payload">Game specific byte array payload.</param>
        /// <returns>True if the request succeeded.</returns>
        bool ActionRequest(byte[] payload);

        /// <summary>
        /// Notification of an event being received from the Foundation,
        /// which is expected to be posted to an event queue.
        /// </summary>
        event EventHandler PostingEvent;

        /// <summary>
        /// Notification of an Action Response being received from the Foundation,
        /// which indicates that a transaction has been opened per game's request.
        /// </summary>
        event EventHandler ActionResponseEvent;
    }
}
