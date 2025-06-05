// -----------------------------------------------------------------------
// <copyright file = "CabinetTypeIdentifier.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System.Collections.Generic;
    using System.Linq;
    using CabinetServices;

    /// <summary>
    /// Determines what the given cabinet type is based off of connected streaming lights.
    /// </summary>
    public static class CabinetTypeIdentifier
    {
        private static readonly List<KeyValuePair<Hardware, CabinetType>> CabinetTypeLookUp =
            new List<KeyValuePair<Hardware, CabinetType>>
                {
                    new KeyValuePair<Hardware, CabinetType>(Hardware.WheelFacade, CabinetType.CrystalCoreWheel),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.AustraliaCrystalCoreTrim, CabinetType.CrystalCoreAustralia),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.StreamingCrystalCoreEdge42, CabinetType.CrystalCore42),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.StreamingCrystalCoreEdge27, CabinetType.CrystalCoreDual27),
                    // CrystalCoreDual27 also has a 23 inch device, so CrystalCoreDual must come after CrystalCoreDual27.
                    new KeyValuePair<Hardware, CabinetType>(Hardware.StreamingCrystalCoreEdge30, CabinetType.CrystalCoreDual),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.Axxis23_23, CabinetType.Axxis),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.QuasarMonitorLights, CabinetType.Quasar),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.CrystalDualPlusVideo4DMonitorBezelLights, CabinetType.CrystalDualPlusVideo4D),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.CrystalCurveMonitorLights, CabinetType.CrystalCurve),
                    new KeyValuePair<Hardware, CabinetType>(Hardware.MegaTowerWheelBezelLights, CabinetType.MegaTower),
                    // MegaTower has some S3000 devices so CabinetType.S3000 must come after CabinetType.MegaTower
                    new KeyValuePair<Hardware, CabinetType>(Hardware.CatalinaTrimLights, CabinetType.S3000),
                };

        /// <summary>
        /// Gets the flag indicating if the cabinet type has been found.
        /// </summary>
        public static bool CabinetTypeFound { get; private set; }

        /// <summary>
        /// Determines what cabinet the given game is playing on. 
        /// </summary>
        /// <returns>Cabinet type the game is currently playing on.</returns>
        public static CabinetType GetCabinetType()
        {
            var cabinetType = CabinetType.Unknown;

            // Convert to the concrete service type in order to access the internal method.
            if(CabinetServiceLocator.Instance.GetService<IPeripheralLightService>() is PeripheralLightService serviceInstance)
            {
                var streamingLightDeviceInquiry = serviceInstance.GetStreamingLightDeviceInquiry();

                // Do not commit to a cabinet type if no streaming lights are detected.
                if(streamingLightDeviceInquiry.IsAnyDeviceConnected())
                {
                    // Note: First(OrDefault) is needed to preserve any cabinet priorities as documented in CabinetTypeLookUp.
                    cabinetType = CabinetTypeLookUp.FirstOrDefault(entry =>
                                                                       streamingLightDeviceInquiry.IsDeviceConnected(entry.Key))
                                                   .Value;

                    CabinetTypeFound = true;
                }
            }

            return cabinetType;
        }

        /// <summary>
        /// Clear all flags.  Used for unit tests only.
        /// </summary>
        internal static void Clear()
        {
            CabinetTypeFound = false;
        }
    }
}