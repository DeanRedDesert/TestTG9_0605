//-----------------------------------------------------------------------
// <copyright file = "NotificationType.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;

    /// <summary>
    /// Notification type is used to determine how a piece of data should be updated for the presentation.
    /// </summary>
    [Serializable]
    public enum NotificationType
    {
        /// <summary>
        /// The data will be updated on a state change except history state.
        /// </summary>
        Synchronous,
        /// <summary>
        /// The data will be updated when the data changes except history state.
        /// </summary>
        Asynchronous,
        /// <summary>
        /// The notification is disabled. The data will not be updated.
        /// </summary>
        Disabled,
        /// <summary>
        /// The data will be updated on a state change in History mode. The data 
        /// value is based on the real time game status rather than a cached copy 
        /// (e.g. a history record).
        /// </summary>
        SynchronousHistory,
        /// <summary>
        /// The data will be updated when the data changes in History mode. The 
        /// data value is based on the real time game status rather than a cached 
        /// copy (e.g. a history record).
        /// </summary>
        AsynchronousHistory
    }

    /// <summary>
    /// The extension method of NotificationType.
    /// </summary>
    public static class NotificationTypeExtension
    {
        /// <summary>
        /// To verify if the notification type is asynchronous except history notification.
        /// </summary>
        /// <param name="notificationType">The notification to check.</param>
        /// <returns>True if the notification type is asynchronous and not history. False otherwise.</returns>
        public static bool IsAsynchronousNonHistory(this NotificationType notificationType)
        {
            return notificationType == NotificationType.Asynchronous;
        }

        /// <summary>
        /// To verify if the notification type is asynchronous.
        /// </summary>
        /// <param name="notificationType">The notification to check.</param>
        /// <returns>True if the notification type is asynchronous. False otherwise.</returns>
        public static bool IsAsynchronous(this NotificationType notificationType)
        {
            return notificationType == NotificationType.Asynchronous ||
                   notificationType == NotificationType.AsynchronousHistory;
        }

        /// <summary>
        /// To verify if the notification type is history.
        /// </summary>
        /// <param name="notificationType">The notification to check.</param>
        /// <returns>True if the notification type is history. False otherwise.</returns>
        public static bool IsHistory(this NotificationType notificationType)
        {
            return notificationType == NotificationType.SynchronousHistory ||
                   notificationType == NotificationType.AsynchronousHistory;
        }
    }
}