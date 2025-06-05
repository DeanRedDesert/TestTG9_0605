//-----------------------------------------------------------------------
// <copyright file = "PeripheralLightDeviceEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event class for peripheral light devices.
    /// </summary>
    public class PeripheralLightDeviceEventArgs : EventArgs
    {
        /// <summary>
        /// List of peripheral devices.
        /// </summary>
        public IList<UsbLightBase> PeripheralLightDevices { get; }

        /// <summary>
        /// Construct an instance of the event arguments with the given device name.
        /// </summary>
        /// <param name="peripheralLightDevices">The list of peripheral light devices.</param>
        public PeripheralLightDeviceEventArgs(IList<UsbLightBase> peripheralLightDevices)
        {
            PeripheralLightDevices = peripheralLightDevices;
        }
    }
}
