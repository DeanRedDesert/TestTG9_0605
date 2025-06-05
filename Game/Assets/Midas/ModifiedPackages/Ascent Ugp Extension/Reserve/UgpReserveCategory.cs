//-----------------------------------------------------------------------
// <copyright file = "UgpReserveCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpReserve category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpReserveCategory : F2XTransactionalCategoryBase<UgpReserveCategoryInternal>, IUgpReserveCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the UgpReserveCategory callbacks.
        /// </summary>
        private readonly IUgpReserveCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UgpReserveCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// The UgpReserveCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpReserveCategory(IF2XTransport transport, IUgpReserveCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            AddMessagehandler<UgpReserveCategorySetReserveParametersSend>(HandleSetReserveParametersSend);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpReserveCategorySetReserveParametersSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleSetReserveParametersSend(UgpReserveCategorySetReserveParametersSend message)
        {
			var parameters = new ReserveParameters(
				message.IsReserveAllowedWithCredits,
				message.IsReserveAllowedWithoutCredits,
				message.ReserveTimeWithCreditsMilliseconds,
				message.ReserveTimeWithoutCreditsMilliseconds);
			callbackHandler.ProcessSetReserveParameters(parameters);

            var reply = CreateReply<UgpReserveCategorySetReserveParametersReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpReserve;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 1;

        #endregion

        #region IUgpReserveCategory Implementation

        /// <inheritdoc/>
		public ReserveParameters GetReserveParameters()
		{
			var request = CreateBasicRequest<UgpReserveCategoryGetReserveParametersSend>();
			var r = SendMessageAndGetReply<UgpReserveCategoryGetReserveParametersReply>(Channel.Foundation, request);
			return new ReserveParameters(r.IsReserveAllowedWithCredits, r.IsReserveAllowedWithoutCredits, r.ReserveTimeWithCreditsMilliseconds, r.ReserveTimeWithoutCreditsMilliseconds);
		}

        /// <inheritdoc/>
        public void SendActivationChanged(bool isActive)
        {
            var request = CreateBasicRequest<UgpReserveCategorySetActivationChangedSend>();
            var messageItem = (UgpReserveCategorySetActivationChangedSend)request.Message.Item;

            messageItem.IsReserveActive = isActive;

            var reply = SendMessageAndGetReply<UgpReserveCategorySetActivationChangedReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        #endregion
    }
}
