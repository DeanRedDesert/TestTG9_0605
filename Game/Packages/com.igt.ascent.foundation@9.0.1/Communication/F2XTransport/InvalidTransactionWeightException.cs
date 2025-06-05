// -----------------------------------------------------------------------
// <copyright file = "InvalidTransactionWeightException.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;

    /// <summary>
    /// The exception that is thrown when a message needs higher transaction weight
    /// than what is available at the moment.
    /// </summary>
    public sealed class InvalidTransactionWeightException : Exception
    {
        #region Constructors

        /// <inheritdoc/>
        public InvalidTransactionWeightException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public InvalidTransactionWeightException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}