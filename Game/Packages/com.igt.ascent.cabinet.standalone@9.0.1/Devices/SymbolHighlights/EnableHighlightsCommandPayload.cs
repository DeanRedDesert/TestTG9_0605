// -----------------------------------------------------------------------
// <copyright file = "EnableHighlightsCommandPayload.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.SymbolHighlights
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Cabinet.SymbolHighlights;
    using IgtUsbDevice;
    using StreamingLights;

    /// <summary>
    /// Command for enabling/disabling symbol highlight features. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1), Serializable]
    internal class EnableHighlightsCommandPayload : UsbCommandPayload
    {
        private readonly short EnabledFeaturesBitmap;

        /// <summary>
        /// Constructor for <see cref="EnableHighlightsCommandPayload"/>.
        /// </summary>
        /// <param name="targetDevice">Targeted device.</param>
        /// <param name="enabledFeatures">Features to enable.</param>
        public EnableHighlightsCommandPayload(byte targetDevice, 
            IList<SymbolHighlightFeature> enabledFeatures) 
            : base((byte)StreamingLightCommandCode.EnableHighlights, targetDevice)
        {
            var features = enabledFeatures.ToList();

            EnabledFeaturesBitmap = 0x0000;

            // Setting the first bit to 1 will enable symbol tracking.
            if(features.Contains(SymbolHighlightFeature.SymbolTracking))
            {
                EnabledFeaturesBitmap += 1;
            }

            // Setting the 2nd bit to 1 will enable hot positions.
            if(features.Contains(SymbolHighlightFeature.SymbolHotPosition))
            {
                EnabledFeaturesBitmap += 1 << 1;
            }
        }

        #region IPackable Overrides

        /// <inheritdoc />
        public override void Pack(Stream stream)
        {
            base.Pack(stream);

            stream.Write(BitConverter.GetBytes(EnabledFeaturesBitmap), 0, 2);
        }

        #endregion
    }
}