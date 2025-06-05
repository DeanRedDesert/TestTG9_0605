//-----------------------------------------------------------------------
// <copyright file = "UgpRunTimeGameEventCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RunTimeGameEvent
{
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpRunTimeGameEvent category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpRunTimeGameEventCategory : F2XTransactionalCategoryBase<UgpRunTimeGameEventCategoryInternal>,
                                                 IUgpRunTimeGameEventCategory
    {
        #region Constructors

        /// /// <summary>
        /// Instantiates a new <see cref="UgpRunTimeGameEventCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public UgpRunTimeGameEventCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpRunTimeGameEvent;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpRunTimeGameEventCategory Implementation

        /// <inheritdoc/>
        public void WaitingForTakeWin(bool value)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryWaitingForInputSend>();

            var messageItem = (UgpGameEventCategoryWaitingForInputSend)request.Message.Item;
            messageItem.InputType = UgpRunTimeGameEventCategoryInternal.GameInputType.TakeWin;
            messageItem.IsWaiting = value;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryWaitingForInputReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void WaitingForStartFeature(bool value)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryWaitingForInputSend>();

            var messageItem = (UgpGameEventCategoryWaitingForInputSend)request.Message.Item;
            messageItem.InputType = UgpRunTimeGameEventCategoryInternal.GameInputType.StartFeature;
            messageItem.IsWaiting = value;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryWaitingForInputReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void WaitingForPlayerSelection(bool value)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryWaitingForInputSend>();

            var messageItem = (UgpGameEventCategoryWaitingForInputSend)request.Message.Item;
            messageItem.InputType = UgpRunTimeGameEventCategoryInternal.GameInputType.PlayerSelection;
            messageItem.IsWaiting = value;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryWaitingForInputReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void WaitingForGenericInput(bool value)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryWaitingForInputSend>();

            var messageItem = (UgpGameEventCategoryWaitingForInputSend)request.Message.Item;
            messageItem.InputType = UgpRunTimeGameEventCategoryInternal.GameInputType.Generic;
            messageItem.IsWaiting = value;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryWaitingForInputReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void PlayerChoice(uint playerChoiceIndex)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryPlayerChoiceSend>();

            var messageItem = (UgpGameEventCategoryPlayerChoiceSend)request.Message.Item;
            messageItem.PlayerChoiceIndex = playerChoiceIndex;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryPlayerChoiceReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void DenomSelectionActive(bool active)
        {
            var request = CreateBasicRequest<UgpGameEventCategoryDenomSelectionActiveSend>();

            var messageItem = (UgpGameEventCategoryDenomSelectionActiveSend)request.Message.Item;
            messageItem.Active = active;

            var reply = SendMessageAndGetReply<UgpGameEventCategoryDenomSelectionActiveReply>(Channel.Game, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        #endregion
    }
}
