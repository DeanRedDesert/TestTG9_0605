//-----------------------------------------------------------------------
// <copyright file = "NonTransactionalParcelCallReceivedEventArgs.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class defines the event arguments indicating that
    /// a non-transactional parcel call has been received.
    /// </summary>
    [Serializable]
    public sealed class NonTransactionalParcelCallReceivedEventArgs : ParcelCallReceivedEventArgsBase
    {
        /// <summary>
        /// Initializes an instance of <see cref="NonTransactionalParcelCallReceivedEventArgs"/>.
        /// </summary>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target entity.</param>
        /// <param name="payload">Payload of the parcel call.</param>
        public NonTransactionalParcelCallReceivedEventArgs(ParcelCommEndpoint source, ParcelCommEndpoint target, byte [] payload)
            : base(source, target, payload, TransactionWeight.None)
        {
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("NonTransactionalParcelCallReceivedEventArgs -");
            builder.Append(base.ToString());

            return builder.ToString();
        }

        #endregion
    }
}
