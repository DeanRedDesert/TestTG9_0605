//-----------------------------------------------------------------------
// <copyright file = "IStreamingLights.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using SymbolHighlights;

    /// <summary>
    /// Interface for controlling streaming peripheral lights.
    /// </summary>
    public interface IStreamingLights
    {
        /// <summary>
        /// Gets the currently supported light version.
        /// </summary>
        byte SupportedLightVersion { get; }

        /// <summary>
        /// The various notification event information from the device.
        /// </summary>
        event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        /// <summary>
        /// Gets a list of the streaming light devices currently available.
        /// </summary>
        /// <returns>A list of available light devices.</returns>
        IEnumerable<LightFeatureDescription> GetLightDevices();

        /// <summary>
        /// Play a light sequence file that is stored on disk.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="filePath">The path to the sequence file to start.</param>
        /// <param name="playMode">The play mode to use when starting this sequence.</param>
        void StartSequenceFile(string featureId, byte groupNumber, string filePath, StreamingLightsPlayMode playMode);

        /// <summary>
        /// Play a light sequence file that is stored in memory.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="sequenceName">The name of the sequence.</param>
        /// <param name="sequenceFile">The base 64 encoded version of the sequence file to play.</param>
        /// <param name="playMode">The play mode to use when starting this sequence.</param>
        void StartSequenceFile(string featureId, byte groupNumber, string sequenceName, byte[] sequenceFile,
            StreamingLightsPlayMode playMode);

        /// <summary>
        /// Sends a chunk of frames to be played on the device.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="frameCount">The number of light frames in the data.</param>
        /// <param name="frameData">The raw Feature 121 light frame data.</param>
        /// <param name="playMode">The play mode to use when starting this chunk.</param>
        /// <param name="identifier">The identifier for the chunk being played.</param>
        void SendFrameChunk(string featureId, byte groupNumber, uint frameCount, byte[] frameData,
            StreamingLightsPlayMode playMode, byte identifier);

        /// <summary>
        /// Breaks the playing segment loop on a light group.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        void BreakLoop(string featureId, byte groupNumber);

        /// <summary>
        /// Sets the light intensity of a light group.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="intensity">The intensity level to set the device to.</param>
        void SetIntensity(string featureId, byte intensity);

        /// <summary>
        /// Gets the light intensity of a light group.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <returns>The current light intensity for the device.</returns>
        byte GetIntensity(string featureId);

        /// <summary>
        /// Enable the given symbol highlight features in the specified list, and disable all features that are not included. 
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="enabledFeatures">Features to enable. Features not in this list will be disabled.</param>
        void EnableGivenSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> enabledFeatures);

        /// <summary>
        /// Disable all symbol highlight features.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        void DisableSymbolHighlights(string featureId, byte groupNumber);

        /// <summary>
        /// Set symbol highlights to the device.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="trackingData">Tracking data to send.</param>
        /// <param name="hotPositionData">Hot position data to send.</param>
        void SetSymbolHighlights(string featureId, byte groupNumber, SymbolTrackingData[] trackingData,
                                 SymbolHotPositionData[] hotPositionData);

        /// <summary>
        /// Clear symbol highlights for the specified features.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="featuresToClear">Features to clear.</param>
        void ClearSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> featuresToClear);

        /// <summary>
        /// Clear symbol highlights for the specified reel.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The target light group number.</param>
        /// <param name="reelIndex">Reel to clear features.</param>
        /// <param name="featuresToClear">Features to clear.</param>
        void ClearSymbolHighlightReel(string featureId, byte groupNumber, int reelIndex, IEnumerable<SymbolHighlightFeature> featuresToClear);
    }
}
