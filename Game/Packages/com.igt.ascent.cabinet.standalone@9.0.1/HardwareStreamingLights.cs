//-----------------------------------------------------------------------
// <copyright file = "HardwareStreamingLights.cs" company = "IGT">
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
    using Devices.StreamingLights;
    using SymbolHighlights;

    /// <summary>
    /// Hardware backed implementation of the streaming lights interface.
    /// </summary>
    public class HardwareStreamingLights : IStreamingLights, ICabinetUpdate
    {
        private readonly IDeviceManager deviceManager;
        private readonly Dictionary<string, StreamingLightDevice> lightDevices;
        private readonly Dictionary<string, byte> intensityLevels;

        private readonly Queue<StreamingLightsNotificationEventArgs> notificationEventQueue =
            new Queue<StreamingLightsNotificationEventArgs>();

        /// <summary>
        /// The default light intensity level.
        /// </summary>
        private const byte DefaultIntensity = 66;

        /// <inheritdoc />
        public byte SupportedLightVersion { get; }

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public HardwareStreamingLights()
        {
            // This needs to match the highest version the SDK supports. The constant in the LightSequence
            // class is not used here because it would create an unnecessary interdependency to the presentation DLL.
            SupportedLightVersion = 3;
        }

        /// <summary>
        /// Construct a new instance using the device manager.
        /// </summary>
        /// <param name="deviceManager">The device manager to utilize.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceManager"/> is null.
        /// </exception>
        public HardwareStreamingLights(IDeviceManager deviceManager)
            : this(deviceManager, false)
        {
        }

        /// <summary>
        /// Construct a new instance using the device manager.
        /// </summary>
        /// <remarks>
        /// This version is added for testing purposes.
        /// Test cases cannot execute hardware related commands.
        /// </remarks>
        /// <param name="deviceManager">The device manager to utilize.</param>
        /// <param name="bypassHardware">
        /// Flag indicating if the devices should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceManager"/> is null.
        /// </exception>
        internal HardwareStreamingLights(IDeviceManager deviceManager, bool bypassHardware)
            : this()
        {
            intensityLevels = new Dictionary<string, byte>();

            this.deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            lightDevices = new Dictionary<string, StreamingLightDevice>();

            var devices = deviceManager.GetDeviceData(DeviceType.StreamingLight);
            foreach(var device in devices)
            {
                AddDevice(device, bypassHardware);
            }

            deviceManager.DeviceConnectedEvent += OnDeviceConnected;
            deviceManager.DeviceRemovedEvent += OnDeviceRemovedEvent;
        }

        #region IStreamingLights Members

        /// <inheritdoc />
        public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        /// <inheritdoc />
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            return from device in lightDevices.Values
                   select device.LightFeatureData;
        }

        /// <inheritdoc />
        public void StartSequenceFile(string featureId, byte groupNumber, string filePath, StreamingLightsPlayMode playMode)
        {
            var device = GetDevice(featureId);
            device.PlayLightSequence(groupNumber, filePath);
        }

        /// <inheritdoc />
        public void StartSequenceFile(string featureId, byte groupNumber, string sequenceName, byte[] sequenceFile, StreamingLightsPlayMode playMode)
        {
            var device = GetDevice(featureId);
            device.PlayLightSequence(groupNumber, sequenceFile, playMode);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> is null.
        /// </exception>
        public void SendFrameChunk(string featureId, byte groupNumber, uint frameCount, byte[] frameData,
            StreamingLightsPlayMode playMode, byte identifier)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            var device = GetDevice(featureId);
            device.PlayChunk(groupNumber, frameCount, frameData, playMode, identifier);
        }

        /// <inheritdoc />
        public void BreakLoop(string featureId, byte groupNumber)
        {
            var device = GetDevice(featureId);
            device.BreakLoop(groupNumber);
        }

        /// <inheritdoc />
        public void SetIntensity(string featureId, byte intensity)
        {
            var device = GetDevice(featureId);
            device.SetIntensity(intensity);
            intensityLevels[featureId] = intensity;
        }

        /// <inheritdoc />
        public byte GetIntensity(string featureId)
        {
            return intensityLevels.ContainsKey(featureId) ?
                intensityLevels[featureId] : DefaultIntensity;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="enabledFeatures"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="enabledFeatures"/> is empty or <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void EnableGivenSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> enabledFeatures)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(enabledFeatures == null)
            {
                throw new ArgumentNullException(nameof(enabledFeatures));
            }

            var featuresToEnable = enabledFeatures.ToList();

            if(featuresToEnable.Count == 0)
            {
                throw new ArgumentException("enabledFeatures cannot be empty.", nameof(enabledFeatures));
            }

            var device = GetDevice(featureId);

            if(device.SupportsSymbolHighlights)
            {
                device.EnableSymbolHighlights(groupNumber, featuresToEnable);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void DisableSymbolHighlights(string featureId, byte groupNumber)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            var device = GetDevice(featureId);

            if(device.SupportsSymbolHighlights)
            {
                device.EnableSymbolHighlights(groupNumber, new List<SymbolHighlightFeature>());
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="trackingData"/> or <paramref name="hotPositionData"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when both <paramref name="trackingData"/> or <paramref name="hotPositionData"/> is empty, or <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void SetSymbolHighlights(string featureId, byte groupNumber, SymbolTrackingData[] trackingData,
            SymbolHotPositionData[] hotPositionData)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(trackingData == null)
            {
                throw new ArgumentNullException(nameof(trackingData));
            }

            if(hotPositionData == null)
            {
                throw new ArgumentNullException(nameof(hotPositionData));
            }

            var tracking = trackingData.ToList();
            var hotPosition = hotPositionData.ToList();

            if(tracking.Count == 0 && hotPosition.Count == 0)
            {
                throw new ArgumentException("Both trackingData and hotPosition cannot be empty. " +
                                            "This method cannot be called with no highlight data.");
            }

            var device = GetDevice(featureId);

            if(device.SupportsSymbolHighlights)
            {
                device.SetSymbolHighlights(groupNumber, tracking, hotPosition);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featuresToClear"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="featuresToClear"/> is empty or <paramref name="featureId"/> is null or empty string.
        /// </exception>
        public void ClearSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(featuresToClear == null)
            {
                throw new ArgumentNullException(nameof(featuresToClear));
            }

            var features = featuresToClear.ToList();

            if(features.Count == 0)
            {
                throw new ArgumentException("featuresToClear cannot be empty.", nameof(featuresToClear));
            }

            var device = GetDevice(featureId);

            if(device.SupportsSymbolHighlights)
            {
                // 0xFF represents clearing all reels for specified feature(s).
                device.ClearSymbolHighlights(groupNumber, features, 0xFF);
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featuresToClear"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="featuresToClear"/> is empty or <paramref name="featureId"/> is null or empty string.
        /// </exception>
        public void ClearSymbolHighlightReel(string featureId, byte groupNumber, int reelIndex, IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(featuresToClear == null)
            {
                throw new ArgumentNullException(nameof(featuresToClear));
            }

            var features = featuresToClear.ToList();

            if(features.Count == 0)
            {
                throw new ArgumentException("featuresToClear cannot be empty.", nameof(featuresToClear));
            }

            var device = GetDevice(featureId);

            if(device.SupportsSymbolHighlights)
            {
                device.ClearSymbolHighlights(groupNumber, features, (byte)reelIndex);
            }
        }

        #endregion

        /// <summary>
        /// Adds a new device to the list of known streaming light devices.
        /// </summary>
        /// <param name="deviceData">
        /// The device information.
        /// </param>
        private void AddDevice(UsbDeviceData deviceData)
        {
            AddDevice(deviceData, false);
        }

        /// <summary>
        /// Adds a new device to the list of known streaming light devices.
        /// </summary>
        /// <param name="deviceData">
        /// The device information.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        private void AddDevice(UsbDeviceData deviceData, bool bypassHardware)
        {
            var newDevice = new StreamingLightDevice(deviceData, bypassHardware);
            deviceManager.RegisterDevice(newDevice);
            newDevice.NotificationEvent += OnDeviceNotificationEvent;

            lightDevices.Add(newDevice.SubFeatureName, newDevice);
        }

        /// <summary>
        /// Get the streaming light device based on the feature ID.
        /// </summary>
        /// <param name="featureId">The feature ID of the device.</param>
        /// <returns>The hardware device for the specified feature ID.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if the feature ID cannot be found.
        /// </exception>
        private StreamingLightDevice GetDevice(string featureId)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            if(!lightDevices.TryGetValue(featureId, out var device))
            {
                throw new InvalidFeatureIdException(featureId);
            }

            return device;
        }

        /// <summary>
        /// Handle Device Removed event from the device manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnDeviceRemovedEvent(object sender, DeviceRemovedEventArgs eventArgs)
        {
            if(eventArgs.DeviceName == DeviceType.StreamingLight)
            {
                var device = GetDevice(eventArgs.DeviceId);
                device.NotificationEvent -= OnDeviceNotificationEvent;
                deviceManager.UnregisterDevice(device);
                lightDevices.Remove(eventArgs.DeviceId);
            }
        }

        /// <summary>
        /// Handle Device Connected event from the device manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnDeviceConnected(object sender, DeviceConnectedEventArgs eventArgs)
        {
            if(eventArgs.DeviceName == DeviceType.StreamingLight)
            {
                AddDevice(deviceManager.GetDeviceData(eventArgs.DeviceName, eventArgs.DeviceId));
            }
        }

        /// <summary>
        /// Handles the device notification event from individual devices and queues them up to be processed
        /// by the presentation thread.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="streamingLightsNotificationEventArgs">The event arguments.</param>
        private void OnDeviceNotificationEvent(object sender, StreamingLightsNotificationEventArgs streamingLightsNotificationEventArgs)
        {
            lock(notificationEventQueue)
            {
                notificationEventQueue.Enqueue(streamingLightsNotificationEventArgs);
            }
        }

        #region Implementation of ICabinetUpdate

        /// <inheritdoc />
        public void Update()
        {
            lock(notificationEventQueue)
            {
                while(notificationEventQueue.Count > 0)
                {
                    var args = notificationEventQueue.Dequeue();
                    NotificationEvent?.Invoke(this, args);
                }
            }
        }

        #endregion
    }
}
