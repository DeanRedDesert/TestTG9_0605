//-----------------------------------------------------------------------
// <copyright file = "EmulatedCashoutServiceCategory.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
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
    /// Category for managing emulated cashout/service windows.
    /// </summary>
    internal class EmulatedCashoutServiceCategory : CategoryBase<CsiEmulatedCashoutService>, IEmulatedCashoutService, ICabinetUpdate
    {
        #region Private Fields

        /// <summary>
        /// List of pending events.
        /// </summary>
        private readonly List<CsiEmulatedCashoutService> pendingEvents = new List<CsiEmulatedCashoutService>();

        #endregion Private Fields

        #region Events

        /// <inheritdoc/>
        public event EventHandler<EventArgs> ShowEvent;

        /// <inheritdoc/>
        public event EventHandler<EventArgs> HideEvent;

        /// <inheritdoc/>
        public event EventHandler<CultureChangedEventArgs> CultureChangedEvent;

        #endregion Events

        public static EmulatedCashoutServiceCategory CreateInstance()
        {
            return new EmulatedCashoutServiceCategory();
        }

        #region Constructor

        /// <summary>
        /// Construct an instance of the category.
        /// </summary>
        private EmulatedCashoutServiceCategory()
        {
        }

        #endregion Constructor

        #region CategoryBase<CsiEmulatedCashoutService> Implementation

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 1;

        /// <inheritdoc/>
        public override Category Category => Category.CsiEmulatedCashoutService;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (!(message is CsiEmulatedCashoutService emulatedCashoutServiceMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiEmulatedCashoutService), message.GetType()));
            }
            if (emulatedCashoutServiceMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. CsiEmulatedCashoutService message contained no event, request, or response.");
            }

            lock (pendingEvents)
            {
                pendingEvents.Add(emulatedCashoutServiceMessage);
            }
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (!(message is CsiEmulatedCashoutService emulatedCashoutServiceMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiEmulatedCashoutService), message.GetType()));
            }
            if (emulatedCashoutServiceMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Service message contained no event, request, or response.");
            }

            //This category has no events.
            throw new UnhandledEventException("Event not handled: " + emulatedCashoutServiceMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiEmulatedCashoutService> Implementation

        #region Internal Methods

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

            if (!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion Internal Methods

        #region IEmulatedCashoutService Implementation

        /// <inheritdoc/>
        public bool RegisterAsEmulatedCashoutServiceWindow()
        {
            var request = new CsiEmulatedCashoutService
            {
                Item = new RegisterAsEmulatedCashoutServiceWindowRequest { }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<RegisterAsEmulatedCashoutServiceWindowResponse>();

            return (response.EmulatedCashoutServiceResponse.ErrorCode == EmulatedCashoutServiceErrorCode.NONE);
        }

        /// <inheritdoc/>
        public bool SetEmulatedCashoutServiceVisible(bool visible)
        {
            var request = new CsiEmulatedCashoutService
            {
                Item = new SetEmulatedCashoutServiceVisibleRequest { Visible = visible }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetResponse<SetEmulatedCashoutServiceVisibleResponse>();

            return (response.EmulatedCashoutServiceResponse.ErrorCode == EmulatedCashoutServiceErrorCode.NONE);
        }

        #endregion IEmulatedCashoutService Implementation

        #region ICabinetUpdate Implementation

        /// <inheritdoc/>
        public void Update()
        {
            var tempEvents = new List<CsiEmulatedCashoutService>();

            lock(pendingEvents)
            {
                tempEvents.AddRange(pendingEvents);
                pendingEvents.Clear();
            }
            foreach(var pendingEvent in tempEvents)
            {
                var eventType = pendingEvent.Item.GetType();
                if(eventType == typeof(ShowEmulatedCashoutServiceEvent))
                {
                    ShowEvent?.Invoke(this, EventArgs.Empty);
                }
                else if(eventType == typeof(HideEmulatedCashoutServiceEvent))
                {
                    HideEvent?.Invoke(this, EventArgs.Empty);
                }
                else if(eventType == typeof(CultureChangedEvent))
                {
                    var cultureChangedEvent = pendingEvent.Item as CultureChangedEvent;
                    CultureChangedEvent?.Invoke(this, new CultureChangedEventArgs(cultureChangedEvent.Culture));
                }
            }
        }

        #endregion ICabinetUpdate Implementation
    }
}
