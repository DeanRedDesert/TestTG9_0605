//-----------------------------------------------------------------------
// <copyright file = "HardwareMechanicalReels.cs" company = "IGT">
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
    using Devices.MechanicalReels;
    using MechanicalReels;

    /// <summary>
    /// Hardware implementation of the reels interface.
    /// This class controls the physical peripheral reel devices.
    /// </summary>
    public class HardwareMechanicalReels : IMechanicalReels, IDeviceControl
    {
        #region Private Fields

        private readonly IDeviceManager deviceManager;

        private readonly Dictionary<string, ReelDevice> reelDevices;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="HardwareMechanicalReels"/> class.
        /// Instantiate reel devices based on the device data obtained
        /// from <paramref name="deviceManager"/>.
        /// </summary>
        /// <param name="deviceManager">
        /// The device manager that provides device data and status update.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceManager"/> is null.
        /// </exception>
        public HardwareMechanicalReels(IDeviceManager deviceManager)
            : this(deviceManager, false)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="HardwareMechanicalReels"/> class.
        /// Instantiate reel devices based on the device data obtained
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
        internal HardwareMechanicalReels(IDeviceManager deviceManager, bool bypassHardware)
        {
            this.deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            this.deviceManager.DeviceConnectedEvent += HandleDeviceConnected;
            this.deviceManager.DeviceRemovedEvent += HandleDeviceRemoved;

            reelDevices = new Dictionary<string, ReelDevice>();

            // Get device data list.
            var deviceDataList = this.deviceManager.GetDeviceData(DeviceType.Reel);

            // Instantiate reel devices based on the device data.
            foreach(var deviceData in deviceDataList)
            {
                AddReelDevice(deviceData, bypassHardware);
            }
        }

        #endregion

        #region IMechanicalReels Implementation

        /// <inheritdoc/>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChangedEvent;

        /// <inheritdoc/>
        public ICollection<ReelFeatureDescription> GetReelDevices()
        {
            return (from reelDevice in reelDevices.Values
                    select reelDevice.FeatureDescription).ToList();
        }

        /// <inheritdoc/>
        public ReelCommandResult RequireDevice(string featureId)
        {
            // Do not verify the feature ID here because requiring a device that is not currently
            // connected is allowed.
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("The feature ID cannot be null or empty.", nameof(featureId));
            }

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetOnlineStatus(string featureId, bool online)
        {
            GetDevice(featureId);
            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult SetRecoveryBehavior(string featureId, RecoveryOrder order, ReelDirection direction)
        {
            var reelDevice = GetDevice(featureId);

            reelDevice.RecoveryOrder = order;
            reelDevice.RecoveryDirection = direction;

            return ReelCommandResult.Success;
        }

        /// <inheritdoc/>
        public ReelCommandResult Spin(string featureId, ICollection<SpinProfile> spinProfiles)
        {
            var reelDevice = GetDevice(featureId);
            return reelDevice.Spin(spinProfiles);
        }

        /// <inheritdoc/>
        public ReelCommandResult Stop(string featureId, ICollection<ReelStop> reelStops)
        {
            var reelDevice = GetDevice(featureId);
            return reelDevice.Stop(reelStops);
        }

        /// <inheritdoc/>
        public ReelCommandResult SetStopOrder(string featureId, ICollection<byte> reels)
        {
            var reelDevice = GetDevice(featureId);
            var results = reelDevice.SetStopOrder(reels);

            return results;
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousSpin(string featureId, ushort speedIndex, ICollection<SynchronousSpinProfile> spinProfiles)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.SynchronousSpin((byte)speedIndex, spinProfiles);
        }

        /// <inheritdoc/>
        public ReelCommandResult SetSynchronousStops(string featureId, ICollection<ReelStop> reelStops)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.SetSynchronousStops(reelStops);
        }

        /// <inheritdoc/>
        public ReelCommandResult SynchronousStop(string featureId, ICollection<byte> reels)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.SynchronousStop(reels);
        }

        /// <inheritdoc/>
        public ReelCommandResult SetToPosition(string featureId, ICollection<byte> reelStops, out bool foundationHandlesTiltWhileRecovering)
        {
            var reelDevice = GetDevice(featureId);
            foundationHandlesTiltWhileRecovering = false;

            return reelDevice.SetToPosition(reelStops);
        }

        /// <inheritdoc/>
        public ReelCommandResult ApplyAttributes(string featureId, IDictionary<byte, SpinAttributes> attributes)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.ApplyAttributes(attributes);
        }

        /// <inheritdoc/>
        public ReelCommandResult ChangeSpeed(string featureId, IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.ChangeSpeed(changeSpeedProfiles);
        }

        #endregion

        #region IDeviceControl Members

        /// <inheritdoc/>
        public event EventHandler<DeviceInformationUpdateEventArgs> DeviceInformationUpdateEvent;

        /// <inheritdoc/>
        public bool Reset(string featureId)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.Reset();
        }

        /// <inheritdoc/>
        public bool SelfTest(string featureId)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.SelfTest();
        }

        /// <inheritdoc/>
        public string PollDevice(string featureId)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.PollDevice();
        }

        /// <inheritdoc/>
        public string GetCommonDescriptors(string featureId)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.CommonDescriptors;
        }

        /// <inheritdoc/>
        public string GetFeatureDescriptors(string featureId)
        {
            var reelDevice = GetDevice(featureId);

            return reelDevice.FeatureDescriptors;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Register for the given reel statuses.
        /// </summary>
        /// <param name="statusEvents">The events to register for.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="statusEvents"/> is null.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        /// <devdoc>
        /// This method used to be part of <see cref="IMechanicalReels"/>.
        /// After being removed from the interface, this implementation is kept around
        /// for the sake of easier testing with Ascent Hardware Tester.
        /// </devdoc>
        public ReelCommandResult RegisterForStatus(IEnumerable<ReelStatusEventArgs> statusEvents)
        {
            var results = ReelCommandResult.Success;

            if(statusEvents == null)
            {
                throw new ArgumentNullException(nameof(statusEvents));
            }

            var reelDeviceGroups = from statusEvent in statusEvents
                                   group statusEvent by statusEvent.FeatureId
                                       into deviceGroupings
                                       select new { FeatureId = deviceGroupings.Key, Events = deviceGroupings };

            foreach(var reelDeviceGroup in reelDeviceGroups)
            {
                var reelDevice = GetDevice(reelDeviceGroup.FeatureId);

                results = reelDevice.RegisterForStatus(reelDeviceGroup.Events.ToList());
            }

            return results;
        }

        /// <summary>
        /// Unregister for the given reel statuses.
        /// </summary>
        /// <param name="statusEvents">The events to unregister for.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="statusEvents"/> is null.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        /// <devdoc>
        /// This method used to be part of <see cref="IMechanicalReels"/>.
        /// After being removed from the interface, this implementation is kept around
        /// for the sake of easier testing with Ascent Hardware Tester.
        /// </devdoc>
        public ReelCommandResult UnregisterForStatus(IEnumerable<ReelStatusEventArgs> statusEvents)
        {
            if(statusEvents == null)
            {
                throw new ArgumentNullException(nameof(statusEvents));
            }

            var results = ReelCommandResult.Success;
            var reelDeviceGroups = from statusEvent in statusEvents
                                   group statusEvent by statusEvent.FeatureId
                                       into deviceGroupings
                                       select new { FeatureId = deviceGroupings.Key, Events = deviceGroupings };

            foreach(var reelDeviceGroup in reelDeviceGroups)
            {
                var reelDevice = GetDevice(reelDeviceGroup.FeatureId);
                results = reelDevice.UnregisterForStatus(reelDeviceGroup.Events.ToList());
            }

            return results;
        }

        /// <summary>
        /// Clear status registration.
        /// </summary>
        /// <param name="featureId">The device to clear the status registration for.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/>  is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown if <paramref name="featureId"/> is not valid.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        /// <devdoc>
        /// This method used to be part of <see cref="IMechanicalReels"/>.
        /// After being removed from the interface, this implementation is kept around
        /// for the sake of easier testing with Ascent Hardware Tester.
        /// </devdoc>
        public ReelCommandResult ClearStatusRegistration(string featureId)
        {
            var reelDevice = GetDevice(featureId);
            var results = reelDevice.ClearStatusRegistrations();
            return results;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Create a reel device based on the given device data.
        /// Add it to the reel device list.
        /// </summary>
        /// <param name="deviceData">
        /// The device data for the new light device.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the devices should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        private void AddReelDevice(UsbDeviceData deviceData, bool bypassHardware = false)
        {
            var reelDevice = new ReelDevice(deviceData, bypassHardware);

            reelDevice.DeviceInformationUpdateEvent += PostDeviceInformationUpdateEvent;
            reelDevice.ReelStatusChanged += PostReelStatusChangedEvent;
            reelDevice.ReelsSpunStateChanged += PostReelsSpunStateChangedEvent;

            reelDevices.Add(reelDevice.SubFeatureName, reelDevice);

            deviceManager.RegisterDevice(reelDevice);

            reelDevice.Reset();
        }

        /// <summary>
        /// Get the reel device by the given feature ID.
        /// </summary>
        /// <param name="featureId">
        /// The ID of the feature to get, this corresponds to the device's sub feature name.
        /// </param>
        /// <returns>The reel device for <paramref name="featureId"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        private ReelDevice GetDevice(string featureId)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            if(!reelDevices.ContainsKey(featureId))
            {
                throw new InvalidFeatureIdException(featureId);
            }

            return reelDevices[featureId];
        }

        /// <summary>
        /// Handle Device Connected event from the device manager.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceConnected(object sender, DeviceConnectedEventArgs eventArgs)
        {
            // Only handle events related to reel devices.
            if(eventArgs.DeviceName == DeviceType.Reel)
            {
                AddReelDevice(deviceManager.GetDeviceData(eventArgs.DeviceName, eventArgs.DeviceId));
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
            // Only handle events related to reel devices.
            if(eventArgs.DeviceName == DeviceType.Reel)
            {
                var reelDevice = GetDevice(eventArgs.DeviceId);

                reelDevice.DeviceInformationUpdateEvent -= PostDeviceInformationUpdateEvent;
                reelDevice.ReelStatusChanged -= PostReelStatusChangedEvent;
                reelDevice.ReelsSpunStateChanged -= PostReelsSpunStateChangedEvent;

                deviceManager.UnregisterDevice(reelDevice);

                reelDevices.Remove(eventArgs.DeviceId);
            }
        }

        /// <summary>
        /// Post Device Information Update event when one is received
        /// from the reel devices.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void PostDeviceInformationUpdateEvent(object sender, DeviceInformationUpdateEventArgs eventArgs)
        {
            DeviceInformationUpdateEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Post the 'ReelStatusChanged' event when one is received
        /// from the reel devices.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void PostReelStatusChangedEvent(object sender, ReelStatusEventArgs eventArgs)
        {
            ReelStatusChangedEvent?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Post the 'ReelsSpunStateChanged' event when one is received.
        /// from the reel devices.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void PostReelsSpunStateChangedEvent(object sender, ReelsSpunEventArgs eventArgs)
        {
            ReelsSpunStateChangedEvent?.Invoke(this, eventArgs);
        }

        #endregion

        #endregion
    }
}
