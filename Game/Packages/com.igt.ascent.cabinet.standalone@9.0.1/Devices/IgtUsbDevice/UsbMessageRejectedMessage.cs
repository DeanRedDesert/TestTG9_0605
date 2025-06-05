//-----------------------------------------------------------------------
// <copyright file = "UsbMessageRejectedMessage.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// The device driver sends this message to inform the host that
    /// the device rejected the last IGT Class message it received.
    /// </summary>
    [Serializable]
    internal class UsbMessageRejectedMessage : IUnpackable
    {
        /// <summary>
        /// Get reason why a message was rejected by the device.
        /// </summary>
        public UsbRejectionReason Reason { get; private set; }

        /// <summary>
        /// Get reason-specific data.
        /// </summary>
        public byte[] Data { get; private set; }

        #region IUnpackable Members

        /// <inheritdoc/>
        /// <remarks>
        /// The value of DataSize could change after unpacking.
        /// </remarks>
        public int DataSize => 1 + (Data?.Length ?? 0);

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

            // Retrieve the must-present field.
            Reason = (UsbRejectionReason)buffer[offset];
            offset++;

            // Get the length of additional data.
            var dataLength = buffer.Length - offset;

            // Retrieve reason-specific data.
            if(dataLength > 0)
            {
                var data = new byte[dataLength];
                Array.Copy(buffer, offset, data, 0, dataLength);

                Data = data;
            }
        }

        #endregion
    }
}
