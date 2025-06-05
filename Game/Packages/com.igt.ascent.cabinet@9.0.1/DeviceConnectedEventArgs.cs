//-----------------------------------------------------------------------
// <copyright file = "DeviceConnectedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Event which indicates that a device has been released.
    /// </summary>
    public class DeviceConnectedEventArgs : DeviceEventArgs
    {
        /// <summary>
        /// A list of groups that belong to the device.
        /// This is empty if a device does not have groups.
        /// </summary>
        public IList<uint> GroupList { get; set; }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was connected.</param>
        /// <param name="deviceId">The id of the connected device.</param>
        public DeviceConnectedEventArgs(DeviceType deviceName, string deviceId)
            : base(deviceName, deviceId)
        {
            GroupList = new List<uint>();
        }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was connected.</param>
        /// <param name="deviceId">The id of the connected device.</param>
        /// <param name="groupList"> A list of groups belonging to a device that were released.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="groupList"/> is null.
        /// </exception>
        public DeviceConnectedEventArgs(DeviceType deviceName, string deviceId, List<uint> groupList)
            : base(deviceName, deviceId)
        {
            GroupList = groupList ?? throw new ArgumentNullException(nameof(groupList));
        }
    }
}
