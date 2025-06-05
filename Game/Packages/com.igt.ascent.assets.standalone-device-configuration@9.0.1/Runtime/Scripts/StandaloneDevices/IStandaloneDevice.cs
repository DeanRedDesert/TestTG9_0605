//-----------------------------------------------------------------------
// <copyright file = "IStandaloneDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Interface defining methods required for standalone devices.
    /// </summary>
    public interface IStandaloneDevice
    {
        /// <summary>
        /// Gets the interface types and their corresponding implementations needed by a standalone device.
        /// </summary>
        /// <returns>Dictionary of device type to device implementation, or null if device is not used.</returns>
        IDictionary<Type, object> GetInterfaceImplementations();
    }
}