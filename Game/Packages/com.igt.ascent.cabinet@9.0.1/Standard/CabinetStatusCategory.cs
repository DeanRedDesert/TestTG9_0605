//-----------------------------------------------------------------------
// <copyright file = "CabinetStatusCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using CsiTransport;
    using Foundation.Transport;

    /// <summary>
    /// Category which manages the cabinet status.
    /// </summary>
    internal class CabinetStatusCategory : CategoryBase<CsiCabinet>
    {
        #region Private Fields

        /// <summary>
        /// Stores the current foundation target and is used to return the correct version information.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        #endregion Private Fields

        #region Events

        /// <summary>
        /// Event which is fired when the activity status changes.
        /// </summary>
        public event EventHandler<ActivityStatusEventArgs> ActivityStatusEvent;

        /// <summary>
        /// Event which is fired when the attract aesthetic configuration changes.
        /// </summary>
        public event EventHandler<AttractAestheticConfigurationEventArgs> AttractAestheticChangedEvent;

        /// <summary>
        /// Optional event for non-game CSI clients that allows for the monitoring of cabinet-specific events,
        /// such as device connections/disconnections.
        /// </summary>
        public event EventHandler<CabinetEventArgs> CabinetEvent;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        /// <param name="target">
        /// The target foundation the game is running against.
        /// It is currently not used, but maybe needed in the future.
        /// </param>
        public CabinetStatusCategory(FoundationTarget target)
        {
            // Cache the Foundation target as it is needed to return the correct category version.
            foundationTarget = target;

            eventHandlers[typeof(ActivityStatusEvent)] =
                activity => HandleActivityStatusEvent(activity as ActivityStatusEvent);
            eventHandlers[typeof(AttractAestheticConfiguration)] =
                data => HandleAttractAestheticChangedEvent(data as AttractAestheticConfiguration);
            eventHandlers[typeof(CabinetEvent)] =
                data => HandleCabinetEvent(data as CabinetEvent);
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Handler for the activity status event.
        /// </summary>
        /// <param name="activityStatusEvent">Activity status event to handle.</param>
        private void HandleActivityStatusEvent(ActivityStatusEvent activityStatusEvent)
        {
            ActivityStatusEvent?.Invoke(this, new ActivityStatusEventArgs(new MachineActivityStatus
                (
                    activityStatusEvent.Active,
                    activityStatusEvent.AttractInterval,
                    activityStatusEvent.InactivityDelay,
                    activityStatusEvent.GameAttractsEnabled,
                    activityStatusEvent.NewGame
                )));
        }

        private void HandleAttractAestheticChangedEvent(AttractAestheticConfiguration newConfiguration)
        {
            AttractAestheticChangedEvent?.Invoke(this, new AttractAestheticConfigurationEventArgs(
                    newConfiguration.GameAttractPlaylistGroup, newConfiguration.GameAttractStyle));
        }

        /// <summary>
        /// Handler for cabinet events.
        /// </summary>
        /// <param name="eventData">
        /// Data specific to the cabinet event.
        /// </param>
        private void HandleCabinetEvent(CabinetEvent eventData)
        {
            CabinetEvent?.Invoke(this, new CabinetEventArgs(eventData.Event, eventData.Data.Value));
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetCabinetResponse<TResponse>() where TResponse : class
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
        /// <exception cref="CabinetStatusCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(CabinetResponse response)
        {
            if(response.ErrorCode != CabinetErrorCode.NONE)
            {
                throw new CabinetStatusCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods

        #region Overrides of CategoryBase<CsiCabinet>

        /// <inheritdoc/>
        public override Category Category => Category.CsiCabinet;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => foundationTarget.IsEqualOrNewer(FoundationTarget.AscentQ1Series) ? (ushort)4 : (ushort)3;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(!(message is CsiCabinet cabinetStatusMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiCabinet), message.GetType()));
            }
            if(cabinetStatusMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Cabinet status message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(cabinetStatusMessage.Item.GetType()))
            {
                eventHandlers[cabinetStatusMessage.Item.GetType()](cabinetStatusMessage.Item);
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + cabinetStatusMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(!(message is CsiCabinet cabinetStatusMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiCabinet), message.GetType()));
            }
            if(cabinetStatusMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Cabinet status message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + cabinetStatusMessage.Item.GetType());
        }

        #endregion Overrides of CategoryBase<CsiCabinet>

        #region Cabinet Methods

        /// <summary>
        /// Request the current activity status.
        /// </summary>
        /// <returns>The current activity status.</returns>
        public ActivityStatusResponseData RequestActivityStatus()
        {
            var request = new CsiCabinet
            {
                Item = new ActivityStatusRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetCabinetResponse<ActivityStatusResponse>();

            return response.Data;
        }

        /// <summary>
        /// Request the current attract aesthetic configuration.
        /// </summary>
        /// <returns>The current attract configuration.</returns>
        public AttractAestheticConfiguration RequestAttractAestheticConfiguration()
        {
            var request = new CsiCabinet
            {
                Item = new AttractAestheticConfigurationRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetCabinetResponse<AttractAestheticConfigurationResponse>();

            return response.Data;
        }

        /// <summary>
        /// Requests that the current CSI client register or unregister for cabinet-specific events,
        /// such as device connects/disconnects.
        /// </summary>
        /// <param name="registerForEvents">
        /// True to register for events, false to unregister for events.
        /// </param>
        /// <returns>
        /// True if the client successfully registered/unregistered for cabinet events.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// RequestCabinetEventRegistration requires targeting CsiCabinetV1 schema version 1.4 or newer.
        /// </exception>
        /// <remarks>
        /// Only non-game CSI clients are allowed to register for cabinet events.
        /// </remarks>
        public bool RequestCabinetEventRegistration(bool registerForEvents)
        {
            var currentSchemaVersion = new Version(VersionMajor, VersionMinor);
            var minimumSchemaVersionRequiredForRequestCabinetEventRegistration = new Version(1, 4);
            if(currentSchemaVersion < minimumSchemaVersionRequiredForRequestCabinetEventRegistration)
            {
                throw new NotSupportedException("RequestCabinetEventRegistration requires targeting CsiCabinetV1 schema version 1.4 or newer.");
            }

            var request = new CsiCabinet
            {
                Item = new CabinetEventRegistrationRequest
                {
                    CabinetEventRegistrationAction = registerForEvents ? CabinetEventRegistrationAction.Register : CabinetEventRegistrationAction.Unregister
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetCabinetResponse<CabinetEventRegistrationResponse>();

            return response.CabinetResponse.ErrorCode == CabinetErrorCode.NONE;
        }

        /// <summary>
        /// Gets the credit display type.
        /// </summary>
        /// <returns>The credit display type.</returns>
        public CabinetCreditDisplayType GetCreditDisplayType()
        {
            var request = new CsiCabinet
            {
                Item = new GetCreditDisplayTypeRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetCabinetResponse<GetCreditDisplayTypeResponse>();

            var displayType = response.CreditDisplayType;
            switch(displayType)
            {
                case CreditDisplayType.CREDIT:
                    return CabinetCreditDisplayType.Credit;

                case CreditDisplayType.CURRENCY:
                    return CabinetCreditDisplayType.Currency;

                default:
                    return CabinetCreditDisplayType.NotSet;
            }
        }

        /// <summary>
        /// Sets the credit display type.
        /// </summary>
        /// <param name="creditDisplayType">The credit display type to set.</param>
        public void SetCreditDisplayType(CabinetCreditDisplayType creditDisplayType)
        {
            CreditDisplayType displayType;
            switch(creditDisplayType)
            {
                case CabinetCreditDisplayType.Credit:
                    displayType = CreditDisplayType.CREDIT;
                    break;

                case CabinetCreditDisplayType.Currency:
                    displayType = CreditDisplayType.CURRENCY;
                    break;

                case CabinetCreditDisplayType.NotSet:
                    displayType = CreditDisplayType.NOT_SET;
                    break;

                default:
                    return;
            }

            var request = new CsiCabinet
            {
                Item = new SetCreditDisplayTypeRequest { CreditDisplayType = displayType }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetCabinetResponse<SetCreditDisplayTypeResponse>();
            CheckResponse(response.CabinetResponse);
        }

        #endregion Cabinet Methods
    }
}
