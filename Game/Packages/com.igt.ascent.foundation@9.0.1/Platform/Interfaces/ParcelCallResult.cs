//-----------------------------------------------------------------------
// <copyright file = "ParcelCallResult.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// This class defines the result of a parcel call.
    /// </summary>
    [Serializable]
    public class ParcelCallResult
    {
        /// <summary>
        /// Gets the status of the parcel call.
        /// </summary>
        public ParcelCallStatus Status { get; private set; }

        /// <summary>
        /// Gets the payload data returned by the parcel call.
        /// </summary>
        /// <remarks>
        /// Note that only transactional parcel calls can return a payload.
        /// Non-transactional parcel calls cannot return a payload.
        /// </remarks>
        public byte[] Payload { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="ParcelCallResult"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="payload"/> will not be returned to the caller
        /// if <paramref name="status"/> is not <see cref="ParcelCallStatus.Success"/>.
        /// </remarks>
        /// <param name="status">The status of the parcel call.</param>
        /// <param name="payload">The payload data returned by the parcel call.</param>
        public ParcelCallResult(ParcelCallStatus status, byte[] payload)
        {
            Status = status;
            Payload = payload;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("ParcelCallResult -");
            builder.AppendLine("\t Status = " + Status);
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
    }
}
