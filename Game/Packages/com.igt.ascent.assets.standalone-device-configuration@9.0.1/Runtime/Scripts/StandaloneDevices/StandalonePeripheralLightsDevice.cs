//-----------------------------------------------------------------------
// <copyright file = "StandalonePeripheralLightsDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System.Collections.Generic;
    using IGT.Game.Core.Communication.Cabinet;
    using IGT.Game.Core.Communication.Cabinet.Standalone;

    #endregion

    public class StandalonePeripheralLightsDevice : StandaloneDeviceBase<IPeripheralLights>
    {
        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            return new VirtualPeripheralLights(new List<LightFeatureDescription>());
        }
    }
}