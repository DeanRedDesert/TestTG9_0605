//-----------------------------------------------------------------------
// <copyright file = "LightDevice.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using CompactSerialization;
    using CSI.Schemas;
    using IgtUsbDevice;

    /// <summary>
    /// This class encapsulates the data and operations
    /// to control a physical light device.
    /// </summary>
    [Serializable]
    public class LightDevice : DeviceBase
    {
        #region Nested Class

        /// <summary>
        /// A record of which light sequence is being played
        /// on a light group.
        /// </summary>
        private class SequenceStatus
        {
            /// <summary>
            /// Get the flag indicating whether a sequence is
            /// running on the light device.
            /// </summary>
            public bool IsActive { get; }

            /// <summary>
            /// Get the sequence number currently running
            /// on the light group.  Meaningful only when
            /// <see cref="IsActive"/> is true.
            /// </summary>
            public uint SequenceNumber { get; }

            /// <summary>
            /// Initialize an instance of <see cref="SequenceStatus"/>
            /// with default values.
            /// </summary>
            public SequenceStatus() : this(false, 0)
            {
            }

            /// <summary>
            /// Initialize an instance of <see cref="SequenceStatus"/>
            /// with given active flag and sequence number.
            /// </summary>
            /// <param name="isActive">Flag indicating if the sequence is active.</param>
            /// <param name="sequenceNumber">The sequence number being played.</param>
            public SequenceStatus(bool isActive, uint sequenceNumber)
            {
                IsActive = isActive;
                SequenceNumber = sequenceNumber;
            }
        }

        #endregion

        #region Constants

        /// <summary>
        /// Constant which indicates all light groups are to be controlled.
        /// </summary>
        public const byte AllGroups = 0xFF;

        /// <summary>
        /// Constant which indicates all lights in a group are to be controlled.
        /// </summary>
        public const ushort AllLights = 0xFFFF;

        /// <summary>
        /// Size of the feature descriptor header.
        /// 
        /// The header of a light device's feature specific descriptor
        /// consists of the following fields:
        ///     <list type="number">
        ///         <item>Total length of the feature specific data (1 byte).</item>
        ///         <item>Descriptor type (1 byte).</item>
        ///         <item>Length of descriptor data for each light group (1 byte).</item>
        ///     </list>
        /// </summary>
        private const int FeatureDescriptorHeaderSize = 3;

        #endregion

        #region Properties

        /// <summary>
        /// Get the device type.
        /// </summary>
        public DeviceType Type { get; private set; }

        /// <summary>
        /// Get the <see cref="LightFeatureDescription"/>
        /// for this light device.
        /// </summary>
        public LightFeatureDescription FeatureDescription
        {
            get
            {
                return new LightFeatureDescription(
                    SubFeatureName,
                    lightSubFeature,
                    groupDescriptors.Select((groupDescriptor, index) =>
                                                 CreateLightGroup((byte)index, groupDescriptor)));
            }
        }

        /// <inheritdoc/>
        public override string FeatureDescriptors
        {
            get
            {
                string result;

                if(groupCount > 0)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("Light Specific Descriptors -");
                    for(var i = 0; i < groupCount; i++)
                    {
                        stringBuilder.AppendFormat("#{0}    ", i);
                        stringBuilder.AppendLine(groupDescriptors[i].ToString());
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

        #endregion

        #region Fields

        /// <summary>
        /// The sub feature of the light device.
        /// </summary>
        private readonly LightSubFeature lightSubFeature;

        /// <summary>
        /// The descriptors for each light group.
        /// </summary>
        private readonly List<LightGeneralDescriptor> groupDescriptors;

        /// <summary>
        /// Number of light groups on this device.
        /// </summary>
        private readonly int groupCount;

        /// <summary>
        /// List of sequence status records, each for a light group.
        /// </summary>
        private readonly List<SequenceStatus> sequenceStatuses;

        #endregion

        #region Methods

        #region Constructor

        /// <summary>
        /// Initialize an instance of <see cref="LightDevice"/>
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
        public LightDevice(UsbDeviceData deviceData, bool bypassHardware = false)
            : base(deviceData, bypassHardware)
        {
            if(deviceData == null)
            {
                throw new ArgumentNullException(nameof(deviceData));
            }

            Type = DeviceType.Light;

            // Light sub feature must be initialized before
            // parsing the feature specific descriptor.
            lightSubFeature = (LightSubFeature)deviceData.FunctionalDescriptor.SubFeatureNumber;

            // There should be only 1 feature specific descriptor to parse.
            if(deviceData.FunctionalDescriptor.NumberAdditionalDescriptors != 1)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect number of feature descriptors {deviceData.FunctionalDescriptor.NumberAdditionalDescriptors} for the light device.  Should be 1.");
            }

            groupDescriptors = ParseFeatureDescriptors(deviceData.FeatureDescriptorData);
            groupCount = groupDescriptors.Count;

            // For each light group, create a sequence status record.
            sequenceStatuses = new List<SequenceStatus>(groupCount);
            for(var i = 0; i < groupCount; i++)
            {
                sequenceStatuses.Add(new SequenceStatus());
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Turn off all of the lights within the specified group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="transitionMode">The transition mode to use when turning off the group.</param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        public void LightsOff(byte groupNumber, TransitionMode transitionMode)
        {
            ValidateGroupNumber(groupNumber);

            var payload = new UsbSingleByteCommandPayload((byte)LightCommandCode.LightsOff,
                                                          groupNumber,
                                                          (byte)transitionMode);

            SendFeatureCommand(payload);
        }

        /// <summary>
        /// Start a light sequence within the specified group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <param name="transitionMode">The transition mode to use when turning off the group.</param>
        /// <param name="parameters">
        /// Byte array containing the parameters for the sequence. If the sequence does not require any parameters,
        /// then null may be passed.
        /// </param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        public void StartSequence(byte groupNumber, uint sequenceNumber, TransitionMode transitionMode, byte[] parameters)
        {
            ValidateGroupNumber(groupNumber);

            // These two payloads have different sizes due to the different data types
            // used to represent the sequence number.
            if(parameters == null)
            {
                var payload = new LightStartSequenceCommandPayload(groupNumber,
                                                                   transitionMode,
                                                                   (ushort)sequenceNumber);
                SendFeatureCommand(payload);
            }
            else
            {
                var payload = new LightStartSequenceWithDataCommandPayload(groupNumber,
                                                                           transitionMode,
                                                                           (ushort)sequenceNumber);
                SendFeatureCommand(payload, parameters);
            }

            SetActiveSequence(groupNumber, sequenceNumber);
        }

        /// <summary>
        /// Check if a sequence is running within the specified group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        public bool IsSequenceRunning(byte groupNumber, uint sequenceNumber)
        {
            // This command does not support All Groups.
            ValidateGroupNumber(groupNumber, false);

            return sequenceStatuses[groupNumber].IsActive &&
                   sequenceStatuses[groupNumber].SequenceNumber == sequenceNumber;
        }

        /// <summary>
        /// Set the brightness for the specified lights in the given group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="lightStates">List of lights and the desired state of each light.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lightStates"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="lightStates"/> is empty.
        /// </exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the length of <paramref name="lightStates"/>
        /// exceeds the number of lights in the specified group.
        /// </exception>
        public void RandomMonochrome(byte groupNumber, IEnumerable<MonochromeLightState> lightStates)
        {
            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            var lightStateList = lightStates.ToList();

            if(!lightStateList.Any())
            {
                throw new ArgumentException("Data list is empty.", nameof(lightStates));
            }

            var lightNumbers = lightStateList.Select(lightState => lightState.LightNumber).ToList();
            ValidateGroupAndLightNumbers(groupNumber, lightNumbers);

            var payload = new UsbCommandPayload((byte)LightCommandCode.RandomMonochrome, groupNumber);

            byte[] data;
            using(var stream = new MemoryStream())
            {
                foreach(var lightState in lightStateList)
                {
                    (lightState  as ICompactSerializable).Serialize(stream);
                }

                data = stream.ToArray();
            }

            SendFeatureCommand(payload, data);
        }

        /// <summary>
        /// Set the brightness a series of lights in the given group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to adjust the brightness for.</param>
        /// <param name="brightnesses">
        /// A list of desired brightness levels. The first number is the desired brightness for startingLight,
        /// subsequent levels are consecutively applied to the lights after startingLight.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="brightnesses"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="brightnesses"/> is empty.
        /// </exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="startingLight"/> or the length of <paramref name="brightnesses"/>
        /// exceeds the number of lights in the specified group.
        /// </exception>
        public void ConsecutiveMonochrome(byte groupNumber, ushort startingLight, IEnumerable<byte> brightnesses)
        {
            if(brightnesses == null)
            {
                throw new ArgumentNullException(nameof(brightnesses));
            }

            var brightnessArray = brightnesses.ToArray();

            if(!brightnessArray.Any())
            {
                throw new ArgumentException("Data list is empty.", nameof(brightnesses));
            }

            ValidateGroupAndLightNumbers(groupNumber, brightnessArray.Length, startingLight);

            var payload = new UsbCommandPayload((byte)LightCommandCode.ConsecutiveMonochrome, groupNumber);

            byte[] data;
            using(var stream = new MemoryStream())
            {
                stream.Write(BitConverter.GetBytes(startingLight), 0, sizeof(ushort));
                stream.Write(brightnessArray, 0, brightnessArray.Length);

                data = stream.ToArray();
            }

            SendFeatureCommand(payload, data);
        }

        /// <summary>
        /// Set the color for the specified lights in the given group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="lightStates">List of lights and the desired state of each light.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lightStates"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="lightStates"/> is empty.
        /// </exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the length of <paramref name="lightStates"/>
        /// exceeds the number of lights in the specified group.
        /// </exception>
        public void RandomRgb(byte groupNumber, IEnumerable<RgbLightState> lightStates)
        {
            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            var lightStateList = lightStates.ToList();

            if(!lightStateList.Any())
            {
                throw new ArgumentException("Data list is empty.", nameof(lightStates));
            }

            var lightNumbers = lightStateList.Select(lightState => lightState.LightNumber).ToList();
            ValidateGroupAndLightNumbers(groupNumber, lightNumbers);

            var payload = new UsbCommandPayload((byte)LightCommandCode.RandomRgb, groupNumber);

            byte[] data;
            using(var stream = new MemoryStream())
            {
                foreach(var lightState in lightStateList)
                {
                    (lightState as ICompactSerializable).Serialize(stream);
                }

                data = stream.ToArray();
            }

            SendFeatureCommand(payload, data);
        }

        /// <summary>
        /// Set the color for a series of lights in the given group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to adjust the color for.</param>
        /// <param name="colors">
        /// List of desired light colors. The first number is the desired color for startingLight, subsequent colors
        /// are consecutively applied to the lights after startingLight.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="colors"/> is empty.
        /// </exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="startingLight"/> or the length of <paramref name="colors"/>
        /// exceeds the number of lights in the specified group.
        /// </exception>
        public void ConsecutiveRgb(byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors)
        {
            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            var colorList = colors.ToList();

            if(!colorList.Any())
            {
                throw new ArgumentException("Data list is empty.", nameof(colors));
            }

            ValidateGroupAndLightNumbers(groupNumber, colorList.Count, startingLight);

            var payload = new UsbCommandPayload((byte)LightCommandCode.ConsecutiveRgb, groupNumber);

            byte[] data;
            using(var stream = new MemoryStream())
            {
                stream.Write(BitConverter.GetBytes(startingLight), 0, sizeof(ushort));

                foreach(var color in colorList)
                {
                    stream.Write(BitConverter.GetBytes(color.PackedColor), 0, sizeof(ushort));
                }

                data = stream.ToArray();
            }

            SendFeatureCommand(payload, data);
        }

        /// <summary>
        /// Set the bitwise light state of the specified series of lights.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="bitsPerLight">The number of bits of control per light.</param>
        /// <param name="lightData">The packed data for light control.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lightData"/> is null.
        /// </exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="startingLight"/> exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="InvalidLightStateSizeException">
        /// Thrown when the packed light state data does not match the expected size for the number of lights being
        /// controlled.
        /// </exception>
        public void BitWiseControl(byte groupNumber, ushort startingLight, byte bitsPerLight, byte[] lightData)
        {
            if(lightData == null)
            {
                throw new ArgumentNullException(nameof(lightData));
            }

            // Since the data is packed, we cannot validate the data length as usual.
            // Use 0 as dataLength to pass the validation here.
            ValidateGroupAndLightNumbers(groupNumber, 0, startingLight);

            // Now verify the length of the packed data.
            var testGroup = groupNumber == AllGroups ? (byte)0 : groupNumber;

            var possibleLights = groupDescriptors[testGroup].LightCount - startingLight;
            var maxBytesForLights = (int)Math.Ceiling(possibleLights * bitsPerLight / 8f);

            if(lightData.Length > maxBytesForLights)
            {
                throw new InvalidLightStateSizeException(SubFeatureName, groupNumber, maxBytesForLights, lightData.Length);
            }

            // Send the command.
            var payload = new LightBitwiseControlCommandPayload(groupNumber, startingLight, bitsPerLight);

            SendFeatureCommand(payload, lightData);
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

            foreach(var statusRecord in statusMessage.StatusRecords)
            {
                // Handle global statuses.
                if(statusRecord.IsGlobalStatus)
                {
                    var status = (GlobalStatusCode)statusRecord.Status;

                    switch(status)
                    {
                        case GlobalStatusCode.CommunicationTimedOut:
                            // Copy what is done in AVP code.
                            Reset();
                            ClearActiveSequence(AllGroups);
                            break;

                        case GlobalStatusCode.SelfTestExited:
                            ClearStatus(statusRecord.Status);
                            break;
                    }
                }
                // Handle light specific statuses.
                else
                {
                    HandleLightStatus(statusRecord, infoBuilder);
                }
            }
        }

        /// <inheritdoc />
        protected override bool SendFeatureCommand(UsbCommandPayload payload, byte[] data)
        {
            // Clear active sequence for all commands other than Start Sequence.
            if(payload.FunctionCode != (byte)LightCommandCode.StartSequence)
            {
                ClearActiveSequence(payload.TargetDevice);
            }

            return base.SendFeatureCommand(payload, data);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Parse the byte array of feature descriptor data of the light device.
        /// </summary>
        /// <param name="featureDescriptorData">The byte array to parse.</param>
        /// <returns>List of light descriptors, each for a light group.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureDescriptorData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="featureDescriptorData"/> contains invalid data.
        /// </exception>
        private List<LightGeneralDescriptor> ParseFeatureDescriptors(byte[] featureDescriptorData)
        {
            if(featureDescriptorData == null)
            {
                throw new ArgumentNullException(nameof(featureDescriptorData));
            }

            List<LightGeneralDescriptor> result;
 
            // Calculate how many group descriptors are defined.
            var descriptorCount = ComputeDescriptorCount(featureDescriptorData);

            // The length of each group descriptor is defined in the header.
            var expectedLength = featureDescriptorData[2];

            // Data for group descriptors starts after the header.
            var offset = FeatureDescriptorHeaderSize;

            // Parse the group descriptors.
            switch(lightSubFeature)
            {
                case LightSubFeature.LightBezel:
                    {
                        if(descriptorCount != 1)
                        {
                            throw new InvalidUsbDeviceDataException(
                                "Light Bezel device cannot have more than 1 light group.");
                        }

                        var bezelDescriptor = new LightBezelDescriptor();
                        bezelDescriptor.Unpack(featureDescriptorData, offset);

                        if(bezelDescriptor.DataSize != expectedLength)
                        {
                            throw new InvalidUsbDeviceDataException(
                                "Mismatched light bezel group descriptor length.");
                        }

                        result = new List<LightGeneralDescriptor> { bezelDescriptor };
                    }
                    break;

                case LightSubFeature.LightBars:
                    {
                        if(descriptorCount != 1)
                        {
                            throw new InvalidUsbDeviceDataException(
                                "Light bar device cannot have more than 1 light group.");
                        }

                        var barDescriptor = new LightBarDescriptor();
                        barDescriptor.Unpack(featureDescriptorData, offset);

                        if(barDescriptor.DataSize != expectedLength)
                        {
                            throw new InvalidUsbDeviceDataException(
                                "Mismatched light bar group descriptor length.");
                        }

                        result = new List<LightGeneralDescriptor> { barDescriptor };
                    }
                    break;

                default:
                    {
                        result = new List<LightGeneralDescriptor>(descriptorCount);

                        for(var i = 0; i < descriptorCount; i++)
                        {
                            var generalDescriptor = new LightGeneralDescriptor();
                            generalDescriptor.Unpack(featureDescriptorData, offset);

                            offset += generalDescriptor.DataSize;

                            // Use the first list element to validate the size.
                            if(i == 0 && generalDescriptor.DataSize != expectedLength)
                            {
                                throw new InvalidUsbDeviceDataException(
                                    "Mismatched light group descriptor length.");
                            }

                            result.Add(generalDescriptor);
                        }
                    }
                    break;
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
            var descriptorType = featureDescriptorData[1];
            var groupDescriptorLength = featureDescriptorData[2];

            if(totalLength <= FeatureDescriptorHeaderSize)
            {
                throw new InvalidUsbDeviceDataException(
                    $"The length of feature descriptor {totalLength} is invalid.");
            }

            if(descriptorType != (byte)UsbDescriptorType.FeatureSpecific)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect descriptor type.  Expected 0x{UsbDescriptorType.FeatureSpecific:X}, but got 0x{descriptorType:X}");
            }

            if(groupDescriptorLength <= 0)
            {
                throw new InvalidUsbDeviceDataException(
                    $"The length of light group descriptor {groupDescriptorLength} is invalid.");
            }

            return (totalLength - FeatureDescriptorHeaderSize) / groupDescriptorLength;
        }

        /// <summary>
        /// Check whether a given group number is valid or not.
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to check.
        /// </param>
        /// <param name="allowAllGroups">
        /// Flag indicating if <see cref="AllGroups"/> is considered valid.
        /// </param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void ValidateGroupNumber(byte groupNumber, bool allowAllGroups = true)
        {
            if(groupNumber >= groupCount && (!allowAllGroups || groupNumber != AllGroups))
            {
                throw new InvalidLightGroupException(SubFeatureName, groupNumber);
            }
        }

        /// <summary>
        /// Check whether the given group number and light numbers are valid or not.
        /// <see cref="AllGroups"/> is considered valid.
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to check.
        /// </param>
        /// <param name="lightNumbers">
        /// List of light numbers to control.
        /// </param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="lightNumbers"/> is null;
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when there are duplicate light numbers, or extra light numbers
        /// in addition to AllLights in <paramref name="lightNumbers"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when any element in <paramref name="lightNumbers"/>
        /// exceeds the number of lights in the group.
        /// </exception>
        private void ValidateGroupAndLightNumbers(byte groupNumber, IList<ushort> lightNumbers)
        {
            ValidateGroupNumber(groupNumber);

            if(lightNumbers == null)
            {
                throw new ArgumentNullException(nameof(lightNumbers));
            }

            if(lightNumbers.Count != lightNumbers.Distinct().Count())
            {
                throw new ArgumentException(
                    "There are duplicate light numbers in the parameter for the light control command.");
            }

            if(lightNumbers.Any(lightNumber => lightNumber == AllLights) && lightNumbers.Count > 1)
            {
                throw new ArgumentException(
                    "No other light number is allowed if one for AllLights is present.");
            }

            var lightCount = ValidateLightCount(groupNumber);

            if(lightNumbers.Any(lightNumber => lightNumber >= lightCount && lightNumber != AllLights))
            {
                throw new ArgumentOutOfRangeException(
                    $"At least one of the light numbers exceeds the light count {lightCount} of {SubFeatureName} Group {groupNumber}.");
            }
        }

        /// <summary>
        /// Check whether the given group number, starting light and data length are valid or not.
        /// <see cref="AllGroups"/> is considered valid.
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to check.
        /// </param>
        /// <param name="dataLength">
        /// Number of data for a control command.
        /// </param>
        /// <param name="startingLight">
        /// The first light to control.
        /// </param>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="startingLight"/> or <paramref name="dataLength"/>
        /// exceeds the number of lights in the group.
        /// </exception>
        private void ValidateGroupAndLightNumbers(byte groupNumber,
                                                  int dataLength,
                                                  ushort startingLight)
        {
            ValidateGroupNumber(groupNumber);

            var lightCount = ValidateLightCount(groupNumber);

            if(startingLight >= lightCount)
            {
                throw new ArgumentOutOfRangeException(
                    $"Starting light {startingLight} exceeds the light count {lightCount} of {SubFeatureName} Group {groupNumber}.");
            }

            if(dataLength > lightCount - startingLight)
            {
                throw new ArgumentOutOfRangeException(
                    $"Light data length {dataLength} exceeds the light count {lightCount} of {SubFeatureName} Group {groupNumber} when starting at {startingLight}.");
            }
        }

        /// <summary>
        /// Get the light count in the specified group, or that of
        /// the first group if <paramref name="groupNumber"/> is
        /// <see cref="AllGroups"/>.  Check if it is valid.
        /// </summary>
        /// <remarks>
        /// <paramref name="groupNumber"/> must be validated before
        /// calling this method.
        /// </remarks>
        /// <param name="groupNumber">The group whose light count to be retrieved.</param>
        /// <returns>The light count in the valid group.</returns>
        private ushort ValidateLightCount(byte groupNumber)
        {
            // If it is for all groups, use group 0 to validate light numbers.
            if(groupNumber == AllGroups)
            {
                groupNumber = 0;
            }

            var lightCount = groupDescriptors[groupNumber].LightCount;

            if(lightCount == 0)
            {
                throw new InvalidOperationException(
                    $"{SubFeatureName} Group {groupNumber} has zero light count, therefore its lights cannot be controlled.");
            }

            return lightCount;
        }

        /// <summary>
        /// Create an object that implements <see cref="ILightGroup"/>
        /// based on the given index and descriptor of a light group.
        /// </summary>
        /// <param name="groupNumber">The index of the light group.</param>
        /// <param name="groupDescriptor">The descriptor of the light group.</param>
        /// <returns>
        /// The description of a light group in <see cref="ILightGroup"/> format.
        /// Null if <paramref name="groupDescriptor"/> is null.
        /// </returns>
        private static ILightGroup CreateLightGroup(byte groupNumber, LightGeneralDescriptor groupDescriptor)
        {
            ILightGroup result = null;

            if(groupDescriptor != null)
            {
                switch(groupDescriptor)
                {
                    case LightBezelDescriptor bezelDescriptor:
                        result = new LightBezelGroup(groupNumber,
                            bezelDescriptor.IsRgb,
                            bezelDescriptor.TopLightCount,
                            bezelDescriptor.BottomLightCount,
                            bezelDescriptor.LeftLightCount,
                            bezelDescriptor.RightLightCount);
                        break;
                    case LightBarDescriptor descriptor:
                    {
                        var barDescriptor = descriptor;

                        result = new LightBarGroup(groupNumber,
                            barDescriptor.IsRgb,
                            barDescriptor.LightCountPerBars);
                        break;
                    }

                    default:
                        result = new LightGroup(groupNumber,
                            groupDescriptor.LightCount,
                            groupDescriptor.IsRgb);
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Set an active sequence status record for a given light group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="sequenceNumber">The sequence number being played.</param>
        private void SetActiveSequence(byte groupNumber, uint sequenceNumber)
        {
            ValidateGroupNumber(groupNumber);

            if(groupNumber != AllGroups)
            {
                sequenceStatuses[groupNumber] = new SequenceStatus(true, sequenceNumber);
            }
            else
            {
                for(byte i = 0; i < groupCount; i++)
                {
                    sequenceStatuses[i] = new SequenceStatus(true, sequenceNumber);
                }
            }
        }

        /// <summary>
        /// Clear the active sequence status record for a given light group.
        /// </summary>
        /// <param name="groupNumber">The number of the target light group.</param>
        private void ClearActiveSequence(byte groupNumber)
        {
            ValidateGroupNumber(groupNumber);

            if(groupNumber != AllGroups)
            {
                sequenceStatuses[groupNumber] = new SequenceStatus();
            }
            else
            {
                for(byte i = 0; i < groupCount; i++)
                {
                    sequenceStatuses[i] = new SequenceStatus();
                }
            }
        }

        /// <summary>
        /// Handle Light specific statuses.
        /// </summary>
        /// <param name="statusRecord">
        /// The light specific status record to process.
        /// </param>
        /// <param name="infoBuilder">
        /// The builder of strings describing the light specific statuses.
        /// If null, no information needs to be returned.
        /// </param>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when <paramref name="statusRecord"/> is not a valid
        /// status record for Light.
        /// </exception>
        private void HandleLightStatus(UsbStatusRecord statusRecord, StringBuilder infoBuilder)
        {
            var decoder = new LightStatusDecoder(statusRecord.Status);

            // Error if the status is not recognized.
            if(!decoder.IsValidStatusCode)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Invalid light status 0x{statusRecord.Status:X} in USB status message.");
            }

            // Error if group number is out of range.
            if(decoder.GroupNumber >= groupCount)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Light status contains invalid group number {decoder.GroupNumber}");
            }

            // Add the light status to the information.
            infoBuilder.AppendFormat("Light Group {0} on {1} Update: {2}",
                                     decoder.GroupNumber, SubFeatureName, decoder.LightStatusCode);
            infoBuilder.AppendLine();

            // TODO: Post tilt when light tilts.
            // Something to consider when standalone tilt manager is in place.
        }

        #endregion

        #endregion
    }
}
