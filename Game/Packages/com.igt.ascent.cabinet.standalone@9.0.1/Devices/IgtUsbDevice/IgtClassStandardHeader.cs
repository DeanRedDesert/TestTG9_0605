//-----------------------------------------------------------------------
// <copyright file = "IgtClassStandardHeader.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.IO;

    /// <summary>
    /// This data structure represents the header of
    /// an IGT class request sent to the device.
    /// </summary>
    [Serializable]
    internal class IgtClassStandardHeader : IPackable, IUnpackable
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        /// <remarks>
        /// The number 4 refers to the sum of sizes of Version, SequeceNumber,
        /// EncryptionAlgorithm and CompressionAlgorithm, each is 1 byte.
        /// </remarks>
        private const int LocalSize = 4 + sizeof(ushort);

        /// <summary>
        /// Get the protocol version.
        /// </summary>
        public byte Version { get; private set; }

        /// <summary>
        /// Get the sequence number of the command.
        /// </summary>
        public byte SequenceNumber { get; private set; }

        /// <summary>
        /// Get the algorithm for encryption.
        /// </summary>
        public byte EncryptionAlgorithm { get; private set; }

        /// <summary>
        /// Get the algorithm for compression.
        /// </summary>
        public byte CompressionAlgorithm { get; private set; }

        /// <summary>
        /// Get the payload length.
        /// </summary>
        public ushort PayloadLength { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="IgtClassStandardHeader"/>
        /// with payload length of zero.
        /// </summary>
        public IgtClassStandardHeader() : this(0)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="IgtClassStandardHeader"/>
        /// with the given payload length.
        /// </summary>
        /// <param name="payloadLength">The length of the request payload.</param>
        public IgtClassStandardHeader(ushort payloadLength)
        {
            Version = DeviceManager.CurrentIgtVersion;
            SequenceNumber = 0;
            EncryptionAlgorithm = 0;
            CompressionAlgorithm = 0;
            PayloadLength = payloadLength;
        }

        #region IPackable Members

        /// <inheritdoc/>
        public void Pack(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            stream.WriteByte(Version);
            stream.WriteByte(SequenceNumber);
            stream.WriteByte(EncryptionAlgorithm);
            stream.WriteByte(CompressionAlgorithm);
            stream.Write(BitConverter.GetBytes(PayloadLength), 0, sizeof(ushort));
        }

        #endregion

        #region IUnpackable Members

        /// <inheritdoc/>
        public int DataSize => LocalSize;

        /// <inheritdoc/>
        public void Unpack(byte[] buffer, int offset)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if(buffer.Length - offset < LocalSize)
            {
                throw new InsufficientDataBufferException(
                    "Data buffer is not big enough to unpack IgtClassStandardHeader.");
            }

            Version = buffer[offset];
            SequenceNumber = buffer[offset + 1];
            EncryptionAlgorithm = buffer[offset + 2];
            CompressionAlgorithm = buffer[offset + 3];
            PayloadLength = BitConverter.ToUInt16(buffer, offset + 4);
        }

        #endregion
    }
}
