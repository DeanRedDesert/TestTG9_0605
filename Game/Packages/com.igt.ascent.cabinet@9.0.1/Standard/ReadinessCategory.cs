//-----------------------------------------------------------------------
// <copyright file = "ReadinessCategory.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Class for handling the CSI readiness category.
    /// </summary>
    internal class ReadinessCategory : CategoryBase<CsiReadiness>, IReadiness, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly List<CsiReadiness> pendingEvents = new List<CsiReadiness>();

        #endregion Private Fields

        #region Constructor

        /// <summary>
        /// Construct an instance of the readiness category.
        /// </summary>
        public ReadinessCategory()
        {
            eventHandlers[typeof(ReadyStateNotificationEvent)] =
                message => HandleReadyStateNotification(message as ReadyStateNotificationEvent);
        }

        #endregion Constructor

        #region CategoryBase<CsiReadiness> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiReadiness;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 0;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            var readinessMessage = message as CsiReadiness;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(readinessMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiReadiness), message.GetType()));
            }
            if(readinessMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Readiness message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(readinessMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Add(readinessMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + readinessMessage.Item.GetType());
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var readinessMessage = message as CsiReadiness;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(readinessMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiReadiness), message.GetType()));
            }
            if(readinessMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Readiness message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + readinessMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiReadiness> Implementation

        #region IReadiness Implementation

        /// <inheritdoc/>
        public event EventHandler<ReadyStateChangedEventArgs> ReadyStateChangedEvent;

        /// <inheritdoc/>
        public ICollection<ReadyStateStatus> GetReadyState(Priority clientPriority)
        {
            var request = new CsiReadiness
            {
                Item = new GetReadyStateRequest
                {
                    ClientPriority = clientPriority
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReadyResponse<GetReadyStateResponse>();
            CheckResponse(response.ReadinessResponse);

            return
                response.ReadyStateResponse.Select(
                    responseStatus =>
                        new ReadyStateStatus(responseStatus.ClientPriority, responseStatus.ClientIdentifier,
                            responseStatus.ReadyState)).ToList();
        }

        /// <inheritdoc/>
        public ReadyStateStatus GetReadyState(Priority clientPriority, string clientIdentifier)
        {
            if(clientIdentifier == null)
            {
                throw new ArgumentNullException(nameof(clientIdentifier), "The client identifier may not be null.");
            }

            var request = new CsiReadiness
            {
                Item = new GetReadyStateRequest
                {
                    ClientIdentifier = clientIdentifier,
                    ClientPriority = clientPriority
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReadyResponse<GetReadyStateResponse>();
            CheckResponse(response.ReadinessResponse);

            return
                response.ReadyStateResponse.Select(
                    responseStatus =>
                        new ReadyStateStatus(responseStatus.ClientPriority, responseStatus.ClientIdentifier,
                            responseStatus.ReadyState)).FirstOrDefault();
        }

        /// <inheritdoc/>
        public void SetReadyState(ReadyState newReadyState)
        {
            var request = new CsiReadiness
            {
                Item = new SetReadyStateRequest
                {
                    ReadyState = newReadyState
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReadyResponse<SetReadyStateResponse>();
            CheckResponse(response.ReadinessResponse);
        }

        /// <inheritdoc/>
        public void SubscribeToReadyNotifications(Priority clientPriority, bool subscribe)
        {
            var subscriptionStatus = subscribe ? SubscriptionStatus.Subscribe : SubscriptionStatus.Unsubscribe;

            var request = new CsiReadiness
            {
                Item = new ReadyStateSubscriptionRequest
                {
                    ClientPriority = clientPriority,
                    Subscription = subscriptionStatus
                }
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetReadyResponse<ReadyStateSubscriptionResponse>();
            CheckResponse(response.ReadinessResponse);
        }

        #endregion IReadiness Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiReadiness>();
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
        private TResponse GetReadyResponse<TResponse>() where TResponse : class
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
        /// <exception cref="ReadinessCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(ReadinessResponse response)
        {
            if(response.ErrorCode != ReadinessErrorCode.NONE)
            {
                throw new ReadinessCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        /// <summary>
        /// Handle notifications of ready state changes.
        /// </summary>
        /// <param name="message">Message containing information about the ready state change.</param>
        private void HandleReadyStateNotification(ReadyStateNotificationEvent message)
        {
            ReadyStateChangedEvent?.Invoke(this,
                    new ReadyStateChangedEventArgs(message.ReadyStateResponse.ClientPriority,
                        message.ReadyStateResponse.ClientIdentifier, message.ReadyStateResponse.ReadyState));
        }

        #endregion Private Methods
    }
}
