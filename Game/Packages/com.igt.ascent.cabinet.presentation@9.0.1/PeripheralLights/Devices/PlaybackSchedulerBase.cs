// -----------------------------------------------------------------------
// <copyright file = "PlaybackSchedulerBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using ChromaBlend;
    using Communication.Cabinet;

    /// <summary>
    /// Base class for playing light sequences on a timer.
    /// </summary>
    internal abstract class PlaybackSchedulerBase
    {
        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="groupId">The ID of the group being scheduled.</param>
        /// <param name="player">The streaming lights player to use.</param>
        /// <param name="blender">The light blender for the group.</param>
        protected PlaybackSchedulerBase(byte groupId, IStreamingLightPlayer player, ChromaBlender blender)
        {
            GroupId = groupId;
            Blender = blender;
            Player = player;
        }

        #region Properties

        /// <summary>
        /// Gets the blender for this group.
        /// </summary>
        protected ChromaBlender Blender { get; }

        /// <summary>
        /// Gets the player for the device.
        /// </summary>
        protected IStreamingLightPlayer Player { get; }

        /// <summary>
        /// Gets the ID of the group this object is scheduling.
        /// </summary>
        protected byte GroupId { get; }

        #endregion

        /// <summary>
        /// Updates the device with the current state of the blender.
        /// </summary>
        /// <param name="restart">
        /// True if the buffers should be played from the beginning. False to update at the current position.
        /// </param>
        public abstract void UpdateDevice(bool restart);

        /// <summary>
        /// Shuts the scheduler down.
        /// </summary>
        public abstract void Shutdown();

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Scheduler for group {GroupId}";
        }

        /// <summary>
        /// Determines if a given StreamingLightCategoryException should be reported or not.
        /// </summary>
        /// <param name="exception">
        /// The exception that was thrown.
        /// </param>
        /// <returns>True if the exception should be thrown/reported.</returns>
        protected bool ShouldLightCategoryErrorBeReported(StreamingLightCategoryException exception)
        {
            return exception != null &&
                   exception.ErrorCode != StreamingLightNotificationCode.ClientDoesNotOwnResource.ToString() &&
                   exception.ErrorCode != StreamingLightNotificationCode.DeviceInTiltState.ToString();
        }
    }
}