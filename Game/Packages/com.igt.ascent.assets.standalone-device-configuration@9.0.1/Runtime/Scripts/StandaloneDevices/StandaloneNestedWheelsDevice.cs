//-----------------------------------------------------------------------
// <copyright file = "StandaloneNestedWheelsDevice.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration.StandaloneDevices
{
    #region Using statements

    using System.Collections.Generic;
    using IGT.Game.Core.Communication.Cabinet.MechanicalReels;
    using IGT.Game.Core.Communication.Cabinet.Standalone;

    #endregion

    public class StandaloneNestedWheelsDevice : StandaloneDeviceBase<IMechanicalReels>
    {
        /// <inheritdoc />
        protected override object CreateVirtualImplementation()
        {
            var mechanicalReels = new VirtualMechanicalReels(
                new List<ReelFeatureDescription>
                {
                    new ReelFeatureDescription(" Nested Wheels",
                        // A space is needed here to match the feature ID defined in the MechanicalReelController.
                        ReelSubFeature.Wheel,
                        new List<ushort> { 1 }, new List<ushort> { 1 },
                        new List<ReelDescription>
                        {
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                            new ReelDescription(22, 1),
                        })
                });

            return mechanicalReels;
        }
    }
}