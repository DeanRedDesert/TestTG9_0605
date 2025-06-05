//-----------------------------------------------------------------------
// <copyright file = "HardwareErrorException.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Exception when there is a hardware error reported by the device.
    /// </summary>
    [Serializable]
    public class HardwareErrorException : Exception
    {
        private const string MessageFormat = "{0} has a hardware error on device ID {1}.";

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="deviceName">The name of the device.</param>
        /// <param name="deviceId">The device ID on the hardware device with the error.</param>
        public HardwareErrorException(string deviceName, byte deviceId)
            : base(string.Format(MessageFormat, deviceName, deviceId))
        {
            DeviceName = deviceName;
            DeviceId = deviceId;
        }

        /// <summary>
        /// Gets the name of the device with the error.
        /// </summary>
        public string DeviceName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the device ID on the hardware device with the error.
        /// </summary>
        public byte DeviceId
        {
            get;
            private set;
        }
    }
}
