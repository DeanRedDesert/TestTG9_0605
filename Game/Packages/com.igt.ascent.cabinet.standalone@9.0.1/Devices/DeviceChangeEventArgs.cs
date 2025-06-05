//-----------------------------------------------------------------------
// <copyright file = "DeviceChangeEventArgs.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;

    /// <summary>
    /// Event indicating that a device change message has been received.
    /// </summary>
    internal class DeviceChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Get the type of the device change event.
        /// </summary>
        public DeviceChangeType Type { get; }

        /// <summary>
        /// Get the path of the device being inserted or removed.
        /// </summary>
        public string DevicePath { get; }

        /// <summary>
        /// Initialize the instance of the event arguments to describe
        /// a specific type of change to a specified device.
        /// </summary>
        /// <param name="type">The type of device change.</param>
        /// <param name="devicePath">The path of the device being changed.</param>
        public DeviceChangeEventArgs(DeviceChangeType type, string devicePath)
        {
            Type = type;
            DevicePath = devicePath;
        }
    }
}
