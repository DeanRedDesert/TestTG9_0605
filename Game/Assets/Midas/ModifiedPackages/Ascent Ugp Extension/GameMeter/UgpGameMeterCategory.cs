//-----------------------------------------------------------------------
// <copyright file = "UgpGameMeterCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.GameMeter
{
    using F2X;
    using F2XTransport;
    using F2X.Schemas.Internal.Types;

    /// <summary>
    /// Implementation of the F2X UgpGameMeter category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpGameMeterCategory : F2XTransactionalCategoryBase<UgpGameMeterCategoryInternal>,
                                          IUgpGameMeterCategory
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UgpGameMeterCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public UgpGameMeterCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpGameMeter;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpGameMeterCategory Members

        /// <inheritdoc/>
        public void SendCurrentBetPerLineAndSelectedLines(long betPerLine, int selectedLines)
        {
            var request = CreateBasicRequest<UgpGameMeterCategoryAspGameMeterSend>();
            var messageItem = (UgpGameMeterCategoryAspGameMeterSend)request.Message.Item;

            messageItem.CurrentBetPerLine = betPerLine;
            messageItem.CurrentlySelectedLines = selectedLines;

            var reply = SendMessageAndGetReply<UgpGameMeterCategoryAspGameMeterReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void SendUpdateGameBetMeterOnBet(string horizontalKey, string verticalKey)
        {
            var request = CreateBasicRequest<UgpGameMeterCategoryGameBetMeterUpdateSend>();
            var messageItem = (UgpGameMeterCategoryGameBetMeterUpdateSend)request.Message.Item;

            messageItem.HorizontalKey = horizontalKey;
            messageItem.VerticalKey = verticalKey;

            var reply = SendMessageAndGetReply<UgpGameMeterCategoryGameBetMeterUpdateReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void SendUpdateGameCreditDenom(long creditDenom)
        {
            var request = CreateBasicRequest<UgpGameMeterCategoryGameCreditDenomSend>();
            var messageItem = (UgpGameMeterCategoryGameCreditDenomSend)request.Message.Item;

            messageItem.GameCreditDenom = creditDenom;

            var reply = SendMessageAndGetReply<UgpGameMeterCategoryGameCreditDenomReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        #endregion
    }
}
