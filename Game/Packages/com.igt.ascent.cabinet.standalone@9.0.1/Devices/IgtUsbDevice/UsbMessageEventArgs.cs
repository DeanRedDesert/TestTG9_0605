//-----------------------------------------------------------------------
// <copyright file = "UsbMessageEventArgs.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Event indicating that an USB interrupt in message has been received.
    /// </summary>
    [Serializable]
    internal class UsbMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The interface number of the device driver that sent the message.
        /// </summary>
        public byte InterfaceNumber { get; private set; }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public UsbReportType Type { get; private set; }

        /// <summary>
        /// The message data sent by the device.
        /// </summary>
        public byte[] MessageData { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="UsbMessageEventArgs"/>.
        /// </summary>
        /// <param name="interfaceNumber">The interface number of the device driver that sent the message.</param>
        /// <param name="type">The type of the message.</param>
        /// <param name="messageData">The message data sent by the device.</param>
        public UsbMessageEventArgs(byte interfaceNumber, UsbReportType type, byte[] messageData)
        {
            InterfaceNumber = interfaceNumber;
            Type = type;
            MessageData = messageData;
        }
    }
}
