//-----------------------------------------------------------------------
// <copyright file = "ReelDevice.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using Cabinet.MechanicalReels;
    using CompactSerialization;
    using CSI.Schemas;
    using IgtUsbDevice;

    /// <summary>
    /// This class encapsulates the data and operations
    /// to control a physical mechanical reel device.
    /// This reel device may support multiple reels.
    /// </summary>
    [Serializable]
    public class ReelDevice : DeviceBase
    {
        #region Nested Class

        /// <summary>
        /// This enumeration defines the operational states
        /// of the reel device.
        /// </summary>
        private enum OperationalState
        {
            PowerUp,
            TiltDetected,
            Tilted,
            RecoveryReady,
            RecoveryInitiated,
            RecoveryInProgress,
            Playable
        }

        #endregion

        #region Constants

        /// <summary>
        /// Constant which indicates all reels are to be controlled.
        /// </summary>
        private const byte AllReels = 0xFF;

        /// <summary>
        /// Size of the feature descriptor header.
        /// 
        /// The header of a reel device's feature specific descriptor
        /// consists of the following fields:
        ///     <list type="number">
        ///         <item>Total length of the feature specific data (1 byte).</item>
        ///         <item>Descriptor type (1 byte).</item>
        ///         <item>Length of descriptor data for each reel (1 byte).</item>
        ///     </list>
        /// </summary>
        private const int FeatureDescriptorHeaderSize = 3;

        #endregion

        #region Events

        /// <summary>
        /// Event posted whenever a registered reel status changes.
        /// </summary>
        public event EventHandler<ReelStatusEventArgs> ReelStatusChanged;

        /// <summary>
        /// Event posted whenever the currently spinning reels go through a state change of type <see cref="ReelsSpunEventArgs"/>.
        /// </summary>
        public event EventHandler<ReelsSpunEventArgs> ReelsSpunStateChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Get the device type.
        /// </summary>
        public DeviceType Type { get; private set; }

        /// <summary>
        /// Get the <see cref="ReelFeatureDescription"/>
        /// for this reel device.
        /// </summary>
        public ReelFeatureDescription FeatureDescription
        {
            get
            {
                return new ReelFeatureDescription(
                    SubFeatureName,
                    reelSubFeature,
                    new List<ushort>(supportedSpeeds),
                    new List<ushort>(supportedFinalDecelProfiles),
                    new List<ReelAccelerationDecelerationTime>(supportedAccelDecelProfiles), 
                    reelUnits.Select(reelUnit => new ReelDescription(reelUnit.Descriptor.NumberOfStops,
                                                                     reelUnit.Descriptor.MaximumSeekTime)));
            }
        }

        /// <inheritdoc/>
        public override string FeatureDescriptors
        {
            get
            {
                string result;

                if(reelCount > 0)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Reel Specific Descriptors -");

                    foreach(var reelUnit in reelUnits)
                    {
                        stringBuilder.AppendLine(
                            $"Reel #{reelUnit.ReelNumber}    {reelUnit.Descriptor}");
                    }

                    result = stringBuilder.ToString();
                }
                else
                {
                    result = "None available.";
                }

                return result;
            }
        }

        /// <summary>
        /// Set the order in which the reels should recover.
        /// </summary>
        public RecoveryOrder RecoveryOrder { private get; set; }

        /// <summary>
        /// Set the direction in which the reels should spin while recovering.
        /// </summary>
        public ReelDirection RecoveryDirection { private get; set; }

        /// <summary>
        /// Get the time, in milliseconds, between each reel to start recovering.
        /// </summary>
        /// <remarks>
        /// It is currently hard coded as the value used in legacy AVP.
        /// 
        /// The time needs to account for the worst case time for a complete spin
        /// (acceleration + constant speed + deceleration). The acceleration time
        /// is 100ms, the time for a spin is 440ms, and the time to decelerate is
        /// 100ms, so the first reel needs to spin for at least 640ms to guarantee a
        /// complete revolution.
        /// </remarks>
        private static ushort RecoveryDelta => 640;

        #endregion

        #region Fields

        /// <summary>
        /// The sub feature of the reel device.
        /// </summary>
        private readonly ReelSubFeature reelSubFeature;

        /// <summary>
        /// The list of reels controlled by this reel device.
        /// </summary>
        private readonly List<ReelUnit> reelUnits;

        /// <summary>
        /// Number of reels on this device.
        /// </summary>
        private readonly int reelCount;

        /// <summary>
        /// Speeds supported by the reel device.
        /// </summary>
        private readonly List<ushort> supportedSpeeds;

        /// <summary>
        /// Deceleration profiles supported by the reel device.
        /// </summary>
        private readonly List<ushort> supportedFinalDecelProfiles;

        /// <summary>
        /// Acceleration/Deceleration profiles supported by the reel device for CSI reel category 1.5+.
        /// </summary>
        private readonly List<ReelAccelerationDecelerationTime> supportedAccelDecelProfiles;

        /// <summary>
        /// This is an internal cache used by <see cref="ReelCommandVerifier"/> methods
        /// to verify command arguments, assuming that <see cref="ReelCommandVerifier"/>
        /// never modifies the feature description passed in.
        /// </summary>
        private readonly ReelFeatureDescription featureDescription;

        /// <summary>
        /// Current state of the set of reels spun.
        /// </summary>
        private ReelsSpunState currrentReelsSpunState;

        /// <summary>
        /// The functional state of the reel device.
        /// </summary>
        private OperationalState operationalState;

        /// <summary>
        /// The flag indicating whether it is time to
        /// clear all tilts for the reel device.
        /// </summary>
        private volatile bool tickClearTilts;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="ReelDevice"/>
        /// based on the given device data.
        /// </summary>
        /// <param name="deviceData">
        /// The device data for the reel device.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="deviceData"/> contains invalid data.
        /// </exception>
        public ReelDevice(UsbDeviceData deviceData)
            : this(deviceData, false)
        {
        }

        /// <summary>
        /// Initialize an instance of <see cref="ReelDevice"/>
        /// based on the given device data.
        /// </summary>
        /// <param name="deviceData">
        /// The device data for the reel device.
        /// </param>
        /// <param name="bypassHardware">
        /// Flag indicating if the device should bypass any operation
        /// that requires the hardware.
        /// Used for testing purposes only.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="deviceData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="deviceData"/> contains invalid data.
        /// </exception>
        public ReelDevice(UsbDeviceData deviceData, bool bypassHardware)
            : base(deviceData, bypassHardware)
        {
            if(deviceData == null)
            {
                throw new ArgumentNullException(nameof(deviceData));
            }

            Type = DeviceType.Reel;

            reelSubFeature = (ReelSubFeature)deviceData.FunctionalDescriptor.SubFeatureNumber;

            // There should be only 1 feature specific descriptor to parse.
            if(deviceData.FunctionalDescriptor.NumberAdditionalDescriptors != 1)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect number of feature descriptors {deviceData.FunctionalDescriptor.NumberAdditionalDescriptors} for the reel device.  Should be 1.");
            }

            // Create list of reels based on the reel descriptors.
            var reelDescriptors = ParseFeatureDescriptors(deviceData.FeatureDescriptorData);
            reelCount = reelDescriptors.Count;

            reelUnits = reelDescriptors.Select((descriptor, index) => new ReelUnit((byte)index, descriptor)).ToList();

            // Query the information on the reel device.
            supportedSpeeds = bypassHardware ? new List<ushort> { 0 } : QuerySpeeds();
            supportedFinalDecelProfiles = bypassHardware ? new List<ushort> { 0 } : QueryDecelerationProfiles();
            supportedAccelDecelProfiles = bypassHardware ? new List<ReelAccelerationDecelerationTime> {
                                                                        new ReelAccelerationDecelerationTime
                                                                            {
                                                                                AccelerationTimeToNextSpeed = 1,
                                                                                DecelerationTimeToPreviousSpeed = 2
                                                                            }} 
                                                        : QueryAccelDeceleProfiles();

            // Store a cache of the feature description for
            // speeding up command verifications.
            featureDescription = FeatureDescription;

            // The status of all reels, used for checking whether to
            // post the ReelsSpunStateChanged event.
            currrentReelsSpunState = ComputeReelsSpinningState();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Register for the given reel statuses on this reel device.
        /// </summary>
        /// <param name="statusEvents">The events to register for.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="statusEvents"/> is null.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult RegisterForStatus(IEnumerable<ReelStatusEventArgs> statusEvents)
        {
            if(statusEvents == null)
            {
                throw new ArgumentNullException(nameof(statusEvents));
            }

            foreach(var statusEvent in statusEvents)
            {
                ReelCommandVerifier.VerifyReelIndex(featureDescription, statusEvent.ReelNumber);
                reelUnits[statusEvent.ReelNumber].StatusRegistrations.Add(statusEvent.Status);
            }

            return ReelCommandResult.Success;
        }

        /// <summary>
        /// Unregister for the given reel statuses on this reel device.
        /// </summary>
        /// <param name="statusEvents">The events to unregister for.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="statusEvents"/> is null.
        /// </exception>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult UnregisterForStatus(IEnumerable<ReelStatusEventArgs> statusEvents)
        {
            if(statusEvents == null)
            {
                throw new ArgumentNullException(nameof(statusEvents));
            }

            foreach(var statusEvent in statusEvents)
            {
                ReelCommandVerifier.VerifyReelIndex(featureDescription, statusEvent.ReelNumber);
                reelUnits[statusEvent.ReelNumber].StatusRegistrations.Remove(statusEvent.Status);
            }

            return ReelCommandResult.Success;
        }

        /// <summary>
        /// Clear all of the status registrations for this reel device.
        /// </summary>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult ClearStatusRegistrations()
        {
            foreach(var reelUnit in reelUnits)
            {
                reelUnit.StatusRegistrations.Clear();
            }

            return ReelCommandResult.Success;
        }

        /// <summary>
        /// Instruct the reels specified by <paramref name="spinProfiles"/> to spin.
        /// The parameters and attributes in the profile will be applied to the spin.
        /// This type of spin does not support synchronous stops (slamming).
        /// </summary>
        /// <param name="spinProfiles">
        /// List of <see cref="SpinProfile"/>, which defines characteristics of the spin.
        /// </param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult Spin(ICollection<SpinProfile> spinProfiles)
        {
            ReelCommandVerifier.VerifySpin(featureDescription, spinProfiles);

            var result = false;

            foreach(var spinProfile in spinProfiles)
            {
                var reelNumber = spinProfile.ReelNumber;

                // Update spin profile for the reel.
                result = SetDeceleration(reelNumber, spinProfile.Deceleration);
                result = result && SetSpeed(reelNumber, spinProfile.Speed);
                result = result && SetDuration(reelNumber, spinProfile.Duration);
                result = result && SetDirection(reelNumber, spinProfile.Direction);
                result = result && SetStop(reelNumber, spinProfile.Stop);
                result = result && SetAttributes(reelNumber, spinProfile.Attributes);

                // Spin the reel.
                result = result && SpinReel(reelNumber);

                if(!result)
                {
                    break;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.SpinFailed;
        }

        /// <summary>
        /// Stop the specified reels at the specified stops as soon as possible using the
        /// configuration settings that apply.
        /// This allows shortening the specified spin time or specifying a stop not known
        /// at spin time.  It does not allow changing a previously specified stop.
        /// </summary>
        /// <param name="reelStops">A list of reels and where to stop them.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult Stop(ICollection<ReelStop> reelStops)
        {
            ReelCommandVerifier.VerifyStops(FeatureDescription, reelStops);

            var result = false;

            foreach(var reelStop in reelStops)
            {
                var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.Stop,
                                                              reelStop.ReelNumber,
                                                              reelStop.Stop);
                result = SendFeatureCommand(payload);

                if(!result)
                {
                    break;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.Failed;
        }

        /// <summary>
        /// Force specified reels to stop in a particular order, regardless of other factors.
        /// This stop order persists for all subsequent spins until the order is removed
        /// by sending an empty collection for the <paramref name="reels"/> parameter.
        /// </summary>
        /// <param name="reels">
        /// A list of reel numbers indicating the order the reels should stop.
        /// An empty collection removes any previously set stop order.
        /// </param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult SetStopOrder(ICollection<byte> reels)
        {
            ReelCommandVerifier.VerifySetStopOrder(FeatureDescription, reels);

            // This command ignores target device id field.
            var payload = new UsbCommandPayload((byte)ReelCommandCode.SetStopOrder, 0);

            SendFeatureCommand(payload, reels.ToArray());

            return ReelCommandResult.Success;
        }

        /// <summary>
        /// Spin two or more reels using the specified speed and profiles.
        /// This is used to synchronize the reels so that they can stop at
        /// their respective stop positions at the same time.
        /// </summary>
        /// <param name="speedIndex">
        /// The speed to use. This is an index into the supported list of speeds
        /// returned by <see cref="QuerySpeeds"/>.
        /// </param>
        /// <param name="syncSpinProfiles">
        /// List of <see cref="SynchronousSpinProfile"/> to use for the synchronous spin.
        /// </param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult SynchronousSpin(byte speedIndex, ICollection<SynchronousSpinProfile> syncSpinProfiles)
        {
            ReelCommandVerifier.VerifySynchronousSpin(FeatureDescription, speedIndex, syncSpinProfiles);

            // Build the additional data buffer.
            byte[] data;
            using(var stream = new MemoryStream())
            {
                foreach(var syncSpinProfile in syncSpinProfiles)
                {
                    (syncSpinProfile as ICompactSerializable).Serialize(stream);
                }

                data = stream.ToArray();
            }

            // This command ignores target device id field.
            var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SynchronousSpin,
                                                          0,
                                                          speedIndex);

            var result = SendFeatureCommand(payload, data);

            // Update the reel's cached spin profile.
            if(result)
            {
                foreach(var syncSpinProfile in syncSpinProfiles)
                {
                    var spinProfile = reelUnits[syncSpinProfile.ReelNumber].SpinProfile;

                    spinProfile.Speed = speedIndex;
                    spinProfile.Duration = syncSpinProfile.Duration;
                    spinProfile.Direction = syncSpinProfile.Direction;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.Failed;
        }

        /// <summary>
        /// Assign a stop position for a reel previously spun using the <see cref="SynchronousSpin"/>
        /// method with a stop of <see cref="SpinProfile.NoStop"/>.
        /// </summary>
        /// <param name="reelStops">A list of reels and where to stop them.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult SetSynchronousStops(ICollection<ReelStop> reelStops)
        {
            ReelCommandVerifier.VerifyStops(FeatureDescription, reelStops);

            // Build the additional data buffer.
            byte[] data;
            using(var stream = new MemoryStream())
            {
                foreach(var reelStop in reelStops)
                {
                    (reelStop as ICompactSerializable).Serialize(stream);
                }

                data = stream.ToArray();
            }

            // This command ignores target device id field.
            var payload = new UsbCommandPayload((byte)ReelCommandCode.SetSynchronousStops, 0);

            var result = SendFeatureCommand(payload, data);

            return result ? ReelCommandResult.Success : ReelCommandResult.Failed;
        }

        /// <summary>
        /// Stop the specified reels spun using the <see cref="SynchronousSpin"/>
        /// method as soon as possible.
        /// The stops must have been set either by <see cref="SynchronousSpin"/>
        /// or <see cref="SetSynchronousStops"/>; If a stop was not set then the
        /// request to stop will be ignored.
        /// </summary>
        /// <param name="reels">
        /// A list of reel numbers indicating which reels to stop.
        /// </param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult SynchronousStop(ICollection<byte> reels)
        {
            ReelCommandVerifier.VerifySynchronousStop(featureDescription, reels);

            // This command ignores target device id field.
            var payload = new UsbCommandPayload((byte)ReelCommandCode.SynchronousStop, 0);

            var result = SendFeatureCommand(payload, reels.ToArray());

            return result ? ReelCommandResult.Success : ReelCommandResult.Failed;
        }

        /// <summary>
        /// Set all reels to a specified reel stop, in the fastest and shortest manner.
        /// </summary>
        /// <param name="reelStops">An collection of reel stop indexes.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult SetToPosition(ICollection<byte> reelStops)
        {
            ReelCommandVerifier.VerifyStops(featureDescription, reelStops);

            var result = false;

            var spinProfiles = reelStops.Select((reelStop, index) => new SpinProfile((byte)index, reelStop, 0, ReelDirection.Shortest)).ToList();

            foreach(var spinProfile in spinProfiles)
            {
                var reelNumber = spinProfile.ReelNumber;

                // Update spin profile for the reel.
                result = SetDeceleration(reelNumber, 0);
                result = result && SetSpeed(reelNumber, 0);
                result = result && SetDuration(reelNumber, 0);
                result = result && SetDirection(reelNumber, ReelDirection.Shortest);
                result = result && SetStop(reelNumber, spinProfile.Stop);
                result = result && SetAttributes(reelNumber, new SpinAttributes());

                // Spin the reel.
                result = result && SpinReel(reelNumber);

                if(!result)
                {
                    break;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.SpinFailed;
        }

        /// <summary>
        /// Applies attributes to each reel specified. This is normally done outside of a spin.
        /// </summary>
        /// <param name="attributes">A dictionary of reel index vs. <see cref="SpinAttributes"/> to apply to that reel.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of the command.</returns>
        public ReelCommandResult ApplyAttributes(IDictionary<byte, SpinAttributes> attributes)
        {
            var result = false;

            foreach(var reelIndex in attributes.Keys)
            {
                ReelCommandVerifier.VerifyReelIndex(featureDescription, reelIndex);
            }

            foreach(var attributeKvp in attributes)
            {
                var reelNumber = attributeKvp.Key;

                result = SetAttributes(reelNumber, attributeKvp.Value);
                result = result && ApplyAttributes(reelNumber);

                if(!result)
                {
                    break;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.SpinFailed;
        }

        /// <summary>
        /// Change the speed and/or direction of a set of spinning reels.
        /// </summary>
        /// <param name="changeSpeedProfiles">
        /// The dictionary of reel indexes vs. an object of type <see cref="ChangeSpeedProfile"/>.
        /// </param>
        /// <returns>The <see cref="ReelCommandResult"/>result of the command.</returns>
        public ReelCommandResult ChangeSpeed(IDictionary<byte, ChangeSpeedProfile> changeSpeedProfiles)
        {
            var result = false;

            foreach(var changeSpeedProfile in changeSpeedProfiles)
            {
                var payload = new ReelChangeSpeedCommandPayload(changeSpeedProfile.Key, changeSpeedProfile.Value);

                result = SendFeatureCommand(payload);

                if(!result)
                {
                    break;
                }
            }

            return result ? ReelCommandResult.Success : ReelCommandResult.ChangeSpeedResultedInOneOrMoreReelErrors;
        }

        #endregion

        #region Override Base Methods

        /// <inheritdoc />
        internal override void HandleStatus(UsbStatusMessage statusMessage, StringBuilder infoBuilder)
        {
            if(statusMessage == null)
            {
                throw new ArgumentNullException(nameof(statusMessage));
            }

            base.HandleStatus(statusMessage, infoBuilder);

            // If it is time to clear tilts, do so.
            if(tickClearTilts)
            {
                tickClearTilts = false;
                ClearAllStatuses();

                infoBuilder.AppendLine("Auto cleared all statuses.");
            }

            var oldOperationalState = operationalState;

            // Handle the status, which might cause an operational state change.
            foreach(var statusRecord in statusMessage.StatusRecords)
            {
                // Handle global statuses.
                if(statusRecord.IsGlobalStatus)
                {
                    var status = (GlobalStatusCode)statusRecord.Status;

                    switch(status)
                    {
                        case GlobalStatusCode.Normal:
                            if(operationalState == OperationalState.PowerUp ||
                               operationalState == OperationalState.Tilted)
                            {
                                operationalState = OperationalState.RecoveryReady;
                            }
                            break;

                        case GlobalStatusCode.CommunicationTimedOut:
                            ClearStatus(statusRecord.Status);
                            if(operationalState != OperationalState.Tilted)
                            {
                                operationalState = OperationalState.TiltDetected;
                            }
                            break;

                        case GlobalStatusCode.SelfTestExited:
                            ClearStatus(statusRecord.Status);
                            break;
                    }
                }
                // Handle reel specific statuses.
                else
                {
                    HandleReelStatus(statusRecord, infoBuilder);
                }
            }

            // Log the operational state change.
            if(operationalState != oldOperationalState)
            {
                infoBuilder.AppendLine($"Operational State: {oldOperationalState} -> {operationalState}");
            }

            oldOperationalState = operationalState;

            // Process the operational state.
            switch(operationalState)
            {
                case OperationalState.TiltDetected:
                    // Put all reels in slow spin.
                    SlowSpin(AllReels);

                    // Auto clear the tilts after a couple of seconds.
                    StartAutoClearing(2000);

                    operationalState = OperationalState.Tilted;

                    // TODO: Post tilt when reels tilt.
                    // Something to consider when standalone tilt manager is in place.

                    // TODO: Wait for user intervention to clear the tilts instead of auto clearing?
                    // Something to consider when a class is in place
                    // to support the interaction with the user.
                    // Standard behaviors:
                    //     Halt reels when reset switch is turned,
                    //     Clear tilts when main door is closed.
                    break;

                case OperationalState.RecoveryReady:
                    // Start recovery spin when it is ready.
                    // The reel status is Halt followed by Stopped after power up, SlowSpin after tilts.
                    // Checking reel statuses first helps avoid possible command rejections due to device busy.
                    if(reelUnits.All(reelUnit => reelUnit.StatusCode == ReelStatusCode.Stopped ||
                                                 reelUnit.StatusCode == ReelStatusCode.SlowSpin))
                    {
                        RecoverySpin();
                        operationalState = OperationalState.RecoveryInitiated;
                    }
                    break;

                case OperationalState.RecoveryInitiated:
                    // Check if all reels have started recovery spin.
                    if(reelUnits.All(reelUnit => reelUnit.StatusCode != ReelStatusCode.Stopped &&
                                                 reelUnit.StatusCode != ReelStatusCode.SlowSpin))
                    {
                        operationalState = OperationalState.RecoveryInProgress;
                    }
                    break;

                case OperationalState.RecoveryInProgress:
                    // Check if recovery spin is complete.
                    if(reelUnits.All(reelUnit => reelUnit.StatusCode == ReelStatusCode.Stopped))
                    {
                        operationalState = OperationalState.Playable;
                    }
                    break;
            }

            // Log the operational state change.
            if(operationalState != oldOperationalState)
            {
                infoBuilder.AppendLine($"Operational State: {oldOperationalState} -> {operationalState}");
            }
        }

        #endregion

        #region Private Methods

        #region Initialization

        /// <summary>
        /// Parse the byte array of feature descriptor data of the reel device.
        /// </summary>
        /// <param name="featureDescriptorData">The byte array to parse.</param>
        /// <returns>List of reel descriptors, each for a reel.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureDescriptorData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="featureDescriptorData"/> contains invalid data.
        /// </exception>
        private static List<ReelDescriptor> ParseFeatureDescriptors(byte[] featureDescriptorData)
        {
            if(featureDescriptorData == null)
            {
                throw new ArgumentNullException(nameof(featureDescriptorData));
            }

            // Calculate how many reel descriptors are defined.
            var descriptorCount = ComputeDescriptorCount(featureDescriptorData);

            // The length of each reel descriptor is defined in the header.
            var expectedLength = featureDescriptorData[2];

            // Data for reel descriptors starts after the header.
            var offset = FeatureDescriptorHeaderSize;

            // Parse the reel descriptors.
            var result = new List<ReelDescriptor>(descriptorCount);

            for(var descriptorIndex = 0; descriptorIndex < descriptorCount; descriptorIndex++)
            {
                var reelDescriptor = new ReelDescriptor();
                reelDescriptor.Unpack(featureDescriptorData, offset);

                offset += reelDescriptor.DataSize;

                // Use the first list element to validate the size.
                if(descriptorIndex == 0 && reelDescriptor.DataSize != expectedLength)
                {
                    throw new InvalidUsbDeviceDataException("Mismatched reel descriptor length.");
                }

                result.Add(reelDescriptor);
            }

            return result;
        }

        /// <summary>
        /// Calculate the number of descriptors from the feature descriptor header.
        /// Validate the data as needed.
        /// </summary>
        /// <param name="featureDescriptorData">Byte array for feature specific data.</param>
        /// <returns>The number of groups in the light device.</returns>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when the data in <paramref name="featureDescriptorData"/> contains invalid data.
        /// </exception>
        private static int ComputeDescriptorCount(byte[] featureDescriptorData)
        {
            if(featureDescriptorData.Length < FeatureDescriptorHeaderSize)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Length of feature descriptor data {featureDescriptorData.Length} is too small.");
            }

            var totalLength = featureDescriptorData[0];
            var desciptorType = featureDescriptorData[1];
            var reelDescriptorLength = featureDescriptorData[2];

            if(totalLength <= FeatureDescriptorHeaderSize)
            {
                throw new InvalidUsbDeviceDataException(
                    $"The length of feature descriptor {totalLength} is invalid.");
            }

            if(desciptorType != (byte)UsbDescriptorType.FeatureSpecific)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect descriptor type.  Expected 0x{UsbDescriptorType.FeatureSpecific:X}, but got 0x{desciptorType:X}");
            }

            if(reelDescriptorLength <= 0)
            {
                throw new InvalidUsbDeviceDataException(
                    $"The length of reel descriptor {reelDescriptorLength} is invalid.");
            }

            return (totalLength - FeatureDescriptorHeaderSize) / reelDescriptorLength;
        }

        /// <summary>
        /// Start the timer thread for auto clearing tilt statuses.
        /// </summary>
        /// <param name="delay">
        /// The time, in milliseconds, to wait before auto clearing tilt statuses.
        /// </param>
        private void StartAutoClearing(int delay)
        {
            var delayedAutoClearing = new Thread(() =>
            {
                Thread.Sleep(delay);
                tickClearTilts = true;

                // Request an active polling, otherwise
                // we never get a chance to clear the tilts.
                OnRequestPolling();
            });

            delayedAutoClearing.Start();
        }

        /// <summary>
        /// Recover the reel device to the last valid reel stops.
        /// </summary>
        /// <remarks>
        /// This method is not power hit tolerant, since standalone cabinet
        /// does not have access to critical data.
        /// This method assumes 0 as the cold start stop positions, since it
        /// has no access to the paytable.
        /// </remarks>
        private void RecoverySpin()
        {
            const byte coldStartStop = 0;

            var spinProfiles = from reelUnit in reelUnits
                               let reelStop = reelUnit.SpinProfile.Stop == SpinProfile.NoStop
                                                   ? coldStartStop
                                                   : reelUnit.SpinProfile.Stop
                               let stopOrder = RecoveryOrder == RecoveryOrder.Ascending
                                                   ? (int)reelUnit.ReelNumber
                                                   : reelCount - reelUnit.ReelNumber - 1
                               select new SpinProfile
                               {
                                   ReelNumber = reelUnit.ReelNumber,
                                   Stop = reelStop,
                                   Duration = (ushort)((stopOrder + 1) * RecoveryDelta),
                                   Direction = RecoveryDirection
                               };

            Spin(spinProfiles.ToList());
        }

        #endregion

        #region  Querying Information

        /// <summary>
        /// Get speeds supported by the reel.
        /// </summary>
        /// <returns>List of supported speeds.</returns>
        private List<ushort> QuerySpeeds()
        {
            var result = new List<ushort>();

            var data = QueryFeature((byte)ReelQueryCode.GetSpeeds);

            // Return empty list if QueryFeature failed.
            if(data != null)
            {
                for(var offset = 0; offset < data.Length; offset += sizeof(ushort))
                {
                    result.Add(BitConverter.ToUInt16(data, offset));
                }
            }

            return result;
        }

        /// <summary>
        /// Get deceleration profiles supported by the reel.
        /// </summary>
        /// <returns>List of supported deceleration profiles.</returns>

        private List<ushort> QueryDecelerationProfiles()
        {
            var result = new List<ushort>();

            var data = QueryFeature((byte)ReelQueryCode.GetDecelerationProfiles);

            // Return empty list if QueryFeature failed.
            if(data != null)
            {
                for(var offset = 0; offset < data.Length; offset += sizeof(ushort))
                {
                    result.Add(BitConverter.ToUInt16(data, offset));
                }
            }

            return result;
        }

        /// <summary>
        /// Get the accel / decel profiles supported by the reel (CSI category ver. 1.5+.)
        /// </summary>
        /// <returns>List of supported accel / decel profiles.</returns>
        private List<ReelAccelerationDecelerationTime> QueryAccelDeceleProfiles()
        {
            var result = new List<ReelAccelerationDecelerationTime>();

            var data = QueryFeature((byte)ReelQueryCode.GetAccelDecelProfiles);

            // Return empty list if QueryFeature failed.
            if(data != null)
            {
                for(var offset = 0; offset < data.Length; offset += 2 * sizeof(ushort))
                {
                    result.Add(new ReelAccelerationDecelerationTime
                        {
                            AccelerationTimeToNextSpeed = BitConverter.ToUInt16(data, offset),
                            DecelerationTimeToPreviousSpeed = BitConverter.ToUInt16(data, offset + sizeof(ushort))
                        }
                    );
                }
            }

            return result;
        }

        #endregion

        #region Commanding a Reel

        // All methods in this section assume that arguments passed in are valid.

        /// <summary>
        /// Select the deceleration profile for the specified reel.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="decelerationIndex">The index of the desired deceleration profile.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetDeceleration(byte reelNumber, ushort decelerationIndex)
        {
            var result = true;

            // Only send the command if the new value is different from the current one.
            var reelUnit = reelUnits[reelNumber];
            if(reelUnit.SpinProfile.Deceleration != decelerationIndex)
            {
                var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SetDeceleration,
                                                              reelNumber,
                                                              (byte)decelerationIndex);
                result = SendFeatureCommand(payload);

                // Update the current value.
                if(result)
                {
                    reelUnit.SpinProfile.Deceleration = decelerationIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Set the speed for the specified reel.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="speedIndex">The index of the desired speed record.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetSpeed(byte reelNumber, byte speedIndex)
        {
            var result = true;

            // Only send the command if the new value is different from the current one.
            var reelUnit = reelUnits[reelNumber];
            if(reelUnit.SpinProfile.Speed != speedIndex)
            {
                var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SetSpeed,
                                                              reelNumber,
                                                              speedIndex);
                result = SendFeatureCommand(payload);

                // Update the current value.
                if(result)
                {
                    reelUnit.SpinProfile.Speed = speedIndex;
                }
            }

            return result;
        }

        /// <summary>
        /// Set the spin duration (acceleration + constant speed) for the specified reel.
        /// as close as possible to <paramref name="milliseconds"/>.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="milliseconds">The desired duration, in milliseconds.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetDuration(byte reelNumber, ushort milliseconds)
        {
            var result = true;

            // Only send the command if the new value is different from the current one.
            var reelUnit = reelUnits[reelNumber];
            if(reelUnit.SpinProfile.Duration != milliseconds)
            {

                var payload = new UsbSingleUshortCommandPayload((byte)ReelCommandCode.SetDuration,
                                                                reelNumber,
                                                                milliseconds);
                result = SendFeatureCommand(payload);

                // Update the current value.
                if(result)
                {
                    reelUnit.SpinProfile.Duration = milliseconds;
                }
            }

            return result;
        }

        /// <summary>
        /// Set the spin direction of the specified reel.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="direction">The desired direction.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetDirection(byte reelNumber, ReelDirection direction)
        {
            var result = true;

            // Only send the command if the new value is different from the current one.
            var reelUnit = reelUnits[reelNumber];
            if(reelUnit.SpinProfile.Direction != direction)
            {
                var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SetDirection,
                                                              reelNumber,
                                                              (byte)direction);
                result = SendFeatureCommand(payload);

                // Update the current value.
                if(result)
                {
                    reelUnit.SpinProfile.Direction = direction;
                }
            }

            return result;
        }

        /// <summary>
        /// Set the desired stop position for the specified reel.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="stop">The desired stop position.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetStop(byte reelNumber, byte stop)
        {
            // The stop value does not persist between spins.
            // Always send the command.
            var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SetStop,
                                                          reelNumber,
                                                          stop);
            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Set the motion attributes for the specified reel.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="spinAttributes">The desired motion attributes.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetAttributes(byte reelNumber, SpinAttributes spinAttributes)
        {
            var result = true;
            var newValue = false;

            // A null attribute means to use the default attributes.
            spinAttributes = spinAttributes ?? new SpinAttributes();

            // Only send the command if the new value is different from the current one.
            var currentAttributes = reelUnits[reelNumber].SpinProfile.Attributes;

            // Set Cock attribute.
            if(currentAttributes.Cock != spinAttributes.Cock)
            {
                result = SetAttribute(reelNumber, spinAttributes.Cock, ReelAttribute.Cock);
                newValue = result;
            }

            // Set Bounce attribute.
            if(currentAttributes.Bounce != spinAttributes.Bounce)
            {
                result = result && SetAttribute(reelNumber, spinAttributes.Bounce, ReelAttribute.Bounce);
                newValue = result;
            }

            // Set Shake attribute.
            if(currentAttributes.Shake != spinAttributes.Shake)
            {
                switch(spinAttributes.Shake)
                {
                    case ShakeLevel.Off:
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.ShakeLow);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.ShakeMedium);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.ShakeHigh);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.ShakeMax);
                        break;

                    case ShakeLevel.Low:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.ShakeLow);
                        break;

                    case ShakeLevel.Medium:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.ShakeMedium);
                        break;

                    case ShakeLevel.High:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.ShakeHigh);
                        break;

                    case ShakeLevel.Max:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.ShakeMax);
                        break;
                }

                newValue = result;
            }

            if(currentAttributes.Hover != spinAttributes.Hover)
            {
                switch(spinAttributes.Hover.Level)
                {
                    case HoverLevel.Off:
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.HoverCustom);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.HoverTop);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.HoverCenter);
                        result = result && SetAttribute(reelNumber, false, ReelAttribute.HoverBottom);
                        break;
                    case HoverLevel.Top:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.HoverTop);
                        break;
                    case HoverLevel.Center:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.HoverCenter);
                        break;
                    case HoverLevel.Bottom:
                        result = result && SetAttribute(reelNumber, true, ReelAttribute.HoverBottom);
                        break;
                    case HoverLevel.Custom:
                        result = result && SetParameterizedAttribute(reelNumber, true, ReelAttribute.HoverCustom,
                            new[] { spinAttributes.Hover.Limits.LowerLimit, spinAttributes.Hover.Limits.UpperLimit });
                        break;
                }
                newValue = result;
            }

            // Update the current value.
            if(newValue)
            {
                reelUnits[reelNumber].SpinProfile.Attributes = new SpinAttributes(spinAttributes.Cock,
                                                                                  spinAttributes.Bounce,
                                                                                  spinAttributes.Shake,
                                                                                  spinAttributes.Hover);
            }

            return result;
        }

        /// <summary>
        /// Enable or disable the specified attribute for the specified reel.
        /// Automatically clear previously selected attributes that conflict
        /// with the new attribute.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="toEnable">Whether to disable or enable <paramref name="attribute"/>.</param>
        /// <param name="attribute">The attribute to manipulate.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetAttribute(byte reelNumber, bool toEnable, ReelAttribute attribute)
        {
            return SetParameterizedAttribute(reelNumber, toEnable, attribute, null);
        }

        /// <summary>
        /// Enable or disable the specified attribute with parameters for the
        /// specified reel. Automatically clear previously selected attributes
        /// that conflict with the new attribute.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="toEnable">Whether to disable or enable <paramref name="attribute"/>.</param>
        /// <param name="attribute">The attribute to manipulate.</param>
        /// <param name="parameters">Additional parameters for the attribute.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SetParameterizedAttribute(byte reelNumber, bool toEnable, ReelAttribute attribute,
                                               byte[] parameters)
        {
            var payload = new ReelSetAttributeCommandPayload(reelNumber, toEnable, attribute);
            return SendFeatureCommand(payload, parameters);
        }

        /// <summary>
        /// Apply the attributes to the specified reel immediately, as opposed to
        /// when the reel receives <see cref="ReelCommandCode.Spin"/> command.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool ApplyAttributes(byte reelNumber)
        {
            var payload = new UsbCommandPayload((byte)ReelCommandCode.ApplyAttributes,
                                                reelNumber);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Spin the specified reel using the current configuration values.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SpinReel(byte reelNumber)
        {
            var payload = new UsbCommandPayload((byte)ReelCommandCode.Spin,
                                                reelNumber);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Stop all activity on the specified reel(s) and put the reel(s)
        /// into safe state operation (normally slow spinning).
        /// </summary>
        /// <param name="reelNumber">
        /// The number identifying the reel.  <see cref="AllReels"/> means
        /// to apply the command to all reels.
        /// </param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool SlowSpin(byte reelNumber)
        {
            var reels = reelNumber == AllReels
                            ? reelUnits.Select(reelUnit => reelUnit.ReelNumber)
                            : new[] { reelNumber };

            var result = false;

            foreach(var reel in reels)
            {
                var payload = new UsbCommandPayload((byte)ReelCommandCode.SlowSpin,
                                                    reel);

                result = SendFeatureCommand(payload);

                if(!result)
                    break;
            }

            return result;
        }

        #endregion

        #region Commands Not Used Yet

        /// <summary>
        /// Set all characteristics for the specified reel to the default values.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool CommandSetDefaults(byte reelNumber)
        {
            var payload = new UsbCommandPayload((byte)ReelCommandCode.SetDefaults,
                                                reelNumber);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Select the desired time for the specified reel to accelerate
        /// from a stop to the terminal speed.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="milliseconds">The desired acceleration time, in milliseconds.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool CommandSetAcceleration(byte reelNumber, ushort milliseconds)
        {
            var payload = new UsbSingleUshortCommandPayload((byte)ReelCommandCode.SetAcceleration,
                                                             reelNumber,
                                                             milliseconds);
            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Stop the specified reel at a valid stop as soon as possible.
        /// This command is legal during a tilt.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        /// <devdoc>
        /// For versions 1.13 and higher, Halt is no longer used to
        /// recover from the slow spin.  Spin can be used instead.
        /// </devdoc>
        private bool CommandHalt(byte reelNumber)
        {
            var payload = new UsbCommandPayload((byte)ReelCommandCode.Halt,
                                                reelNumber);

            return SendFeatureCommand(payload);
        }

        /// <summary>
        /// Inform the driver whether the reel device is mounted in a
        /// non-standard orientation.
        /// </summary>
        /// <param name="orientation">The orientation of mounted reel device.</param>
        /// <returns>True if command succeeds, false otherwise.</returns>
        private bool CommandSetOrientation(ReelMountingOrientation orientation)
        {
            // This command only accepts 0xFF as the target device id.
            var payload = new UsbSingleByteCommandPayload((byte)ReelCommandCode.SetOrientation,
                                                          AllReels,
                                                          (byte)orientation);
            return SendFeatureCommand(payload);
        }

        #endregion

        #region Handling Messages

        /// <summary>
        /// Handle Reel specific statuses.
        /// </summary>
        /// <param name="statusRecord">
        /// The reel specific status record to process.
        /// </param>
        /// <param name="infoBuilder">
        /// The builder of strings describing the reel specific statuses.
        /// If null, no information needs to be returned.
        /// </param>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="statusRecord"/> is not a valid
        /// status record for Reel.
        /// </exception>
        private void HandleReelStatus(UsbStatusRecord statusRecord, StringBuilder infoBuilder)
        {
            var decoder = new ReelStatusDecoder(statusRecord.Status);

            // Error if the status is not recognized.
            if(!decoder.IsValidStatusCode)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Invalid reel status 0x{statusRecord.Status:X} in USB status message.");
            }

            // Error if reel number is out of range.
            if(decoder.ReelNumber >= reelCount)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Reel status contains invalid reel number {decoder.ReelNumber}");
            }

            // Add the reel status to the information.
            if(infoBuilder != null)
            {
                infoBuilder.AppendFormat("Reel {0} Update: {1}", decoder.ReelNumber, decoder.ReelStatusCode);
                infoBuilder.AppendLine();
            }

            // Update the information in reel unit.
            reelUnits[decoder.ReelNumber].StatusCode = decoder.ReelStatusCode;

            // If the status is one of the those we care...
            if(decoder.IsDefinedStatus)
            {
                ProcessReelStatusChange(decoder.ReelNumber, decoder.ReelStatus);
            }
            // The rest probably are tilts.
            else
            {
                ProcessReelTilts(decoder.ReelStatusCode);
            }
        }

        /// <summary>
        /// Process a reel status change.  Raise appropriate events as needed.
        /// </summary>
        /// <param name="reelNumber">The number identifying the reel.</param>
        /// <param name="reelStatus">The latest reel status of the reel.</param>
        private void ProcessReelStatusChange(byte reelNumber, ReelStatus reelStatus)
        {
            var reelUnit = reelUnits[reelNumber];

            // Update reel unit's status.
            reelUnit.Status = reelStatus;

            // Check if the status is registered for notification.
            var statusChangedHandler = ReelStatusChanged;

            if(statusChangedHandler != null && reelUnit.StatusRegistrations.Contains(reelStatus))
            {
                var reelStatusEvent = new ReelStatusEventArgs
                {
                    FeatureId = SubFeatureName,
                    ReelNumber = reelNumber,
                    Status = reelStatus
                };

                statusChangedHandler(this, reelStatusEvent);
            }

            // Check if it triggers a ReelsSpunStateChanged change.
            var spunStateChangedHandler = ReelsSpunStateChanged;

            if(spunStateChangedHandler != null)
            {
                var newState = ComputeReelsSpinningState();

                if(currrentReelsSpunState != newState)
                {
                    currrentReelsSpunState = newState;

                    var reelDeviceEvent = new ReelsSpunEventArgs
                    {
                        FeatureId = SubFeatureName,
                        ReelsSpunState = newState
                    };

                    spunStateChangedHandler(this, reelDeviceEvent);
                }
            }
        }

        /// <summary>
        /// Process a reel status code that does not corresponds to one of
        /// the defined <see cref="ReelStatus"/>.  Usually it means a tilt.
        /// </summary>
        /// <param name="statusCode">The status code to process.</param>
        private void ProcessReelTilts(ReelStatusCode statusCode)
        {
            switch(statusCode)
            {
                case ReelStatusCode.MovedFromStationary:
                case ReelStatusCode.StalledWhileMoving:
                case ReelStatusCode.StopNotFound:
                case ReelStatusCode.OpticSequenceError:
                case ReelStatusCode.Disconnected:
                    if(operationalState != OperationalState.Tilted)
                    {
                        operationalState = OperationalState.TiltDetected;
                    }
                    break;
            }
        }

        /// <summary>
        /// Compute the reels spinning state based on the statuses
        /// of all available reels.
        /// </summary>
        /// <returns>The new reels spinning state.</returns>
        private ReelsSpunState ComputeReelsSpinningState()
        {
            var result = currrentReelsSpunState;

            if(reelCount == reelUnits.Count(reel => reel.Status == ReelStatus.Stopped))
            {
                result = ReelsSpunState.AllStopped;
            }
            else if(reelCount == reelUnits.Count(reel => reel.Status == ReelStatus.ConstantSpeed))
            {
                result = ReelsSpunState.AllCompletedSpinUp;
            }
            else if(reelCount == reelUnits.Count(reel => reel.Status == ReelStatus.Accelerating ||
                                                         reel.Status == ReelStatus.ConstantSpeed))
            {
                result = ReelsSpunState.AllSpinningUp;
            }
            else if(reelCount == reelUnits.Count(reel => reel.Status == ReelStatus.Decelerating ||
                                                         reel.Status == ReelStatus.Stopped))
            {
                result = ReelsSpunState.AllSpinningDown;
            }

            return result;
        }

        #endregion

        #endregion

        #endregion
    }
}
