//-----------------------------------------------------------------------
// <copyright file = "InvalidUsbDeviceDataException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// This exception indicates that an error occurred when
    /// processing USB Device Data.
    /// </summary>
    [Serializable]
    public class InvalidUsbDeviceDataException : Exception
    {
        /// <summary>
        /// Initialize an instance of <see cref="InvalidUsbDeviceDataException"/>
        /// with a message.
        /// </summary>
        /// <param name="message"></param>
        public InvalidUsbDeviceDataException(string message)
            : base(message)
        {
        }
    }
}
