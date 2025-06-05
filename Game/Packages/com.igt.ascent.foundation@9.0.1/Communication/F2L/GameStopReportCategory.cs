//-----------------------------------------------------------------------
// <copyright file = "GameStopReportCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System.Collections.Generic;
    using F2XTransport;
    using Schemas.Internal;

    /// <summary>
    /// Implementation of the F2L game stop reporting API category.
    /// </summary>
    public class GameStopReportCategory : F2LTransactionalCategoryBase<GameStopReport>, IGameStopReportCategory
    {
        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the game stop reporting category.
        /// </summary>
        /// <param name="transport">Transport the category handler will be installed in.</param>
        public GameStopReportCategory(IF2XTransport transport) : base(transport)
        {
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion
        {
            get { return 2; }
        }

        /// <inheritdoc />
        public override uint MinorVersion
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.GameStopReport; }
        }

        #endregion

        #region IGameStopReportCategory Implementation

        /// <inheritdoc/>
        public void ReportReelStops(ICollection<uint> physicalReelStops)
        {
            var request = CreateTransactionalRequest<GameStopReportReportReelStopsSend>();
            var content = (GameStopReportReportReelStopsSend)request.Message.Item;
            content.ReelStop.AddRange(physicalReelStops);

            var reply = SendMessageAndGetReply<GameStopReportReportReelStopsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc/>
        public void ReportPokerHand(bool finalHand, ICollection<PokerCard> cards)
        {
            var request = CreateTransactionalRequest<GameStopReportReportPokerHandSend>();
            var content = (GameStopReportReportPokerHandSend)request.Message.Item;
            content.HandStatus = finalHand;
            content.Card.AddRange(cards);
            var reply = SendMessageAndGetReply<GameStopReportReportPokerHandReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        #endregion
    }
}
