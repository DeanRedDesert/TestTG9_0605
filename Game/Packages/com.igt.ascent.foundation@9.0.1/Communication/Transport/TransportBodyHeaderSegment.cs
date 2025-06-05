//-----------------------------------------------------------------------
// <copyright file = "TransportBodyHeaderSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents the transport body header segment of a binary message.
    /// </summary>
    internal sealed class TransportBodyHeaderSegment : IBinaryMessageSegment
    {
        /// <summary>
        /// Gets the size of this segment in bytes.
        /// </summary>
        public static readonly int SegmentSize = Marshal.SizeOf(Enum.GetUnderlyingType(typeof(BodyType)));

        /// <summary>
        /// The type of transport message this object represents.
        /// </summary>
        public BodyType BodyType { get; set; }

        #region IBinaryMessageSegment Implementation

        /// <inheritdoc/>
        public int Size => SegmentSize;

        /// <inheritdoc/>
        public void Read(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageUnderflowException(
                    "Byte array does not have sufficient bytes to fill this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            BodyType = (BodyType)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, startPosition));
        }

        /// <inheritdoc/>
        public void Write(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageOverflowException(
                    "Byte array does not have sufficient bytes to contain this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)BodyType));
            Array.Copy(bytes, 0, buffer, startPosition, bytes.Length);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return "BodyType: " + BodyType;
        }
    }
}