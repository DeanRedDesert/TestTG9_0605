// -----------------------------------------------------------------------
// <copyright file = "BankPlayPropertiesProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using System;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A provider for the bank play properties.
    /// </summary>
    public class BankPlayPropertiesProvider : ObserverProviderBase<BankPlayProperties>
    {
        #region Constants

        private const string DefaultName = nameof(BankPlayPropertiesProvider);
        private const string CanBetObserverName = nameof(CanBet);
        private const string CanCommitGameCycleName = nameof(CanCommitGameCycle);
        private const string CashoutOfferableName = nameof(CashoutOfferable);
        private const string PlayerBettableTransferOfferableName = nameof(PlayerBettableTransferOfferable);

        #endregion

        #region Private Fields

        private BankPlayProperties bankPlayProperties;

        /// <devdoc>
        /// All of the services that are updated together when raising the Asynchronous Provider Changed Event.
        /// </devdoc>
        private static readonly ServiceSignature[] BankPlayUpdateServiceSignatures =
        {
            new ServiceSignature(CanBetObserverName),
            new ServiceSignature(CanCommitGameCycleName),
            new ServiceSignature(CashoutOfferableName),
            new ServiceSignature(PlayerBettableTransferOfferableName),
        };

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the flag that indicates if the player can bet.
        /// </summary>
        [AsynchronousGameService]
        public bool CanBet => bankPlayProperties.CanBet;

        /// <summary>
        /// Gets the flag that indicates if the player can commit a game-cycle.
        /// </summary>
        [AsynchronousGameService]
        public bool CanCommitGameCycle => bankPlayProperties.CanCommitGameCycle;

        /// <summary>
        /// Gets the flag that indicates if a player cash-out request is offerable.
        /// </summary>
        [AsynchronousGameService]
        public bool CashoutOfferable => bankPlayProperties.CashoutOfferable;

        /// <summary>
        /// Gets the flag that indicates if the player may request money to
        /// be moved to/from "player transferable" to/from "player bettable".
        /// </summary>
        [AsynchronousGameService]
        public bool PlayerBettableTransferOfferable => bankPlayProperties.PlayerBettableTransferOfferable;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public BankPlayPropertiesProvider(IObservable<BankPlayProperties> observable, string name = DefaultName)
            : base(observable, name)
        {
        }

        #endregion


        #region Observer

        /// <inheritdoc/>
        public override void OnNext(BankPlayProperties value)
        {
            if(value == null)
            {
                return;
            }

            bankPlayProperties = value;

            OnAsynchronousProviderChanged(new AsynchronousProviderChangedEventArgs(BankPlayUpdateServiceSignatures, false));
        }

        #endregion
    }
}