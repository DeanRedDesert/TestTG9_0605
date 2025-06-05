//-----------------------------------------------------------------------
// <copyright file = "StreamingLightsNotificationEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event arguments for streaming light notification events.
    /// </summary>
    public class StreamingLightsNotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="featureId">The interface name that generated the event.</param>
        /// <param name="groupId">The group ID the event is for.</param>
        /// <param name="notificationCode">The notification code from the device.</param>
        public StreamingLightsNotificationEventArgs(string featureId, byte groupId,
                                                    StreamingLightNotificationCode notificationCode)
        {
            FeatureId = featureId;
            GroupId = groupId;
            NotificationCode = notificationCode;
        }

        #region Properties

        /// <summary>
        /// Gets the feature name for the device that generated the event.
        /// </summary>
        public string FeatureId { get; }

        /// <summary>
        /// Gets the group ID the event is for.
        /// </summary>
        public byte GroupId { get; }

        /// <summary>
        /// Gets the notification code sent from the device.
        /// </summary>
        public StreamingLightNotificationCode NotificationCode { get; }

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{FeatureId}/{GroupId}/{NotificationCode}";
        }
    }
}