//-----------------------------------------------------------------------
// <copyright file = "UgpRandomNumberCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber
{
    using System.Collections.Generic;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpRandomNumber category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpRandomNumberCategory : F2XTransactionalCategoryBase<UgpRandomNumberCategoryInternal>,
                                             IUgpRandomNumberCategory
    {
        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UgpRandomNumberCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        public UgpRandomNumberCategory(IF2XTransport transport)
            : base(transport)
        {
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpRandomNumber;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpRandomNumberCategory Implementation

        /// <inheritdoc/>
        public IList<double> GetRandomNumbers(uint numberOfRandomNumbers)
        {
            var request = CreateBasicRequest<UgpRandomNumbersSend>();
            var messageItem = (UgpRandomNumbersSend)request.Message.Item;
            messageItem.RequiredNumberOfRandomNumbers = numberOfRandomNumbers;

            var reply = SendMessageAndGetReply<UgpRandomNumbersReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });

            return reply.RandomNumbers;
        }

        #endregion
    }
}
