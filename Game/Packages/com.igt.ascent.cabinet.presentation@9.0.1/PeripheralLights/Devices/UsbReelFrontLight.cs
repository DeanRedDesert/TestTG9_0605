//-----------------------------------------------------------------------
// <copyright file = "UsbReelFrontLight.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using Communication.Cabinet;

    /// <summary>
    /// Represents USB reel front light hardware.
    /// </summary>
    public class UsbReelFrontLight : UsbIndividualLightControl
    {
        /// <summary>
        /// Construct a USB front light instance.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbReelFrontLight(string featureName, LightFeatureDescription featureDescription,
                                   IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {
            
        }
    }
}
