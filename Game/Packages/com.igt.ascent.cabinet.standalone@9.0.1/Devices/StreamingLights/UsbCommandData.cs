//-----------------------------------------------------------------------
// <copyright file = "UsbCommandData.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using IgtUsbDevice;

    /// <summary>
    /// Represents the data needed to send a USB command.
    /// </summary>
    internal struct UsbCommandData
    {
        /// <summary>
        /// Construct a new instance using the USB command and its data.
        /// </summary>
        /// <param name="usbCommand">The USB command information.</param>
        /// <param name="data">The data associated with <paramref name="usbCommand"/>.</param>
        public UsbCommandData(UsbCommandPayload usbCommand, byte[] data)
            : this()
        {
            UsbCommand = usbCommand;
            Data = data;
        }

        /// <summary>
        /// The USB command information.
        /// </summary>
        public UsbCommandPayload UsbCommand
        {
            get;
        }

        /// <summary>
        /// The data associated with the command.
        /// </summary>
        public byte[] Data
        {
            get;
        }
    }
}
