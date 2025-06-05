//-----------------------------------------------------------------------
// <copyright file = "UsbRejectionReason.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Enumeration used to indicate the reason why
    /// an IGT Class message was rejected by the device.
    /// </summary>
    [Serializable]
    internal enum UsbRejectionReason : byte
    {
        /// <summary>
        /// Invalid request type.
        /// </summary>
        InvalidRequestType = 0xFF,

        /// <summary>
        /// Invalid request.
        /// </summary>
        InvalidRequest = 0xFE,

        /// <summary>
        /// Invalid value.
        /// </summary>
        InvalidValue = 0xFD,

        /// <summary>
        /// Invalid interface number.
        /// </summary>
        InvalidInterfaceNumber = 0xFC,

        /// <summary>
        /// Message length does not match the length in the protocol.
        /// </summary>
        LengthMismatch = 0xFB,

        /// <summary>
        /// Invalid standard header.
        /// </summary>
        InvalidStandardHeader = 0xFA,

        /// <summary>
        /// Invalid payload data.
        /// </summary>
        InvalidPayloadData = 0xF9,

        /// <summary>
        /// Message is too long for peripheral's receive buffer.
        /// </summary>
        MessageTooLong = 0xF8,

        /// <summary>
        /// The device is busy.
        /// </summary>
        Busy = 0xF7,

        /// <summary>
        /// The device cannot process the command because it is in a tilt.
        /// </summary>
        InTilt = 0xF6,

        /// <summary>
        /// The device cannot process the command because it is in self-test.
        /// </summary>
        InSelfTest = 0xF5,

        /// <summary>
        /// The device is in a state in which it cannot process the command.
        /// </summary>
        CommandNotSupportedInCurrentState = 0xF4,

        /// <summary>
        /// The device does not support the requested functionality.
        /// </summary>
        FunctionalityNotSupported = 0xF3,

        /// <summary>
        /// Cannot decrypt message.
        /// </summary>
        CannotDecryptMessage = 0xF2
    }
}