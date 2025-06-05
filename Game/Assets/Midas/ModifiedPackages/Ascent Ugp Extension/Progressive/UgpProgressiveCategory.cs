//-----------------------------------------------------------------------
// <copyright file = "UgpProgressiveCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System.Collections.Generic;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpProgressive category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpProgressiveCategory : F2XTransactionalCategoryBase<UgpProgressiveCategoryInternal>,
                                            IUgpProgressiveCategory
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UgpProgressiveCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public UgpProgressiveCategory(IF2XTransport transport)
           : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpProgressive;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpProgressiveCategory Implementation

        /// <inheritdoc/>
        public ProgressiveLevelInfo GetProgressiveLevel(string progressiveId)
        {
            var request = CreateBasicRequest<ProgressiveLevelInfoSend>();
            var message = (ProgressiveLevelInfoSend)request.Message.Item;
            message.ProgressiveLevelId = progressiveId;

            var reply = SendMessageAndGetReply<ProgressiveLevelInfoReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });

            return reply.Info;
        }

        /// <inheritdoc/>
        public IEnumerable<ProgressiveLevelInfo> GetAllProgressives()
        {
            var request = CreateBasicRequest<ProgressiveListSend>();

            var reply = SendMessageAndGetReply<ProgressiveListReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });

            return reply.ProgressivesList;
        }

        #endregion
    }
}
