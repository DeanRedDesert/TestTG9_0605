//-----------------------------------------------------------------------
// <copyright file = "IDeviceRecovery.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// The interface for recovering device states.
    /// </summary>
    internal interface IDeviceRecovery
    {
        /// <summary>
        /// Recover the device state with a specified feature ID.
        /// </summary>
        /// <param name="featureId">The feature ID of the device to recover.</param>
        void RecoverDevice(string featureId);
    }
}
