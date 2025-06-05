//-----------------------------------------------------------------------
// <copyright file = "TransactionalParcelCallReceivedEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class defines the event arguments indicating that
    /// a transactional parcel call has been received.
    /// </summary>
    [Serializable]
    public sealed class TransactionalParcelCallReceivedEventArgs : ParcelCallReceivedEventArgsBase,
                                                                   ITransactionDowngrade<TransactionalParcelCallReceivedEventArgs>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the result of the transactional parcel call.
        /// </summary>
        public ParcelCallResult CallResult { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionalParcelCallReceivedEventArgs"/>
        /// with heavyweight transaction.
        /// </summary>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target entity.</param>
        /// <param name="payload">Payload of the parcel call.</param>
        public TransactionalParcelCallReceivedEventArgs(ParcelCommEndpoint source, ParcelCommEndpoint target, byte [] payload)
            : this(source, target, payload, TransactionWeight.Heavy)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TransactionalParcelCallReceivedEventArgs"/>
        /// with specified transaction weight.
        /// </summary>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target entity.</param>
        /// <param name="payload">Payload of the parcel call.</param>
        /// <param name="transactionWeight">The transaction weight of the event. </param>
        private TransactionalParcelCallReceivedEventArgs(ParcelCommEndpoint source, ParcelCommEndpoint target, byte [] payload,
                                                         TransactionWeight transactionWeight)
            : base(source, target, payload, transactionWeight)
        {
        }


        #endregion

        #region ITransactionDowngrade Implementation

        /// <inheritdoc/>
        public TransactionalParcelCallReceivedEventArgs Downgrade(TransactionWeight newTransactionWeight)
        {
            if(newTransactionWeight >= TransactionWeight)
            {
                throw new InvalidOperationException(
                    $"New transaction weight {newTransactionWeight} is no less than the current value {TransactionWeight}");
            }

            return new TransactionalParcelCallReceivedEventArgs(Source, Target, Payload, newTransactionWeight);
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("TransactionalParcelCallReceivedEventArgs -");
            builder.Append(base.ToString());

            builder.AppendLine("\t Call Result: " + CallResult);

            return builder.ToString();
        }

        #endregion
    }
}
