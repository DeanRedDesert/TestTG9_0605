// -----------------------------------------------------------------------
// <copyright file = "ParcelCallReceivedEventArgsBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <inheritdoc/>
    /// <summary>
    /// Parcel call base event indicating a parcel call to the game has been received.
    /// </summary>
    [Serializable]
    public abstract class ParcelCallReceivedEventArgsBase : PlatformEventArgs
    {
        /// <summary>
        /// Gets the source entity of the received parcel call.
        /// </summary>
        public ParcelCommEndpoint Source { get; private set; }

        /// <summary>
        /// Gets the target entity of the received parcel call.
        /// </summary>
        public ParcelCommEndpoint Target { get; private set; }

        /// <summary>
        /// Gets the binary payload of the received parcel call.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="ParcelCallReceivedEventArgsBase"/>.
        /// </summary>
        /// <param name="source">The source entity.</param>
        /// <param name="target">The target entity.</param>
        /// <param name="payload">Payload of the parcel call.</param>
        /// <param name="transactionWeight">The transaction weight of the event.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="target"/> is null
        /// </exception>
        protected ParcelCallReceivedEventArgsBase(ParcelCommEndpoint source,
                                                  ParcelCommEndpoint target,
                                                  byte[] payload,
                                                  TransactionWeight transactionWeight)
            : base(transactionWeight)
        {
            if(source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if(target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            Source = source;
            Target = target;
            Payload = payload;
        }

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(base.ToString());
            builder.AppendLine("\t Source: " + Source);
            builder.AppendLine("\t Target: " + Target);
            builder.AppendLine("\t Payload: " + (Payload == null ? "n/a" : Payload.Length + " bytes"));

            if(Payload != null && Payload.Length > 0)
            {
                builder.Append("\t\t");
                foreach(var byteData in Payload)
                {
                    builder.AppendFormat("0x{0:X} ", byteData);
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        #endregion
    }
}
