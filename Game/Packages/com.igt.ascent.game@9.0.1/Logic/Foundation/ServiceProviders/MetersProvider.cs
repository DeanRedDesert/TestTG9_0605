//-----------------------------------------------------------------------
// <copyright file = "MetersProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    ///    Core Logic Service Provider for Meter Data. Retrieves meter values
    ///    from the foundation.
    /// </summary>
    public class MetersProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Private Fields

        /// <summary>
        /// Source of meter information.
        /// </summary>
        private readonly IMeterSource meterSource;

        /// <summary>
        /// The dictionary between the meter source item <see cref="MeterSourceItem"/>
        /// and game service provided by this class.
        /// </summary>
        private static readonly Dictionary<MeterSourceItem, string> MeterSourceToGameServiceDictionary = 
            new Dictionary<MeterSourceItem, string>
            {
                  {MeterSourceItem.CreditMeter, "CreditMeter"},
                  {MeterSourceItem.WagerableMeter, "WagerableMeter"}, // Banked Credits
                  {MeterSourceItem.PaidMeter, "PaidMeter"},
                  {MeterSourceItem.IsWinPending, "IsWinPending"}
            };

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a provider with the given meter source and behavior.
        /// </summary>
        /// <param name="meterSource">Meter source to access meter information.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="meterSource"/> is null.
        /// </exception>
        public MetersProvider(IMeterSource meterSource)
        {
            if(meterSource == null)
            {
                throw new ArgumentNullException("meterSource", "A meter source is required.");
            }

            this.meterSource = meterSource;
            meterSource.MeterSourceUpdatedEvent += OnMeterSourceUpdatedEvent;
        }

        #endregion

        #region Machine Meters

        /// <summary>
        ///    Machine Credit Meter, retrieved from the Foundation.
        /// </summary>
        [AsynchronousGameService]
        public long CreditMeter
        {
            get { return meterSource.CreditMeter; }
        }

        /// <summary>
        ///    Machine Wagerable Meter, retrieved from the Foundation.
        /// </summary>
        [AsynchronousGameService]
        public long WagerableMeter
        {
            get { return meterSource.WagerableMeter; }
        }

        /// <summary>
        /// The current win is pending or not, which is set
        /// by MoneyWonEvent or OutcomeResponseEvent from the Foundation.
        /// </summary>
        [AsynchronousGameService]
        public bool IsWinPending
        {
            get { return meterSource.IsWinPending; }
        }

        /// <summary>
        ///    Machine Paid Meter, retrieved from the Foundation.
        /// </summary>
        [AsynchronousGameService]
        public long PaidMeter
        {
            get { return meterSource.PaidMeter; }
        }

        #endregion

        /// <summary>
        /// Credit meter behavior defined by the foundation.
        /// </summary>
        [GameService]
        public CreditMeterDisplayBehaviorMode CreditMeterBehavior
        {
            get { return meterSource.CreditMeterDisplayBehavior; }
        }

        #region Private Members

        /// <summary>
        /// Handler for meter source updated event from the meter source <see cref="IMeterSource"/>>
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="moneyEvent"><see cref="MeterSourceUpdatedEventArgs"/>
        /// containing details of the meter source item collection updated event.
        /// </param>
        private void OnMeterSourceUpdatedEvent(object sender, MeterSourceUpdatedEventArgs moneyEvent)
        {
            var tempHandler = AsynchronousProviderChanged;
            if (tempHandler != null)
            {
                var serviceSignature = (from moneySourceItem in moneyEvent.MeterSourceItems
                                        where MeterSourceToGameServiceDictionary.ContainsKey(moneySourceItem)
                                        select new ServiceSignature
                                            (MeterSourceToGameServiceDictionary[moneySourceItem])).ToList();
                tempHandler(this, new AsynchronousProviderChangedEventArgs(serviceSignature, true));
            }
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            meterSource.MeterSourceUpdatedEvent -= OnMeterSourceUpdatedEvent;

            var meterListener = meterSource as IGameLibEventListener;
            if(meterListener != null)
            {
                meterListener.UnregisterGameLibEvents(gameLib);
            }
        }

        #endregion
    }
}
