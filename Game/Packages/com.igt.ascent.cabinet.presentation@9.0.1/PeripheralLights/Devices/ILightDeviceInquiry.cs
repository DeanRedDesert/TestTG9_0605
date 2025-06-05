// -----------------------------------------------------------------------
// <copyright file = "ILightDeviceInquiry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    /// <summary>
    /// This interface defines the methods of querying the light device status.
    /// </summary>
    internal interface ILightDeviceInquiry
    {
        /// <summary>
        /// Queries if any light device is connected.
        /// </summary>
        /// <returns>True if at least one light device is connected; False otherwise.</returns>
        bool IsAnyDeviceConnected();

        /// <summary>
        /// Queries if a light device of the given hardware type is connected.
        /// </summary>
        /// <returns>True if the specified light device is connected; False otherwise.</returns>
        bool IsDeviceConnected(Hardware hardware);

        /// <summary>
        /// Queries if a light device of the given feature name is acquired.
        /// </summary>
        /// <returns>True if the specified light device is acquired; False otherwise.</returns>
        bool IsDeviceAcquired(string featureName);
    }
}