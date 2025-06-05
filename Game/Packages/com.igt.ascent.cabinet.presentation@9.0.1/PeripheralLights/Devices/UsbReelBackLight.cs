//-----------------------------------------------------------------------
// <copyright file = "UsbReelBackLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using Communication.Cabinet;

    /// <summary>
    /// Represents a USB mechanical reel backlight.
    /// </summary>
    public class UsbReelBackLight : UsbIndividualLightControl
    {
        /// <summary>
        /// Construct a USB reel backlight.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbReelBackLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        { }
    }
}
