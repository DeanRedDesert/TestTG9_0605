//-----------------------------------------------------------------------
// <copyright file = "WapSignControlCategory.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using F2XTransport;
    using Schemas.Internal;

    /// <summary>
    /// Implementation of the F2L <see cref="WapSignControl"/> category.
    /// WapSignControl category of messages. These messages are to allow WAP signs to receive WAP level updates and
    /// player awareness information from a WAP server.
    /// Category: 30; Major Version: 1
    /// </summary>
    public class WapSignControlCategory : F2LCategoryBase<WapSignControl>, IWapSignControlCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the WapSignControlCategory callbacks.
        /// </summary>
        private readonly IWapSignControlCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="WapSignControlCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// WapSignControlCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public WapSignControlCategory(IF2XTransport transport, IWapSignControlCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<WapSignControlTransportWAPHostDataToBinSend>(HandleTransportWAPHostDataToBin);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.WapSignControl;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IWapSignControlCategory Members

        /// <inheritdoc/>
        public WapSignControlGetProgressiveLevelValuesReply GetProgressiveLevelValues()
        {
            var request = CreateBasicRequest<WapSignControlGetProgressiveLevelValuesSend>();

            var reply = SendMessageAndGetReply<WapSignControlGetProgressiveLevelValuesReply>(Channel.Game, request);
            CheckReply(reply.Reply);
            return reply;
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the WapSignControlTransportWAPHostDataToBinSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleTransportWAPHostDataToBin(WapSignControlTransportWAPHostDataToBinSend message)
        {
            var errorMessage = callbackHandler.ProcessTransportWAPHostDataToBin(message.BinaryData);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<WapSignControlTransportWAPHostDataToBinReply>(errorCode, errorMessage);
            SendFoundationNonTransactionalChannelResponse(replyMessage);
        }

        #endregion

    }

}

