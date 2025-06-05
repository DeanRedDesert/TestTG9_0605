//-----------------------------------------------------------------------
// <copyright file = "StandaloneStreamingLightsDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices.ComTypes;
    using IGT.Game.Core.Communication.Cabinet;
    using IGT.Game.Core.Communication.Cabinet.Standalone;
    using IGT.Game.SDKAssets.StandaloneDeviceConfiguration;

    #endregion

    public class StandaloneStreamingLightsDevice : StandaloneDeviceBase<IStreamingLights>
    {
        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            return new VirtualStreamingLights();
        }
    }
}