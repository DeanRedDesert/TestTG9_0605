// -----------------------------------------------------------------------
// <copyright file = "TouchScreenConnectionChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event indicating a touch screen device has been connected or disconnected.
    /// </summary>
    public class TouchScreenConnectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TouchScreenConnectionChangedEventArgs"/>.
        /// </summary>
        /// <param name="deviceId">The id of the touch screen that was connected or disconnected.</param>
        /// <param name="driver">Driver identifier for the device.</param>
        /// <param name="driverSubClass">Sub-class identifier for the device.</param>
        /// <param name="connected">Whether the device has been connected or disconnected.</param>
        public TouchScreenConnectionChangedEventArgs(uint deviceId, ushort driver, ushort driverSubClass,
                                                     bool connected)
        {
            DeviceId = deviceId;
            Driver = driver;
            DriverSubClass = driverSubClass;
            Connected = connected;
        }

        /// <summary>
        /// Gets the id of the touch screen that was connected or disconnected.
        /// </summary>
        public uint DeviceId { get; }

        /// <summary>
        /// Gets the driver identifier for the device.
        /// </summary>
        public ushort Driver { get; }

        /// <summary>
        /// Gets the sub-class identifier for the device.
        /// </summary>
        public ushort DriverSubClass { get; }

        /// <summary>
        /// Gets whether the device has been connected or disconnected.
        /// </summary>
        public bool Connected { get; }
    }
}
