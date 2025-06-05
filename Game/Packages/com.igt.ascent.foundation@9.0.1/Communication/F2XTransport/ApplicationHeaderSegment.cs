//-----------------------------------------------------------------------
// <copyright file = "ApplicationHeaderSegment.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    using System;
    using System.Net;
    using Transport;

    /// <summary>
    /// Represents the application header segment of a binary message.
    /// </summary>
    internal sealed class ApplicationHeaderSegment : IBinaryMessageSegment
    {
        /// <summary>
        /// Gets the size in bytes for segments of this type.
        /// </summary>
        public static int SegmentSize = sizeof(int) + sizeof(int) + sizeof(byte) + sizeof(int) + sizeof(int);

        /// <summary>
        /// The application message number.
        /// </summary>
        public uint MessageNumber { get; set; }

        /// <summary>
        /// The API category the message is intended for.
        /// </summary>
        public uint ApiCategory { get; set; }

        /// <summary>
        /// The channel the message was sent on.
        /// </summary>
        public byte Channel { get; set; }

        /// <summary>
        /// The transaction identifier for the message.
        /// </summary>
        public uint TransactionIdentifier { get; set; }

        /// <summary>
        /// The status code for the message, so that errors can be reported
        /// when a message is sent in an invalid situation or is not able
        /// to be properly decoded.
        /// </summary>
        public int StatusCode { get; set; }


        ///<inheritdoc/>
        public int Size
        {
            get { return SegmentSize; }
        }

        ///<inheritdoc/>
        public void Read(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageUnderflowException(
                    "Byte array does not have sufficient bytes to fill this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            var position = startPosition;
            MessageNumber = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
            position += sizeof(int);

            ApiCategory = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
            position += sizeof(int);

            Channel = buffer[position++];

            TransactionIdentifier = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
            position += sizeof(int);

            StatusCode = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
        }

        ///<inheritdoc/>
        public void Write(byte[] buffer, int startPosition)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if(buffer.Length - startPosition < Size)
            {
                throw new BinaryMessageOverflowException(
                    "Byte array does not have sufficient bytes to contain this segment.", Size, buffer.Length - startPosition,
                    GetType());
            }
            var position = startPosition;
            var bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)MessageNumber));
            Array.Copy(bytes, 0, buffer, position, bytes.Length);
            position += bytes.Length;

            bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)ApiCategory));
            Array.Copy(bytes, 0, buffer, position, bytes.Length);
            position += bytes.Length;

            buffer[position++] = Channel;

            bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((int)TransactionIdentifier));
            Array.Copy(bytes, 0, buffer, position, bytes.Length);
            position += bytes.Length;

            bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(StatusCode));
            Array.Copy(bytes, 0, buffer, position, bytes.Length);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return "MessageNumber: " + MessageNumber + " ApiCategory: " + ApiCategory + " Channel: " + Channel +
                " TransactionIdentifier: " + TransactionIdentifier + " StatusCode: " + StatusCode;
        }
    }
}