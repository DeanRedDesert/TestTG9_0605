// -----------------------------------------------------------------------
// <copyright file = "StreamingLightManager.cs" company = "IGT">
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
    /// This class manages light devices thru IStreamingLights interface.
    /// </summary>
    internal class StreamingLightManager : LightManagerBase
    {
        #region Fields

        /// <summary>
        /// The interface for communicating with the CSI Manager to control the streaming light devices.
        /// </summary>
        private IStreamingLights lightInterface;

        /// <summary>
        /// The current light intensity value.
        /// </summary>
        private byte currentLightIntensity = 66;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="StreamingLightManager"/>.
        /// </summary>
        /// <param name="hardwareSpecList">
        /// The list of hardware specifications to be supported by this manager.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="hardwareSpecList"/> contains any non-streaming light specification.
        /// </exception>
        public StreamingLightManager(IList<HardwareSpec> hardwareSpecList)
            : base(hardwareSpecList)
        {
            if(hardwareSpecList.Any(spec => !spec.IsStreamingLight))
            {
                throw new ArgumentException("StreamingLightManager cannot support non-streaming light.",
                                            nameof(hardwareSpecList));
            }
        }

        #endregion

        #region Device Intensity Functions

        /// <summary>
        /// Sets the light intensity for all applicable connected devices.
        /// </summary>
        /// <param name="intensity">The intensity to set the lights to.</param>
        public void SetLightIntensity(byte intensity)
        {
            currentLightIntensity = intensity;

            // Only streaming light devices support adjustable intensity.
            var devices = AllLightObjects.OfType<UsbStreamingLight>();
            foreach(var device in devices)
            {
                device.SetIntensity(intensity);
            }
        }

        /// <summary>
        /// Gets the current light intensity for all applicable connected devices.
        /// </summary>
        /// <returns>The light intensity level for the first group on the first device.</returns>
        public byte GetLightIntensity()
        {
            // Only streaming light devices support adjustable intensity.
            var firstDevice = LightObjects.OfType<UsbStreamingLight>()
                                          .FirstOrDefault(device => device.DeviceAcquired && device.SupportsIntensityControl);

            return firstDevice?.GetIntensity() ?? (byte)0;
        }

        /// <summary>
        /// Handles a streaming light notification event.
        /// </summary>
        /// <param name="eventArgs">The notification event to handle.</param>
        public void HandleStreamingLightNotification(StreamingLightsNotificationEventArgs eventArgs)
        {
            if(LightObjectsByFeatureName.TryGetValue(eventArgs.FeatureId, out var lightObject))
            {
                if(lightObject is UsbStreamingLight streamingLight)
                {
                    streamingLight.RaiseNotificationEvent(eventArgs);
                }
            }
        }

        #endregion

        #region LightManagerBase Overrides

        /// <inheritdoc/>
        protected override DeviceType LightDeviceType => DeviceType.StreamingLight;

        /// <inheritdoc/>
        protected override void ConvertAndSetLightInterface(object lightInterfaceObject)
        {
            switch(lightInterfaceObject)
            {
                case null:
                    lightInterface = null;
                    break;
                case IStreamingLights lights:
                {
                    lightInterface = lights;

                    var devices = AllLightObjects.OfType<UsbStreamingLight>();
                    foreach(var device in devices)
                    {
                        // Reset the interfaces on the devices in case they are different.
                        device.StreamingLightsInterface = lightInterface;
                    }

                    break;
                }

                default:
                    throw new ArgumentException("StreamingLightManager cannot work with interface other than IStreamingLights",
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
            return new UsbStreamingLight(featureName, GetFeatureDescription(featureName), lightInterface);
        }

        /// <inheritdoc/>
        protected override void OnDeviceAcquisition(UsbLightBase device)
        {
            base.OnDeviceAcquisition(device);

            if(device is UsbStreamingLight streamingLight)
            {
                // Make sure the light intensity on the device matches the current level on the machine.
                streamingLight.SetIntensity(currentLightIntensity);
            }
        }

        #endregion

        #region Enable/Disable

        /// <summary>
        /// Enables or disables all <see cref="UsbStreamingLight" />.
        /// </summary>
        /// <param name="hardware">The <see cref="Hardware" /> type of the lights.</param>
        /// <param name="enabled"><code>true</code> if the lights should be enabled; else <code>false</code>.</param>
        /// <returns><code>true</code> if lights were successfully enables or disables; else <code>false</code>.</returns>
        public bool UpdateStreamingLightEnabledFlag(Hardware hardware, bool enabled)
        {
            var updated = false;
            if(LightObjectsByHardware.ContainsKey(hardware))
            {
                if(LightObjectsByHardware[hardware] is UsbStreamingLight light)
                {
                    light.IsEnabled = enabled;
                    updated = true;
                }
            }
            return updated;
        }

        /// <summary>
        /// Clears all the streaming light layers and updates them
        /// </summary>
        public void ClearStreamingLights()
        {
            foreach (var usbLightBase in AllLightObjects)
            {
                var usbLight = (UsbStreamingLight) usbLightBase;
                usbLight.ClearGroups(true);
            }
        }

        #endregion
    }
}