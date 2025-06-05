// -----------------------------------------------------------------------
// <copyright file = "PeripheralLightManager.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// This class manages light devices thru IPeripheralLights interface.
    /// </summary>
    internal class PeripheralLightManager : LightManagerBase
    {
        #region Fields

        /// <summary>
        /// The interface for communicating with the CSI Manager to control the peripheral light devices.
        /// </summary>
        private IPeripheralLights lightInterface;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="PeripheralLightManager"/>.
        /// </summary>
        /// <param name="hardwareSpecList">
        /// The list of hardware specifications to be supported by this manager.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="hardwareSpecList"/> contains any streaming light specification.
        /// </exception>
        public PeripheralLightManager(IList<HardwareSpec> hardwareSpecList)
            : base(hardwareSpecList)
        {
            if(hardwareSpecList.Any(spec => spec.IsStreamingLight))
            {
                throw new ArgumentException("PeripheralLightManager cannot support any streaming light.",
                                            nameof(hardwareSpecList));
            }
        }

        #endregion

        #region LightManagerBase Overrides

        /// <inheritdoc/>
        protected override DeviceType LightDeviceType => DeviceType.Light;

        /// <inheritdoc/>
        protected override void ConvertAndSetLightInterface(object lightInterfaceObject)
        {
            switch(lightInterfaceObject)
            {
                case null:
                    lightInterface = null;
                    break;
                case IPeripheralLights lights:
                {
                    lightInterface = lights;

                    var devices = AllLightObjects.OfType<UsbPeripheralLight>();
                    foreach(var device in devices)
                    {
                        // Reset the interfaces on the devices in case they are different.
                        device.LightInterface = lightInterface;
                    }

                    break;
                }

                default:
                    throw new ArgumentException("PeripheralLightManager cannot work with interface other than IPeripheralLights",
                        nameof(lightInterfaceObject));
            }
        }

        /// <inheritdoc/>
        protected override object GetLightInterface()
        {
            return lightInterface;
        }

        /// <inheritdoc/>
        protected override IList<LightFeatureDescription> QueryFeatureDescriptions()
        {
            return lightInterface != null
                       ? lightInterface.GetLightDevices().ToList()
                       : new List<LightFeatureDescription>();
        }

        /// <inheritdoc/>
        protected override UsbLightBase CreateFreeLightObject(string featureName)
        {
            return new UsbPeripheralLight(featureName, GetFeatureDescription(featureName), lightInterface);
        }

        /// <inheritdoc/>
        protected override void OnDeviceAcquisition(UsbLightBase device)
        {
            base.OnDeviceAcquisition(device);

            if(lightInterface is IDeviceRecovery deviceRecovery)
            {
                deviceRecovery.RecoverDevice(device.FeatureName);
            }
        }

        #endregion
    }
}