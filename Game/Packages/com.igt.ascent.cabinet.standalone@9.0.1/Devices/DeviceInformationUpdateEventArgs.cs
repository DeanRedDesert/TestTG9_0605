//-----------------------------------------------------------------------
// <copyright file = "DeviceInformationUpdateEventArgs.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices
{
    using System;

    /// <summary>
    /// Event indicating that the information on a device has been updated.
    /// </summary>
    [Serializable]
    public class DeviceInformationUpdateEventArgs : EventArgs
    {
        /// <summary>
        /// Get the updated device information.
        /// </summary>
        public string DeviceInformation { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="DeviceInformationUpdateEventArgs"/>.
        /// </summary>
        /// <param name="deviceInformation">
        /// The latest update of the information on a device.
        /// </param>
        public DeviceInformationUpdateEventArgs(string deviceInformation)
        {
            DeviceInformation = deviceInformation;
        }
    }
}
