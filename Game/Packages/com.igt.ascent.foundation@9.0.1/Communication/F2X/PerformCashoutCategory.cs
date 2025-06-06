//-----------------------------------------------------------------------
// <copyright file = "PerformCashoutCategory.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
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
    using Schemas.Internal.PerformCashout;

    /// <summary>
    /// Implementation of the F2X <see cref="PerformCashout"/> category.
    /// The PerformCashout category of messages allows the client to take Bank's request to perform cashout (e.g.print a
    /// ticket using a host-generated validation number)
    /// Category: 158; Major Version: 1
    /// </summary>
    public class PerformCashoutCategory : F2XTransactionalCategoryBase<PerformCashout>, IPerformCashoutCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the PerformCashoutCategory callbacks.
        /// </summary>
        private readonly IPerformCashoutCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="PerformCashoutCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// PerformCashoutCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public PerformCashoutCategory(IF2XTransport transport, IPerformCashoutCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<PerformCashoutRequestSend>(HandleRequest);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.PerformCashout;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IPerformCashoutCategory Members

        /// <inheritdoc/>
        public void Response(bool success, string failureReason)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<PerformCashoutResponseSend>();
            var content = (PerformCashoutResponseSend)request.Message.Item;
            content.Success = success;
            content.FailureReason = failureReason;

            var reply = SendMessageAndGetReply<PerformCashoutResponseReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the PerformCashoutRequestSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleRequest(PerformCashoutRequestSend message)
        {
            bool callbackResult;
            var errorMessage = callbackHandler.ProcessRequest(message.Amount, out callbackResult);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<PerformCashoutRequestReply>(errorCode, errorMessage);
            var reply = (PerformCashoutRequestReply)replyMessage.Message.Item;
            reply.Content.Accepted = callbackResult;
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

