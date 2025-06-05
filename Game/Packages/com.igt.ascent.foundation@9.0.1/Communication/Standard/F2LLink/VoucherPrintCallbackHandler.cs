//-----------------------------------------------------------------------
// <copyright file = "VoucherPrintCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using F2L;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the voucher print category.
    /// </summary>
    internal class VoucherPrintCallbackHandler : IVoucherPrintCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks callbackInterface;

        /// <summary>
        /// Construct an instance of the IVoucherPrintCategoryCallbacks.
        /// </summary>
        /// <param name="callbackHandler">The call back handler used to post the voucher print events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="callbackHandler"/> is null.
        /// </exception>
        public VoucherPrintCallbackHandler(IEventCallbacks callbackHandler)
        {
            if(callbackHandler == null)
            {
                throw new ArgumentNullException("callbackHandler", "Parameter can not be null.");
            }

            callbackInterface = callbackHandler;
        }

        #region IVoucherPrintCategoryCallbacks

        /// <inheritdoc />
        public void ProcessVoucherPrintEvent(VoucherPrintEvent printEvent, long amount)
        {
            callbackInterface.PostEvent(new VoucherPrintEventArgs(printEvent, amount));
        }

        #endregion
    }
}
