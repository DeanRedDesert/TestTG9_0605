//-----------------------------------------------------------------------
// <copyright file = "DeviceRemovedEventArgs.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using CSI.Schemas;

    /// <summary>
    /// Event which indicates that a device has been released.
    /// </summary>
    public class DeviceRemovedEventArgs : DeviceEventArgs
    {
        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="deviceName">The name of the device which was removed.</param>
        /// <param name="deviceId">The id of the removed device.</param>
        public DeviceRemovedEventArgs(DeviceType deviceName, string deviceId)
            : base(deviceName, deviceId)
        {
        }
    }
}