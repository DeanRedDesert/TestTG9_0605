// -----------------------------------------------------------------------
// <copyright file = "IVideoTopperService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.VideoTopper
{
    using System;
    using CabinetServices;
    using Communication.Cabinet;

    /// <summary>
    /// The cabinet service that provides access to the video topper device.
    /// </summary>
    /// <devdoc>
    /// This interface derives from ICabinetService rather than IDeviceService because
    /// the old static VideoTopperController defined similar APIs with different names.
    /// Inheriting from IDeviceService will give the user different APIs to do the same
    /// thing, which could be very confusing to the users.
    /// </devdoc>
    public interface IVideoTopperService : ICabinetService
    {
        #region Events

        /// <summary>
        /// Event triggered when the video topper device is acquired;
        /// </summary>
        event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// Event triggered when the video topper device is released;
        /// </summary>
        event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <summary>
        /// Event triggered when the video topper device is connected;
        /// </summary>
        event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <summary>
        /// Event triggered when the video topper device is removed;
        /// </summary>
        event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <summary>
        /// Event triggered when a video topper device content status changes.
        /// </summary>
        event EventHandler<VideoTopperContentEventArgs> ContentEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the path where the client executable is installed.
        /// </summary>
        string GameMountPoint { get; }

        /// <summary>
        /// Gets the flag indicating if video topper device is acquired.
        /// </summary>
        bool IsDeviceAcquired { get; }

        /// <summary>
        /// Gets the flag indicating if video topper device is connected;
        /// </summary>
        bool IsDeviceConnected { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the video topper device.
        /// </summary>
        /// <remarks>
        /// This will try to acquire the ownership of video topper device and return an interface to control the device.
        /// The user must check for <see cref="IsDeviceAcquired"/> flag before start using the returned interface.
        /// </remarks>
        /// <returns>
        /// Instance of the video topper device.
        /// </returns>
        IVideoTopper GetDevice();

        /// <summary>
        /// Releases the video topper device.
        /// </summary>
        void ReleaseDevice();

        /// <summary>
        /// Gets the interface for querying video topper information without having to acquire
        /// the video topper device first.
        /// </summary>
        /// <returns>
        /// The interface for querying video topper information.
        /// </returns>
        /// <remarks>
        /// It will simply return the video topper information interface without acquiring the
        /// ownership of video topper device.
        /// </remarks>
        IVideoTopperInformation GetVideoTopperInformationInterface();

        /// <summary>
        /// Plays a movie file on the video topper device.
        /// </summary>
        /// <param name="contentPath">Relative path of the requested video file.</param>
        /// <param name="seekTime">Point in which video playback should begin.</param>
        /// <param name="loop">Flag indicating if the video should be constantly re-played.</param>
        void PlayMovie(string contentPath, uint seekTime, bool loop);

        /// <summary>
        /// Shows an image file on the video topper device.
        /// </summary>
        /// <param name="contentPath">Relative path of the requested image file.</param>
        /// <param name="duration">Duration, in seconds, of how long the image should be displayed.</param>
        void ShowImage(string contentPath, uint duration);

        /// <summary>
        /// Validates a file extension against a predefined list of valid media types.
        /// </summary>
        /// <param name="fullyQualifiedPath">Fully qualified path of the requested media file.</param>
        /// <param name="mediaType">The type of media being validated(e.g "video" or "image").</param>
        ///<returns>True if the given media is valid; False otherwise.</returns>
        bool IsMediaFile(string fullyQualifiedPath, string mediaType);

        #endregion
    }
}