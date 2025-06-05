//-----------------------------------------------------------------------
// <copyright file = "FeatureZeroDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.UsbFeatureZero
{
    using System;
    using IgtUsbDevice;

    /// <summary>
    /// This data structure stores information on a Feature Zero.
    /// </summary>
    [Serializable]
    internal class FeatureZeroDescriptor : IUnpackable
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        private const int LocalSize = 1 + sizeof(ushort);

        /// <summary>
        /// Header version supported by the device.
        /// </summary>
        public byte HeaderVersion { get; set; }

        /// <summary>
        /// Bitmap of supported verification schemes.
        /// </summary>
        public ushort VerificationTypes { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Header Version ({HeaderVersion}) / Verification Types (0x{VerificationTypes:X})";
        }

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
                    "Data buffer is not big enough to unpack FeatureZeroDescriptor.");
            }

            HeaderVersion = buffer[offset];
            VerificationTypes = BitConverter.ToUInt16(buffer, offset + 1);
        }

        #endregion
    }
}
