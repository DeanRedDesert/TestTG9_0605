//-----------------------------------------------------------------------
// <copyright file = "GameLibMeterSource.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Adapter that implements IMeterSource backed by GameLib access.
    /// </summary>
    public class GameLibMeterSource : IMeterSource, IGameLibEventListener
    {
        private readonly IGameLib gameLib;

        /// <summary>
        /// The storage path of the data about win is pending <see cref="isWinPending"/> or not in critical data.
        /// </summary>
        private const string IsWinPendingPath = "GameLibMeterSource/IsWinPending";

        /// <summary>
        /// The win pending flag indicates that the current win has been added into credit meter or not.
        /// </summary>
        private bool? isWinPending;

        /// <summary>
        /// Construct an instance of the meter source with the given GameLib.
        /// </summary>
        /// <param name="gameLib">The IGameLib implementation to use for meter information.</param>
        public GameLibMeterSource(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib",
                                                "The gameLib parameter is required to collect meter information.");
            }
            this.gameLib = gameLib;
            gameLib.MoneyEvent += OnMoneyEvent;
            gameLib.OutcomeResponseEvent += OnOutcomeResponseEvent;
        }

        /// <summary>
        /// Handler for money events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="moneyEvent"><see cref="MoneyEventArgs"/> containing details of the money movement.</param>
        private void OnMoneyEvent(object sender, MoneyEventArgs moneyEvent)
        {
            // Wagerable meter added for Banked Credits mode
            var sourceItems = new List<MeterSourceItem> { MeterSourceItem.CreditMeter, MeterSourceItem.WagerableMeter, MeterSourceItem.PaidMeter };
            if (moneyEvent.Type == MoneyEventType.MoneyWon)
            {
                IsWinPending = false;
                sourceItems.Add(MeterSourceItem.IsWinPending);
            }
            var eventArgs = new MeterSourceUpdatedEventArgs(sourceItems);

            var tempHandler = MeterSourceUpdatedEvent;
            if(tempHandler != null)
            {
                tempHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Handler for outcome response event from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="outcomeResponseEvent"><see cref="OutcomeResponseEventArgs"/>
        /// Containing details of outcome response event.
        /// </param>
        private void OnOutcomeResponseEvent(object sender, OutcomeResponseEventArgs outcomeResponseEvent)
        {
            if(!IsWinPending)
            {
                var sourceItems = new List<MeterSourceItem> { MeterSourceItem.CreditMeter, MeterSourceItem.PaidMeter };
                IsWinPending = true;
                sourceItems.Add(MeterSourceItem.IsWinPending);

                var eventArgs = new MeterSourceUpdatedEventArgs(sourceItems);

                var tempHandler = MeterSourceUpdatedEvent;
                if(tempHandler != null)
                {
                    tempHandler(this, eventArgs);
                }
            }
        }

        #region IMeterSource Implementation

        /// <inheritdoc />
        public long CreditMeter
        {
            get
            {
                if(gameLib.GameContextMode != GameMode.Utility)
                {
                    var playerMeters = gameLib.GetPlayerMeters();

                    // Banked Credits mode retains credit meter
                    return playerMeters.Bank;
                }

                // If the game is in utility mode return 0 for the credit meter regardless
                // of the actual amount on the EGM. This is not regulatory but an IGT design
                // convention that customers are used to.
                return 0;

            }
        }

        /// <inheritdoc />
        public long WagerableMeter
        {
            get
            {
                if(gameLib.GameContextMode != GameMode.Utility)
                {
                    var playerMeters = gameLib.GetPlayerMeters();

                    return playerMeters.Wagerable;
                }

                // If the game is in utility mode return 0 for the wagerable meter regardless
                // of the actual amount on the EGM. This is not regulatory but an IGT design
                // convention that customers are used to.
                return 0;

            }
        }

        /// <inheritdoc />
        public bool IsWinPending
        {
            get
            {
                if(!isWinPending.HasValue)
                {
                    isWinPending = gameLib.ReadCriticalData<bool>(CriticalDataScope.Payvar, IsWinPendingPath);
                }
                return isWinPending.Value;
            }
            private set
            {
                isWinPending = value;
                gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsWinPendingPath, isWinPending);
            }
        }

        /// <inheritdoc />
        public long PaidMeter
        {
            get
            {
                if(gameLib.GameContextMode != GameMode.Utility)
                {
                    var playerMeters = gameLib.GetPlayerMeters();
                    return playerMeters.Paid;
                }

                // If the game is in utility mode return 0 for the win meter regardless
                // of the actual amount on the EGM. This is not regulatory but an IGT design
                // convention that customers are used to.
                return 0;
            }
        }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorMode CreditMeterDisplayBehavior
        {
            get { return gameLib.GetCreditMeterBehavior(); }
        }

        /// <inheritdoc />
        public event EventHandler<MeterSourceUpdatedEventArgs> MeterSourceUpdatedEvent;

        #endregion

        #region IGameLibEventListener Implementation

        /// <inheritdoc />
        /// <devdoc>The gameLib parameter is re-named to prevent warnings.</devdoc>
        public void UnregisterGameLibEvents(IGameLib gameLibToUnregister)
        {
            gameLibToUnregister.MoneyEvent -= OnMoneyEvent;
            gameLibToUnregister.OutcomeResponseEvent -= OnOutcomeResponseEvent;
        }

        #endregion
    }
}