// -----------------------------------------------------------------------
// <copyright file = "ExtensionParcelCallReceivedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System.Text;

    /// <summary>
    /// This class defines the event arguments indicating that
    /// a parcel call has been received from an extension.
    /// </summary>
    public class ExtensionParcelCallReceivedEventArgs : ParcelCallReceivedEventArgs
    {
        /// <summary>
        /// Gets the identifier of the extension that originates the parcel call.
        /// </summary>
        public string OriginatorIdentifier { get; }

        /// <summary>
        /// Initializes an instance of <see cref="ExtensionParcelCallReceivedEventArgs"/>.
        /// </summary>
        /// <param name="originatorIdentifier">Identifier of the originator of the parcel call.</param>
        /// <param name="payload">Payload of the parcel call.</param>
        /// <param name="isTransactional">Whether the event is for a transactional parcel call.</param>
        public ExtensionParcelCallReceivedEventArgs(string originatorIdentifier, byte[] payload, bool isTransactional = false)
            : base(payload, isTransactional)
        {
            OriginatorIdentifier = originatorIdentifier;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder()
                          .AppendLine("ExtensionParcelCallReceivedEventArgs -")
                          .AppendLine("\t Originator Identifier = " + (OriginatorIdentifier ?? "n/a"))
                          .Append(base.ToString());

            return builder.ToString();
        }
    }
}
