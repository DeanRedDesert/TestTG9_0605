//-----------------------------------------------------------------------
// <copyright file = "ButtonCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CsiTransport;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using Logging;
    using ButtonFunction = ButtonFunction;

    /// <summary>
    /// Category which handles buttons.
    /// </summary>
    public class ButtonCategory : CategoryBase<CsiButton>, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        ///<summary>
        /// Dictionary of lamp states to Update on each Update call of ICabinetUpdate.
        /// </summary>
        private Dictionary<ButtonId, bool> lampStateList;

        /// <summary>
        /// Mapping manager that could convert between <see cref="ButtonPanelLocation"/> and DeviceId.
        /// </summary>
        private readonly PanelLocationManager panelLocationManager;

        /// <summary>
        /// Error code to represent if downloading an image to the button panel failed.
        /// </summary>
        private const int ImageOperationFailedErrorCode = -1;

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="ButtonCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region Events

        /// <summary>
        /// Event which indicates that a button has been pressed.
        /// </summary>
        public event EventHandler<CabinetButtonPressedEventArgs> ButtonPressedEvent;

        #endregion Events

        #region Constructor

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// </param>
        public ButtonCategory(FoundationTarget target)
        {
            panelLocationManager = new PanelLocationManager();
            eventHandlers[typeof(ButtonEvent)] =
                (message) => HandleButtonEvent(message as ButtonEvent);

            foundationTarget = target;
        }

        #endregion Constructor

        #region ICabinetCategory Members

        /// <inheritdoc/>
        public override Category Category => Category.CsiButton;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => (ushort)(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds) ? 6 : 4);

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var buttonMessage = message as CsiButton;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(buttonMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiButton),
                                                                message.GetType()));
            }

            if(buttonMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Button message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(buttonMessage.Item.GetType()))
            {
                eventHandlers[buttonMessage.Item.GetType()](buttonMessage.Item);
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + buttonMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var buttonMessage = message as CsiButton;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(buttonMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiButton),
                                                                message.GetType()));
            }

            if(buttonMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Button message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + buttonMessage.Item.GetType());
        }

        #endregion ICabinetCategory Members

        #region ICabinetUpdate Members

        /// <summary>
        /// Updates the ButtonCategory with the CabinetLib.
        /// </summary>
        public void Update()
        {
            UpdateLampStates();
        }

        #endregion ICabinetUpdate Members

        #region Private Methods

        /// <summary>
        /// Handle a button panel button event.
        /// </summary>
        /// <param name="buttonEvent">The event to handle.</param>
        private void HandleButtonEvent(ButtonEvent buttonEvent)
        {
            if(ButtonPressedEvent != null)
            {
                var buttonIdentifier = panelLocationManager.ConvertToButtonIdentifier(buttonEvent.Button.ButtonId);
                var eventArgs = buttonEvent.timestampSpecified
                                    ? new CabinetButtonPressedEventArgs(buttonIdentifier,
                                                                        buttonEvent.Button.Pressed,
                                                                        buttonEvent.Button.ButtonFunctions.Select(function => (ButtonFunction)function).ToList(),
                                                                        (long)buttonEvent.timestamp)
                                    : new CabinetButtonPressedEventArgs(buttonIdentifier,
                                                                        buttonEvent.Button.Pressed,
                                                                        buttonEvent.Button.ButtonFunctions.Select(function => (ButtonFunction)function).ToList());

                ButtonPressedEvent(this, eventArgs);
            }
        }

        /// <summary>
        /// Update the list of LampStates.
        /// </summary>
        private void UpdateLampStates()
        {
            if(lampStateList != null)
            {
                var request = new CsiButton
                              {
                                  Item = new SetLampStateRequest
                                         {
                                             Lamps = new LampStates
                                                     {
                                                         Lamp = lampStateList.Select(entry => new LampState { ButtonId = entry.Key, State = entry.Value }).ToList()
                                                     }
                                         }
                              };

                try
                {
                    Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                    var response = GetButtonResponse<SetLampStateResponse>();
                    CheckResponse(response.ButtonResponse, true);
                }
                finally
                {
                    lampStateList.Clear();
                    lampStateList = null;
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
        private TResponse GetButtonResponse<TResponse>() where TResponse : class
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
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation.</param>
        /// <exception cref="ButtonCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        /// <exception cref="ClientDoesNotOwnResourceException">
        /// Thrown if the response indicates that the client lost control of the buttons.
        /// </exception>
        /// <exception cref="ConnectionLostException">
        /// Thrown if the response indicates that the client lost connection with the buttons.
        /// </exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(ButtonResponse response, bool errorOnFailure)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case ButtonErrorCode.NONE:
                case ButtonErrorCode.CONNECTION_LOST:
                case ButtonErrorCode.SET_ALL_LAMPS_FAILED:
                case ButtonErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case ButtonErrorCode.PERIPHERAL_STATUS_ERROR:
                    break;

                default:
                    shouldThrow = errorOnFailure;
                    break;
            }

            if(shouldThrow)
            {
                throw new ButtonCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods

        #region Button Functions

        /// <summary>
        /// Sets the state of all lamps.  They can be a mix of on and off.
        /// </summary>
        /// <param name="lampStates">
        /// The list of states to use to set ALL lamps. The lamp ID values are the same as the button ID values.
        /// 0xFF represents all buttons on all button panels.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="lampStates"/> are null.</exception>
        public void SetLampStates(IList<ButtonLampState> lampStates)
        {
            if(lampStates == null)
            {
                throw new ArgumentNullException(nameof(lampStates));
            }

            if(lampStateList == null)
            {
                lampStateList = new Dictionary<ButtonId, bool>();
            }

            foreach(var lamp in lampStates)
            {
                var buttonId = panelLocationManager.ConvertToButtonId(lamp.ButtonId);
                if(buttonId != null)
                {
                    lampStateList[buttonId] = lamp.State;
                }
            }
        }

        /// <summary>
        /// Get the state of a lamp or lamps.
        /// </summary>
        /// <param name="buttonIds">List of button IDs to get the state of.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The lamp states of the requested buttons.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the buttonIds parameter is null.</exception>
        public IList<ButtonLampState> GetLampState(IList<ButtonIdentifier> buttonIds, bool errorOnFailure = true)
        {
            if(buttonIds == null)
            {
                throw new ArgumentNullException(nameof(buttonIds));
            }

            var buttonIdList = buttonIds.Select(identifier => panelLocationManager.ConvertToButtonId(identifier))
                                        .Where(id => id != null)
                                        .ToList();

            var lampList = new List<ButtonLampState>();
            var requestedKeysAreInCache = false;

            if(lampStateList != null)
            {
                requestedKeysAreInCache = true;

                foreach(var buttonId in buttonIdList)
                {
                    if(lampStateList.ContainsKey(buttonId))
                    {
                        lampList.Add(new ButtonLampState(panelLocationManager.ConvertToButtonIdentifier(buttonId),
                                                         lampStateList[buttonId]));
                    }
                    else
                    {
                        requestedKeysAreInCache = false;
                    }
                }
            }

            if(!requestedKeysAreInCache)
            {
                var request = new CsiButton
                              {
                                  Item = new GetLampStateRequest { ButtonId = buttonIdList }
                              };

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetButtonResponse<GetLampStateResponse>();
                CheckResponse(response.ButtonResponse, errorOnFailure);

                if(response.Lamps != null)
                {
                    lampList.Clear();
                    lampList.AddRange(from lamp in response.Lamps.Lamp
                                      select new ButtonLampState(panelLocationManager.ConvertToButtonIdentifier(lamp.ButtonId),
                                                                 lamp.State));
                }
            }

            return lampList;
        }

        /// <summary>
        ///  Gets the configuration of the current button panel.
        /// </summary>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The configuration of the button panel.</returns>
        /// <remarks>
        /// This is not implemented as a property because it would violate the performance expectations of a property.
        /// </remarks>
        public IList<ConfigurationResponseData> GetButtonPanelConfiguration(bool errorOnFailure = true)
        {
            var request = new CsiButton
                          {
                              Item = new ConfigurationRequest()
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetButtonResponse<ConfigurationResponse>();
            CheckResponse(response.ButtonResponse, errorOnFailure);

            panelLocationManager.UpdateLocationMapping(response.Data ?? new List<ConfigurationResponseData>());

            return response.Data;
        }

        /// <summary>
        /// Sends an image set to the dynamic buttons.
        /// </summary>
        /// <param name="fileName">The full path to the image set file to load.</param>
        /// <param name="panelLocation">Location of the panel.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The image set id. When the download failed return -1.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the fileName is null.</exception>
        public int DownloadImageSet(string fileName, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
            if(fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if(!panelLocationManager.GetDeviceId(panelLocation, out var deviceId))
            {
                return ImageOperationFailedErrorCode;
            }

            var request = new CsiButton
                          {
                              Item = new DownloadImageSetRequest
                                     {
                                         Filename = fileName,
                                         DeviceId = deviceId
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetButtonResponse<DownloadImageSetResponse>();
            CheckResponse(response.ButtonResponse, errorOnFailure);

            return response.ImageSetIdSpecified ? response.ImageSetId : -1;
        }

        /// <summary>
        /// Removes the given image set from the dynamic buttons.
        /// </summary>
        /// <param name="setId">The ID of the image set to remove.</param>
        /// <param name="panelLocation">Location of the panel.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The image set id. When the removal failed return -1.</returns>
        public int RemoveImageSet(ushort setId, ButtonPanelLocation panelLocation, bool errorOnFailure = true)
        {
            if(!panelLocationManager.GetDeviceId(panelLocation, out var deviceId))
            {
                return ImageOperationFailedErrorCode;
            }

            var request = new CsiButton
                          {
                              Item = new RemoveImageSetRequest
                                     {
                                         ImageSetId = setId,
                                         DeviceId = deviceId
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetButtonResponse<RemoveImageSetResponse>();
            CheckResponse(response.ButtonResponse, errorOnFailure);

            return response.ImageSetIdSpecified ? response.ImageSetId : -1;
        }

        /// <summary>
        /// Plays an animation on a dynamic button.
        /// </summary>
        /// <param name="buttonIdentifier">The ID of the button to play the animation on.</param>
        /// <param name="imageSetId">The ID of the image set to use for the animation.</param>
        /// <param name="animationId">The ID of the animation to use.</param>
        /// <param name="frameDelay">The frame delay of the animation, in milliseconds.</param>
        /// <param name="repeat">Flag to set repeat on or off</param>
        /// <param name="transitionMode">Flag to set transition as true or false.</param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The id of the button that responded to the request or null if failure.</returns>
        public ButtonIdentifier PlayAnimation(ushort animationId,
                                              ButtonIdentifier buttonIdentifier,
                                              ushort imageSetId,
                                              bool repeat,
                                              bool transitionMode,
                                              ushort frameDelay,
                                              bool errorOnFailure = true)
        {
            var buttonId = panelLocationManager.ConvertToButtonId(buttonIdentifier);
            if(buttonId == null)
            {
                return new ButtonIdentifier(ButtonPanelLocation.Unknown, (int)SwitchId.InvalidButtonId);
            }

            var request = new CsiButton
                          {
                              Item = new PlayAnimationRequest
                                     {
                                         AnimationId = animationId,
                                         ButtonId = buttonId,
                                         ImageSetId = imageSetId,
                                         Repeat = repeat,
                                         TransitionMode = transitionMode,
                                         FrameDelay = frameDelay
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetButtonResponse<PlayAnimationResponse>();
            CheckResponse(response.ButtonResponse, errorOnFailure);

            return response.ButtonId == null ? null : panelLocationManager.ConvertToButtonIdentifier(response.ButtonId);
        }

        /// <summary>
        /// Sets all of the pixels on a given button on or off.
        /// </summary>
        /// <param name="buttonIdentifier">
        /// The button to set the pixels on.
        /// 0xFF represents all buttons on all button panels.
        /// </param>
        /// <param name="pixelState">
        /// If true, all pixels will be turned on.  If false, all pixels will be turned off.
        /// </param>
        /// <param name="errorOnFailure">Flag indicating whether an error should be thrown if the command failed to execute on the Foundation. Defaults to true.</param>
        /// <returns>The id of the button that responded or null if failure.</returns>
        public ButtonIdentifier SetAllPixels(ButtonIdentifier buttonIdentifier, bool pixelState, bool errorOnFailure = true)
        {
            var buttonId = panelLocationManager.ConvertToButtonId(buttonIdentifier);
            if(buttonId == null)
            {
                return new ButtonIdentifier(ButtonPanelLocation.Unknown, (int)SwitchId.InvalidButtonId);
            }

            var request = new CsiButton
                          {
                              Item = new SetAllPixelsRequest
                                     {
                                         ButtonId = buttonId,
                                         PixelState = pixelState,
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetButtonResponse<SetAllPixelsResponse>();
            CheckResponse(response.ButtonResponse, errorOnFailure);

            return response.ButtonId == null ? null : panelLocationManager.ConvertToButtonIdentifier(response.ButtonId);
        }

        /// <summary>
        /// Convert the device Id from a panel location.
        /// </summary>
        /// <param name="panelLocation">Location of the panel.</param>
        /// <param name="deviceId">The device Id retrieved.</param>
        /// <returns>True if the conversion succeeds. Otherwise, false.</returns>
        public bool GetDeviceId(ButtonPanelLocation panelLocation, out string deviceId)
        {
            return panelLocationManager.GetDeviceId(panelLocation, out deviceId);
        }

        #endregion Button Functions
    }
}
