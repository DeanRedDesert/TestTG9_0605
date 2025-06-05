//-----------------------------------------------------------------------
// <copyright file = "HardwarePeripheralLights.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSI.Schemas;
    using Devices;
    using Devices.IgtUsbDevice;
    using Devices.PeripheralLights;

    /// <summary>
    /// Hardware implementation of the lights interface.
    /// This class controls the physical peripheral light devices.
    /// </summary>
    public class HardwarePeripheralLights : IPeripheralLights, IDeviceControl
    {
        #region Private Fields

        private readonly IDeviceManager deviceManager;

        private readonly Dictionary<string, LightDevice> lightDevices;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="HardwarePeripheralLights"/> class.
        /// Instantiate light devices based on the device data obtained
        /// from <paramref name="deviceManager"/>.
        /// </summary>
        /// <param name="deviceManager">
        /// The device manager that provides device data and status update.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceManager"/> is null.
        /// </exception>
        public HardwarePeripheralLights(IDeviceManager deviceManager)
            : this(deviceManager, false)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="HardwarePeripheralLights"/> class.
        /// Instantiate light devices based on the device data obtained
        /// from <paramref name="deviceManager"/>.
        /// </summary>
        /// <remarks>
        /// This version is added for testing purposes.
        /// Test cases cannot execute hardware related commands.
        /// </remarks>
        /// <param name="deviceManager">
        /// The device manager that provides device data and status update.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the devices should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceManager"/> is null.
        /// </exception>
        internal HardwarePeripheralLights(IDeviceManager deviceManager, bool bypassHardware)
        {
            this.deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            this.deviceManager.DeviceConnectedEvent += HandleDeviceConnected;
            this.deviceManager.DeviceRemovedEvent += HandleDeviceRemoved;

            lightDevices = new Dictionary<string, LightDevice>();

            // Get device data list.
            var deviceDataList = this.deviceManager.GetDeviceData(DeviceType.Light);

            // Instantiate light devices based on the device data.
            foreach(var deviceData in deviceDataList)
            {
                AddLightDevice(deviceData, bypassHardware);
            }
        }

        #endregion

        #region IPeripheralLights Members

        #region Device Enumeration and Acquisition

        /// <inheritdoc/>
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            return (from lightDevice in lightDevices.Values
                    select lightDevice.FeatureDescription).ToList();
        }

        /// <inheritdoc/>
        public bool RequiresDeviceAcquisition => true;

        #endregion

        #region Manual Light Control

        /// <inheritdoc/>
        public void TurnOffGroup(string featureId, byte groupNumber, TransitionMode transitionMode)
        {
            var lightDevice = GetDevice(featureId);

            lightDevice.LightsOff(groupNumber, transitionMode);
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId, byte groupNumber, IEnumerable<MonochromeLightState> lightStates)
        {
            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            var lightDevice = GetDevice(featureId);

            lightDevice.RandomMonochrome(groupNumber, lightStates);
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId, byte groupNumber, ushort startingLight, IEnumerable<byte> brightnesses)
        {
            if(brightnesses == null)
            {
                throw new ArgumentNullException(nameof(brightnesses));
            }

            var lightDevice = GetDevice(featureId);

            lightDevice.ConsecutiveMonochrome(groupNumber, startingLight, brightnesses);
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, IEnumerable<RgbLightState> lightStates)
        {
            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            var lightDevice = GetDevice(featureId);

            lightDevice.RandomRgb(groupNumber, lightStates);
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors)
        {
            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            var lightDevice = GetDevice(featureId);

            lightDevice.ConsecutiveRgb(groupNumber, startingLight, colors);
        }

        #endregion

        #region Sequence Control

        /// <inheritdoc/>
        public void StartSequence(string featureId, byte groupNumber, uint sequenceNumber, TransitionMode transitionMode, byte[] parameters)
        {
            var lightDevice = GetDevice(featureId);

            lightDevice.StartSequence(groupNumber, sequenceNumber, transitionMode, parameters);
        }

        /// <inheritdoc/>
        public bool IsSequenceRunning(string featureId, byte groupNumber, uint sequenceNumber)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.IsSequenceRunning(groupNumber, sequenceNumber);
        }

        #endregion

        #region Bitwise Light Control

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<bool> lightStates)
        {
            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            const byte bitsPerLight = 1;
            var byteData = (from state in lightStates select state ? (byte)0x01 : (byte)0x00).ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);

            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<BitwiseLightIntensity> lightIntensities)
        {
            if(lightIntensities == null)
            {
                throw new ArgumentNullException(nameof(lightIntensities));
            }

            const byte bitsPerLight = 2;
            var byteData = lightIntensities.Cast<byte>().ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);

            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<BitwiseLightColor> lightColors)
        {
            if(lightColors == null)
            {
                throw new ArgumentNullException(nameof(lightColors));
            }

            const byte bitsPerLight = 4;
            var byteData = lightColors.Cast<byte>().ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);

            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<Rgb6> lightColors)
        {
            if(lightColors == null)
            {
                throw new ArgumentNullException(nameof(lightColors));
            }

            const byte bitsPerLight = 6;
            var byteData = (from color in lightColors select color.PackedColor).ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);

            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        byte bitsPerLight,
                                        byte[] lightData)
        {
            if(lightData == null)
            {
                throw new ArgumentNullException(nameof(lightData));
            }

            var lightDevice = GetDevice(featureId);

            lightDevice.BitWiseControl(groupNumber, startingLight, bitsPerLight, lightData);
        }

        #endregion

        #endregion

        #region IDeviceControl Members

        /// <inheritdoc/>
        public event EventHandler<DeviceInformationUpdateEventArgs> DeviceInformationUpdateEvent;

        /// <inheritdoc/>
        public bool Reset(string featureId)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.Reset();
        }

        /// <inheritdoc/>
        public bool SelfTest(string featureId)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.SelfTest();
        }

        /// <inheritdoc/>
        public string PollDevice(string featureId)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.PollDevice();
        }

        /// <inheritdoc/>
        public string GetCommonDescriptors(string featureId)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.CommonDescriptors;
        }

        /// <inheritdoc/>
        public string GetFeatureDescriptors(string featureId)
        {
            var lightDevice = GetDevice(featureId);

            return lightDevice.FeatureDescriptors;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a light device based on the given device data.
        /// Add it to the light device list.
        /// </summary>
        /// <param name="deviceData">
        /// The device data for the new light device.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        private void AddLightDevice(UsbDeviceData deviceData, bool bypassHardware = false)
        {
            var lightDevice = new LightDevice(deviceData, bypassHardware);

            lightDevice.DeviceInformationUpdateEvent += PostDeviceInformationUpdateEvent;

            lightDevices.Add(lightDevice.SubFeatureName, lightDevice);

            deviceManager.RegisterDevice(lightDevice);

            lightDevice.Reset();
        }

        /// <summary>
        /// Get the light device by the given feature ID.
        /// </summary>
        /// <param name="featureId">
        /// The ID of the feature to get, this corresponds to the device's sub feature name.
        /// </param>
        /// <returns>The light device for <paramref name="featureId"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        private LightDevice GetDevice(string featureId)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            if(!lightDevices.ContainsKey(featureId))
            {
                throw new InvalidFeatureIdException(featureId);
            }

            return lightDevices[featureId];
        }

        /// <summary>
        /// Handle Device Connected event from the device manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceConnected(object sender, DeviceConnectedEventArgs eventArgs)
        {
            // Only handle events related to light devices.
            if(eventArgs.DeviceName == DeviceType.Light)
            {
                AddLightDevice(deviceManager.GetDeviceData(eventArgs.DeviceName, eventArgs.DeviceId));
            }
        }

        /// <summary>
        /// Handle Device Removed event from the device manager.
        /// Release the device if the removed one is acquired by the game.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs eventArgs)
        {
            // Only handle events related to light devices.
            if(eventArgs.DeviceName == DeviceType.Light)
            {
                var lightDevice = GetDevice(eventArgs.DeviceId);

                lightDevice.DeviceInformationUpdateEvent -= PostDeviceInformationUpdateEvent;

                deviceManager.UnregisterDevice(lightDevice);

                lightDevices.Remove(eventArgs.DeviceId);
            }
        }

        /// <summary>
        /// Post Device Information Update event when one is received
        /// from the light devices.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void PostDeviceInformationUpdateEvent(object sender, DeviceInformationUpdateEventArgs eventArgs)
        {
            DeviceInformationUpdateEvent?.Invoke(this, eventArgs);
        }

        #endregion

        #endregion
    }
}
