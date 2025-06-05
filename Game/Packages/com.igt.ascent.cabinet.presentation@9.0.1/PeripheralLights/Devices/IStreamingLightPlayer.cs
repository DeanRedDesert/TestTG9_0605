// -----------------------------------------------------------------------
// <copyright file = "IStreamingLightPlayer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System.Collections.Generic;
    using Communication.Cabinet;
    using Streaming;

    /// <summary>
    /// The interface for playing a streaming light sequence.
    /// </summary>
    public interface IStreamingLightPlayer
    {
        /// <summary>
        /// Gets or sets the cabinet interface for the streaming lights.
        /// </summary>
        IStreamingLights LightInterface { get; set; }

        /// <summary>
        /// Plays a light sequence from a sequence stored in memory.
        /// </summary>
        /// <param name="groupId">The group ID of the device to play on.</param>
        /// <param name="sequence">The light sequence to play.</param>
        /// <param name="playMode">The play mode to use.</param>
        /// <returns>The number of frames played.</returns>
        int PlaySequenceFromMemory(byte groupId, ILightSequence sequence, StreamingLightsPlayMode playMode);

        /// <summary>
        /// Plays a light sequence chunk.
        /// </summary>
        /// <param name="groupId">The group ID of the device to play on.</param>
        /// <param name="chunk">The chunk of frames to play.</param>
        /// <param name="playMode">The play mode to use.</param>
        /// <param name="identifier">The identifier of the chunk being played.</param>
        void PlayChunk(byte groupId, IList<Frame> chunk, StreamingLightsPlayMode playMode, byte identifier);
    }
}