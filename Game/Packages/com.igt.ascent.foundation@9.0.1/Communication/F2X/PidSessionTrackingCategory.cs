//-----------------------------------------------------------------------
// <copyright file = "PidSessionTrackingCategory.cs" company = "IGT">
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
    using F2XTransport;
    using Schemas.Internal.PIDSessionTracking;

    /// <summary>
    /// Implementation of the F2X <see cref="PIDSessionTracking"/> category.
    /// PIDSessionTracking category. Defines messages to handle the PID (Player Information Display) session tracking
    /// information which is displayed by games on the screen.
    /// Category: 1039; Major Version: 1
    /// </summary>
    public class PidSessionTrackingCategory : F2XTransactionalCategoryBase<PIDSessionTracking>, IPidSessionTrackingCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the PidSessionTrackingCategory callbacks.
        /// </summary>
        private readonly IPidSessionTrackingCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="PidSessionTrackingCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// PidSessionTrackingCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public PidSessionTrackingCategory(IF2XTransport transport, IPidSessionTrackingCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<SessionTrackingStatusChangedSend>(HandleSessionTrackingStatusChanged);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.PidSessionTracking;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IPidSessionTrackingCategory Members

        /// <inheritdoc/>
        public SessionData GetSessionData()
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<GetSessionDataSend>();

            var reply = SendMessageAndGetReply<GetSessionDataReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.SessionData;
        }

        /// <inheritdoc/>
        public void StartTracking()
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<StartTrackingSend>();

            var reply = SendMessageAndGetReply<StartTrackingReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        /// <inheritdoc/>
        public void StopTracking()
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<StopTrackingSend>();

            var reply = SendMessageAndGetReply<StopTrackingReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the SessionTrackingStatusChangedSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleSessionTrackingStatusChanged(SessionTrackingStatusChangedSend message)
        {
            var errorMessage = callbackHandler.ProcessSessionTrackingStatusChanged(message.IsSessionTrackingActive);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<SessionTrackingStatusChangedReply>(errorCode, errorMessage);
            SendFoundationNonTransactionalChannelResponse(replyMessage);
        }

        #endregion

    }

}

