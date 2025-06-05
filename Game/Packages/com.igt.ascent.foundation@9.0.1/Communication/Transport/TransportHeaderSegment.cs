//-----------------------------------------------------------------------
// <copyright file = "TransportHeaderSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents the transport header segment of a binary message.
    /// </summary>
    public sealed class TransportHeaderSegment : IBinaryMessageSegment
    {
        /// <summary>
        /// Gets the size of this segment in bytes.
        /// </summary>
        public static readonly int SegmentSize = sizeof(int) + Marshal.SizeOf(Enum.GetUnderlyingType(typeof(MessageType))) + sizeof(int);

        /// <summary>
        /// The size of the foundation message.
        /// </summary>
        public uint PacketLength { get; set; }

        /// <summary>
        /// The type of the foundation message.
        /// </summary>
        public MessageType MessageType { get; set; }

        /// <summary>
        /// The CRC of the length and message type.
        /// </summary>
        public uint Crc { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TransportHeaderSegment"/> with default values.
        /// </summary>
        public TransportHeaderSegment() { }

        /// <summary>
        /// Initializes a new instance of <see cref="TransportHeaderSegment"/> with the given packet length and message type.
        /// </summary>
        /// <param name="packetLength">The size if the foundation message, including this header.</param>
        /// <param name="messageType">The type of foundation message.</param>
        public TransportHeaderSegment(uint packetLength, MessageType messageType)
        {
            PacketLength = packetLength;
            MessageType = messageType;
            Crc = CalculateCrc();
        }

        /// <summary>
        /// Calculates the <see cref="Crc"/> property from the current values of <see cref="PacketLength"/> and <see cref="MessageType"/>.
        /// </summary>
        public uint CalculateCrc()
        {
            var bytes = GetBytesWithoutCrc();
            return Crc32.Calculate(bytes);
        }

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
            var position = startPosition;
            PacketLength = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));

            position += sizeof(int);
            MessageType = (MessageType)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));

            position += sizeof(int);
            Crc = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
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
            var position = startPosition;
            var bytes = GetBytesWithoutCrc();
            Array.Copy(bytes, 0, buffer, position, bytes.Length);

            position += bytes.Length;
            bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)Crc));
            Array.Copy(bytes, 0, buffer, position, bytes.Length);
        }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return "PacketLength: " + PacketLength + " MessageType: " + MessageType + " Crc: " + Crc;
        }

        /// <summary>
        /// Gets a byte array with the contents of everything in the segment but the CRC value.
        /// </summary>
        /// <returns>A byte array with the segment contents, omitting the CRC value.</returns>
        private byte[] GetBytesWithoutCrc()
        {
            var bytes = new byte[Size - sizeof(int)];
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)PacketLength)), bytes, sizeof(int));
            Array.Copy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)MessageType)), 0, bytes, sizeof(int), sizeof(int));
            return bytes;
        }
    }
}