//-----------------------------------------------------------------------
// <copyright file = "UsbMessageHeader.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// This data structure represents the header of an
    /// IGT class interrupt in message sent by the device.
    /// </summary>
    internal class UsbMessageHeader : IUnpackable
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        private const int LocalSize = 2;

        /// <summary>
        /// Get the interface number of the report sender.
        /// </summary>
        public byte InterfaceNumber { get; private set; }

        /// <summary>
        /// Get the type of the report.
        /// </summary>
        public UsbReportType ReportType { get; private set; }

        #region IUnpackable Members

        /// <inheritdoc/>
        public virtual int DataSize => LocalSize;

        /// <inheritdoc/>
        public virtual void Unpack(byte[] buffer, int offset)
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
                    "Data buffer is not big enough to unpack UsbMessageHeader.");
            }

            InterfaceNumber = buffer[offset];
            ReportType = (UsbReportType)buffer[offset + 1];
        }

        #endregion
    }
}
