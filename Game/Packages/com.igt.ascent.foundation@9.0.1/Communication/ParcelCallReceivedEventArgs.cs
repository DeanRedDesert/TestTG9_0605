//-----------------------------------------------------------------------
// <copyright file = "ParcelCallReceivedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class defines the event arguments indicating that
    /// a parcel call has been received.
    /// </summary>
    /// <devdoc>
    /// Technically, this class can be merged into ExtensionParcelCallReceivedEventArgs.
    /// But it would be a breaking change, so it is deferred until there is more compelling reasons to do it.
    /// 
    /// Furthermore, IThemeToExtParcelComm can be modified to use TransactionalParcelCallReceivedEventArgs
    /// and NonTransactionalParcelCallReceivedEventArgs instead of ExtensionParcelCallReceivedEventArgs,
    /// in which case both this class and ExtensionParcelCallReceivedEventArgs can be removed.
    /// However, that would require all games using parcel comm to update its code to handle the new event types
    /// hence it is deferred as well.
    /// </devdoc>
    [Serializable]
    public class ParcelCallReceivedEventArgs : PlatformEventArgs
    {
        /// <summary>
        /// Gets the binary payload of the parcel call.
        /// </summary>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Gets or sets the result of the parcel call.
        /// </summary>
        /// <remarks>
        /// Note that only the handlers of transactional parcel call events can set a ParcelCallResult.
        /// Non-transactional parcel call events won't send back the ParcelCallResult to the call originator.
        /// The ParcelCallResult set for the non-transactional parcel call events will be ignored.
        /// </remarks>
        public ParcelCallResult CallResult { get; set; }

        /// <summary>
        /// Initialize an instance of <see cref="ParcelCallReceivedEventArgs"/>.
        /// </summary>
        /// <param name="payload">Payload of the parcel call.</param>
        /// <param name="isTransactional">Whether the event is for a transactional parcel call.</param>
        protected ParcelCallReceivedEventArgs(byte[] payload, bool isTransactional = false)
            : base(isTransactional ? TransactionWeight.Heavy : TransactionWeight.None)
        {
            Payload = payload;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ParcelCallReceivedEvent -");
            builder.AppendLine("\t Payload: " + (Payload == null ? "n/a" : Payload.Length + " bytes"));

            if(Payload?.Length > 0)
            {
                builder.Append("\t\t");
                foreach(var byteData in Payload)
                {
                    builder.AppendFormat("0x{0:X} ", byteData);
                }

                builder.AppendLine();
            }

            builder.AppendLine("\t Call Result: " + CallResult);

            builder.AppendLine();

            return builder.ToString();
        }
    }
}
