//-----------------------------------------------------------------------
// <copyright file = "BonusingCategory.cs" company = "IGT">
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
    using Schemas.Internal.Bonusing;
    using Schemas.Internal.Types;

    /// <summary>
    /// Implementation of the F2X <see cref="Bonusing"/> category.
    /// Bonusing category of messages.  Category: 3014  Version: 1
    /// Category: 3014; Major Version: 1
    /// </summary>
    public class BonusingCategory : F2XTransactionalCategoryBase<Bonusing>, IBonusingCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the BonusingCategory callbacks.
        /// </summary>
        private readonly IBonusingCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="BonusingCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// BonusingCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public BonusingCategory(IF2XTransport transport, IBonusingCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<BonusPaidSend>(HandleBonusPaid);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.Bonusing;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IBonusingCategory Members

        /// <inheritdoc/>
        public bool EscrowBonusWin(string transferId, Amount amount)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<EscrowBonusWinSend>();
            var content = (EscrowBonusWinSend)request.Message.Item;
            content.TransferId = transferId;
            content.Amount = amount;

            var reply = SendMessageAndGetReply<EscrowBonusWinReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Accepted;
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the BonusPaidSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleBonusPaid(BonusPaidSend message)
        {
            var errorMessage = callbackHandler.ProcessBonusPaid(message.TransferId, message.Amount);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<BonusPaidReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

    }

}

