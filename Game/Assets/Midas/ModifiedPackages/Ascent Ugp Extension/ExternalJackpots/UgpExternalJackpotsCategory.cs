//-----------------------------------------------------------------------
// <copyright file = "UgpExternalJackpotsCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.Linq;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpExternalJackpots category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpExternalJackpotsCategory : F2XTransactionalCategoryBase<UgpExternalJackpotsCategoryInternal>,
                                                 IUgpExternalJackpotsCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the UgpExternalJackpotsCategory callbacks.
        /// </summary>
        private readonly IUgpExternalJackpotsCategoryCallbacks callbackHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="UgpExternalJackpotsCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// UgpExternalJackpotsCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpExternalJackpotsCategory(IF2XTransport transport,
                                           IUgpExternalJackpotsCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            AddMessagehandler<UgpExternalJackpotsCategoryUpdateJackpotsSend>(HandleUpdateJackpotsSend);
        }

        #endregion

		#region IUgpExternalJackpotsCategory Methods

		public ExternalJackpots GetExternalJackpots()
		{
			var request = CreateBasicRequest<UgpExternalJackpotsCategoryGetJackpotsSend>();
			var reply = SendMessageAndGetReply<UgpExternalJackpotsCategoryGetJackpotsReply>(Channel.Foundation, request);
			return new ExternalJackpots
			{
				Jackpots = reply.Jackpots.Select(j => new ExternalJackpot
				{
					Name = j.Name,
					Value = j.Value,
					IconId = j.IconId,
					IsVisible = j.IsVisible
				}).ToList()
			};
		}

		#endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpExternalJackpotsCategoryUpdateJackpotsSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleUpdateJackpotsSend(UgpExternalJackpotsCategoryUpdateJackpotsSend message)
        {
            callbackHandler.ProcessUpdateJackpots(new ExternalJackpots
            {
                IconId = message.IconId,
                IsVisible = message.IsVisible,
                Jackpots = message.Jackpots.Select(
                                jackpot => new ExternalJackpot()
                                                {
                                                    IconId = jackpot.IconId,
                                                    IsVisible = jackpot.IsVisible,
                                                    Name = jackpot.Name,
                                                    Value = jackpot.Value
                                                }).ToList()
            });

            var reply = CreateReply<UgpExternalJackpotsCategoryUpdateJackpotsReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpExternalJackpots;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 1;

        #endregion
    }
}
