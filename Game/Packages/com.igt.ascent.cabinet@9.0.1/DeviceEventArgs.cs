//-----------------------------------------------------------------------
// <copyright file = "DeviceEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Event arguments for device events.
    /// </summary>
    public class DeviceEventArgs : EventArgs
    {
        /// <summary>
        /// Construct an instance of the event arguments with the given device information.
        /// </summary>
        /// <param name="deviceName">The name of the device which the event is for.</param>
        /// <param name="deviceId">The id of the device.</param>
        public DeviceEventArgs(DeviceType deviceName, string deviceId)
        {
            DeviceName = deviceName;
            DeviceId = deviceId;
        }

        /// <summary>
        /// The device which the event is for.
        /// </summary>
        public DeviceType DeviceName { get; }

        /// <summary>
        /// The device id of the acquired device.
        /// </summary>
        /// <remarks>If a device id is not provided by the CSI Manager then this property will be null.</remarks>
        public string DeviceId { get; }
    }
}