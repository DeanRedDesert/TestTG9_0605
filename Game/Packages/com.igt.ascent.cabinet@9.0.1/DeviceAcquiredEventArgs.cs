//-----------------------------------------------------------------------
// <copyright file = "DeviceAcquiredEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Event which indicates that a device has been acquired.
    /// </summary>
    public class DeviceAcquiredEventArgs : DeviceEventArgs
    {
        /// <summary>
        /// A list of groups that belong to the device that were acquired.
        /// This is empty if a device does not have groups.
        /// </summary>
        public IList<uint> GroupList { get; set; }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was acquired.</param>
        /// <param name="deviceId">The id of the acquiredDevice.</param>
        public DeviceAcquiredEventArgs(DeviceType deviceName, string deviceId) : base(deviceName, deviceId)
        {
            GroupList = new List<uint>();
        }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was acquired.</param>
        /// <param name="deviceId">The id of the acquiredDevice.</param>
        /// <param name="groupList"> A list of groups belonging to a device that were released.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="groupList"/> is null.
        /// </exception>
        public DeviceAcquiredEventArgs(DeviceType deviceName, string deviceId, List<uint> groupList) : base(deviceName, deviceId)
        {
            GroupList = groupList ?? throw new ArgumentNullException(nameof(groupList));
        }
    }
}