//-----------------------------------------------------------------------
// <copyright file = "StandaloneHaptic.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Provide a virtual implementatioon of the haptic category for use in standalone applications
    /// where the devices would not be physically present and need to be simulated.
    /// </summary>
    class StandaloneHaptic : IHaptic
    {
        /// <summary>
        /// A set of haptic devices typically present on an EGM.  For use in standalone situations only.
        /// </summary>
        private readonly List<HapticDevice> dummyDevices = new List<HapticDevice>
        {
            new HapticDevice
            {
                DeviceId = "Haptic Device 2AC7 0201",
                HapticDeviceLocation = HapticDeviceLocation.MainMonitorBottom,
                HapticDeviceType = HapticDeviceType.Ultrasonic
            },
            new HapticDevice
            {
                DeviceId = "Haptic Device 2AC7 0202",
                HapticDeviceLocation = HapticDeviceLocation.MainMonitorTop,
                HapticDeviceType = HapticDeviceType.Ultrasonic
            }
        };

        #region IHpatic interface

        /// <inheritdoc/>
        public IEnumerable<HapticDevice> GetAvailableHapticDevices()
        {
            return dummyDevices;
        }

        /// <inheritdoc/>
        public HapticDeviceStatus GetHapticDeviceStatus(string deviceIdToGetStatus)
        {
            if(dummyDevices.Find (d => d.DeviceId == deviceIdToGetStatus) == null)
            {
                throw new HapticCategoryException ("Unknown Device ID", "The provided device ID ( " + deviceIdToGetStatus + " ) does not match any of StandaloneHaptic's dummy devices");
            }

            return HapticDeviceStatus.OK;
        }

        #endregion

    }
}
