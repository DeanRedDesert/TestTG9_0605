//-----------------------------------------------------------------------
// <copyright file = "UsbStreamingLight.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using ChromaBlend;
    using Communication.Cabinet;
    using Streaming;
    using StreamingLightGroup = Communication.Cabinet.StreamingLightGroup;

    /// <summary>
    /// This class represents a USB light device that supports streaming and
    /// is controlled via the IStreamingLights interface.
    /// </summary>
    public class UsbStreamingLight : UsbLightBase, IStreamingDeviceInformation
    {
        #region Constants

        private static readonly Color DefaultRecoveryColor = Color.Blue;

        #endregion

        #region Fields

        private LightSequence recoverySequence;
        private Color recoveryColor;
        private bool deviceAcquired;
        private readonly Dictionary<byte, ChromaBlender> blenders = new Dictionary<byte, ChromaBlender>();
        private readonly Dictionary<byte, PlaybackSchedulerBase> schedulers = new Dictionary<byte, PlaybackSchedulerBase>();
        private IStreamingLights streamingLightsInterface;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UsbStreamingLight"/>.
        /// </summary>
        /// <param name="featureName">The feature name of the light device.</param>
        /// <param name="featureDescription">The feature description of the light device.</param>
        /// <param name="streamingLights">The interface to use to communicate with the streaming lights hardware.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="streamingLights"/> is null.
        /// </exception>
        internal UsbStreamingLight(string featureName, LightFeatureDescription featureDescription, IStreamingLights streamingLights)
            : base(featureName, featureDescription)
        {
            IsEnabled = true;

            streamingLightsInterface = streamingLights ?? throw new ArgumentNullException(nameof(streamingLights));

            if(FeatureDescription != null)
            {
                SupportsIntensityControl = FeatureDescription.Groups.Cast<StreamingLightGroup>()
                                                             .Any(group => group.IntensityControlSupported);

                SupportsSymbolHighlightsControl = FeatureDescription.Groups.Cast<StreamingLightGroup>()
                                                             .Any(group => group.SymbolHighlightsSupported);
            }

            Player = new StreamingLightPlayer(featureName, streamingLightsInterface);
            CreateGroups();

            recoveryColor = DefaultRecoveryColor;
        }

        #endregion

        #region IStreamingDeviceInformation Members

        /// <summary>
        /// Device notification events.
        /// </summary>
        public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        #endregion

        #region IDeviceInformation Overrides

        /// <inheritdoc/>
        public override bool DeviceAcquired
        {
            get => deviceAcquired;
            internal set
            {
                if(deviceAcquired != value)
                {
                    deviceAcquired = value;
                    if(deviceAcquired && StreamingLightsInterface != null)
                    {
                        var sequence = recoverySequence ?? CreateSingleFrameLightSequence(0, recoveryColor);
                        if(sequence != null)
                        {
                            PlaySequenceFromMemory(0, sequence, StreamingLightsPlayMode.Restart);
                        }
                    }
                }
            }
        }

        #endregion

        #region UsbLightBase Overrides

        /// <inheritdoc/>
        public override void SetColor(byte groupId, Color color)
        {
            SetColor(groupId, color, 0, null);
        }

        /// <inheritdoc/>
        public override void SetLightState(byte groupId, bool lightOn)
        {
            var color = lightOn ? Color.White : Color.Black;

            // SetLightState is not counted as client light content.
            // Do not set the client content present flag.
            SettingNonClientContent = true;

            SetColor(groupId, color, 0, null);

            SettingNonClientContent = false;
        }

        /// <inheritdoc/>
        internal override void SetUniversalColor(Color universalColor)
        {
            SettingNonClientContent = true;

            // Streaming Light does not support AllGroups.
            for(var groupId = 0; groupId < GroupCount; groupId++)
            {
                SetColor((byte)groupId, universalColor);
            }

            // For now, change the recovery color for SetUniversalColor call only.
            // Whether to do the same for SetColor needs more investigation.
            SetRecoveryColor(universalColor);

            SettingNonClientContent = false;
        }

        /// <inheritdoc/>
        internal override void SetFeatureDescription(LightFeatureDescription featureDescription)
        {
            var oldGroupCount = GroupCount;

            base.SetFeatureDescription(featureDescription);
            if(FeatureDescription != null)
            {
                if(GroupCount != oldGroupCount)
                {
                    foreach(var groupScheduler in schedulers)
                    {
                        groupScheduler.Value.Shutdown();
                    }
                    blenders.Clear();
                    schedulers.Clear();
                    CreateGroups();
                }

                SupportsIntensityControl = FeatureDescription.Groups.Cast<StreamingLightGroup>()
                    .Any(group => group.IntensityControlSupported);
				SupportsSymbolHighlightsControl = FeatureDescription.Groups.Cast<StreamingLightGroup>()
                    .Any(group => group.SymbolHighlightsSupported);											 
															 
            }
        }

        /// <inheritdoc/>
        protected override bool ShouldLightCategoryErrorBeReported(Exception exception)
        {
            return exception is StreamingLightCategoryException streamingLightCategoryException &&
                   streamingLightCategoryException.ErrorCode != StreamingLightNotificationCode.ClientDoesNotOwnResource.ToString() &&
                   streamingLightCategoryException.ErrorCode != StreamingLightNotificationCode.DeviceInTiltState.ToString();
        }

        #endregion

        #region Properties

        /// <summary>
        /// <code>True if this device is enabled.</code>
        /// </summary>
        public bool IsEnabled { get; internal set; }

        /// <summary>
        /// Gets the flag indicating if the device supports intensity control or not.
        /// </summary>
        public bool SupportsIntensityControl { get; private set; }

        /// <summary>
        /// Get the flag indicating if the device supports symbol highlights.
        /// </summary>
        public bool SupportsSymbolHighlightsControl { get; private set; }

        /// <summary>
        /// Gets or sets the interface for playing light sequences.
        /// </summary>
        protected internal IStreamingLightPlayer Player { get; set; }

        /// <summary>
        /// Gets or sets the interface used to control the streaming lights.
        /// </summary>
        protected internal IStreamingLights StreamingLightsInterface
        {
            get => streamingLightsInterface;
            internal set
            {
                streamingLightsInterface = value;
                Player.LightInterface = value;
                if(value != null)
                {
                    foreach(var blender in blenders.Values)
                    {
                        blender.LightSequenceVersion = streamingLightsInterface.SupportedLightVersion;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the path where the game executable is installed.
        /// </summary>
        protected internal string GameMountPoint { get; internal set; }

        #endregion

        #region Play Light Sequence

        /// <summary>
        /// Plays a light sequence from a file.
        /// </summary>
        /// <param name="groupId">The group ID to play the sequence on.</param>
        /// <param name="fileName">The light sequence to play.</param>
        /// <param name="layerNumber">The layer number to play the sequence on. Default layer is 0</param>
        /// <param name="blendEffect">The blending effect to use on the sequence. Default blendEffect is null.</param>
        /// <param name="restart">Specify if you would like the sequence to restart or continue from the current position.
        /// If continuing the sequence, the number of segments and frames must be the same as the number of frames and segments in the sequence 
        /// currently playing on the device. Default is true.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if <paramref name="fileName"/> cannot be found.
        /// </exception>
        /// <exception cref="StreamingLightHardwareMismatchException">
        /// Thrown if the specified light sequence has a different hardware type
        /// than the current device.
        /// </exception>
        public void PlayLightSequence(byte groupId, string fileName, byte layerNumber = 0,
            IBlendEffect blendEffect = null, bool restart = true)
        {
            if(!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(GameMountPoint, fileName);
            }

            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException("Unable to find specified light sequence file.", fileName);
            }

            var sequence = new LightSequence(fileName);

            PlayLightSequence(groupId, sequence, layerNumber, blendEffect, restart);
        }

        /// <summary>
        /// Plays a light sequence from memory.
        /// </summary>
        /// <param name="groupId">The group ID to play the sequence on.</param>
        /// <param name="sequence">The light sequence to play.</param>
        /// <param name="layerNumber">The layer number to play the sequence on. Default layer is 0</param>
        /// <param name="blendEffect">The blending effect to use on the sequence. Default blendEffect is null.</param>
        /// <param name="restart">Specify if you would like the sequence to restart or continue from the current position.
        /// If continuing the sequence, the number of segments and frames must be the same as the number of frames and segments in the sequence 
        /// currently playing on the device. Default is true</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sequence"/> is null.
        /// </exception>
        /// <exception cref="StreamingLightHardwareMismatchException">
        /// Thrown if the specified light sequence has a different hardware type
        /// than the current device.
        /// </exception>
        public void PlayLightSequence(byte groupId, LightSequence sequence, byte layerNumber = 0,
            IBlendEffect blendEffect = null, bool restart = true)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            // Check to make sure the sequence is intended for this device.
            CheckSequenceHardwareTypeCompatibility(sequence);

            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!IsEnabled)
            {
                return;
            }

            blenders[groupId].AddLayer(sequence, layerNumber, blendEffect);

            UpdateDeviceWithCurrentSequence(groupId, restart);
        }

        /// <summary>
        /// Returns a list of names generated from the LightSequence objects within the specified group.
        /// </summary>
        /// <param name="groupId">The group ID to clear the layers on.</param>
        public IDictionary<byte, string> GetLightSequenceNamesFromGroup(byte groupId)
        {
            return blenders.TryGetValue(groupId, out var blender)
                       ? blender.GetLightSequenceNames()
                       : null;
        }

        #endregion

        #region Color and Sequence Update Functions

        /// <summary>
        /// Sets Color by layer.
        /// </summary>
        /// <param name="groupId">The group ID to set the color on.</param>
        /// <param name="color">The color to set.</param>
        /// <param name="layerNumber">The layer to set the color on.</param>
        /// <param name="effect">The blending effect to use on the layer.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="color"/> is empty.
        /// </exception>
        public void SetColor(byte groupId, Color color, byte layerNumber, IBlendEffect effect)
        {
            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(!IsEnabled)
            {
                return;
            }

            if(!IsValidandReady(groupId) || !DeviceAcquired)
            {
                return;
            }

            var sequence = CreateSingleFrameLightSequence(groupId, color);
            blenders[groupId].AddLayer(sequence, layerNumber, effect);
            UpdateDeviceWithCurrentSequence(groupId, true);
        }

        /// <summary>
        /// Tells the currently looping segment to stop looping and continue
        /// to the next sequence at the end of the current segment.
        /// </summary>
        /// <param name="groupId">The group ID to change.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        public void BreakLoop(byte groupId)
        {
            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!IsEnabled)
            {
                return;
            }

            StreamingLightsInterface.BreakLoop(FeatureName, groupId);
        }

        /// <summary>
        /// Updates the specified light group with the current blended sequences in memory.
        /// If all the sequences in memory have completed playing, the device is not changed.
        /// </summary>
        /// <remarks>This is useful when combined with the <see cref="AddLayer"/> function.</remarks>
        /// <param name="groupId">The group ID to update.</param>
        /// <param name="restart">Restart the sequence.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        public void UpdateDeviceWithCurrentSequence(byte groupId, bool restart = false)
        {
            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!IsEnabled)
            {
                return;
            }

            schedulers[groupId].UpdateDevice(restart);

            FlagClientContentPresent();
        }

        /// <summary>
        /// Sets a recovery sequence of lights, this will be used any time the game is given the light devices
        /// while in the middle of processing lights.
        /// </summary>
        /// <param name="sequence">The sequence to play during recovery.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sequence"/> is null.
        /// </exception>
        public void SetRecoverySequence(LightSequence sequence)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence), "Can not set the recovery sequence to null.");
            }

            if(!IsEnabled)
            {
                return;
            }

            recoverySequence = sequence;
        }

        /// <summary>
        /// Sets the recovery sequence to a single color, this will be used any time the game is given the light
        /// devices while in the middle of processing lights.
        /// </summary>
        /// <param name="color">The color to set the lights to during recovery.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="color"/> is empty.
        /// </exception>
        public void SetRecoveryColor(Color color)
        {
            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.");
            }

            if(!IsEnabled)
            {
                return;
            }

            recoveryColor = color;
        }

        #endregion

        #region Layer Functions

        /// <summary>
        /// Adds a new layer to the device but does not trigger an update of the device with the new
        /// sequence.
        /// </summary>
        /// <param name="groupId">The group ID to change.</param>
        /// <param name="fileName">The filename of the light sequence to add.</param>
        /// <param name="layerNumber">The layer to play the sequence on.</param>
        /// <param name="effect">The blending effect to use for the layer.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown if <paramref name="fileName"/> cannot be found.
        /// </exception>
        /// <exception cref="StreamingLightHardwareMismatchException">
        /// Thrown if the specified light sequence has a different hardware type
        /// than the current device.
        /// </exception>
        public void AddLayer(byte groupId, string fileName, byte layerNumber, IBlendEffect effect)
        {
            if(!Path.IsPathRooted(fileName))
            {
                fileName = Path.Combine(GameMountPoint, fileName);
            }

            if(!File.Exists(fileName))
            {
                throw new FileNotFoundException("Unable to find specified light sequence file.", fileName);
            }

            var sequence = new LightSequence(fileName);
            if((Hardware)sequence.LightDevice != HardwareType)
            {
                throw new StreamingLightHardwareMismatchException(
                    $"The sequence {fileName} is not intended for this device.",
                    (Hardware)sequence.LightDevice, HardwareType);
            }

            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!IsEnabled)
            {
                return;
            }

            blenders[groupId].AddLayer(sequence, layerNumber, effect);
        }

        /// <summary>
        /// Removes a currently playing layer from the device.
        /// </summary>
        /// <param name="groupId">The group ID to change.</param>
        /// <param name="layerNumber">The layer number to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        public void RemoveLayer(byte groupId, byte layerNumber)
        {
            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!IsEnabled)
            {
                return;
            }

            blenders[groupId].RemoveLayer(layerNumber);

            if(GetLayerCount(groupId) < 1)
            {
                // If there are no layers left, turn the device off.
                TurnDeviceOff(groupId);
            }
        }

        /// <summary>
        /// Gets the number of layers in a given group.
        /// </summary>
        /// <param name="groupId">The group ID to get the layer count for.</param>
        /// <returns>The number of layers being blended in the group.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        public int GetLayerCount(byte groupId)
        {
            return IsValidandReady(groupId) ? blenders[groupId].LayerCount : 0;
        }

        /// <summary>
        /// Clears the layers from a group. After the layers are cleared
        /// the device is not updated. Layer zero is preserved unless <paramref name="clearLayerZero"/>
        /// is true.
        /// </summary>
        /// <param name="groupId">
        /// The group ID to clear the layers on.
        /// </param>
        /// <param name="clearLayerZero">
        /// If true layer zero will be wiped from the device.
        /// If false all layers except zero will be cleared.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups,
        /// or no blender is found for <paramref name="groupId"/>.
        /// </exception>
        public void ClearLayers(byte groupId, bool clearLayerZero)
        {
            if(!IsValidandReady(groupId))
            {
                return;
            }

            if(!blenders.ContainsKey(groupId))
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"No blender is found for group {groupId}.");
            }

            if(!IsEnabled)
            {
                return;
            }

            if(clearLayerZero)
            {
                blenders[groupId].Clear();
            }
            else
            {
                blenders[groupId].ClearAllLayersButZero();
            }
        }

        /// <summary>
        /// Clears the layers from all groups then turns the devices off
        /// Layer zero is preserved unless <paramref name="clearLayerZero"/>
        /// is true.
        /// </summary>
        /// <param name="clearLayerZero">
        /// If true layer zero will be wiped from the device.
        /// If false all layers except zero will be cleared.
        /// </param>
        public void ClearGroups(bool clearLayerZero)
        {
            foreach (var groupId in blenders.Keys)
            {
                ClearLayers(groupId, clearLayerZero);
                TurnDeviceOff(groupId);
            }
        }

        #endregion

        #region Object Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var debugText = new StringBuilder();

            debugText.AppendFormat("-- Information for {0} --\n", HardwareType);
            debugText.AppendFormat("Acquired: {0}\n", DeviceAcquired);
            if(DeviceAcquired)
            {
                debugText.AppendFormat("Supports intensity control: {0}\n", SupportsIntensityControl);
                debugText.AppendFormat("Supports symbol highlight control: {0}\n", SupportsSymbolHighlightsControl);
                debugText.AppendFormat("Number of groups: {0}\n", GroupCount);
                if(GroupCount > 0)
                {
                    for(byte groupId = 0; groupId < GroupCount; groupId++)
                    {
                        debugText.AppendFormat("- Group {0} -\n", groupId);
                        debugText.AppendFormat("Light count: {0}\n", GetLightCount(groupId));
                    }
                }
            }
            else
            {
                debugText.AppendFormat("Found at creation: {0}\n", WasFeatureFoundAtCreation);
                if(AcquireFailureReason.HasValue)
                {
                    debugText.AppendFormat("Failure reason: {0}\n", AcquireFailureReason.Value);
                }
                else
                {
                    debugText.AppendLine("No failure reason was provided.");
                }
            }

            return debugText.ToString();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the light intensity of the light device.
        /// </summary>
        /// <param name="intensity">The light intensity to set the device to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="intensity"/> is larger than 100.
        /// </exception>
        /// <exception cref="StreamingLightCategoryException">
        /// Thrown if an error occurs when setting the intensity.
        /// </exception>
        internal void SetIntensity(byte intensity)
        {
            if(intensity > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(intensity), "The intensity value cannot be larger than 100.");
            }

            if(DeviceAcquired && SupportsIntensityControl)
            {
                try
                {
                    StreamingLightsInterface.SetIntensity(FeatureName, intensity);
                }
                catch(StreamingLightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }
                
            }
        }

        /// <summary>
        /// Gets the light intensity for a light group.
        /// </summary>
        /// <returns>The light intensity level or 0 if the device isn't acquired.</returns>
        /// <exception cref="StreamingLightCategoryException">
        /// Thrown if an error occurs when getting the intensity.
        /// </exception>
        internal byte GetIntensity()
        {
            if(DeviceAcquired && SupportsIntensityControl)
            {
                try
                {
                    return StreamingLightsInterface.GetIntensity(FeatureName);
                }
                catch(StreamingLightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Raises notification events for this device.
        /// </summary>
        /// <param name="args">The event arguments.</param>
        internal void RaiseNotificationEvent(StreamingLightsNotificationEventArgs args)
        {
            NotificationEvent?.Invoke(this, args);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Test if a groupId is valid and if the device is ready.
        /// </summary>
        /// <param name="groupId">The group ID to access.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups the device has, or is AllGroups.
        /// </exception>
        protected bool IsValidandReady(byte groupId)
        {
            ValidateGroupId(groupId);

            // Must have at least 1 group.
            return GroupCount != 0;
        }

        /// <summary>
        /// Gets the number of lights in the device.
        /// If the device is unavailable, 0 is returned.
        /// </summary>
        /// <param name="groupId">The group id to get the light count for.</param>
        /// <returns>The number of lights in the group.</returns>
        protected ushort GetLightCount(byte groupId)
        {
            ushort result = 0;

            if(DeviceAcquired)
            {
                var groupInfo = GetGroupInformation(groupId);
                if(groupInfo != null)
                {
                    result = groupInfo.Count;
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a light sequence that has one frame of a solid color.
        /// </summary>
        /// <param name="groupId">The group ID the sequence is for.</param>
        /// <param name="color">The color of the frame.</param>
        /// <returns>A single frame light sequence.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if this function is called while the device hasn't been acquired.
        /// </exception>
        protected LightSequence CreateSingleFrameLightSequence(byte groupId, Color color)
        {
            if(!DeviceAcquired)
            {
                throw new InvalidOperationException("Light sequence cannot be created while the device is not acquired.");
            }

            var count = GetLightCount(groupId);
            if(count == 0)
            {
                return null;
            }

            var sequence = new LightSequence((StreamingLightHardware)HardwareType, 1)
                               {
                                   Name = "Auto generated single light frame",
                               };
            var segment = new Segment();
            var colorList = new List<Color>(Enumerable.Repeat(color, count));
            var frame = new Frame(colorList)
                            {
                                DisplayTime = 1000,
                            };

            segment.AddFrame(frame);
            sequence.AddSegment(segment);

            return sequence;
        }

        /// <summary>
        /// Plays a light sequence from memory.
        /// </summary>
        /// <param name="groupId">The group ID to play the sequence on.</param>
        /// <param name="sequence">The light sequence to play.</param>
        /// <param name="playMode">The play mode to use when playing the light sequence.</param>
        /// <exception cref="InvalidLightSequenceException">
        /// Thrown if <paramref name="sequence"/> is a version that is not supported.
        /// </exception>
        /// <exception cref="StreamingLightCategoryException">
        /// Thrown if an error occurs when playing the sequence.
        /// </exception>
        protected void PlaySequenceFromMemory(byte groupId, LightSequence sequence, StreamingLightsPlayMode playMode)
        {
            if(DeviceAcquired && IsEnabled)
            {
                try
                {
                    Player.PlaySequenceFromMemory(groupId, sequence, playMode);
                }
                catch(StreamingLightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Turns the device off without using the light blender.
        /// </summary>
        /// <param name="groupId">The group ID to turn off.</param>
        protected void TurnDeviceOff(byte groupId)
        {
            SetLightState(groupId, false);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Populates the dictionaries for the light groups.
        /// </summary>
        private void CreateGroups()
        {
            for(byte groupId = 0; groupId < GroupCount; ++groupId)
            {
                blenders.Add(groupId, new ChromaBlender());
                if(streamingLightsInterface.SupportedLightVersion < 3)
                {
                    schedulers.Add(groupId, new LegacyPlaybackScheduler(groupId, Player, blenders[groupId],
                        this));
                }
                else
                {
                    schedulers.Add(groupId, new ChunkPlaybackScheduler(groupId, Player, blenders[groupId],
                        this));
                }
            }
        }

        /// <summary>
        /// Checks to see if the sequence can be played on the streaming lights. 
        /// Generic back lit toppers and video toppers can play the same light sequence, 
        /// as they have the same LED configuration.
        /// </summary>
        /// <param name="sequence">Light sequence to perform the check on.</param>
        /// <returns>True if the light sequence will play on this streaming light; False otherwise.</returns>
        private void CheckSequenceHardwareTypeCompatibility(ILightSequence sequence)
        {
            var sequenceHardware = (Hardware)sequence.LightDevice;

            if(sequenceHardware != HardwareType &&
               !(sequenceHardware == Hardware.VideoTopper && HardwareType == Hardware.GenericBacklitTopper) &&
               !(sequenceHardware == Hardware.GenericBacklitTopper && HardwareType == Hardware.VideoTopper))
            {
                throw new StreamingLightHardwareMismatchException(
                    $"The sequence {sequence.Name} is not intended for this device.",
                    (Hardware)sequence.LightDevice, HardwareType);
            }
        }

        #endregion
    }
}
