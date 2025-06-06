//-----------------------------------------------------------------------
// <copyright file = "GameCycleEventsCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2XTransport;
    using Schemas.Internal.GameCycleEvents;

    /// <summary>
    /// Implementation of the F2X <see cref="GameCycleEvents"/> category.
    /// Game Cycle Events category of messages.  Category: 3017  Version: 1
    /// Category: 3017; Major Version: 1
    /// </summary>
    public class GameCycleEventsCategory : F2XTransactionalCategoryBase<GameCycleEvents>, IGameCycleEventsCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the GameCycleEventsCategory callbacks.
        /// </summary>
        private readonly IGameCycleEventsCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="GameCycleEventsCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// GameCycleEventsCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public GameCycleEventsCategory(IF2XTransport transport, IGameCycleEventsCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<GameCycleEventSend>(HandleGameCycleEvent);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.GameCycleEvents;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IGameCycleEventsCategory Members

        /// <inheritdoc/>
        public void SetGameCycleEventRegistration(IEnumerable<GameCycleEventType> events)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<SetGameCycleEventRegistrationSend>();
            var content = (SetGameCycleEventRegistrationSend)request.Message.Item;
            content.Events = events.ToList();

            var reply = SendMessageAndGetReply<SetGameCycleEventRegistrationReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the GameCycleEventSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleGameCycleEvent(GameCycleEventSend message)
        {
            var errorMessage = callbackHandler.ProcessGameCycleEvent(message.GameCycleEvent);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<GameCycleEventReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

