//-----------------------------------------------------------------------
// <copyright file = "VideoTopperCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Class for handling the CSI video topper category.
    /// </summary>
    internal class VideoTopperCategory : CategoryBase<CsiVideoTopper>, IVideoTopper, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly List<CsiVideoTopper> pendingEvents = new List<CsiVideoTopper>();

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="VideoTopperCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region Constructror

        /// <summary>
        /// Construct an instance of the video topper category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// </param>
        public VideoTopperCategory(FoundationTarget target)
        {
            eventHandlers[typeof(VideoTopperContentEvent)] =
                message => HandleVideoTopperContentEvent(message as VideoTopperContentEvent);

            foundationTarget = target;
        }

        #endregion Constructror

        #region CategoryBase<CsiVideoTopper> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiVideoTopper;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                ushort version = 1;
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentMSeries))
                {
                    version = 3;
                }
                else if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds))
                {
                    version = 2;
                }

                return version;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var topperMessage = message as CsiVideoTopper;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(topperMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiLight),
                                                                message.GetType()));
            }

            if(topperMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Video topper message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(topperMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Add(topperMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + topperMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var topperMessage = message as CsiVideoTopper;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(topperMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiLight),
                                                                message.GetType()));
            }

            if(topperMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Video topper message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + topperMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiVideoTopper> Implementation

        #region IVideoTopper Implementation

        /// <inheritdoc/>
        public event EventHandler<VideoTopperContentEventArgs> ContentEvent;

        /// <inheritdoc/>
        public void CacheContent(string contentPath)
        {
            var request = new CsiVideoTopper
                          {
                              Item = new CacheContentRequest
                                     {
                                         ContentPath = contentPath,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetVideoTopperResponse<CacheContentResponse>();
            CheckResponse(response.VideoTopperResponse);
        }

        /// <inheritdoc/>
        public string PlayMovie(string contentPath, uint seekTime, bool loop, uint playCount = 0)
        {
            var request = new CsiVideoTopper
                          {
                              Item = new PlayContentRequest
                                     {
                                         ContentPath = contentPath,
                                         LoopVideo = loop,
                                         SeekTime = seekTime,
                                     }
                          };

            if(playCount != 0)
            {
                if(VersionMinor < 3)
                {
                    throw new VideoTopperCategoryException("Feature not supported",
                                                           "Playing content with a play count requires video topper category version 1.3 or newer.");
                }

                var item = (PlayContentRequest)request.Item;
                item.PlayCount = playCount;
                item.PlayCountSpecified = true;
            }

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetVideoTopperResponse<PlayContentResponse>();
            CheckResponse(response.VideoTopperResponse);

            return response.ContentKey;
        }

        /// <inheritdoc/>
        public string ShowImage(string contentPath, uint duration)
        {
            var request = new CsiVideoTopper
                          {
                              Item = new PlayContentRequest
                                     {
                                         ContentPath = contentPath,
                                         ImageDuration = duration,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetVideoTopperResponse<PlayContentResponse>();
            CheckResponse(response.VideoTopperResponse);

            return response.ContentKey;
        }

        /// <inheritdoc/>
        public void StopContent(string contentKey)
        {
            var request = new CsiVideoTopper
                          {
                              Item = new StopContentRequest
                                     {
                                         ContentKey = contentKey,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetVideoTopperResponse<StopContentResponse>();
            CheckResponse(response.VideoTopperResponse);
        }

        /// <inheritdoc/>
        public VideoTopperCapabilities GetDeviceCapabilities()
        {
            var request = new CsiVideoTopper
                          {
                              Item = new DeviceCapabilitiesRequest()
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetVideoTopperResponse<DeviceCapabilitiesResponse>();
            CheckResponse(response.VideoTopperResponse);

            // As of 3/29/2018, the foundation will always return a VideoTopperCapabilities without any error,
            // as following:
            //      Width=1280, Height=720,
            //      Media types: mp4, jpg, png
            return new VideoTopperCapabilities(response.VideoTopperResolution.Width,
                                               response.VideoTopperResolution.Height,
                                               response.SupportedMedia.Select(media => new SupportedMedia
                                                                                       {
                                                                                           FileExtension = media.FileExtension,
                                                                                           MimeType = media.MimeType
                                                                                       })
                                                       .ToList(),
                                               response.SupportsPortals.ToPublic());
        }

        #endregion IVideoTopper Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiVideoTopper>();

            lock(pendingEvents)
            {
                tempEvents.AddRange(pendingEvents);
                pendingEvents.Clear();
            }

            foreach(var pendingEvent in tempEvents)
            {
                //Events should only be placed in the queue if they are handled.
                eventHandlers[pendingEvent.Item.GetType()](pendingEvent.Item);
            }
        }

        #endregion ICabinetUpdate Implementation

        #region Private Methods

        /// <summary>
        /// Handle content events from the video topper category.
        /// </summary>
        /// <param name="contentEvent">The content event to handle.</param>
        private void HandleVideoTopperContentEvent(VideoTopperContentEvent contentEvent)
        {
            ContentEvent?.Invoke(this, new VideoTopperContentEventArgs(contentEvent.ContentKey, contentEvent.Event));
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="VideoTopperCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(VideoTopperResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case VideoTopperErrorCode.NONE:
                case VideoTopperErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case VideoTopperErrorCode.VIDEO_TOPPER_DRIVER_ERROR:
                case VideoTopperErrorCode.OTHER_ERROR:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                var errorString = response.ErrorCode.ToString();

                // If the error has a coresponding value in the public enum VideoTopperError
                if(Enum.TryParse(errorString, out VideoTopperError videoTopperError))
                {
                    throw new VideoTopperException(videoTopperError, response.ErrorDescription);
                }
                else
                {
                    throw new VideoTopperCategoryException(errorString, response.ErrorDescription);
                }
            }
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetVideoTopperResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();
            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion Private Methods
    }
}
