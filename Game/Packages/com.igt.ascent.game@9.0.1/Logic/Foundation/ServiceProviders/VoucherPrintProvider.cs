//-----------------------------------------------------------------------
// <copyright file = "VoucherPrintProvider.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Services;
    using Session;

    /// <summary>
    /// The provider gives the information about voucher print notification.
    /// </summary>
    public class VoucherPrintProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Game Service

        /// <summary>
        /// Get the information of voucher print notification which provides voucher print event type,
        /// voucher print amount value in base units.
        /// Null if no voucher print notification event has been received yet.
        /// </summary>
        /// <remarks>
        /// Each voucher print notification service should be consumed only once. Before using the payload,
        /// be sure to check it is not null and whether it has a new session id.
        /// </remarks>
        [AsynchronousGameService]
        public VoucherPrintInfo VoucherPrintNotification
        {
            get;
            private set;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for voucher print provider.
        /// </summary>
        /// <param name="iGameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="iGameLib"/> is null.
        /// </exception>
        public VoucherPrintProvider(IGameLib iGameLib)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException("iGameLib", "Parameter cannot be null.");
            }

            iGameLib.VoucherPrintNotificationEvent += HandleVoucherPrintEvent;

            VoucherPrintNotification = null;
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            iGameLib.VoucherPrintNotificationEvent -= HandleVoucherPrintEvent;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler for voucher print events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="voucherPrintEventArgs">
        /// <see cref="VoucherPrintEventArgs"/> containing details of the voucher print notification.
        /// </param>
        private void HandleVoucherPrintEvent(object sender, VoucherPrintEventArgs voucherPrintEventArgs)
        {
            VoucherPrintNotification = new VoucherPrintInfo(voucherPrintEventArgs.VoucherPrintEvent,
                                                            voucherPrintEventArgs.Value, UniqueIdentifier.New());

            var tempHandler = AsynchronousProviderChanged;
            if(tempHandler != null)
            {
                tempHandler(this, new AsynchronousProviderChangedEventArgs("VoucherPrintNotification"));
            }
        }

        #endregion
    }
}
