//-----------------------------------------------------------------------
// <copyright file = "StreamingLightsCategory.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;
    using SymbolHighlights;

    // Make sure the StreamingLightGroup is the one from the Cabinet namespace and not the one from the CSI schema.
    using StreamingLightGroup = StreamingLightGroup;

    /// <summary>
    /// Category for controlling streaming light devices.
    /// </summary>
    internal class StreamingLightsCategory : CategoryBase<CsiStreamingLight>, IStreamingLights, ICabinetUpdate
    {
        #region Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// The queue of event messages from the CSI host.
        /// </summary>
        private readonly Queue<CsiStreamingLight> eventQueue = new Queue<CsiStreamingLight>();

        /// <summary>
        /// The queue of commands to be sent to the host on the next update.
        /// </summary>
        private readonly Queue<CsiStreamingLight> startSequenceCommandQueue = new Queue<CsiStreamingLight>();

        /// <summary>
        /// The unique ID of the thread that created this instance.
        /// </summary>
        private readonly int creatorThreadId;

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="ResourceManagementCategory"/> was constructed with.
        /// </summary>
        // TODO: private member unused
        // ReSharper disable once NotAccessedField.Local
        private readonly FoundationTarget foundationTarget;

        /// <summary>
        /// Symbol tracking data to be sent with a <see cref="ClearSymbolHighlightsRequest"/> to clear all tracked symbols.
        /// </summary>
        private readonly CSI.Schemas.Internal.SymbolTrackingData clearAllSymbolTrackingData =
            new CSI.Schemas.Internal.SymbolTrackingData
            {
                ReelIndex = 0xFF,
                ReelStop = 0xFF,
                RowIndex = 0xFF
            };

        /// <summary>
        /// Hot position data to be sent with a <see cref="ClearStatusRegistrationRequest"/> to clear all hot positions.
        /// </summary>
        private readonly CSI.Schemas.Internal.SymbolHotPositionData clearAllHotPositionData =
            new CSI.Schemas.Internal.SymbolHotPositionData
            {
                ReelIndex = 0xFF,
                ReelStop = 0xFF,
                WindowStopIndex = 0xFF
            };

        #endregion Fields

        /// <inheritdoc />
        public byte SupportedLightVersion { get; }

        /// <summary>
        /// Gets if an invoke is required before sending the CSI command.
        /// </summary>
        private bool InvokeRequired => Thread.CurrentThread.ManagedThreadId != creatorThreadId;

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="currentFoundationTarget">The current foundation target to utilize.</param>
        public StreamingLightsCategory(FoundationTarget currentFoundationTarget)
        {
            SupportedLightVersion = 2;

            eventHandlers[typeof(NotificationEvent)] =
                message => HandleNotificationEvent(message as NotificationEvent);

            creatorThreadId = Thread.CurrentThread.ManagedThreadId;

            foundationTarget = currentFoundationTarget;
        }

        #region CategoryBase<CsiStreamingLight> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiStreamingLight;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 3;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var lightMessage = message as CsiStreamingLight;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(lightMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiLight),
                                                                message.GetType()));
            }

            if(lightMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Streaming light message contained no event, request or response.");
            }

            lock(eventQueue)
            {
                eventQueue.Enqueue(lightMessage);
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var lightMessage = message as CsiStreamingLight;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(lightMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiLight),
                                                                message.GetType()));
            }

            if(lightMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Streaming light message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + lightMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiStreamingLight> Implementation

        #region IStreamingLights Implementation

        /// <inheritdoc />
        public event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;

        /// <inheritdoc/>
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            var request = new CsiStreamingLight { Item = new GetLightDevicesRequest1() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<GetLightDevicesResponse1>();
            CheckResponse(response.StreamingLightResponse);

            if(response.StreamingLightFeatures == null)
            {
                return new List<LightFeatureDescription>();
            }

            return from feature in response.StreamingLightFeatures
                   let subFeature = GetSubFeature(feature.LightType)
                   let groups =
                       feature.Group.Select(
                           (groupData, groupIndex) =>
                               (ILightGroup)new StreamingLightGroup((byte)groupIndex,
                                                                    groupData.NumberOfLights,
                                                                    groupData.RealTimeFrameControlSupported,
                                                                    feature.AdjustableIntensity,
                                                                    groupData.SymbolHighlightsSupportedSpecified && groupData.SymbolHighlightsSupported))
                   select new LightFeatureDescription(feature.InterfaceName, subFeature, groups);
        }

        /// <inheritdoc/>
        public void StartSequenceFile(string featureId,
                                      byte groupNumber,
                                      string filePath,
                                      StreamingLightsPlayMode playMode)
        {
            var request = new CsiStreamingLight
                          {
                              Item = new StartLightSequenceRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         Item = filePath,
                                         PlayMode = GetPlayMode(playMode),
                                     }
                          };

            SendStartLightSequenceRequest(request);
        }

        /// <inheritdoc/>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> or <paramref name="sequenceFile"/> is null;
        /// </exception>
        public void StartSequenceFile(string featureId,
                                      byte groupNumber,
                                      string sequenceName,
                                      byte[] sequenceFile,
                                      StreamingLightsPlayMode playMode)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            if(sequenceFile == null)
            {
                throw new ArgumentNullException(nameof(sequenceFile));
            }

            if(sequenceName == null)
            {
                throw new ArgumentNullException(nameof(sequenceName));
            }

            var request = new CsiStreamingLight
                          {
                              Item = new StartLightSequenceRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         Item = new StreamingLightSequenceData
                                                { SequenceName = sequenceName, SequenceData = sequenceFile },
                                         PlayMode = GetPlayMode(playMode),
                                     }
                          };

            SendStartLightSequenceRequest(request);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="featureId"/> is null.
        /// </exception>
        public void SendFrameChunk(string featureId,
                                   byte groupNumber,
                                   uint frameCount,
                                   byte[] frameData,
                                   StreamingLightsPlayMode playMode,
                                   byte identifier)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            var request = new CsiStreamingLight
                          {
                              Item = new StartLightSequenceRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         Item = new StreamingLightFrameData
                                                {
                                                    FrameName = $"{featureId}/{groupNumber}/{identifier}",
                                                    FrameCount = frameCount,
                                                    FrameData = frameData,
                                                    FrameDataSize = Convert.ToUInt32(frameData.LongLength)
                                                },
                                         PlayMode = GetPlayMode(playMode)
                                     }
                          };

            SendStartLightSequenceRequest(request);
        }

        /// <inheritdoc/>
        public void BreakLoop(string featureId, byte groupNumber)
        {
            var request = new CsiStreamingLight
                          {
                              Item = new BreakLoopRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<BreakLoopResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc/>
        public void SetIntensity(string featureId, byte intensity)
        {
            var request = new CsiStreamingLight
                          {
                              Item = new SetLightIntensityRequest
                                     {
                                         InterfaceName = featureId,
                                         Intensity = intensity,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetLightIntensityResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc/>
        public byte GetIntensity(string featureId)
        {
            var request = new CsiStreamingLight
                          {
                              Item = new GetLightIntensityRequest
                                     {
                                         InterfaceName = featureId,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<GetLightIntensityResponse>();
            CheckResponse(response.StreamingLightResponse);

            return response.Intensity;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="enabledFeatures"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="enabledFeatures"/> is empty or <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void EnableGivenSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> enabledFeatures)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(enabledFeatures == null)
            {
                throw new ArgumentNullException(nameof(enabledFeatures));
            }

            var symbolHighlightFeatures = enabledFeatures as IList<SymbolHighlightFeature> ?? enabledFeatures.ToList();

            if(symbolHighlightFeatures.Count == 0)
            {
                throw new ArgumentException("enabledFeatures cannot be empty.", nameof(enabledFeatures));
            }

            var featuresToEnable = symbolHighlightFeatures.Select(feature => feature.ToInternal()).ToList();

            var request = new CsiStreamingLight
                          {
                              Item = new SetEnabledSymbolHighlightFeaturesRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         GroupNumberSpecified = groupNumber != 0xFF,
                                         EnabledSymbolHighlightFeatures = featuresToEnable
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetEnabledSymbolHighlightFeaturesResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void DisableSymbolHighlights(string featureId, byte groupNumber)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            var request = new CsiStreamingLight
                          {
                              Item = new SetEnabledSymbolHighlightFeaturesRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         GroupNumberSpecified = groupNumber != 0xFF,
                                         EnabledSymbolHighlightFeatures = new List<SymbolHighlightFeatureTypes>()
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetEnabledSymbolHighlightFeaturesResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="trackingData"/> or <paramref name="hotPositionData"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when both <paramref name="trackingData"/> or <paramref name="hotPositionData"/> is empty,
        /// or <paramref name="featureId"/> is null or empty.
        /// </exception>
        public void SetSymbolHighlights(string featureId,
                                        byte groupNumber,
                                        SymbolHighlights.SymbolTrackingData[] trackingData,
                                        SymbolHighlights.SymbolHotPositionData[] hotPositionData)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(trackingData == null)
            {
                throw new ArgumentNullException(nameof(trackingData));
            }

            if(hotPositionData == null)
            {
                throw new ArgumentNullException(nameof(hotPositionData));
            }

            var trackingList = trackingData.Select(t => t.ToInternal()).ToList();
            var hotPositionList = hotPositionData.Select(t => t.ToInternal()).ToList();

            if(trackingList.Count == 0 && hotPositionList.Count == 0)
            {
                throw new ArgumentException("Both trackingData and hotPosition cannot be empty. " +
                                            "This method cannot be called with no highlight data.");
            }

            var request = new CsiStreamingLight
                          {
                              Item = new SetSymbolHighlightsRequest
                                     {
                                         InterfaceName = featureId,
                                         GroupNumber = groupNumber,
                                         GroupNumberSpecified = groupNumber != 0xFF,
                                         SymbolTrackingDataList = trackingList.Count > 0
                                                                      ? trackingList
                                                                      : null,
                                         SymbolHotPositionDataList = hotPositionList.Count > 0
                                                                         ? hotPositionList
                                                                         : null
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetSymbolHighlightsResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featuresToClear"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="featuresToClear"/> is empty or <paramref name="featureId"/> is null or empty string.
        /// </exception>
        public void ClearSymbolHighlights(string featureId, byte groupNumber, IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException(nameof(featureId));
            }

            if(featuresToClear == null)
            {
                throw new ArgumentNullException(nameof(featuresToClear));
            }

            var symbolHighlightsToClear = featuresToClear as IList<SymbolHighlightFeature> ?? featuresToClear.ToList();

            if(symbolHighlightsToClear.Count == 0)
            {
                throw new ArgumentException("featuresToClear cannot be empty.", nameof(featuresToClear));
            }

            var item = new ClearSymbolHighlightsRequest
                       {
                           InterfaceName = featureId,
                           GroupNumber = groupNumber,
                           GroupNumberSpecified = groupNumber != 0xFF,
                           SymbolTrackingDataList = null,
                           SymbolHotPositionDataList = null
                       };

            if(symbolHighlightsToClear.Contains(SymbolHighlightFeature.SymbolTracking))
            {
                item.SymbolTrackingDataList = new List<CSI.Schemas.Internal.SymbolTrackingData>
                                              {
                                                  clearAllSymbolTrackingData
                                              };
            }

            if(symbolHighlightsToClear.Contains(SymbolHighlightFeature.SymbolHotPosition))
            {
                item.SymbolHotPositionDataList = new List<CSI.Schemas.Internal.SymbolHotPositionData>
                                                 {
                                                     clearAllHotPositionData
                                                 };
            }

            var request = new CsiStreamingLight
                          {
                              Item = item
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<ClearSymbolHighlightsResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featuresToClear"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="featuresToClear"/> is empty or <paramref name="featureId"/> is null or empty string.
        /// </exception>
        public void ClearSymbolHighlightReel(string featureId,
                                             byte groupNumber,
                                             int reelIndex,
                                             IEnumerable<SymbolHighlightFeature> featuresToClear)
        {
            if(string.IsNullOrEmpty(featureId))
            {
                throw new ArgumentException("featureId");
            }

            if(featuresToClear == null)
            {
                throw new ArgumentNullException(nameof(featuresToClear));
            }

            var symbolHighlightsToClear = featuresToClear as IList<SymbolHighlightFeature> ?? featuresToClear.ToList();

            if(symbolHighlightsToClear.Count == 0)
            {
                throw new ArgumentException("featuresToClear cannot be empty.", nameof(featuresToClear));
            }

            var item = new ClearSymbolHighlightsRequest
                       {
                           InterfaceName = featureId,
                           GroupNumber = groupNumber,
                           GroupNumberSpecified = groupNumber != 0xFF,
                           SymbolTrackingDataList = null,
                           SymbolHotPositionDataList = null
                       };

            if(symbolHighlightsToClear.Contains(SymbolHighlightFeature.SymbolTracking))
            {
                item.SymbolTrackingDataList = new List<CSI.Schemas.Internal.SymbolTrackingData>
                                              {
                                                  new CSI.Schemas.Internal.SymbolTrackingData
                                                  {
                                                      ReelIndex = (byte)reelIndex,
                                                      ReelStop = 0xFF,
                                                      RowIndex = 0xFF
                                                  }
                                              };
            }

            if(symbolHighlightsToClear.Contains(SymbolHighlightFeature.SymbolHotPosition))
            {
                item.SymbolHotPositionDataList = new List<CSI.Schemas.Internal.SymbolHotPositionData>
                                                 {
                                                     new CSI.Schemas.Internal.SymbolHotPositionData
                                                     {
                                                         ReelIndex = (byte)reelIndex,
                                                         ReelStop = 0xFF,
                                                         WindowStopIndex = 0xFF
                                                     }
                                                 };
            }

            var request = new CsiStreamingLight
                          {
                              Item = item
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<ClearSymbolHighlightsResponse>();
            CheckResponse(response.StreamingLightResponse);
        }

        #endregion IStreamingLights Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc />
        void ICabinetUpdate.Update()
        {
            FlushStartSequenceCommandQueue();

            IList<CsiStreamingLight> streamingLightEvents;

            // Lock statements should limit the amount of processing that could be done
            // within the statement, which is why there is why the event processing
            // is done after the lock statement.
            lock(eventQueue)
            {
                streamingLightEvents = eventQueue.ToList();
                eventQueue.Clear();
            }

            foreach(var streamingLightEvent in streamingLightEvents)
            {
                if(eventHandlers.ContainsKey(streamingLightEvent.Item.GetType()))
                {
                    eventHandlers[streamingLightEvent.Item.GetType()](streamingLightEvent.Item);
                }
                else
                {
                    throw new UnhandledEventException("Event not handled: " + streamingLightEvent.Item.GetType());
                }
            }
        }

        #endregion ICabinetUpdate Implementation

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetLightResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="LightCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(StreamingLightResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case StreamingLightErrorCode.NONE:
                case StreamingLightErrorCode.UNKNOWN_DRIVER_ERROR:
                case StreamingLightErrorCode.FILE_NOT_FOUND:
                case StreamingLightErrorCode.INVALID_SEQUENCE:
                case StreamingLightErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case StreamingLightErrorCode.QUEUE_EMPTY:
                case StreamingLightErrorCode.SEQUENCE_COMPLETE:
                case StreamingLightErrorCode.DEVICE_IN_TILT_STATE:
                case StreamingLightErrorCode.QUEUE_FULL:
                case StreamingLightErrorCode.INVALID_FRAME:
                case StreamingLightErrorCode.INVALID_COMMAND:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new StreamingLightCategoryException(((StreamingLightNotificationCode)response.ErrorCode).ToString(),
                                                          response.ErrorDescription);
            }
        }

        /// <summary>
        /// Get the LightSubFeature based on the device type.
        /// </summary>
        /// <param name="deviceType">The device type to get the feature for.</param>
        /// <returns>The LightSubFeature for the device type.</returns>
        private static LightSubFeature GetSubFeature(LightType1 deviceType)
        {
            switch(deviceType)
            {
                case LightType1.AccentLights:
                    return LightSubFeature.AccentLights;

                case LightType1.BonusGameLights:
                    return LightSubFeature.BonusGameLights;

                case LightType1.Candle:
                    return LightSubFeature.Candle;

                case LightType1.LightBars:
                    return LightSubFeature.LightBars;

                case LightType1.LightBezel:
                    return LightSubFeature.LightBezel;

                case LightType1.ReelBackLights:
                    return LightSubFeature.ReelBacklights;

                case LightType1.CardReaderBezel:
                    return LightSubFeature.CardReaderBezel;

                case LightType1.TopperLightRing:
                    return LightSubFeature.TopperLightRing;

                case LightType1.ReelDividerLights:
                    return LightSubFeature.ReelDividerLights;

                case LightType1.ReelHighlights:
                    return LightSubFeature.ReelHighlights;

                default:
                    //If the type is unknown then assume to extra functionality.
                    return LightSubFeature.BonusGameLights;
            }
        }

        /// <summary>
        /// Gets the CSI schema version of PlayMode from the Communication.Cabinet PlayMode enum.
        /// </summary>
        /// <param name="playMode">The play mode.</param>
        /// <returns>The converted enum value.</returns>
        private static PlayMode GetPlayMode(StreamingLightsPlayMode playMode)
        {
            switch(playMode)
            {
                case StreamingLightsPlayMode.Continue:
                    return PlayMode.Continue;

                case StreamingLightsPlayMode.Restart:
                    return PlayMode.Restart;

                case StreamingLightsPlayMode.Queue:
                    return PlayMode.Queue;

                default:
                    return PlayMode.Restart;
            }
        }

        /// <summary>
        /// Handles the light notification event from the CSI.
        /// </summary>
        /// <param name="notificationEvent">The event to handle.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="notificationEvent"/> is null.
        /// </exception>
        private void HandleNotificationEvent(NotificationEvent notificationEvent)
        {
            if(notificationEvent == null)
            {
                throw new ArgumentNullException(nameof(notificationEvent));
            }

            NotificationEvent?.Invoke(this,
                                      new StreamingLightsNotificationEventArgs(notificationEvent.InterfaceName,
                                                                               Convert.ToByte(notificationEvent.GroupId),
                                                                               (StreamingLightNotificationCode)notificationEvent.Notification));
        }

        /// <summary>
        /// Sends any commands held in the start sequence command queue.
        /// </summary>
        private void FlushStartSequenceCommandQueue()
        {
            IList<CsiStreamingLight> startSequenceCommands;

            // Lock statements should limit the amount of processing that could be done
            // within the statement, which is why there is why the command processing
            // is done after the lock statement.
            lock(startSequenceCommandQueue)
            {
                startSequenceCommands = startSequenceCommandQueue.ToList();
                startSequenceCommandQueue.Clear();
            }

            foreach(var command in startSequenceCommands)
            {
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, command));
                var response = GetLightResponse<StartLightSequenceResponse>();
                CheckResponse(response.StreamingLightResponse);
            }
        }

        /// <summary>
        /// Sends a light sequence request or queues it if this method is not called on
        /// the main thread.
        /// </summary>
        /// <param name="request">The request to send.</param>
        private void SendStartLightSequenceRequest(CsiStreamingLight request)
        {
            lock(startSequenceCommandQueue)
            {
                startSequenceCommandQueue.Enqueue(request);
            }

            if(!InvokeRequired)
            {
                // If the invoke isn't required, send all the current commands.
                FlushStartSequenceCommandQueue();
            }
        }
    }
}
