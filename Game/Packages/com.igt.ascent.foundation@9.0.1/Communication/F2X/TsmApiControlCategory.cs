//-----------------------------------------------------------------------
// <copyright file = "TsmApiControlCategory.cs" company = "IGT">
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
    using Schemas.Internal.TsmApiControl;

    /// <summary>
    /// Implementation of the F2X <see cref="TsmApiControl"/> category.
    /// (F2E Only) TSM API Control category of messages.  Category: 3003  Version: 1
    /// </summary>
    public class TsmApiControlCategory : F2XCategoryBase<TsmApiControl>
    {
        #region Fields

        /// <summary>
        /// Object which implements the TsmApiControlCategory callbacks.
        /// </summary>
        private readonly ITsmApiControlCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TsmApiControlCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// TsmApiControlCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public TsmApiControlCategory(IF2XTransport transport, ITsmApiControlCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<GetTsmApiVersionsSend>(HandleGetTsmApiVersions);
            AddMessagehandler<SetTsmApiVersionsSend>(HandleSetTsmApiVersions);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.TsmApiControl;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the GetTsmApiVersionsSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleGetTsmApiVersions(GetTsmApiVersionsSend message)
        {
            GetTsmApiVersionsReplyContentCategoryVersions callbackResult;
            var errorMessage = callbackHandler.ProcessGetTsmApiVersions(message.Tsm, message.Extensions, out callbackResult);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<GetTsmApiVersionsReply>(errorCode, errorMessage);
            var reply = (GetTsmApiVersionsReply)replyMessage.Message.Item;
            reply.Content.CategoryVersions = callbackResult;
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the SetTsmApiVersionsSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleSetTsmApiVersions(SetTsmApiVersionsSend message)
        {
            bool callbackResult;
            var errorMessage = callbackHandler.ProcessSetTsmApiVersions(message.CategoryVersions, out callbackResult);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<SetTsmApiVersionsReply>(errorCode, errorMessage);
            var reply = (SetTsmApiVersionsReply)replyMessage.Message.Item;
            reply.Content.CategoriesAccepted = callbackResult;
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

