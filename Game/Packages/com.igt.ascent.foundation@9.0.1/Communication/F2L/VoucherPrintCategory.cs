//-----------------------------------------------------------------------
// <copyright file = "VoucherPrintCategory.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using F2XTransport;
    using Schemas.Internal;

    /// <summary>
    /// Implementation of the F2L voucher printer API category.
    /// </summary>
    public class VoucherPrintCategory: F2LTransactionalCategoryBase<VoucherPrint>
    {
        #region Fields

        /// <summary>
        /// Object which implements the voucher print category callbacks.
        /// </summary>
        private readonly IVoucherPrintCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the voucher print category.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        /// <param name="callbackHandler">Voucher print category callback handler.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callbackHandler"/> is null.
        /// </exception>
        public VoucherPrintCategory(IF2XTransport transport, IVoucherPrintCategoryCallbacks callbackHandler)
            : base(transport)
        {
            if(callbackHandler == null)
            {
                throw new ArgumentNullException("callbackHandler", "Argument can not be null.");
            }

            this.callbackHandler = callbackHandler;

            ConfigureHandlers();
        }

        /// <summary>
        /// Configure the handler table for all voucher printer category messages which can be received.
        /// </summary>
        private void ConfigureHandlers()
        {
            AddMessagehandler<VoucherPrintPrintNotificationSend>(HandleVoucherPrintPrintNotificationSend);
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion
        {
            get { return 1; }
        }

        /// <inheritdoc />
        public override uint MinorVersion
        {
            get { return 0; }
        }

        /// <inheritdoc />
        public override MessageCategory Category
        {
            get { return MessageCategory.VoucherPrint; }
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the VoucherPrintPrintNotificationSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleVoucherPrintPrintNotificationSend(VoucherPrintPrintNotificationSend message)
        {
            callbackHandler.ProcessVoucherPrintEvent(
                (Ascent.Communication.Platform.GameLib.Interfaces.VoucherPrintEvent)message.Notification,
                message.Value.Value);

            var reply = CreateReply<VoucherPrintPrintNotificationReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion
    }
}
