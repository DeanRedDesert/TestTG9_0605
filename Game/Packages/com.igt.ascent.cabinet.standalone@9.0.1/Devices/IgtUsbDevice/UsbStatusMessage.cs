//-----------------------------------------------------------------------
// <copyright file = "UsbStatusMessage.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The device driver sends this message whenever its status changes.
    /// </summary>
    [Serializable]
    internal class UsbStatusMessage : IUnpackable
    {
        /// <summary>
        /// Get the list of status records contained in the message.
        /// </summary>
        public IList<UsbStatusRecord> StatusRecords { get; private set; }

        #region IUnpackable Members

        /// <inheritdoc/>
        /// <remarks>
        /// The value of DataSize could change after unpacking.
        /// </remarks>
        public int DataSize
        {
            get { return StatusRecords?.Sum(record => record.DataSize) ?? 0; }
        }

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

            StatusRecords = new List<UsbStatusRecord>();

            while(offset < buffer.Length)
            {
                var record = new UsbStatusRecord();
                record.Unpack(buffer, offset);

                StatusRecords.Add(record);

                offset += record.DataSize;
            }
        }

        #endregion
    }
}
