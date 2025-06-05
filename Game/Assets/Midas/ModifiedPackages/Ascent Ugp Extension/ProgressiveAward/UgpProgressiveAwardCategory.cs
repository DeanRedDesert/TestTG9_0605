//-----------------------------------------------------------------------
// <copyright file = "UgpProgressiveAwardCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpProgressiveAward category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpProgressiveAwardCategory : F2XTransactionalCategoryBase<UgpProgressiveAwardCategoryInternal>,
                                                 IUgpProgressiveAwardCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the UgpProgressiveAwardCategory callbacks.
        /// </summary>
        private readonly IUgpProgressiveAwardCategoryCallbacks callbackHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="UgpProgressiveAwardCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// UgpProgressiveAwardCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpProgressiveAwardCategory(IF2XTransport transport,
                                           IUgpProgressiveAwardCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            AddMessagehandler<UgpProgressiveAwardCategoryProgressivePaidSend>(HandleProgressivePaidSend);
            AddMessagehandler<UgpProgressiveAwardCategoryProgressiveVerifiedSend>(HandleProgressiveVerfifedSend);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpProgressiveAwardCategoryProgressiveVerifiedSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleProgressiveVerfifedSend(UgpProgressiveAwardCategoryProgressiveVerifiedSend message)
        {
            callbackHandler.ProcessProgressiveVerified(message.AwardIndex,
                                                       message.ProgressiveLevelId,
                                                       message.VerifiedAmount,
                                                       message.PayType);

            var reply = CreateReply<UgpProgressiveAwardCategoryProgressiveVerifiedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the UgpProgressiveAwardCategoryProgressivePaidSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleProgressivePaidSend(UgpProgressiveAwardCategoryProgressivePaidSend message)
        {
            callbackHandler.ProcessProgressivePaid(message.AwardIndex,
                                                   message.ProgressiveLevelId,
                                                   message.TransferredAmount);

            var reply = CreateReply<UgpProgressiveAwardCategoryProgressivePaidReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpProgressiveAward;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpProgressiveAwardCategory Implementation

        /// <inheritdoc/>
        public void StartingProgressiveAward(int progressiveAwardIndex, string progressiveLevelId, long defaultVerifiedAmount)
        {
            var request = CreateBasicRequest<UgpProgressiveAwardCategoryProgressiveAwardStartingSend>();
            var message = (UgpProgressiveAwardCategoryProgressiveAwardStartingSend)request.Message.Item;
            message.AwardIndex = progressiveAwardIndex;
            message.ProgressiveLevelId = progressiveLevelId;

            var reply = SendMessageAndGetReply<UgpProgressiveAwardCategoryProgressiveAwardStartingReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount)
        {
            var request = CreateBasicRequest<UgpProgressiveAwardCategoryProgressiveFinishedDispaySend>();
            var message = (UgpProgressiveAwardCategoryProgressiveFinishedDispaySend)request.Message.Item;
            message.AwardIndex = progressiveAwardIndex;
            message.ProgressiveLevelId = progressiveLevelId;

            var reply = SendMessageAndGetReply<UgpProgressiveAwardCategoryProgressiveFinishedDispayReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        #endregion
    }
}
