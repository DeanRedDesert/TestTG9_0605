//-----------------------------------------------------------------------
// <copyright file = "StreamingLightDevice.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.StreamingLights
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Cabinet.SymbolHighlights;
    using IgtUsbDevice;
    using Presentation.PeripheralLights.Streaming;
    using SymbolHighlights;

    /// <summary>
    /// Provides control of a streaming light (Feature 121) device.
    /// </summary>
    public sealed class StreamingLightDevice : DeviceBase
    {
        #region Fields

        private const byte StreamingLightFeatureHeaderSize = 11;
        private readonly List<StreamingLightGroupDescriptor> groupDescriptors;
        private byte streamingLightCommandId;
        private readonly object commandIdLock = new object();
        private readonly Dictionary<byte, DeviceFrameQueue> groupQueues;
        private readonly Dictionary<byte, UsbCommandData> lastGroupCommand;
        private byte groupCount;
        private bool hasBeenReset;

        /// <remarks>
        /// This field is for downloading instead of streaming.
        /// </remarks>
        // ReSharper disable once NotAccessedField.Local
        private uint maxTransferSize;

        /// <remarks>
        /// This field is for downloading instead of streaming.
        /// </remarks>
        // ReSharper disable once NotAccessedField.Local
        private uint maximumSequenceBytes;

        private static readonly Dictionary<ushort, LightSubFeature> SubFeatureIdToEnum =
            new Dictionary<ushort, LightSubFeature>
            {
                { 0x80, LightSubFeature.BonusGameLights },
                { 0x81, LightSubFeature.LightBezel },
                { 0x82, LightSubFeature.LightBars },
                { 0x83, LightSubFeature.ReelBacklights },
                { 0x84, LightSubFeature.Candle },
                { 0x85, LightSubFeature.AccentLights },
                { 0x86, LightSubFeature.CardReaderBezel },
                { 0x87, LightSubFeature.TopperLightRing },
                { 0x88, LightSubFeature.ReelDividerLights },
                { 0x89, LightSubFeature.ReelHighlights }
            };

        #endregion

        /// <summary>
        /// Construct a new streaming light device given the device data.
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
        /// Thrown if <paramref name="deviceData"/> is null.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown if more than one feature specific descriptor is returned by the device.
        /// Thrown if the device has a sub feature number that is not valid.
        /// </exception>
        public StreamingLightDevice(UsbDeviceData deviceData, bool bypassHardware = false)
            : base(deviceData, bypassHardware)
        {
            if(deviceData == null)
            {
                throw new ArgumentNullException(nameof(deviceData));
            }

            groupDescriptors = new List<StreamingLightGroupDescriptor>();
            groupQueues = new Dictionary<byte, DeviceFrameQueue>();
            lastGroupCommand = new Dictionary<byte, UsbCommandData>();

            // There should be only 1 feature specific descriptor to parse.
            if(deviceData.FunctionalDescriptor.NumberAdditionalDescriptors != 1)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Incorrect number of feature descriptors {deviceData.FunctionalDescriptor.NumberAdditionalDescriptors} for the light device.  Should be 1.");
            }

            if(SubFeatureIdToEnum.ContainsKey(deviceData.FunctionalDescriptor.SubFeatureNumber))
            {
                SubFeatureType = SubFeatureIdToEnum[deviceData.FunctionalDescriptor.SubFeatureNumber];
            }
            else
            {
                throw new InvalidUsbDeviceDataException(
                    $"The sub feature number of {deviceData.FunctionalDescriptor.SubFeatureNumber:X2} was not recognized as a valid sub feature type.");
            }

            ParseFeatureDescriptors(deviceData.FeatureDescriptorData);
        }

        /// <summary>
        /// Notification events about this device.
        /// </summary>
        public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        #region Properties

        /// <inheritdoc />
        public override string FeatureDescriptors
        {
            get
            {
                var text = new StringBuilder();

                text.AppendLine("Streaming Light Group Descriptors:");
                if(groupCount > 0)
                {
                    for(var groupIndex = 0; groupIndex < groupCount; groupIndex++)
                    {
                        text.AppendFormat("{0}:    ", groupIndex);
                        text.AppendLine(groupDescriptors[groupIndex].ToString());
                    }
                }
                else
                {
                    text.AppendLine("    (No Groups)");
                }

                return text.ToString();
            }
        }

        /// <summary>
        /// Gets the streaming light feature data for this device.
        /// </summary>
        public LightFeatureDescription LightFeatureData
        {
            get
            {
                var groups = new List<ILightGroup>();
                for(byte index = 0; index < groupCount; index++)
                {
                    var group = groupDescriptors[index];
                    groups.Add(new StreamingLightGroup(index, group.NumberOfLights,
                                                       group.RealTimeControlSupported,
                                                       group.IntensityControlSupported,
                                                       group.SymbolHighlightsSupported));
                }
                return new LightFeatureDescription(SubFeatureName, SubFeatureType, groups);
            }
        }

        /// <summary>
        /// The light device sub feature type.
        /// </summary>
        public LightSubFeature SubFeatureType
        {
            get;
        }

        /// <summary>
        /// Get if the device supports symbol highlights.
        /// </summary>
        public bool SupportsSymbolHighlights
        {
            get
            {
                return groupDescriptors.Any(descriptor => descriptor.SymbolHighlightsSupported);
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        /// <exception cref="HardwareErrorException">
        /// Thrown if the device reports a hardware error.
        /// </exception>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown if the streaming status code is not valid.
        /// </exception>
        /// <exception cref="InvalidFrameDataException">
        /// Thrown if a frame is rejected by the device as invalid.
        /// </exception>
        internal override void HandleStatus(UsbStatusMessage statusMessage, StringBuilder infoBuilder)
        {
            base.HandleStatus(statusMessage, infoBuilder);
            foreach(var statusRecord in statusMessage.StatusRecords)
            {
                if(statusRecord.IsGlobalStatus)
                {
                    continue;
                }
                var streamingStatus = new StreamingStatusDecoder(statusRecord.Status);
                if(streamingStatus.IsValidStatusCode)
                {
                    infoBuilder.AppendLine(streamingStatus.ToString());
                    switch(streamingStatus.StatusCode)
                    {
                        case StreamingStatusCode.RealTimeCommandStatus:
                            var commandStatus = new RealTimeCommandStatus(statusRecord.NonTextData);
                            infoBuilder.AppendLine(commandStatus.ToString());
                            if(commandStatus.Status == RealTimeCommandStatus.StatusCode.FrameDataInvalid)
                            {
                                // For this status message the number of frames field contains the
                                // frame number that was invalid.
                                throw new InvalidFrameDataException(commandStatus.CommandId, commandStatus.NumberOfFrames);
                            }
                            break;
                        case StreamingStatusCode.RequestedNumberOfFramesAvailable:
                        case StreamingStatusCode.GroupReady:
                            // Only the group number is needed for now.
                            ResendFrameCommand(streamingStatus.GroupNumber);
                            break;
                        case StreamingStatusCode.RequestedThresholdReached:
                            HandleThresholdReachedStatus(streamingStatus.GroupNumber,
                                new RequestedThresholdReachedStatus(statusRecord.NonTextData));
                            break;
                        case StreamingStatusCode.HardwareError:
                            throw new HardwareErrorException(SubFeatureName, streamingStatus.GroupNumber);
                        case StreamingStatusCode.RleDecompressionError:
                            var decompressionError = new RleDecompressionError(statusRecord.NonTextData);
                            throw new InvalidFrameDataException(decompressionError.CommandId, decompressionError.Error);
                    }
                }
                else
                {
                    throw new InvalidUsbDeviceDataException(
                        $"The device status of {statusRecord.Status:X4} was not recognized as a valid status code.");
                }
            }
        }

        /// <summary>
        /// Sets the device to a single solid color.
        /// </summary>
        /// <param name="groupNumber">The group number to control.</param>
        /// <param name="color">The RGB15 color to set the device to.</param>
        /// <returns>True if the command was successful.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        public bool SetColor(byte groupNumber, Rgb15 color)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            // Reset the device first if this is the first command being sent.
            if(!hasBeenReset)
            {
                hasBeenReset = true;
                Reset();
            }

            var command = new UsbSingleUshortCommandPayload((byte)StreamingLightCommandCode.SetLights,
                                                            groupNumber, color.PackedColor);
            return SendFeatureCommand(command);
        }

        /// <summary>
        /// Play a light sequence from disk.
        /// </summary>
        /// <param name="groupNumber">The group number to play the sequence on.</param>
        /// <param name="lightSequenceFile">The full path to the light sequence on disk.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        public void PlayLightSequence(byte groupNumber, string lightSequenceFile)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            if(BypassHardware)
                return;

            var sequence = new LightSequence(lightSequenceFile);
            PlayLightSequence(groupNumber, sequence, StreamingLightsPlayMode.Restart);
        }

        /// <summary>
        /// Play light sequence from memory.
        /// </summary>
        /// <param name="groupNumber">The group number to play the sequence on.</param>
        /// <param name="sequence">The light sequence to play.</param>
        /// <param name="playMode">The play mode to use when starting the sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="sequence"/> cannot be cast to <see cref="LightSequence"/>.
        /// Thrown if <paramref name="sequence"/> has any frames that exceed 20FPS.
        /// </exception>
        /// <remarks>
        /// The <paramref name="sequence"/> parameter isn't strongly typed to prevent the other
        /// assemblies that utilize this class from requiring a reference to the assembly where
        /// LightSequence is defined.
        /// </remarks>
        public void PlayLightSequence(byte groupNumber, object sequence, StreamingLightsPlayMode playMode)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            if(!(sequence is LightSequence lightSequence))
            {
                throw new ArgumentException("The sequence provided was not cast-able to LightSequence.", nameof(sequence));
            }

            if(lightSequence.Segments.Any(segment => segment.Frames.Any(frame => frame.DisplayTime < 33)))
            {
                throw new ArgumentException("The lighting sequence cannot have any frames that exceed 30FPS.", nameof(sequence));
            }

            var queue = groupQueues[groupNumber];
            if(playMode == StreamingLightsPlayMode.Restart)
            {
                var frames = lightSequence.Segments.SelectMany(segment => segment.Frames).ToList();
                queue.AddChunk(frames, playMode, 0);
            }

            // Reset the device first if this is the first command being sent.
            if(!hasBeenReset)
            {
                hasBeenReset = true;
                Reset();
            }

            // Only need to restart the frame sending process on a restart.
            // With continue just rely on the next request from the device for
            // more frames to send the updated sequence.
            if(playMode == StreamingLightsPlayMode.Restart)
            {
                var lightData = queue.GetFrames(out var frameCount);
                SendLightFrames(groupNumber, frameCount, lightData, false);
            }
        }

        /// <summary>
        /// Plays a light sequence from a base 64 encoded stream of bytes.
        /// </summary>
        /// <param name="groupNumber">The group number to play the sequence on.</param>
        /// <param name="base64EncodedSequence">The light sequence to decode and play.</param>
        /// <param name="playMode">The play mode for the sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        public void PlayLightSequence(byte groupNumber, byte[] base64EncodedSequence, StreamingLightsPlayMode playMode)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            var encoder = new ASCIIEncoding();
            var base64String = encoder.GetString(base64EncodedSequence);
            var sequenceData = Convert.FromBase64String(base64String);
            LightSequence lightSequence;

            using(var streamBuffer = new MemoryStream())
            {
                streamBuffer.Write(sequenceData, 0, sequenceData.Length);
                streamBuffer.Seek(0, SeekOrigin.Begin);
                lightSequence = new LightSequence(streamBuffer);
            }

            PlayLightSequence(groupNumber, lightSequence, playMode);
        }

        /// <summary>
        /// Plays a chunk of light frames on the device.
        /// </summary>
        /// <param name="groupNumber">The group number to play the chunk on.</param>
        /// <param name="frameCount">The number of frames that <paramref name="chunk"/> contains.</param>
        /// <param name="chunk">The serialized frame data for the chunk.</param>
        /// <param name="playMode">The play mode to use.</param>
        /// <param name="identifier">The identifier for the chunk being played.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        public void PlayChunk(byte groupNumber, uint frameCount, byte[] chunk, StreamingLightsPlayMode playMode, byte identifier)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            var queue = groupQueues[groupNumber];

            var frames = new List<Frame>();
            // Convert the byte data back into a list of frames.
            using(var buffer = new MemoryStream(chunk))
            {
                var framesToRead = frameCount;
                while(framesToRead > 0)
                {
                    var newFrame = new Frame();
                    newFrame.DeserializeByVersion(buffer, 1);
                    frames.Add(newFrame);
                    framesToRead--;
                }
            }

            queue.AddChunk(frames, playMode, identifier);
            // Reset the device first if this is the first command being sent.
            if(!hasBeenReset)
            {
                hasBeenReset = true;
                Reset();
            }

            // Only need to restart the frame sending process on a restart.
            // With continue just rely on the next request from the device for
            // more frames to send the updated sequence.
            if(playMode == StreamingLightsPlayMode.Restart)
            {
                var lightData = queue.GetFrames(out var transmitFrameCount);
                SendLightFrames(groupNumber, transmitFrameCount, lightData, false);
            }
        }

        /// <summary>
        /// Breaks the loop of a segment that is currently looping.
        /// </summary>
        /// <param name="groupNumber">The group number to break the loop on.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// </exception>
        public void BreakLoop(byte groupNumber)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }
        }

        /// <summary>
        /// Set the light intensity of a light group.
        /// </summary>
        /// <param name="intensity">The intensity level to set the group to.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="intensity"/> is greater than 100.
        /// </exception>
        public void SetIntensity(byte intensity)
        {
            // Reset the device first if this is the first command being sent.
            if(!hasBeenReset)
            {
                hasBeenReset = true;
                Reset();
            }

            if(intensity > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), "The intensity cannot be larger than 100.");
            }

            // Device/Group ID 0 is used because this command ignores the device ID field and applies it to the entire device.
            var command = new UsbSingleByteCommandPayload((byte)StreamingLightCommandCode.SetOverallIntensity, 0, intensity);
            SendFeatureCommand(command);
        }

        /// <summary>
        /// Enable or disable the specified symbol highlight features.
        /// </summary>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="enabledFeatures">Features to enable. Features not in this list will be disabled.</param>
        public void EnableSymbolHighlights(byte groupNumber, IList<SymbolHighlightFeature> enabledFeatures)
        {
            var enableSymbolHighlightsCommand = new EnableHighlightsCommandPayload(groupNumber, enabledFeatures);
            SendFeatureCommand(enableSymbolHighlightsCommand);
        }

        /// <summary>
        /// Set Symbol highlights.
        /// </summary>
        /// <param name="groupNumber">The group to set symbol highlight to.</param>
        /// <param name="symbolTrackingData">Symbol tracking data to set.</param>
        /// <param name="symbolHotPositionData">Hot position data to set.</param>
        public void SetSymbolHighlights(byte groupNumber, IList<SymbolTrackingData> symbolTrackingData,
            IList<SymbolHotPositionData> symbolHotPositionData)
        {
            var data = SetSymbolHighlightsData.GenerateByteData(symbolTrackingData, symbolHotPositionData, out var crcValue);

            var setSymbolHighlightCommand = new SetSymbolHighlightsCommandPayload(groupNumber,
                (ushort)(symbolTrackingData.Count + symbolHotPositionData.Count), crcValue);
            SendFeatureCommand(setSymbolHighlightCommand, data);
        }

        /// <summary>
        /// Clear symbol highlights for the specified highlight types.
        /// </summary>
        /// <param name="groupNumber">Group to clear highlights.</param>
        /// <param name="featuresToClear">Highlight features to clear.</param>
        /// <param name="reelIndex">Reel index to clear.</param>
        public void ClearSymbolHighlights(byte groupNumber, IList<SymbolHighlightFeature> featuresToClear, byte reelIndex)
        {
            var data = ClearSymbolHighlightsData.GenerateByteData(featuresToClear, reelIndex, out var crc);

            var clearSymbolHighlightCommand = new ClearSymbolHighlightsCommandPayload(groupNumber, (ushort)featuresToClear.Count, crc);

            SendFeatureCommand(clearSymbolHighlightCommand, data);
        }

        /// <summary>
        /// Sends light frames to a group on the device.
        /// </summary>
        /// <param name="groupNumber">The group number to control.</param>
        /// <param name="numberOfFrames">The number of frames being sent to the device.</param>
        /// <param name="lightData">The light frames to send.</param>
        /// <param name="append">To append the new frames onto the device queue or not.</param>
        /// <returns>True if the command was sent successfully.</returns>
        /// <remarks>
        /// Even if the command is sent successfully to the device, that doesn't ensure
        /// the light frames were successfully accepted into the device queue. If the group is
        /// busy or there isn't enough space, the device will send back a status message indicating
        /// as such.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is larger than or equal to <see cref="groupCount"/>.
        /// Thrown if <paramref name="numberOfFrames"/> is larger than the device queue.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the group doesn't support real time control.
        /// </exception>
        // ReSharper disable once UnusedMethodReturnValue.Local
        private bool SendLightFrames(byte groupNumber, ushort numberOfFrames,
                                     byte[] lightData, bool append)
        {
            if(groupNumber >= groupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(groupNumber));
            }

            if(!groupDescriptors[groupNumber].RealTimeControlSupported)
            {
                throw new InvalidOperationException("Specified group doesn't support real time control.");
            }

            if(numberOfFrames > groupDescriptors[groupNumber].NumberOfFramesSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFrames),
                    $"Too many frames were provided. Got: {numberOfFrames} Maximum Allowed: {groupDescriptors[groupNumber].NumberOfFramesSupported}");
            }

            if(numberOfFrames == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfFrames), "Must send at least one frame.");
            }

            var commandId = GetUniqueCommandId();
            var command = new SendFramesCommand(groupNumber, numberOfFrames,
                                                groupQueues[groupNumber].FrameDisplayThreshold,
                                                commandId, append);

            // The command information needs to be saved in case the group is busy or
            // there isn't enough room in the queue and the command needs to be re-sent.
            lastGroupCommand[groupNumber] = new UsbCommandData(command, lightData);

            // If the frame queue is empty at this point, let the client know since at this point
            // all available frames are about to be sent to the device.
            if(groupQueues[groupNumber].IsEmpty)
            {
                RaiseNotificationEvent(groupNumber, StreamingLightNotificationCode.QueueEmpty);
            }

            return SendFeatureCommand(command, lightData);
        }

        /// <summary>
        /// A thread safe function for getting a new unique command ID.
        /// </summary>
        /// <returns>A unique command ID.</returns>
        private byte GetUniqueCommandId()
        {
            byte commandId;
            lock(commandIdLock)
            {
                commandId = streamingLightCommandId;
                streamingLightCommandId++;
            }

            return commandId;
        }

        /// <summary>
        /// Parses the feature descriptor data for the device and creates the appropriate
        /// device queues.
        /// </summary>
        /// <param name="featureDescriptorData">The feature descriptor data.</param>
        private void ParseFeatureDescriptors(byte[] featureDescriptorData)
        {
            var totalSize = featureDescriptorData[0];
            if(totalSize < StreamingLightFeatureHeaderSize)
            {
                throw new InvalidUsbDeviceDataException(
                    $"The length of the feature descriptor is invalid. Got: {totalSize} Expected at least: {StreamingLightFeatureHeaderSize}");
            }

            if(featureDescriptorData[1] != (byte)UsbDescriptorType.FeatureSpecific)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Invalid feature descriptor type. Got: 0x{featureDescriptorData[1]:X2} Expected: 0x{(byte)UsbDescriptorType.FeatureSpecific:X2}");
            }

            maxTransferSize = BitConverter.ToUInt32(featureDescriptorData, 2);
            maximumSequenceBytes = BitConverter.ToUInt32(featureDescriptorData, 6);

            groupCount = featureDescriptorData[10];
            int offset = StreamingLightFeatureHeaderSize;
            for(var index = 0; index < groupCount; index++)
            {
                var groupDescriptor = new StreamingLightGroupDescriptor();
                groupDescriptor.Unpack(featureDescriptorData, offset);
                offset += groupDescriptor.DataSize;
                groupDescriptors.Add(groupDescriptor);
                if(groupDescriptor.NumberOfFramesSupported > 0)
                {
                    groupQueues.Add((byte)index, new DeviceFrameQueue(groupDescriptor.NumberOfFramesSupported));
                }
            }
        }

        /// <summary>
        /// Sends additional frames to the device when it has reached the requested threshold.
        /// </summary>
        /// <param name="groupNumber">The group number that reported the status.</param>
        /// <param name="status">The requested threshold reached status information.</param>
        private void HandleThresholdReachedStatus(byte groupNumber,
                                                  RequestedThresholdReachedStatus status)
        {
            var queue = groupQueues[groupNumber];
            if(!queue.IsEmpty)
            {
                var lightData = queue.GetFrames(status.NumberOfFrames, out var frameCount);
                if(frameCount > 0)
                {
                    SendLightFrames(groupNumber, frameCount, lightData, true);
                }
            }
        }

        /// <summary>
        /// Resends the last frame command that was sent to the device.
        /// </summary>
        /// <param name="groupNumber">The group number to resend the command on.</param>
        private void ResendFrameCommand(byte groupNumber)
        {
            if(lastGroupCommand.ContainsKey(groupNumber))
            {
                var lastCommand = lastGroupCommand[groupNumber];
                SendFeatureCommand(lastCommand.UsbCommand, lastCommand.Data);

                // Remove the command from the table since there should only ever be a need
                // to re-send it once.
                lastGroupCommand.Remove(groupNumber);
            }
        }

        /// <summary>
        /// Raises the notification event for a given group with the specified code.
        /// </summary>
        /// <param name="groupNumber">The group number reporting the notification.</param>
        /// <param name="code">The notification code to send.</param>
        private void RaiseNotificationEvent(byte groupNumber, StreamingLightNotificationCode code)
        {
            NotificationEvent?.Invoke(this, new StreamingLightsNotificationEventArgs(SubFeatureName, groupNumber, code));
        }

        #endregion
    }
}
