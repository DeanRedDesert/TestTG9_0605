//-----------------------------------------------------------------------
// <copyright file = "DeviceRequestPollingEventArgs.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Event indicating that a device has requested an active polling.
    /// </summary>
    [Serializable]
    internal class DeviceRequestPollingEventArgs : EventArgs
    {
        /// <summary>
        /// Get the interface number of the device that
        /// requested the active polling.
        /// </summary>
        public byte InterfaceNumber { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="DeviceRequestPollingEventArgs"/>.
        /// </summary>
        /// <param name="interfaceNumber">
        /// The interface number of the device that requested the active polling.
        /// </param>
        public DeviceRequestPollingEventArgs(byte interfaceNumber)
        {
            InterfaceNumber = interfaceNumber;
        }
    }
}
