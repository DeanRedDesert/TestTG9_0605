//-----------------------------------------------------------------------
// <copyright file = "TsmActivationCategory.cs" company = "IGT">
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
    using Schemas.Internal.TsmActivation;

    /// <summary>
    /// Implementation of the F2X <see cref="TsmActivation"/> category.
    /// (F2E Only) TSM Activation category of messages.  Category: 3004  Version: 1
    /// </summary>
    public class TsmActivationCategory : F2XCategoryBase<TsmActivation>
    {
        #region Fields

        /// <summary>
        /// Object which implements the TsmActivationCategory callbacks.
        /// </summary>
        private readonly ITsmActivationCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="TsmActivationCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// TsmActivationCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public TsmActivationCategory(IF2XTransport transport, ITsmActivationCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<ActivateTsmContextSend>(HandleActivateTsmContext);
            AddMessagehandler<InactivateTsmContextSend>(HandleInactivateTsmContext);
            AddMessagehandler<NewTsmContextSend>(HandleNewTsmContext);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.TsmActivation;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the ActivateTsmContextSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleActivateTsmContext(ActivateTsmContextSend message)
        {
            var errorMessage = callbackHandler.ProcessActivateTsmContext();
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<ActivateTsmContextReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the InactivateTsmContextSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleInactivateTsmContext(InactivateTsmContextSend message)
        {
            var errorMessage = callbackHandler.ProcessInactivateTsmContext();
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<InactivateTsmContextReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the NewTsmContextSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleNewTsmContext(NewTsmContextSend message)
        {
            var errorMessage = callbackHandler.ProcessNewTsmContext();
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<NewTsmContextReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

