//-----------------------------------------------------------------------
// <copyright file = "StandaloneMechanicalReelsDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System;
    using System.Collections.Generic;
    using IGT.Game.Core.Communication.Cabinet.Standalone;
    using IGT.Game.SDKAssets.StandaloneDeviceConfiguration;
    using IGT.Game.Core.Communication.Cabinet.MechanicalReels;

    #endregion

    public class StandaloneMechanicalReelsDevice : StandaloneDeviceBase<IMechanicalReels>
    {
        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            var mechanicalReels = new VirtualMechanicalReels(
                new List<ReelFeatureDescription>
                {
                    new ReelFeatureDescription("ReelShelf",
                        ReelSubFeature.GamePlayReels,
                        new List<ushort> { 1 }, new List<ushort> { 1 },
                        new List<ReelDescription>
                        {
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                        })
                });

            return mechanicalReels;
        }
    }
}