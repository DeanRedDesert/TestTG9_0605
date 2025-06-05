//-----------------------------------------------------------------------
// <copyright file = "ITransactionVerification.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;

    /// <summary>
    /// Interface for validating transactions.
    /// </summary>
    public interface ITransactionVerification
    {
        /// <summary>
        /// Check if an open transaction is available for the operation.
        /// Should be called by all transactional platform methods.
        /// </summary>
        /// <exception cref="Exception">
        /// Thrown when there is no open transaction available. The concrete type of the exception will depend on the
        /// platform implementation.
        /// </exception>
        /// <devdoc>
        /// This file and IGameLib need clean up to check open transaction for all methods.
        /// </devdoc>
        void MustHaveOpenTransaction();
    }
}
