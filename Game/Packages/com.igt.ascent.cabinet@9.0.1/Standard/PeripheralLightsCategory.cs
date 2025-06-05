//-----------------------------------------------------------------------
// <copyright file = "PeripheralLightsCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas.Internal;
    using CsiTransport;
    using Foundation.Transport;

    internal class PeripheralLightsCategory : CategoryBase<CsiLight>, IPeripheralLights, ICandle, ICabinetUpdate
    {
        #region Private Members

        /// <summary>
        /// The minimum Category version that supports candle events/requests.
        /// </summary>
        private static readonly Version MinCandleCategoryVersion = new Version(1, 4);

        /// <summary>
        /// The Categories configured version.
        /// </summary>
        private readonly Version currentCategoryVersion;

        #endregion Private Members

        #region CategoryBase<CsiLight> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiLight;

        /// <inheritdoc/>
        public override ushort VersionMajor => (ushort)currentCategoryVersion.Major;

        /// <inheritdoc/>
        /// <remarks>
        /// Versions 1.2 and 1.3 have never been used/tested by the SDK, so they are being skipped.
        /// </remarks>
        public override ushort VersionMinor => (ushort)currentCategoryVersion.Minor;

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly List<CsiLight> pendingEvents = new List<CsiLight>();

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var lightMessage = message as CsiLight;

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
                    "Invalid message body. Light message contained no event, request or response.");
            }

            // If the event can be processed then add to pending events, so it can be processed on Update.
            if(eventHandlers.ContainsKey(lightMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Add(lightMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + lightMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var lightMessage = message as CsiLight;

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
                    "Invalid message body. Light message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + lightMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiLight> Implementation

        #region IPeripheralLights Implementation

        /// <inheritdoc/>
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            var request = new CsiLight { Item = new GetLightDevicesRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<GetLightDevicesResponse>();
            CheckResponse(response.LightResponse);

            if(response.Features == null)
            {
                return new List<LightFeatureDescription>();
            }

            return (from feature in response.Features.Feature
                    let subFeature = GetSubFeature(feature.LightType)
                    let groups =
                        feature.Group.Select(
                            (groupData, groupIndex) => GetLightGroupDescription((byte)groupIndex, groupData, subFeature))
                    select new LightFeatureDescription(feature.FeatureId, subFeature, groups));
        }

        /// <inheritdoc/>
        public bool RequiresDeviceAcquisition => true;

        /// <inheritdoc/>
        public void TurnOffGroup(string featureId, byte groupNumber, TransitionMode transitionMode)
        {
            var request = new CsiLight
                          {
                              Item = new TurnOffRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         TransitionMode = (byte)transitionMode
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<TurnOffResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId,
                                            byte groupNumber,
                                            IEnumerable<MonochromeLightState> lightStates)
        {
            var intensities = new SetIntensityRequestRandom();

            foreach(var monochromeLightState in lightStates)
            {
                intensities.Light.Add(new SetIntensityRequestRandomLight
                                      {
                                          Intensity = monochromeLightState.Brightness,
                                          LightId = (byte)monochromeLightState.LightNumber
                                      });
            }

            var request = new CsiLight
                          {
                              Item = new SetIntensityRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         Item = intensities
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetIntensityResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId,
                                            byte groupNumber,
                                            ushort startingLight,
                                            IEnumerable<byte> brightnesses)
        {
            var intensityString = string.Join(" ", brightnesses.Select(brightness => brightness.ToString()).ToArray());
            var intensities = new SetIntensityRequestConsecutive
                              {
                                  Intensity = intensityString,
                                  StartingLight = (byte)startingLight
                              };

            var request = new CsiLight
                          {
                              Item = new SetIntensityRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         Item = intensities
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetIntensityResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, IEnumerable<RgbLightState> lightStates)
        {
            var randomColors = new SetColorRequestRandom();

            foreach(var rgbLightState in lightStates)
            {
                randomColors.Light.Add(new SetColorRequestRandomLight
                                       {
                                           Color = rgbLightState.Color.PackedColor,
                                           LightId = rgbLightState.LightNumber
                                       });
            }

            var request = new CsiLight
                          {
                              Item = new SetColorRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         Item = randomColors
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetColorResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors)
        {
            var colorString = string.Join(" ", colors.Select(color => color.PackedColor.ToString()).ToArray());
            var consecutiveColors = new SetColorRequestConsecutive
                                    {
                                        Color = colorString,
                                        StartingLight = startingLight
                                    };

            var request = new CsiLight
                          {
                              Item = new SetColorRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         Item = consecutiveColors
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SetColorResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void StartSequence(string featureId,
                                  byte groupNumber,
                                  uint sequenceNumber,
                                  TransitionMode transitionMode,
                                  byte[] parameters)
        {
            var request = new CsiLight
                          {
                              Item = new StartSequenceRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         Parameters = parameters,
                                         Sequence = sequenceNumber,
                                         TransitionMode = (byte)transitionMode
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<StartSequenceResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public bool IsSequenceRunning(string featureId, byte groupNumber, uint sequenceNumber)
        {
            var request = new CsiLight
                          {
                              Item = new SequenceRunningRequest
                                     {
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         SequenceNumber = sequenceNumber
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<SequenceRunningResponse>();
            CheckResponse(response.LightResponse);

            return response.RunningSpecified ? response.Running : false;
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId,
                                        byte groupNumber,
                                        ushort startingLight,
                                        IEnumerable<bool> lightStates)
        {
            const byte bitsPerLight = 1;
            var byteData = (from state in lightStates select state ? (byte)0x01 : (byte)0x00).ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);
            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId,
                                        byte groupNumber,
                                        ushort startingLight,
                                        IEnumerable<BitwiseLightIntensity> lightIntensities)
        {
            const byte bitsPerLight = 2;
            var byteData = lightIntensities.Cast<byte>().ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);
            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId,
                                        byte groupNumber,
                                        ushort startingLight,
                                        IEnumerable<BitwiseLightColor> lightColors)
        {
            const byte bitsPerLight = 4;
            var byteData = lightColors.Cast<byte>().ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);
            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId,
                                        byte groupNumber,
                                        ushort startingLight,
                                        IEnumerable<Rgb6> lightColors)
        {
            const byte bitsPerLight = 6;
            var byteData = (from color in lightColors select color.PackedColor).ToArray();
            var packedData = LightUtility.PackBits(bitsPerLight, byteData);
            BitwiseLightControl(featureId, groupNumber, startingLight, bitsPerLight, packedData);
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId,
                                        byte groupNumber,
                                        ushort startingLight,
                                        byte bitsPerLight,
                                        byte[] lightData)
        {
            var request = new CsiLight
                          {
                              Item = new BitwiseControlRequest
                                     {
                                         BitControl = bitsPerLight,
                                         FeatureId = featureId,
                                         Group = groupNumber,
                                         LightData = lightData,
                                         StartLight = (byte)startingLight
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<BitwiseControlResponse>();
            CheckResponse(response.LightResponse);
        }

        #endregion IPeripheralLights Implementation

        #region ICandle Implementation

        /// <inheritdoc/>
        public event EventHandler<CandleStateChangedEventArgs> CandleStateChangedEvent;

        /// <inheritdoc/>
        public bool CandleIlluminated(CandleID candleId)
        {
            if(candleId == CandleID.Invalid || candleId == CandleID.All)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to make a CandleStateRequest for.", nameof(candleId));
            }

            if(currentCategoryVersion < MinCandleCategoryVersion)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiLight
                          {
                              Item = new CandleStateRequest
                                     {
                                         Candle = (Candle)candleId
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<CandleStateResponse>();
            CheckResponse(response.LightResponse);

            return response.Illuminated;
        }

        /// <inheritdoc/>
        public void RegisterForCandleStateChangeEvents(CandleID candleId)
        {
            if(currentCategoryVersion < MinCandleCategoryVersion)
            {
                // Do not attempt to register for candle states if the given target doesn't support it.
                return;
            }

            if(candleId == CandleID.Invalid)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to register events for.", nameof(candleId));
            }

            var request = new CsiLight
                          {
                              Item = new CandleStateChangeRegistrationRequest
                                     {
                                         Candle = (Candle)candleId,
                                         RegistrationAction = RegistrationAction.Register
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<CandleStateChangeRegistrationResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <inheritdoc/>
        public void UnregisterForCandleStateChangeEvents(CandleID candleId)
        {
            if(currentCategoryVersion < MinCandleCategoryVersion)
            {
                // Do not attempt to unregister for candle states if the given target doesn't support it.
                return;
            }

            if(candleId == CandleID.Invalid)
            {
                throw new ArgumentException($"Candle ID \'{candleId}\' is an invalid Candle to unregister events for.", nameof(candleId));
            }

            var request = new CsiLight
                          {
                              Item = new CandleStateChangeRegistrationRequest
                                     {
                                         Candle = (Candle)candleId,
                                         RegistrationAction = RegistrationAction.Unregister
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetLightResponse<CandleStateChangeRegistrationResponse>();
            CheckResponse(response.LightResponse);
        }

        /// <summary>
        /// Handle a candle state changed event.
        /// </summary>
        /// <param name="candleStateChangeEvent">Arguments containing the candle state change info.</param>
        private void HandleCandleStateChangedEvent(CandleStateChangeEvent candleStateChangeEvent)
        {
            CandleStateChangedEvent?.Invoke(this,
                                            new CandleStateChangedEventArgs((CandleID)candleStateChangeEvent.Candle,
                                                                            candleStateChangeEvent.Illuminated));
        }

        #endregion ICandle Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiLight>();

            // Lock statements should limit the amount of processing that could be done
            // within the statement, which is why the event processing is done after the lock statement.
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
        private static void CheckResponse(LightResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case LightErrorCode.NONE:
                case LightErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case LightErrorCode.UNKNOWN_DRIVER_ERROR:
                case LightErrorCode.CLIENT_ALREADY_REGISTERED:
                case LightErrorCode.CLIENT_NOT_REGISTERED:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new LightCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        /// <summary>
        /// Get the LightSubFeature based on the device type.
        /// </summary>
        /// <param name="deviceType">The device type to get the feature for.</param>
        /// <returns>The LightSubFeature for the device type.</returns>
        private static LightSubFeature GetSubFeature(LightType deviceType)
        {
            switch(deviceType)
            {
                case LightType.AccentLights:
                    return LightSubFeature.AccentLights;

                case LightType.BonusGameLights:
                    return LightSubFeature.BonusGameLights;

                case LightType.CandleLights:
                    return LightSubFeature.Candle;

                case LightType.LightBar:
                    return LightSubFeature.LightBars;

                case LightType.LightBezel:
                    return LightSubFeature.LightBezel;

                case LightType.ReelBackLights:
                    return LightSubFeature.ReelBacklights;

                case LightType.CardReaderBezel:
                    return LightSubFeature.CardReaderBezel;

                case LightType.TopperLightRing:
                    return LightSubFeature.TopperLightRing;

                default:
                    //If the type is unknown then assume to extra functionality.
                    return LightSubFeature.BonusGameLights;
            }
        }

        /// <summary>
        /// Get a description of the light group based on the information provided by the CSI Manager.
        /// </summary>
        /// <param name="groupNumber">The number of the light group.</param>
        /// <param name="group">Information about the light group.</param>
        /// <param name="featureType">The type of the light device.</param>
        /// <returns>A ILightGroup describing the device.</returns>
        private static ILightGroup GetLightGroupDescription(byte groupNumber,
                                                            FeatureDataGroup group,
                                                            LightSubFeature featureType)
        {
            switch(featureType)
            {
                case LightSubFeature.LightBars:
                {
                    if(!(@group.Item is FeatureDataGroupLightBar bar))
                    {
                        throw new LightGroupDescriptionException(featureType, "No light bar description available.");
                    }

                    return new LightBarGroup(groupNumber, @group.HasRGB, bar.NumberOfLights.Select(count => (byte)count));
                }

                case LightSubFeature.LightBezel:
                {
                    if(!(@group.Item is FeatureDataGroupLightBezel lightBezel))
                    {
                        throw new LightGroupDescriptionException(featureType, "No light bezel description available.");
                    }

                    return new LightBezelGroup(groupNumber,
                                               @group.HasRGB,
                                               (byte)lightBezel.LightsOnTop,
                                               (byte)lightBezel.LightsOnBottom,
                                               (byte)lightBezel.LightsOnLeft,
                                               (byte)lightBezel.LightsOnRight);
                }

                default:
                    return new LightGroup(groupNumber, @group.NumberOfLights, @group.HasRGB);
            }
        }

        #endregion Private Methods

        #region Constructors

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">Foundation version to target.</param>
        public PeripheralLightsCategory(FoundationTarget target)
        {
            // Versions 1.2 and 1.3 have never been used/tested by the SDK, so they are being skipped.
            currentCategoryVersion = new Version(1, target.IsEqualOrNewer(FoundationTarget.AscentQ3Series) ? 4 : 1);

            eventHandlers[typeof(CandleStateChangeEvent)] = message => HandleCandleStateChangedEvent(message as CandleStateChangeEvent);
        }

        #endregion Constructors
    }
}
