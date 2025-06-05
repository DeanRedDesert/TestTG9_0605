//-----------------------------------------------------------------------
// <copyright file = "VirtualVideoTopper.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Timers;

    /// <summary>
    /// Virtual implementation of the video topper device.
    /// </summary>
    public class VirtualVideoTopper : IVideoTopper, ICabinetUpdate, IDisposable
    {
        /// <summary>
        /// Width of the video topper resolution.
        /// </summary>
        private const uint Width = 1280;

        /// <summary>
        /// Height of the video topper resolution.
        /// </summary>
        private const uint Height = 720;

        /// <summary>
        /// List of video topper supported media.
        /// </summary>
        private static readonly ReadOnlyCollection<SupportedMedia> SupportedMedia =
            new ReadOnlyCollection<SupportedMedia>(new[]
            {
                new SupportedMedia
                {
                    FileExtension = "mp4",
                    MimeType = "video/mp4"
                },
                new SupportedMedia
                {
                    FileExtension = "webm",
                    MimeType = "video/webm"
                },
                new SupportedMedia
                {
                    FileExtension = "jpg",
                    MimeType = "image/jpeg"
                },
                new SupportedMedia
                {
                    FileExtension = "png",
                    MimeType = "image/png"
                },
                new SupportedMedia
                {
                    FileExtension = "webp",
                    MimeType = "image/webp"
                }
            });

        /// <summary>
        /// Dictionary that holds cached content and the status of the content.
        /// </summary>
        private readonly Dictionary<string, TopperContentStatus> contents =
            new Dictionary<string, TopperContentStatus>();

        /// <summary>
        /// The queue of <see cref="EventArgs"/>.
        /// </summary>
        private readonly Queue<EventArgs> topperEventQueue = new Queue<EventArgs>();

        /// <summary>
        /// Flag indicating if this object has been disposed.
        /// </summary>
        private bool disposed;

        /// <inheritdoc/>
        public void Update()
        {
            var tempEventList = new List<EventArgs>();

            lock(topperEventQueue)
            {
                tempEventList.AddRange(topperEventQueue);
                topperEventQueue.Clear();
            }

            foreach(var eventToPost in tempEventList)
            {
                if(eventToPost is VideoTopperContentEventArgs args)
                {
                    ContentEvent?.Invoke(this, args);
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            //The finalizer does not need to execute if the object has been disposed.
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public event EventHandler<VideoTopperContentEventArgs> ContentEvent;

        /// <inheritdoc/>
        /// <exception cref="VideoTopperException">Exception thrown if content path is null or empty.</exception>
        public void CacheContent(string contentPath)
        {
            if(string.IsNullOrEmpty(contentPath))
            {
                throw new VideoTopperException(VideoTopperError.ContentPathDoesNotExist, "Content path is null or invalid");
            }

            if(!contents.ContainsKey(contentPath))
            {
                contents[contentPath] = TopperContentStatus.Stopped;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="VideoTopperException">Exception thrown if content path is null or empty.</exception>
        public string PlayMovie(string contentPath, uint seekTime, bool loop, uint playCount = 0)
        {
            if(string.IsNullOrEmpty(contentPath))
            {
                throw new VideoTopperException(VideoTopperError.ContentPathDoesNotExist, "Content path is null or invalid");
            }

            contents[contentPath] = TopperContentStatus.Started;
            EnqueueEvent(new VideoTopperContentEventArgs(contentPath,
                CSI.Schemas.Internal.ContentEvent.CONTENT_STARTED));
            
            return contentPath;
        }

        /// <inheritdoc/>
        /// <exception cref="VideoTopperException">Exception thrown if content path is null or empty.</exception>
        public string ShowImage(string contentPath, uint duration)
        {
            if(string.IsNullOrEmpty(contentPath))
            {
                throw new VideoTopperException(VideoTopperError.ContentPathDoesNotExist, "Content path is null or invalid");
            }

            contents[contentPath] = TopperContentStatus.Started;
            var timer = new Timer(duration);
            timer.Elapsed += (sender, args) => StopContent(contentPath);
            timer.Start();
            EnqueueEvent(new VideoTopperContentEventArgs(contentPath,
                CSI.Schemas.Internal.ContentEvent.CONTENT_STARTED));
            return contentPath;
        }

        /// <inheritdoc/>
        /// <exception cref="VideoTopperException">Exception thrown if content path is null or empty.</exception>
        public void StopContent(string contentKey)
        {
            if(string.IsNullOrEmpty(contentKey))
            {
                throw new VideoTopperException(VideoTopperError.ContentKeyDoesNotExist, "Content key is null or invalid");
            }

            contents[contentKey] = TopperContentStatus.Stopped;
            EnqueueEvent(new VideoTopperContentEventArgs(contentKey,
                CSI.Schemas.Internal.ContentEvent.CONTENT_STOPPED));
        }

        /// <inheritdoc/>
        public VideoTopperCapabilities GetDeviceCapabilities()
        {
            return new VideoTopperCapabilities(Width, Height, new List<SupportedMedia>(SupportedMedia), VideoTopperPortalSupport.PortalSupported);
        }

        /// <summary>
        /// Enqueues the events in the order they were triggered.
        /// </summary>
        /// <param name="eventArgs">Event arguments of the triggered event.</param>
        private void EnqueueEvent(EventArgs eventArgs)
        {
            lock(topperEventQueue)
            {
                topperEventQueue.Enqueue(eventArgs);
            }
        }

        /// <summary>
        /// Dispose unmanaged and disposable resources held by this object.
        /// </summary>
        /// <param name="disposing">True if called from dispose function.</param>
        private void Dispose(bool disposing)
        {
            if(!disposed && disposing)
            {
                // Release all event subscriptions.
                ContentEvent = null;
                contents.Clear();
            }

            disposed = true;
        }
    }
}