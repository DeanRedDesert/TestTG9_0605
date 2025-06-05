//-----------------------------------------------------------------------
// <copyright file = "DeviceReleasedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
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
    public class DeviceReleasedEventArgs : DeviceEventArgs
    {
        /// <summary>
        /// Reason why the device was lost.
        /// </summary>
        public DeviceAcquisitionFailureReason Reason { get; }

        /// <summary>
        /// A list of groups that belong to the device.
        /// This is empty if a device does not have groups.
        /// </summary>
        public IList<uint> GroupList { get; set; }
        
        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was released.</param>
        /// <param name="deviceId">The id of the released device.</param>
        /// <param name="reason">
        /// The reason that the device was lost. If the reason indicates that the request is queued, then a new request
        /// to acquire the device does not need to be made.
        /// </param>
        public DeviceReleasedEventArgs(DeviceType deviceName, string deviceId, DeviceAcquisitionFailureReason reason)
            : base(deviceName, deviceId)
        {
            Reason = reason;
            GroupList = new List<uint>();
        }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was released.</param>
        /// <param name="deviceId">The id of the released device.</param>
        /// <param name="groupList"> A list of groups belonging to a device that were released.</param>
        /// <param name="reason">
        /// The reason that the device was lost. If the reason indicates that the request is queued, then a new request
        /// to acquire the device does not need to be made.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="groupList"/> is null.
        /// </exception>
        public DeviceReleasedEventArgs(DeviceType deviceName, string deviceId, List<uint> groupList,
            DeviceAcquisitionFailureReason reason)
            : base(deviceName, deviceId)
        {
            Reason = reason;
            GroupList = groupList ?? throw new ArgumentNullException(nameof(groupList));
        }
    }
}
