//-----------------------------------------------------------------------
// <copyright file = "ServiceCategory.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CsiTransport;
    using CSI.Schemas.Internal;
    using Foundation.Transport;

    /// <summary>
    /// Category for managing service requests.
    /// </summary>
    internal class ServiceCategory : CategoryBase<CsiService>, IService, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly List<CsiService> pendingEvents = new List<CsiService>();

        /// <summary>
        /// The minimum CsiService schema version required to emulate cashout/service buttons.
        /// </summary>
        private static readonly Version MinimumSchemaVersionRequiredForEmulatedServiceButtonEnabled = new Version(1, 3);

        /// <summary>
        /// The minimum CsiService schema version required to emulate cashout/service buttons.
        /// </summary>
        private static readonly Version MinimumSchemaVersionRequiredForEmulatedCashoutServiceSupport = new Version(1, 2);

        /// <summary>
        /// The minimum CsiService schema version required request for Cashout.
        /// </summary>
        private static readonly Version MinimumSchemaVersionRequiredForRequestCashOut = new Version(1, 1);

        /// <summary>
        /// The minimum CsiService schema version required for getting player call attendant and player service request states..
        /// </summary>
        private static readonly Version MinimumSchemaVersionRequiredForPlayerServiceStates = new Version(1, 4);

        /// <summary>
        /// The Categories configured version.
        /// </summary>
        private readonly Version currentSchemaVersion;

        #endregion Private Fields

        #region Events

        /// <inheritdoc/>
        public event EventHandler<PromptPlayerOnCashoutConfigItemChangedEventArgs> PromptPlayerOnCashoutConfigItemChangedEvent;

        /// <inheritdoc/>
        public event EventHandler<EmulatedServiceButtonEnabledConfigItemChangedEventArgs> EmulatedServiceButtonEnabledConfigItemChangedEvent;

        /// <inheritdoc />
        public event EventHandler<PlayerCallAttendantStateChangedEventArgs> PlayerCallAttendantStateChangedEvent;

        /// <inheritdoc />
        public event EventHandler<PlayerServiceRequestStateChangedEventArgs> PlayerServiceRequestStateChangedEvent;

        #endregion Events

        public static ServiceCategory CreateInstance(FoundationTarget currentFoundationTarget)
        {
            // Only supported for the foundation target shown below.
            return currentFoundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds) ||
                   currentFoundationTarget == FoundationTarget.AscentJSeriesMps
                ? new ServiceCategory(currentFoundationTarget)
                : null;
        }

        #region Constructor

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// </param>
        private ServiceCategory(FoundationTarget target)
        {
            var minorVersion = 0;

            if(target.IsEqualOrNewer(FoundationTarget.AscentS1Series))
            {
                minorVersion = 4;
            }
            else if(target.IsEqualOrNewer(FoundationTarget.AscentR1Series))
            {
                minorVersion = 3;
            }
            else if(target.IsEqualOrNewer(FoundationTarget.AscentQ3Series))
            {
                minorVersion = 2;
            }
            else if(target.IsEqualOrNewer(FoundationTarget.AscentQ1Series))
            {
                minorVersion = 1;
            }

            currentSchemaVersion = new Version(1, minorVersion);

            eventHandlers[typeof(PromptPlayerOnCashoutConfigItemChangedEvent)] =
                data => HandleCsiPromptPlayerOnCashoutConfigItemChangedEvent(data as PromptPlayerOnCashoutConfigItemChangedEvent);

            eventHandlers[typeof(EmulatedServiceButtonEnabledConfigItemChangedEvent)] =
                data => HandleCsiEmulatedServiceButtonEnabledConfigItemChangedEvent(data as EmulatedServiceButtonEnabledConfigItemChangedEvent);

            eventHandlers[typeof(CallAttendantChangedEvent)] =
                data => HandleCsiCallAttendantChangedEvent(data as CallAttendantChangedEvent);

            eventHandlers[typeof(PlayerServiceRequestChangedEvent)] =
                data => HandleCsiPlayerServiceRequestChangedEvent(data as PlayerServiceRequestChangedEvent);
        }

        #endregion Constructor

        #region CategoryBase<CsiService> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiService;

        /// <inheritdoc/>
        public override ushort VersionMajor => (ushort)currentSchemaVersion.Major;

        /// <inheritdoc/>
        public override ushort VersionMinor => (ushort)currentSchemaVersion.Minor;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(!(message is CsiService serviceMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiService), message.GetType()));
            }
            if(serviceMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Service message contained no event, request, or response.");
            }

            if(eventHandlers.ContainsKey(serviceMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Add(serviceMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + serviceMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(!(message is CsiService serviceMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiService), message.GetType()));
            }
            if(serviceMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Service message contained no event, request, or response.");
            }

            //This category has no events.
            throw new UnhandledEventException("Event not handled: " + serviceMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiService> Implementation

        #region Private Methods

        /// <summary>
        /// Handler for the CSI 'Prompt Player On Cashout' config item changed event.
        /// </summary>
        /// <param name="csiEventData"><see cref="PromptPlayerOnCashoutConfigItemChangedEvent"/> event.</param>
        private void HandleCsiPromptPlayerOnCashoutConfigItemChangedEvent(PromptPlayerOnCashoutConfigItemChangedEvent csiEventData)
        {
            PromptPlayerOnCashoutConfigItemChangedEvent?.Invoke(
                this,
                new PromptPlayerOnCashoutConfigItemChangedEventArgs(csiEventData.Prompt));
        }

        /// <summary>
        /// Handler for the CSI 'Emulated Service Button Enabled' config item changed event.
        /// </summary>
        /// <param name="csiEventData"><see cref="PromptPlayerOnCashoutConfigItemChangedEvent"/> event.</param>
        private void HandleCsiEmulatedServiceButtonEnabledConfigItemChangedEvent(EmulatedServiceButtonEnabledConfigItemChangedEvent csiEventData)
        {
            EmulatedServiceButtonEnabledConfigItemChangedEvent?.Invoke(
                this,
                new EmulatedServiceButtonEnabledConfigItemChangedEventArgs(csiEventData.Enabled));
        }

        /// <summary>
        /// Handler for the CSI Call Attendant State Changed event.
        /// </summary>
        /// <param name="csiEventData">The CSI event data received.</param>
        private void HandleCsiCallAttendantChangedEvent(CallAttendantChangedEvent csiEventData)
        {
            PlayerCallAttendantStateChangedEvent?.Invoke(
                this,
                new PlayerCallAttendantStateChangedEventArgs(csiEventData.CallAttendant));
        }

        /// <summary>
        /// Handler for the CSI Call Attendant State Changed event.
        /// </summary>
        /// <param name="csiEventData">The CSI event data received.</param>
        private void HandleCsiPlayerServiceRequestChangedEvent(PlayerServiceRequestChangedEvent csiEventData)
        {
            PlayerServiceRequestStateChangedEvent?.Invoke(
                this,
                new PlayerServiceRequestStateChangedEventArgs(csiEventData.PlayerServiceRequested));
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion Private Methods

        #region IService Implementation

        /// <inheritdoc/>
        public void RequestService()
        {
            var request = new CsiService
            {
                Item = new ServiceRequest()
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            //Message has no response.
        }

        /// <inheritdoc/>
        /// <exception cref="NotSupportedException">
        /// RequestCashOut requires targeting CsiServiceV1 schema version 1.1 or newer.
        /// </exception>
        public void RequestCashOut()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForRequestCashOut)
            {
                throw new NotSupportedException($"RequestCashOut requires targeting CsiServiceV1 schema version 1.1 or newer, while the current version is {currentSchemaVersion}.");
            }

            var request = new CsiService
            {
                Item = new CashOutRequest()
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            //Message has no response.
        }

        /// <inheritdoc/>
        public bool RegisterForPromptPlayerOnCashoutConfigItemChangedEvents()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedCashoutServiceSupport)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
            {
                Item = new RegisterForPromptPlayerOnCashoutConfigItemChangedEventsRequest { RegistrationAction = RegistrationAction1.Register }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForPromptPlayerOnCashoutConfigItemChangedEventsResponse>();

            return(response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE) || (response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_ALREADY_REGISTERED);
        }

        /// <inheritdoc/>
        public bool UnregisterForPromptPlayerOnCashoutConfigItemChangedEvents()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedCashoutServiceSupport)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
            {
                Item = new RegisterForPromptPlayerOnCashoutConfigItemChangedEventsRequest { RegistrationAction = RegistrationAction1.Unregister }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForPromptPlayerOnCashoutConfigItemChangedEventsResponse>();

            return(response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE) || (response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_NOT_REGISTERED);
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the foundation fails to return the 'Prompt on Player Cashout' config item value.
        /// </exception>
        public bool GetPromptPlayerOnCashoutConfigItemValue()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedCashoutServiceSupport)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
            {
                Item = new GetPromptPlayerOnCashoutConfigItemValueRequest()
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<GetPromptPlayerOnCashoutConfigItemValueResponse>();

            if((response.ServiceResponse.ErrorCode != ServiceErrorCode.NONE) || (response.PromptSpecified == false))
            {
                throw new InvalidMessageException("Foundation failed to return the 'Prompt on Player Cashout' config item value.");
            }
            return response.Prompt;
        }

        /// <inheritdoc/>
        /// <exception cref="InvalidMessageException">
        /// Thrown if the foundation fails to return the EGM's buttons to emulate.
        /// </exception>
        public IReadOnlyList<Cabinet.EmulatableButton> GetTheButtonsThatTheEgmRequiresToBeEmulated()
        {
            var buttonsToEmulate = new List<Cabinet.EmulatableButton>();
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedCashoutServiceSupport)
            {
                // Return default value if the current target does not support this API.
                return buttonsToEmulate;
            }

            var request = new CsiService
            {
                Item = new GetTheButtonsThatTheEGMRequiresToBeEmulatedRequest()
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<GetTheButtonsThatTheEGMRequiresToBeEmulatedResponse>();

            if(response.ServiceResponse.ErrorCode != ServiceErrorCode.NONE)
            {
                throw new InvalidMessageException("Foundation failed to return the EGM's buttons to emulate.");
            }

            foreach(var button in response.EmulatableButton)
            {
                switch(button)
                {
                    case EmulatableButton.Cashout:
                        buttonsToEmulate.Add(Cabinet.EmulatableButton.Cashout);
                        break;

                    case EmulatableButton.Service:
                        buttonsToEmulate.Add(Cabinet.EmulatableButton.Service);
                        break;
                }
            }
            return buttonsToEmulate;
        }

        /// <inheritdoc/>
        public bool RegisterForEmulatedServiceButtonEnabledConfigItemChangedEvents()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedServiceButtonEnabled)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
            {
                Item = new RegisterForEmulatedServiceButtonEnabledConfigItemChangedEventsRequest { RegistrationAction = RegistrationAction1.Register }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForEmulatedServiceButtonEnabledConfigItemChangedEventsResponse>();

            return (response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE) || (response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_ALREADY_REGISTERED);
        }

        /// <inheritdoc/>
        public bool UnregisterForEmulatedServiceButtonEnabledConfigItemChangedEvents()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForEmulatedServiceButtonEnabled)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
            {
                Item = new RegisterForEmulatedServiceButtonEnabledConfigItemChangedEventsRequest { RegistrationAction = RegistrationAction1.Unregister }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForEmulatedServiceButtonEnabledConfigItemChangedEventsResponse>();

            return (response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE) || (response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_NOT_REGISTERED);
        }

        /// <inheritdoc />
        public bool GetPlayerCallAttendantState()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new GetCallAttendantRequest()
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<GetCallAttendantResponse>();

            if((response.ServiceResponse.ErrorCode != ServiceErrorCode.NONE) || (response.StateSpecified == false))
            {
                throw new InvalidMessageException("Foundation failed to return the Call Attendant state value.");
            }
            return response.State;
        }

        /// <inheritdoc />
        public bool RegisterPlayerCallAttendantStateEvent()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new RegisterForCallAttendantChangedEventsRequest()
                                             {
                                                 RegistrationAction = RegistrationAction1.Register
                                             }
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForCallAttendantChangedEventsResponse>();

            return response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE ||
                   response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_ALREADY_REGISTERED;
        }

        /// <inheritdoc />
        public bool UnregisterPlayerCallAttendantStateEvent()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new RegisterForCallAttendantChangedEventsRequest()
                                             {
                                                 RegistrationAction = RegistrationAction1.Unregister
                                             }
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForCallAttendantChangedEventsResponse>();

            return response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE || 
                   response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_NOT_REGISTERED;
        }

        /// <inheritdoc />
        public bool GetPlayerServiceRequestState()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new GetPlayerServiceRequest()
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<GetPlayerServiceResponse>();

            if((response.ServiceResponse.ErrorCode != ServiceErrorCode.NONE) || (response.StateSpecified == false))
            {
                throw new InvalidMessageException("Foundation failed to return the Player Service Request state value.");
            }
            return response.State;
        }

        /// <inheritdoc />
        public bool RegisterPlayerServiceRequestStateEvent()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new RegisterForPlayerServiceRequestChangedEventsRequest()
                                             {
                                                 RegistrationAction = RegistrationAction1.Register
                                             }
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForPlayerServiceRequestChangedEventsResponse>();

            return response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE ||
                   response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_ALREADY_REGISTERED;
        }

        /// <inheritdoc />
        public bool UnregisterPlayerServiceRequestStateEvent()
        {
            if(currentSchemaVersion < MinimumSchemaVersionRequiredForPlayerServiceStates)
            {
                // Return default value if the current target does not support this API.
                return false;
            }

            var request = new CsiService
                              {
                                  Item = new RegisterForPlayerServiceRequestChangedEventsRequest()
                                             {
                                                 RegistrationAction = RegistrationAction1.Unregister
                                             }
                              };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterForPlayerServiceRequestChangedEventsResponse>();

            return response.ServiceResponse.ErrorCode == ServiceErrorCode.NONE ||
                   response.ServiceResponse.ErrorCode == ServiceErrorCode.CLIENT_NOT_REGISTERED;
        }

        #endregion IService Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiService>();

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
    }
}
