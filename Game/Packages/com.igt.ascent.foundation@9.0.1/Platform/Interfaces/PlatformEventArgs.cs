// -----------------------------------------------------------------------
// <copyright file = "PlatformEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// This class represents an event with a Transaction Weight, indicating
    /// whether it carries a lightweight, heavyweight or no transaction with it.
    /// </summary>
    /// <devdoc>
    /// This should be the base class for all events received from Foundation.
    /// </devdoc>
    [Serializable]
    public abstract class PlatformEventArgs : EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the transaction weight of the event.
        /// </summary>
        public TransactionWeight TransactionWeight { get; private set; }

        /// <summary>
        /// Gets a flag indicating whether the event is transactional, either lightweight or heavyweight.
        /// </summary>
        public bool IsTransactional => TransactionWeight != TransactionWeight.None;

        /// <summary>
        /// Gets a flag indicating whether the event is lightweight transactional.
        /// </summary>
        public bool IsLightweight => TransactionWeight == TransactionWeight.Light;

        /// <summary>
        /// Gets a flag indicating whether the event is heavyweight transactional.
        /// </summary>
        public bool IsHeavyweight => TransactionWeight == TransactionWeight.Heavy;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        /// <summary>
        /// Initializes a new instance of <see cref="PlatformEventArgs"/>.
        /// </summary>
        /// <param name="transactionWeight">The transaction weight of the event.</param>
        protected PlatformEventArgs(TransactionWeight transactionWeight)
        {
            TransactionWeight = transactionWeight;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("PlatformEventArgs -");
            builder.AppendLine("\t TransactionWeight: " + TransactionWeight);

            return builder.ToString();
        }

        #endregion

    }
}