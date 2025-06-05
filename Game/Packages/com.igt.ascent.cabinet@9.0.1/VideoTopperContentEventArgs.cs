//-----------------------------------------------------------------------
// <copyright file = "VideoTopperContentEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas.Internal;

    /// <summary>
    /// Enumeration containing content statuses.
    /// </summary>
    public enum TopperContentStatus
    {
        /// <summary>
        /// Status indicating the content has started.
        /// </summary>
        Started,

        /// <summary>
        /// Event indicating that the content has stopped.
        /// </summary>
        Stopped
    }

    /// <summary>
    /// Event indicating a change in topper content status.
    /// </summary>
    public class VideoTopperContentEventArgs : EventArgs
    {
        /// <summary>
        /// Dictionary for converting the content status used by the content to an abstracted SDK status.
        /// </summary>
        private readonly Dictionary<ContentEvent, TopperContentStatus> statusConversionTable =
            new Dictionary<ContentEvent, TopperContentStatus>
            {
                {ContentEvent.CONTENT_STARTED, TopperContentStatus.Started},
                {ContentEvent.CONTENT_STOPPED, TopperContentStatus.Stopped}
            }; 

        /// <summary>
        /// The content key associated with the event.
        /// </summary>
        public string ContentKey { get; }

        /// <summary>
        /// The status of the content.
        /// </summary>
        public TopperContentStatus Status { get; }

        /// <summary>
        /// Construct the event arguments with the given parameters.
        /// </summary>
        /// <param name="contentKey">Content key the event is associated with.</param>
        /// <param name="status">The status of the content.</param>
        public VideoTopperContentEventArgs(string contentKey, ContentEvent status)
        {
            if(string.IsNullOrEmpty(contentKey))
            {
                throw new ArgumentException("The content key may not be null or empty.", nameof(contentKey));
            }
            ContentKey = contentKey;
            if(statusConversionTable.ContainsKey(status))
            {
                Status = statusConversionTable[status];
            }
            else
            {
                throw new KeyNotFoundException("The specified status is not supported: " + status);
            }
        }
    }
}
