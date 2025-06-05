// -----------------------------------------------------------------------
// <copyright file = "LegacyPlaybackScheduler.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System.Linq;
    using System.Timers;
    using ChromaBlend;
    using Communication.Cabinet;
    using Streaming;

    /// <summary>
    /// Plays the sequence using the older method of wrapping chunks in full light sequences.
    /// </summary>
    internal class LegacyPlaybackScheduler : PlaybackSchedulerBase
    {
        private readonly Timer playTimer;
        private readonly IDeviceInformation deviceInformation;
        private int activeFrameCount;

        /// <summary>
        /// A lock object for the play timer.
        /// </summary>
        private readonly object playTimerLock = new object();

        /// <summary>
        /// A boolean flag indicating if the play timer has been disposed.
        /// </summary>
        private bool isPlayTimerDisposed;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="groupId">The ID of the group being scheduled.</param>
        /// <param name="player">The streaming lights player to use.</param>
        /// <param name="blender">The light blender for the group.</param>
        /// <param name="deviceInfo">The information on the device being scheduled.</param>
        public LegacyPlaybackScheduler(byte groupId, IStreamingLightPlayer player, ChromaBlender blender, IDeviceInformation deviceInfo) :
            base(groupId, player, blender)
        {
            deviceInformation = deviceInfo;
            playTimer = new Timer { AutoReset = false };
            playTimer.Elapsed += PlayTimerOnElapsed;
            isPlayTimerDisposed = false;
        }

        /// <inheritdoc />
        public override void UpdateDevice(bool restart)
        {
            if(!deviceInformation.DeviceAcquired)
            {
                return;
            }

            if(Blender.LayerCount > 0)
            {
                var sequence = restart || activeFrameCount == 0 ? Blender.CreateLightSequenceFromChunk(deviceInformation.HardwareType)
                    : Blender.CreateLightSequenceFromPreviousChunk(deviceInformation.HardwareType, activeFrameCount);
                // This display time can be 0 if all the frame buffers are at their end and there
                // are no frames to display.
                if(sequence?.DisplayTime > 0)
                {
                    if(!restart)
                    {
                        var numberOfFramesInNewChunk = sequence.Segments.Sum(segment => segment.Frames.Count);
                        if(activeFrameCount != numberOfFramesInNewChunk)
                        {
                            Logging.Log.WriteWarning(
                                $"Device {deviceInformation.HardwareType} was told to update with mode 'continue'" +
                                $" however the number of frames in the new chunk ({numberOfFramesInNewChunk}) " +
                                $"did not match the currently playing chunk ({activeFrameCount}.");
                            return;
                        }
                    }

                    PlaySequenceFromMemory(GroupId, sequence,
                        restart ? StreamingLightsPlayMode.Restart : StreamingLightsPlayMode.Continue);

                    lock(playTimerLock)
                    {
                        // If this isn't a restart the timer should not be reset.
                        if(!isPlayTimerDisposed && (!playTimer.Enabled || restart))
                        {
                            // Ensure playTimer.Enabled is set to false to help prevent lingering light sequences.
                            playTimer.Enabled = false;

                            // Update playTimer.Interval only when necessary to help prevent "KeySystem.Threading.Timer 
                            // already exist in list" exception.
                            if(!playTimer.Interval.Equals(sequence.DisplayTime))
                            {
                                playTimer.Interval = sequence.DisplayTime;
                            }

                            playTimer.Enabled = true;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Shutdown()
        {
            lock(playTimerLock)
            {
                if(!isPlayTimerDisposed)
                {
                    playTimer.Enabled = false;
                    playTimer.Dispose();
                    isPlayTimerDisposed = true;
                }
            }
        }

        /// <summary>
        /// Raised when the playback timer has elapsed.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="elapsedEventArgs">The event arguments.</param>
        private void PlayTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var shouldStartTimer = false;

            // Test if the group is valid before using it.
            var lightSequence = Blender.CreateLightSequenceFromChunk(deviceInformation.HardwareType);
            if(lightSequence.Segments.Any(segment => segment.Frames.Count != 0))
            {
                if(deviceInformation.DeviceAcquired)
                {
                    PlaySequenceFromMemory(GroupId, lightSequence, StreamingLightsPlayMode.Restart);
                }

                shouldStartTimer = true;
            }

            lock(playTimerLock)
            {
                // If the timer hasn't already been enabled, re-enable it.
                // This relies on the assumption that playTimer.Enabled is set to false before invoking this callback,
                // which is true in Mono and MS implementations.
                // Check if the timer has been disposed here because this callback can occur after the Dispose or Stop
                // method has been called or after the Enabled property has been set to false, because the signal to
                // raise the Elapsed event is always queued for execution on a thread pool thread.
                if(!isPlayTimerDisposed && shouldStartTimer && !playTimer.Enabled)
                {
                    playTimer.Interval = lightSequence.DisplayTime;
                    playTimer.Enabled = true;
                }
            }
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
        private void PlaySequenceFromMemory(byte groupId, LightSequence sequence, StreamingLightsPlayMode playMode)
        {
            if(deviceInformation.DeviceAcquired)
            {
                try
                {
                    activeFrameCount = Player.PlaySequenceFromMemory(groupId, sequence, playMode);
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
    }
}