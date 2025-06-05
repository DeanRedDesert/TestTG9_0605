//-----------------------------------------------------------------------
// <copyright file = "UsbReportType.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Enumeration used to specify type of interrupt in messages
    /// sent from the IGT class peripheral.
    /// </summary>
    [Serializable]
    internal enum UsbReportType : byte
    {
        /// <summary>
        /// A control message has been rejected.
        /// </summary>
        ControlMessageRejected = 0xFF,

        /// <summary>
        /// The status of the device has changed.
        /// </summary>
        Status = 0xFE,

        /// <summary>
        /// An event has occurred.
        /// </summary>
        Event  = 0xFD,

        /// <summary>
        /// A sequence message has been processed.
        /// </summary>
        SequenceMessageProcessed = 0xFC,

        /// <summary>
        /// A bulk message has been rejected.
        /// </summary>
        BulkMessageRejected = 0xFB
    }
}