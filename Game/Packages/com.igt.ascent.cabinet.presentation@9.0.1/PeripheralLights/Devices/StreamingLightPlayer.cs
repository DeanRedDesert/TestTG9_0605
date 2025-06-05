// -----------------------------------------------------------------------
// <copyright file = "StreamingLightPlayer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ChromaBlend;
    using Communication.Cabinet;
    using Streaming;

    /// <summary>
    /// Handles playing streaming light data on the device.
    /// </summary>
    /// <remarks>
    /// This is broken out from the streaming light device class to make it easier to unit test the light functionality.
    /// </remarks>
    internal class StreamingLightPlayer : IStreamingLightPlayer
    {
        /// <summary>
        /// The initialize of the chunk buffer. 30 frames * 30 bytes.
        /// </summary>
        private const int ChunkBufferInitSize = 30 * 30;
        private readonly string name;

        /// <summary>
        /// Construct a new instance given a feature name and a streaming light interface.
        /// </summary>
        /// <param name="featureName">The feature name of the device.</param>
        /// <param name="lightsInterface">The streaming light interface to use.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="featureName"/> is null or empty.
        /// </exception>
        public StreamingLightPlayer(string featureName, IStreamingLights lightsInterface)
        {
            if(string.IsNullOrEmpty(featureName))
            {
                throw new ArgumentException("The feature name cannot be null or empty.", nameof(featureName));
            }

            name = featureName;
            LightInterface = lightsInterface;
        }

        #region Implementation of IStreamingLightPlayer

        /// <inheritdoc />
        public IStreamingLights LightInterface { get; set; }

        /// <inheritdoc />
        public int PlaySequenceFromMemory(byte groupId, ILightSequence sequence, StreamingLightsPlayMode playMode)
        {
            if(LightInterface.SupportedLightVersion < sequence.Version || sequence.Version < 1)
            {
                throw new InvalidLightSequenceException
                (
                    $"Version {sequence.Version} LightSequence not supported on this Foundation. This Foundation only supports up to version {LightInterface.SupportedLightVersion}."
                );
            }
            LightInterface.StartSequenceFile(name, groupId, sequence.UniqueId,
                                                       sequence.GetSequenceBytes(), playMode);
            return sequence.Segments.Sum(segment => segment.Frames.Count);
        }

        /// <inheritdoc />
        public void PlayChunk(byte groupId, IList<Frame> chunk, StreamingLightsPlayMode playMode, byte identifier)
        {
            byte[] serializedFrameData;
            using(var buffer = new MemoryStream(ChunkBufferInitSize))
            {
                foreach(var frame in chunk)
                {
                    frame.SerializeByVersion(buffer, LightInterface.SupportedLightVersion);
                }
                buffer.Seek(0, SeekOrigin.Begin);
                serializedFrameData = new byte[buffer.Length];
                buffer.Read(serializedFrameData, 0, serializedFrameData.Length);
            }

            LightInterface.SendFrameChunk(name, groupId, Convert.ToUInt32(chunk.Count), serializedFrameData, playMode, identifier);
        }

        #endregion
    }
}