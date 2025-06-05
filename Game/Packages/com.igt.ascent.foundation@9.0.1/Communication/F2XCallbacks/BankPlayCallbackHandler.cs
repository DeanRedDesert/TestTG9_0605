//-----------------------------------------------------------------------
// <copyright file = "BankPlayCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Communication.Platform.ShellLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.Types;
    using F2XTransport;
    using F2XBankPlayProperties = F2X.Schemas.Internal.BankPlay.BankPlayProperties;
    using F2XBettableTransferDirection = F2X.Schemas.Internal.BankPlay.BettableTransferDirection;
    using F2XFundsTransferAccountSource = F2X.Schemas.Internal.BankPlay.FundsTransferAccountSource;
    using F2XMoneyInSource = F2X.Schemas.Internal.BankPlay.MoneyInSource;
    using F2XMoneyOutSource = F2X.Schemas.Internal.BankPlay.MoneyOutSource;
    using F2XPlayerMeters = F2X.Schemas.Internal.BankPlay.PlayerMeters;

    /// <summary>
    /// This class is responsible for handling callbacks from the <see cref="BankPlayCategory"/>.
    /// </summary>
    internal class BankPlayCallbackHandler : IBankPlayCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// The callback interface for handling non transactional events.
        /// </summary>
        private readonly INonTransactionalEventCallbacks nonTransactionalEventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the <see cref="BankPlayCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for handling transactional events.
        /// </param>
        /// <param name="nonTransactionalEventCallbacks">
        /// The callback interface for handling non transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="eventCallbacksInterface"/> or <paramref name="nonTransactionalEventCallbacks"/>
        /// is null.
        /// </exception>
        public BankPlayCallbackHandler(IEventCallbacks eventCallbacksInterface,
                                       INonTransactionalEventCallbacks nonTransactionalEventCallbacks)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
            this.nonTransactionalEventCallbacks = nonTransactionalEventCallbacks ?? throw new ArgumentNullException(nameof(nonTransactionalEventCallbacks));
        }

        #endregion

        #region IBankPlayCategoryCallbacks

        /// <inheritdoc/>
        public string ProcessMoneyBet(F2XPlayerMeters playerMeters, Amount amountBet)
        {
            var eventArgs = new MoneyBetEventArgs(amountBet.Value, ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneyBettableTransfer(F2XPlayerMeters playerMeters, Amount amountMoved,
            bool gameTransferRequestSuccessful, bool gameTransferRequestSuccessfulSpecified,
            F2XBettableTransferDirection bettableTransferDirection)
        {
            var eventArgs = new MoneyBettableTransferEventArgs(
                amountMoved.Value,
                gameTransferRequestSuccessfulSpecified ? (bool?)gameTransferRequestSuccessful : null,
                (BettableTransferDirection)(int)bettableTransferDirection,
                ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneyCommittedChanged(F2XPlayerMeters playerMeters, Amount amountCommitted)
        {
            var eventArgs = new MoneyCommittedChangedEventArgs(amountCommitted.Value, ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneyIn(F2XPlayerMeters playerMeters, Amount amountIn, F2XMoneyInSource moneyInSource,
            F2XFundsTransferAccountSource fundsTransferAccountSource, bool fundsTransferAccountSourceSpecified)
        {
            var eventArgs = new MoneyInEventArgs(
                amountIn.Value,
                (MoneyInSource)(int)moneyInSource,
                fundsTransferAccountSourceSpecified ? (FundsTransferAccountSource?)(int)fundsTransferAccountSource : null,
                ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneyOut(F2XPlayerMeters playerMeters, Amount amountOut, F2XMoneyOutSource moneyOutSource)
        {
            var eventArgs = new MoneyOutEventArgs(amountOut.Value, (MoneyOutSource)(int)moneyOutSource, ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneySet(F2XPlayerMeters playerMeters)
        {
            var eventArgs = new MoneySetEventArgs(ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessMoneyWon(F2XPlayerMeters playerMeters, Amount amountWon)
        {
            var eventArgs = new MoneyWonEventArgs(amountWon.Value, ToPublic(playerMeters));

            eventCallbacksInterface.PostEvent(eventArgs);

            return null;
        }

        /// <inheritdoc/>
        public string ProcessUpdateBankPlayProperties(F2XBankPlayProperties bankPlayProperties)
        {
            nonTransactionalEventCallbacks.EnqueueEvent(new BankPlayPropertiesUpdateEventArgs(
                bankPlayProperties.CanBetSpecified ? (bool?)bankPlayProperties.CanBet : null,
                bankPlayProperties.CanCommitGameCycleSpecified ? (bool?)bankPlayProperties.CanCommitGameCycle : null,
                bankPlayProperties.CashoutOfferableSpecified ? (bool?)bankPlayProperties.CashoutOfferable : null,
                bankPlayProperties.PlayerBettableTransferOfferableSpecified ?
                   (bool?)bankPlayProperties.PlayerBettableTransferOfferable : null));

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Convert an F2X <see cref="F2XPlayerMeters"/> to a public <see cref="GamingMeters"/>.
        /// </summary>
        /// <param name="playerMeters">
        /// The F2X player meters to convert.  This category was written before the new name of GamingMeters.
        /// </param>
        /// <returns>The conversion result.</returns>
        private static GamingMeters ToPublic(F2XPlayerMeters playerMeters)
        {
            if(playerMeters == null)
            {
                throw new ArgumentNullException(nameof(playerMeters));
            }

            return new GamingMeters(playerMeters.PlayerTransferable.Value,
                                    playerMeters.PlayerBettable.Value,
                                    playerMeters.PaidMeter.Value);
        }

        #endregion
    }
}