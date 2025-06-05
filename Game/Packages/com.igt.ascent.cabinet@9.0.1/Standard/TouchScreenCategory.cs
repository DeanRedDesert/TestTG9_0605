//-----------------------------------------------------------------------
// <copyright file = "TouchScreenCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Category for touch screen.
    /// </summary>
    internal class TouchScreenCategory : CategoryBase<CsiTouchScreen>, ITouchScreen, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// The queue of event messages from the CSI host.
        /// </summary>
        private readonly Queue<CsiTouchScreen> eventQueue = new Queue<CsiTouchScreen>();

        /// <summary>
        /// flag set when the DPP's debounce interval is set to the minimum;
        /// used so that reset messages are not unnecessarily sent.
        /// </summary>
        private bool dppDebounceIntervalSetToMinimum;

        /// <summary>
        /// Ascent foundation target
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        /// <summary>
        /// Initializes a new instance of <see cref="TouchScreenCategory"/>.
        /// </summary>
        public TouchScreenCategory(FoundationTarget ascentFoundationTarget)
        {
            foundationTarget = ascentFoundationTarget;

            eventHandlers[typeof(TouchScreenInfoEvent)] =
                message => HandleTouchScreenInfoEvent(message as TouchScreenInfoEvent);

            eventHandlers[typeof(TouchDisplayTargetEvent)] =
                message => HandleTouchDisplayTargetEvent(message as TouchDisplayTargetEvent);

            eventHandlers[typeof(TouchCalibrationCompleteEvent)] =
                message => HandleTouchCalibrationCompleteEvent(message as TouchCalibrationCompleteEvent);

            eventHandlers[typeof(TouchScreenConnectionChangedEvent)] =
                message => HandleTouchScreenConnectionChangedEvent(message as TouchScreenConnectionChangedEvent);
        }

        #region CategoryBase<CsiTouchScreen> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiTouchScreen;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentN01Series))
                {
                    return 4;
                }

                return 3;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiTouchScreen touchScreenMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiTouchScreen),
                                                                message.GetType()));
            }

            if(touchScreenMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. TouchScreen message contained no event, request or response.");
            }

            lock(eventQueue)
            {
                eventQueue.Enqueue(touchScreenMessage);
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiTouchScreen touchScreenMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiTouchScreen),
                                                                message.GetType()));
            }

            if(touchScreenMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. TouchScreen message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + touchScreenMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiTouchScreen> Implementation

        #region ITouchScreen Implementation

        /// <inheritdoc/>
        public event EventHandler<TouchScreenInfoEventArgs> TouchScreenInfoEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchDisplayTargetEventArgs> TouchDisplayTargetEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchCalibrationCompleteEventArgs> TouchCalibrationCompleteEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchScreenConnectionChangedEventArgs> TouchScreenConnectionChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<TouchScreenExclusiveModeChangedEventArgs> TouchScreenExclusiveModeChangedEvent;

        /// <inheritdoc/>
        public IEnumerable<TouchScreenInfo> GetTouchScreenInfo()
        {
            var request = new CsiTouchScreen
                          {
                              Item = new GetTouchScreenInfoRequest()
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetTouchScreenResponse<GetTouchScreenInfoResponse>();
            CheckResponse(response.TouchScreenResponse);

            return response.TouchScreens;
        }

        /// <inheritdoc/>
        public void RequestMinimumDigitizerDebounceIntervalForDpp()
        {
            var request = new CsiTouchScreen
                          {
                              Item = new RequestMinimumDigitizerDebounceIntervalForDPP()
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetTouchScreenResponse<DPPDigitizerDebounceIntervalResponse>().TouchScreenResponse);

            dppDebounceIntervalSetToMinimum = true;
        }

        /// <inheritdoc/>
        public void ResetDigitizerDebounceIntervalForDpp()
        {
            if(dppDebounceIntervalSetToMinimum)
            {
                var request = new CsiTouchScreen
                              {
                                  Item = new ResetDigitizerDebounceIntervalForDPP()
                              };

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                CheckResponse(GetTouchScreenResponse<DPPDigitizerDebounceIntervalResponse>().TouchScreenResponse);

                dppDebounceIntervalSetToMinimum = false;
            }
        }

        /// <inheritdoc/>
        public void SetDigitizerToMinimumDebounce(DigitizerRole role)
        {
            var request = new CsiTouchScreen
                          {
                              Item = new SetDigitizerToMinimumDebounceIntervalRequest
                                     {
                                         digitizer = role
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetTouchScreenResponse<DigitizerDebounceIntervalResponse>().TouchScreenResponse);
        }

        /// <inheritdoc/>
        public void ResetDigitizerDebounceInterval(DigitizerRole role)
        {
            var request = new CsiTouchScreen
                          {
                              Item = new ResetDigitizerDebounceIntervalRequest()
                                     {
                                         digitizer = role
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetTouchScreenResponse<DigitizerDebounceIntervalResponse>().TouchScreenResponse);
        }

        /// <ineritdoc/>
        public void SetDigitizerExclusiveMode(DigitizerRole role, bool exclusive)
        {
            var currentSchemaVersion = new Version(VersionMajor, VersionMinor);
            var minimumSchemaVersionRequiredForSetDigitizerExclusiveMode = new Version(1, 4);
            if(currentSchemaVersion < minimumSchemaVersionRequiredForSetDigitizerExclusiveMode)
            {
                throw new NotSupportedException("SetDigitizerExclusiveMode requires targeting CsiTouchScreenV1 schema version 1.4 or newer.");
            }

            var request = new CsiTouchScreen
                          {
                              Item = new SetDigitizerExclusiveModeRequest()
                                     {
                                         Instance = new DigitizerExclusiveMode()
                                                    {
                                                        DigitizerRole = role,
                                                        ExclusiveModeRequested = exclusive
                                                    }
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var setDigitizerExclusiveModeResponse = GetTouchScreenResponse<SetDigitizerExclusiveModeResponse>();
            CheckResponse(setDigitizerExclusiveModeResponse.TouchScreenResponse);

            for(var index = 0; index < setDigitizerExclusiveModeResponse.Instance.Count; index++)
            {
                TouchScreenExclusiveModeChangedEvent?.Invoke(
                    this,
                    new TouchScreenExclusiveModeChangedEventArgs(setDigitizerExclusiveModeResponse.Instance[index].DigitizerRole,
                                                                 setDigitizerExclusiveModeResponse.Instance[index].ExclusiveModeRequested));
            }
        }

        #endregion ITouchScreen Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        void ICabinetUpdate.Update()
        {
            lock(eventQueue)
            {
                while(eventQueue.Count > 0)
                {
                    var touchMessage = eventQueue.Dequeue();
                    if(eventHandlers.ContainsKey(touchMessage.Item.GetType()))
                    {
                        eventHandlers[touchMessage.Item.GetType()](touchMessage.Item);
                    }
                    else
                    {
                        throw new UnhandledEventException("Event not handled: " + touchMessage.Item.GetType());
                    }
                }
            }
        }

        #endregion ICabinetUpdate Implementation

        #region Private Methods

        /// <summary>
        /// Handles the touch screen info event.
        /// </summary>
        /// <param name="touchScreenInfoEvent">The event to handle.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="touchScreenInfoEvent"/> is null.
        /// </exception>
        private void HandleTouchScreenInfoEvent(TouchScreenInfoEvent touchScreenInfoEvent)
        {
            if(touchScreenInfoEvent == null)
            {
                throw new ArgumentNullException(nameof(touchScreenInfoEvent));
            }

            TouchScreenInfoEvent?.Invoke(this,
                                         new TouchScreenInfoEventArgs(touchScreenInfoEvent.TouchScreens));
        }

        /// <summary>
        /// Handles the touch display target event.
        /// </summary>
        /// <param name="touchDisplayTargetEvent">The event to handle.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="touchDisplayTargetEvent"/> is null.
        /// </exception>
        private void HandleTouchDisplayTargetEvent(TouchDisplayTargetEvent touchDisplayTargetEvent)
        {
            if(touchDisplayTargetEvent == null)
            {
                throw new ArgumentNullException(nameof(touchDisplayTargetEvent));
            }

            TouchDisplayTargetEvent?.Invoke(this,
                                            new TouchDisplayTargetEventArgs(touchDisplayTargetEvent.x,
                                                                            touchDisplayTargetEvent.y,
                                                                            touchDisplayTargetEvent.display,
                                                                            touchDisplayTargetEvent.offscreen));
        }

        /// <summary>
        /// Handles the touch calibration complete event.
        /// </summary>
        /// <param name="touchCalibrationCompleteEvent">The event to handle.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="touchCalibrationCompleteEvent"/> is null.
        /// </exception>
        private void HandleTouchCalibrationCompleteEvent(TouchCalibrationCompleteEvent touchCalibrationCompleteEvent)
        {
            if(touchCalibrationCompleteEvent == null)
            {
                throw new ArgumentNullException(nameof(touchCalibrationCompleteEvent));
            }

            TouchCalibrationCompleteEvent?.Invoke(this,
                                                  new TouchCalibrationCompleteEventArgs(touchCalibrationCompleteEvent.display));
        }

        /// <summary>
        /// Handles the touch screen connection changed event.
        /// </summary>
        /// <param name="connectionChangedEvent">The event to handle.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="connectionChangedEvent"/> is null.
        /// </exception>
        private void HandleTouchScreenConnectionChangedEvent(TouchScreenConnectionChangedEvent connectionChangedEvent)
        {
            if(connectionChangedEvent == null)
            {
                throw new ArgumentNullException(nameof(connectionChangedEvent));
            }

            TouchScreenConnectionChangedEvent?.Invoke(this,
                                                      new TouchScreenConnectionChangedEventArgs(connectionChangedEvent.deviceId,
                                                                                                connectionChangedEvent.driver,
                                                                                                connectionChangedEvent.driverSubClass,
                                                                                                connectionChangedEvent.connectionStatus ==
                                                                                                ConnectionStatus.Connected));
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetTouchScreenResponse<TResponse>() where TResponse : class
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
        /// <exception cref="TouchScreenCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(TouchScreenResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case TouchScreenErrorCode.NONE:
                case TouchScreenErrorCode.REQUEST_PROHIBITED_IN_THIS_JURISDICTION:
                case TouchScreenErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new TouchScreenCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods
    }
}
