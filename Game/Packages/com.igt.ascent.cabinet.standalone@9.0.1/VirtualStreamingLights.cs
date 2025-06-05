//-----------------------------------------------------------------------
// <copyright file = "VirtualStreamingLights.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using SymbolHighlights;

    /// <summary>
    /// A standalone implementation of the streaming lights interface.
    /// </summary>
    public class VirtualStreamingLights : IStreamingLights
    {
        /// <summary>
        /// The intensity levels for each device and each group within the device.
        /// </summary>
        private readonly Dictionary<string, byte> lightDeviceIntensityLevels = new Dictionary<string, byte>();

        /// <summary>
        /// The default light intensity level.
        /// </summary>
        private const byte DefaultIntensity = 66;

        /// <inheritdoc />
        public byte SupportedLightVersion { get; }

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public VirtualStreamingLights()
        {
            // Use max value to avoid code rework with each additional version.
            SupportedLightVersion = byte.MaxValue;
        }

        /// <inheritdoc />
        public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        /// <inheritdoc />
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            return new List<LightFeatureDescription>();
        }

        /// <inheritdoc />
        public void StartSequenceFile(string featureId, byte groupNumber, string filePath, StreamingLightsPlayMode playMode)
        {
            if(!File.Exists(filePath))
            {
                NotificationEvent?.Invoke(this, new StreamingLightsNotificationEventArgs(featureId, groupNumber, StreamingLightNotificationCode.FileNotFound));
            }
        }

        /// <inheritdoc />
        public void StartSequenceFile(string featureId, byte groupNumber, string sequenceName, byte[] sequenceFile, StreamingLightsPlayMode playMode)
        {
            
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
        }

        /// <inheritdoc />
        public void BreakLoop(string featureId, byte groupNumber)
        {
            
        }

        /// <inheritdoc />
        public void SetIntensity(string featureId, byte intensity)
        {
            lightDeviceIntensityLevels[featureId] = intensity;
        }

        /// <inheritdoc />
        public byte GetIntensity(string featureId)
        {
            return lightDeviceIntensityLevels.ContainsKey(featureId) ?
                lightDeviceIntensityLevels[featureId] : DefaultIntensity;
        }

        /// <inheritdoc />
        public void EnableGivenSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> enabledFeatures)
        {
        }

        /// <inheritdoc />
        public void DisableSymbolHighlights(string featureId, byte groupNumber)
        {
        }

        /// <inheritdoc />
        public void SetSymbolHighlights(string featureId, byte groupNumber, SymbolTrackingData[] trackingData, SymbolHotPositionData[] hotPositionData)
        {
        }

        /// <inheritdoc />
        public void ClearSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
        }

        /// <inheritdoc />
        public void ClearSymbolHighlightReel(string featureId, byte groupNumber, int reelIndex, IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
        }
    }
}
