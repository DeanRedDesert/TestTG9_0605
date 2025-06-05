//-----------------------------------------------------------------------
// <copyright file = "ITransactionCallbacks.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    /// <summary>
    /// This interface defines callback methods related to transactions.
    /// Implementations of the methods in this interface must be thread safe.
    /// </summary>
    public interface ITransactionCallbacks
    {
        /// <summary>
        /// Process an Action Response from the Foundation,
        /// which is the notification that a transaction has been
        /// opened per game's request.
        /// </summary>
        /// <param name="payload">
        /// The payload in the action response sent by the Foundation.
        /// </param>
        void ProcessActionResponse(byte[] payload);
    }
}
