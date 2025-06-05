//-----------------------------------------------------------------------
// <copyright file = "IVideoTopper.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Interface for controlling a video topper device.
    /// The device must be acquired first before calling the methods here.
    /// </summary>
    public interface IVideoTopper : IVideoTopperInformation
    {
        /// <summary>
        /// Event that indicates a change in content status.
        /// </summary>
        event EventHandler<VideoTopperContentEventArgs> ContentEvent; 

        /// <summary>
        /// Cache the specified content to be played on the video topper.
        /// </summary>
        /// <param name="contentPath">The fully qualified path to the content.</param>
        void CacheContent(string contentPath);

        /// <summary>
        /// Play the specified movie on the video topper.
        /// </summary>
        /// <param name="contentPath">The fully qualified path to the content.</param>
        /// <param name="seekTime">Time to seek within the movie to start playing.</param>
        /// <param name="loop">Loop the movie.</param>
        /// <returns>Content key for the playing movie.</returns>
        /// <param name="playCount">The number of times to play the movie if looping. Loops forever if 0.</param>
        /// <returns>Content key for the playing movie.</returns>
        /// <exception cref="VideoTopperCategoryException">
        /// Thrown if not supported by the device category version.
        /// </exception>
        string PlayMovie(string contentPath, uint seekTime, bool loop, uint playCount = 0);

        /// <summary>
        /// Show the specified image on the video topper.
        /// </summary>
        /// <param name="contentPath">The fully qualified path to the content.</param>
        /// <param name="duration">The duration, in seconds, to show the image.</param>
        /// <returns>Content key for the image.</returns>
        string ShowImage(string contentPath, uint duration);

        /// <summary>
        /// Stop the specified content.
        /// </summary>
        /// <param name="contentKey">Content key return from ShowImage or PlayMovie.</param>
        void StopContent(string contentKey);
    }
}
