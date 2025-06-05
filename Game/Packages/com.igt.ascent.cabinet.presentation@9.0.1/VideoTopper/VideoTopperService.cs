//-----------------------------------------------------------------------
// <copyright file = "VideoTopperService.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.VideoTopper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using CabinetServices;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;

    /// <summary>
    /// The cabinet service that provides access to the video topper device.
    /// </summary>
    public class VideoTopperService : DeviceServiceBase, IVideoTopperService
    {
        #region Constants

        /// <summary>
        /// Flag indicating the type of media being validated in the IsMediaFile method is a video.
        /// </summary>
        private const string VideoType = "video";

        /// <summary>
        /// Flag indicating the type of media being validated in the IsMediaFile method is an image.
        /// </summary>
        private const string ImageType = "image";

        #endregion

        #region Private Fields

        /// <summary>
        /// The CSI interface used to control the video topper device.
        /// </summary>
        private IVideoTopper videoTopperInterface;

        /// <summary>
        /// Device identifier for the type of device that was requested.
        /// </summary>
        private string videoTopperDeviceId;

        /// <summary>
        /// A cached list of media that supported by the video topper device.
        /// </summary>
        private List<SupportedMedia> supportedMedium;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="IVideoTopperService"/>.
        /// </summary>
        /// <param name="mountPoint">Directory containing the game executable.</param>
        /// <param name="clientPriority">CSI client priority for accessing devices.</param>
        public VideoTopperService(string mountPoint, Priority clientPriority) : base(clientPriority, DeviceType.VideoTopper)
        {
            GameMountPoint = mountPoint;
        }

        #endregion

        #region Overrides of DeviceServiceBase

        /// <inheritdoc />
        public override void Disconnect()
        {
            supportedMedium = null;
            UnsubscribeEventHandlers();

            base.Disconnect();
        }

        /// <inheritdoc />
        protected override void OnAsyncConnect()
        {
            base.OnAsyncConnect();

            videoTopperInterface = CabinetLib.GetInterface<IVideoTopper>();

            var connectedVideoToppers = GetConnectedDeviceIdentifiers().ToList();
            IsDeviceConnected = connectedVideoToppers.Count > 0;
            videoTopperDeviceId = connectedVideoToppers.FirstOrDefault();

            UnsubscribeEventHandlers();
            SubscribeEventHandlers();
        }

        /// <inheritdoc />
        protected override void OnDeviceAcquired(string deviceId)
        {
            base.OnDeviceAcquired(deviceId);

            DeviceAcquiredEvent?.Invoke(this, new DeviceAcquiredEventArgs(ServiceDeviceType, deviceId));
        }

        /// <inheritdoc />
        protected override void OnDeviceReleased(string deviceId, DeviceAcquisitionFailureReason reason)
        {
            base.OnDeviceReleased(deviceId, reason);

            DeviceReleasedEvent?.Invoke(this, new DeviceReleasedEventArgs(ServiceDeviceType, deviceId, reason));
        }

        /// <inheritdoc />
        protected override void OnDeviceConnected(string deviceId)
        {
            base.OnDeviceConnected(deviceId);

            IsDeviceConnected = true;
            DeviceConnectedEvent?.Invoke(this, new DeviceConnectedEventArgs(ServiceDeviceType, deviceId));
        }

        /// <inheritdoc />
        protected override void OnDeviceRemoved(string deviceId)
        {
            base.OnDeviceRemoved(deviceId);

            IsDeviceConnected = false;
            supportedMedium = null;
            DeviceRemovedEvent?.Invoke(this, new DeviceRemovedEventArgs(ServiceDeviceType, deviceId));
        }

        #endregion

        #region Implementation of IVideoTopperService

        /// <inheritdoc />
        public event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceConnectedEventArgs> DeviceConnectedEvent;

        /// <inheritdoc />
        public event EventHandler<DeviceRemovedEventArgs> DeviceRemovedEvent;

        /// <inheritdoc />
        public event EventHandler<VideoTopperContentEventArgs> ContentEvent;

        /// <inheritdoc />
        public string GameMountPoint { get; }

        /// <inheritdoc />
        public bool IsDeviceAcquired => IsAcquired(videoTopperDeviceId);

        /// <inheritdoc />
        public bool IsDeviceConnected { get; private set; }

        /// <inheritdoc />
        public IVideoTopper GetDevice()
        {
            var success = Acquire(videoTopperDeviceId);

            return success ? videoTopperInterface : null;
        }

        /// <inheritdoc />
        public void ReleaseDevice()
        {
            Release(videoTopperDeviceId);
        }

        /// <inheritdoc />
        public IVideoTopperInformation GetVideoTopperInformationInterface()
        {
            return videoTopperInterface;
        }

        /// <inheritdoc />
        public void PlayMovie(string contentPath, uint seekTime, bool loop)
        {
            VerifyCabinetIsConnected();

            var fullyQualifiedPath = PrefixMountPoint(contentPath);

            if(IsMediaFile(fullyQualifiedPath, VideoType))
            {
                // Use GetDevice to make sure that the device is acquired before executing the command.
                GetDevice()?.PlayMovie(fullyQualifiedPath, seekTime, loop);
            }
            else
            {
                throw new VideoTopperException(VideoTopperError.ContentPathDoesNotExist, 
                                               $"Video file '{fullyQualifiedPath}' is invalid.");
            }
        }

        /// <inheritdoc />
        public void ShowImage(string contentPath, uint duration)
        {
            VerifyCabinetIsConnected();

            var fullyQualifiedPath = PrefixMountPoint(contentPath);

            if(IsMediaFile(fullyQualifiedPath, ImageType))
            {
                // Use GetDevice to make sure that the device is acquired before executing the command.
                GetDevice()?.ShowImage(fullyQualifiedPath, duration);
            }
            else
            {
                throw new VideoTopperException(VideoTopperError.ContentPathDoesNotExist, 
                                               $"Image file '{fullyQualifiedPath}' is invalid.");
            }
        }

        /// <inheritdoc />
        public bool IsMediaFile(string fullyQualifiedPath, string mediaType)
        {
            VerifyCabinetIsConnected();

            var result = false;

            if(!string.IsNullOrEmpty(fullyQualifiedPath) && File.Exists(fullyQualifiedPath))
            {
                // We only initialize the list of supported medium once per connection.
                if(supportedMedium == null)
                {
                    supportedMedium = videoTopperInterface.GetDeviceCapabilities()?.SupportedMedia ?? new List<SupportedMedia>();
                }

                if(supportedMedium.Count > 0)
                {
                    var ext = Path.GetExtension(fullyQualifiedPath).Replace(".", "");

                    result = supportedMedium.Any(media => media.FileExtension.Equals(ext, StringComparison.OrdinalIgnoreCase) &&
                                                          media.MimeType.StartsWith(mediaType, StringComparison.OrdinalIgnoreCase));
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void SubscribeEventHandlers()
        {
            if(videoTopperInterface != null)
            {
                videoTopperInterface.ContentEvent += OnContentEvent;
            }
        }

        private void UnsubscribeEventHandlers()
        {
            if(videoTopperInterface != null)
            {
                videoTopperInterface.ContentEvent -= OnContentEvent;
            }
        }

        private void OnContentEvent(object sender, VideoTopperContentEventArgs e)
        {
            ContentEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// Prefixes the content path provided with the mount point of the game.
        /// </summary>
        /// <param name="contentPath">Relative path of the requested media file.</param>
        ///<returns>Fully qualified path of the requested media file.</returns>
        private string PrefixMountPoint(string contentPath)
        {
            return Path.Combine(GameMountPoint, contentPath);
        }

        #endregion
    }
}
