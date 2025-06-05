//-----------------------------------------------------------------------
// <copyright file = "UsbStatusRecord.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Text;

    /// <summary>
    /// This class encapsulates the data describing a device status.
    /// </summary>
    [Serializable]
    internal class UsbStatusRecord : IUnpackable
    {
        /// <summary>
        /// The minimum length of a status record, which
        /// should have at least Length and Status fields.
        /// </summary>
        private const int MinimumLength = 1 + sizeof(ushort);

        /// <summary>
        /// Get the number of bytes in the status record.
        /// </summary>
        public byte Length { get; private set; }

        /// <summary>
        /// Get the status of the device.
        /// </summary>
        public ushort Status { get; private set; }

        /// <summary>
        /// Get the format of the additional data for the status,
        /// if one presents.
        /// </summary>
        public UsbDataFormat DataFormat { get; private set; }

        /// <summary>
        /// Get non-text data for the status.
        /// Valid when <see cref="DataFormat"/> is neither
        /// <see cref="UsbDataFormat.Ascii"/> nor
        /// <see cref="UsbDataFormat.Unicode"/>.
        /// </summary>
        public byte[] NonTextData { get; private set; }

        /// <summary>
        /// Get the text data for the status.
        /// valid when <see cref="DataFormat"/> is either
        /// <see cref="UsbDataFormat.Ascii"/> or
        /// <see cref="UsbDataFormat.Unicode"/>.
        /// </summary>
        public string TextData { get; private set; }

        /// <summary>
        /// Get the flag indicating whether the status is a global status code.
        /// </summary>
        public bool IsGlobalStatus => Enum.IsDefined(typeof(GlobalStatusCode), Status);

        #region IUnpackable Members

        /// <inheritdoc/>
        /// <remarks>
        /// The value of DataSize could change after unpacking.
        /// </remarks>
        public int DataSize => Length;

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

            if(buffer.Length - offset < MinimumLength)
            {
                throw new InsufficientDataBufferException(
                    "Data buffer is not big enough to unpack UsbStatusRecord.");
            }

            // Retrieve the must-present fields.
            Length = buffer[offset];
            Status = BitConverter.ToUInt16(buffer, offset + 1);

            offset += MinimumLength;

            // Check if additional data present.
            if(Length > MinimumLength)
            {
                if(buffer.Length - offset < Length - MinimumLength)
                {
                    throw new InsufficientDataBufferException(
                        "Data buffer is not big enough to unpack UsbStatusRecord.");
                }

                // Retrieve data format.
                DataFormat = (UsbDataFormat)buffer[offset];

                // Get the length of additional data, excluding MinimumLength and DataFormat.
                var dataLength = Length - MinimumLength - 1;

                // If Length is greater than MinimumLength, there should be
                // at least 1 byte of additional data.
                if(dataLength <= 0)
                {
                    throw new InvalidUsbDeviceDataException(
                        $"Invalid length of {Length} for USB Status Message.");
                }

                // Retrieve additional data.
                var data = new byte[dataLength];
                Array.Copy(buffer, offset + 1, data, 0, dataLength);

                // Convert the data to appropriate types based on data format.
                switch(DataFormat)
                {
                    case UsbDataFormat.Ascii:
                        TextData = Encoding.ASCII.GetString(data);
                        break;

                    case UsbDataFormat.Unicode:
                        TextData = Encoding.Unicode.GetString(data);
                        break;

                    default:
                        NonTextData = data;
                        break;
                }
            }
        }

        #endregion
    }
}
