//-----------------------------------------------------------------------
// <copyright file = "BankSynchronizationCategory.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Category for bank synchronization.
    /// </summary>
    internal sealed class BankSynchronizationCategory : CategoryBase<CsiBankSynchronization>, IBankSynchronization, ICabinetUpdate
    {
        /// <summary>
        /// Construct an instance of the bank synchronization category.
        /// </summary>
        // ReSharper disable once UnusedParameter.Local
        public BankSynchronizationCategory(FoundationTarget currentFoundationTarget)
        {
            eventHandlers[typeof(ReceivedSynchronizationPayload)] =
                message => HandleGameEventMessage(message as ReceivedSynchronizationPayload);
        }

        /// <summary>
        /// List of event handlers for this category.
        /// </summary>
        private readonly Dictionary<Type, Action<object>> eventHandlers = new Dictionary<Type, Action<object>>();

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly Queue<CsiBankSynchronization> pendingEvents = new Queue<CsiBankSynchronization>();

        #region CategoryBase<CsiBankSynchronization> Members

        /// <inheritdoc />
        public override Category Category => Category.CsiBankSynchronization;

        /// <inheritdoc />
        public override ushort VersionMajor => 1;

        /// <inheritdoc />
        public override ushort VersionMinor => 2;

        /// <inheritdoc />
        public override void HandleEvent(object message)
        {
            var syncMessage = message as CsiBankSynchronization;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(syncMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiBankSynchronization), message.GetType()));
            }

            if(syncMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Bank synchronization message contained no event, request or response.");
            }

            if(eventHandlers.ContainsKey(syncMessage.Item.GetType()))
            {
                lock(pendingEvents)
                {
                    pendingEvents.Enqueue(syncMessage);
                }
            }
            else
            {
                throw new UnhandledEventException("Event not handled: " + syncMessage.Item.GetType());
            }
        }

        /// <inheritdoc />
        public override void HandleRequest(object message, ulong requestId)
        {
            var syncMessage = message as CsiBankSynchronization;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(syncMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiBankSynchronization), message.GetType()));
            }

            if(syncMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Bank synchronization message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + syncMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiBankSynchronization> Members

        #region IBankSynchronization Members

        /// <inheritdoc />
        public BankSynchronizationInformation GetSynchronizationStatus()
        {
            var request = new CsiBankSynchronization
            {
                Item = new BankSynchronizationInfoRequest()
            };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetBankSyncResponse<BankSynchronizationInfoResponse>();
            // Right now there is just disabled and none as the error codes.
            var enabled = response.SynchronizationInfoError.ErrorCode != BankSynchronizationInfoErrorCode.FeatureDisabled;
            var syncInfo = response.SynchronizationInfo;

            return new BankSynchronizationInformation(enabled, syncInfo.TimeFrame,
                syncInfo.TimeFramePrecision, syncInfo.PositionInBank, syncInfo.TotalBankPositions);
        }

        /// <inheritdoc />
        public bool GameEventsEnabled { get; private set; }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">Thrown if the game is targeting the wrong foundation version.</exception>
        public bool RegisterForGameEvents()
        {
            var wasEnabled = false;

            if(!GameEventsEnabled)
            {
                var request = new CsiBankSynchronization
                {
                    Item = new MessageRegistrationRequest1
                    {
                        Action = MessageRegistrationAction.REGISTER,
                        MessageType = MessageRegistrationType.GAME
                    }
                };

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                var response = GetBankSyncResponse<MessageRegistrationResponse>();
                wasEnabled = response.ErrorCode == MessageRegistrationErrorCode.None;
                GameEventsEnabled = wasEnabled;
            }

            return wasEnabled;
        }

        /// <inheritdoc />
        public void UnregisterForGameEvents()
        {
            if(GameEventsEnabled)
            {
                var request = new CsiBankSynchronization
                {
                    Item = new MessageRegistrationRequest1
                    {
                        Action = MessageRegistrationAction.UNREGISTER,
                        MessageType = MessageRegistrationType.GAME
                    }
                };

                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
                GetBankSyncResponse<MessageRegistrationResponse>();

                GameEventsEnabled = false;
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="messagePayload"/> is null.</exception>
        public void SendGameEvent(string messagePayload)
        {
            if(messagePayload == null)
            {
                throw new ArgumentNullException(nameof(messagePayload));
            }

            if(GameEventsEnabled)
            {
                var payloadSize = (ulong)System.Text.Encoding.UTF8.GetByteCount(messagePayload);
                var request = new CsiBankSynchronization
                {
                    Item = new SendSynchronizationPayload
                    {
                        Destination = MessageRegistrationType.GAME,
                        SendPayload = new SynchronizationPayloadData
                        {
                            PayloadData = messagePayload,
                            PayloadSize = payloadSize
                        }
                    }
                };

                // The foundation does not send a response so there is no reason to listen for one.
                Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            }
        }

        /// <inheritdoc />
        public void CleanUpGameEvents()
        {
            GameEventsEnabled = false;
        }

        /// <inheritdoc />
        public event EventHandler<GameMessageReceivedEventArgs> GameMessageReceivedEvent;

        #endregion IBankSynchronization Members

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiBankSynchronization>();
            lock(pendingEvents)
            {
                tempEvents.AddRange(pendingEvents);
                pendingEvents.Clear();
            }
            foreach(var pendingEvent in tempEvents)
            {
                // Events should only be placed in the queue if they are handled.
                eventHandlers[pendingEvent.Item.GetType()](pendingEvent.Item);
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
        private TResponse GetBankSyncResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Handle notifications of game event messages.
        /// </summary>
        /// <param name="message">
        /// The game message payload data. If any data in the payload is null, the message will be discarded.
        /// </param>
        private void HandleGameEventMessage(ReceivedSynchronizationPayload message)
        {
            if(GameEventsEnabled && message?.ReceivedPayload?.PayloadData != null)
            {
                GameMessageReceivedEvent?.Invoke(this,
                        new GameMessageReceivedEventArgs(message.ReceivedPayload.PayloadData));
            }
        }
    }
}
