// -----------------------------------------------------------------------
// <copyright file = "IDeviceInformation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using Communication.Cabinet;

    /// <summary>
    /// The basic information about a light device.
    /// </summary>
    public interface IDeviceInformation
    {
        /// <summary>
        /// The reason reported by the CSI as to why the device was not acquired.
        /// This information is mainly useful for diagnostic purposes.
        /// </summary>
        DeviceAcquisitionFailureReason? AcquireFailureReason { get; }

        /// <summary>
        /// Indicates if the device has been acquired by the game and can be used.
        /// </summary>
        bool DeviceAcquired { get; }

        /// <summary>
        /// The number of light groups in the light feature.
        /// </summary>
        ushort GroupCount { get; }

        /// <summary>
        /// The hardware type of the peripheral light.
        /// </summary>
        Hardware HardwareType { get; }

        /// <summary>
        /// Indicates if the hardware device was found when the instance was created.
        /// Used for diagnostic purposes.
        /// </summary>
        bool WasFeatureFoundAtCreation { get; }
    }
}