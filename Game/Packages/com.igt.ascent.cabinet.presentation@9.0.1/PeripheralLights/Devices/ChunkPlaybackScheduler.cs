// -----------------------------------------------------------------------
// <copyright file = "ChunkPlaybackScheduler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using ChromaBlend;
    using Communication.Cabinet;
    using Streaming;

    /// <summary>
    /// The chunk based playback scheduler. (Requires G series or later)
    /// </summary>
    internal class ChunkPlaybackScheduler : PlaybackSchedulerBase
    {
        private readonly IStreamingDeviceInformation deviceInformation;
        private int activeFrameCount;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="groupId">The ID of the group being scheduled.</param>
        /// <param name="player">The streaming lights player to use.</param>
        /// <param name="blender">The light blender for the group.</param>
        /// <param name="deviceInfo">The information on the device being scheduled.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="deviceInfo"/> is null.
        /// </exception>
        public ChunkPlaybackScheduler(byte groupId, IStreamingLightPlayer player, ChromaBlender blender, IStreamingDeviceInformation deviceInfo)
            : base(groupId, player, blender)
        {
            deviceInformation = deviceInfo ?? throw new ArgumentNullException(nameof(deviceInfo));
            deviceInformation.NotificationEvent += OnDeviceNotificationEvent;
        }

        #region Overrides of PlaybackSchedulerBase

        /// <inheritdoc />
        public override void UpdateDevice(bool restart)
        {
            if(!deviceInformation.DeviceAcquired)
            {
                return;
            }

            if(Blender.LayerCount > 0)
            {
                byte chunkIdentifier;
                var sequence = restart || activeFrameCount == 0 ? Blender.GetNextChunk(out chunkIdentifier)
                    : Blender.ReBlendPreviousChunk(activeFrameCount, out chunkIdentifier);

                // If there are no frames returned then all the buffers are at their end and there is nothing to display.
                if(sequence != null && sequence.Count > 0)
                {
                    if(!restart)
                    {
                        var numberOfFramesInNewChunk = sequence.Count;
                        if(activeFrameCount != numberOfFramesInNewChunk)
                        {
                            Logging.Log.WriteWarning(
                                $"Device {deviceInformation.HardwareType} was told to update with mode 'continue' however the number of frames in the new chunk ({numberOfFramesInNewChunk}) did not match the currently playing chunk ({activeFrameCount}.");
                            return;
                        }
                    }

                    activeFrameCount = sequence.Count;
                    PlayChunk(sequence,
                        restart ? StreamingLightsPlayMode.Restart : StreamingLightsPlayMode.Continue,
                        chunkIdentifier);
                }
            }
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            deviceInformation.NotificationEvent -= OnDeviceNotificationEvent;
        }

        #endregion

        /// <summary>
        /// Plays a chunk of frames and supresses light category exceptions that can be ignored.
        /// </summary>
        /// <param name="chunk">The chunk of frames to play.</param>
        /// <param name="playMode">The play mode to use.</param>
        /// <param name="identifier">The identifier for the chunk being played.</param>
        private void PlayChunk(IList<Frame> chunk, StreamingLightsPlayMode playMode, byte identifier)
        {
            try
            {
                Player.PlayChunk(GroupId, chunk, playMode, identifier);
            }
            catch(StreamingLightCategoryException ex)
            {
                if(ShouldLightCategoryErrorBeReported(ex))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Handles the device notification event and sends another chunk if the queue is empty.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void OnDeviceNotificationEvent(object sender, StreamingLightsNotificationEventArgs eventArgs)
        {
            if(eventArgs.GroupId == GroupId && eventArgs.NotificationCode == StreamingLightNotificationCode.QueueEmpty)
            {
                // The foundation queue is now empty and additional chunks need to be enqueued.
                var newChunk = Blender.GetNextChunk(out var chunkIdentifier);
                if(newChunk != null && newChunk.Count > 0)
                {
                    activeFrameCount = newChunk.Count;
                    PlayChunk(newChunk, StreamingLightsPlayMode.Queue, chunkIdentifier);
                }
            }
        }
    }
}