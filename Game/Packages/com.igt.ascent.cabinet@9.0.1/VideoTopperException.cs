//-----------------------------------------------------------------------
// <copyright file = "VideoTopperException.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Exceptions for the video topper device.
    /// </summary>
    public class VideoTopperException : VideoTopperCategoryException
    {
        /// <summary>
        /// The error that caused the exception.
        /// </summary>
        public VideoTopperError Error{ get; }

        /// <summary>
        /// Initializes a new instance of <see cref="VideoTopperException"/>.
        /// </summary>
        /// <param name="error">The error that caused the exception.</param>
        /// <param name="description">The description of the error.</param>
        public VideoTopperException(VideoTopperError error, string description) : base(error.ToString(), description)
        {
            Error = error;
        }
    }
}
