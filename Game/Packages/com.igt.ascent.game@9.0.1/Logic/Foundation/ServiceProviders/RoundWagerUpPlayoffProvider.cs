//-----------------------------------------------------------------------
// <copyright file = "RoundWagerUpPlayoffProvider.cs" company = "IGT">
//     Copyright (c) IGT 2015-2018.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using Services;
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class provides the logic data specific to the residual credit playoff.
    /// </summary>
    public class RoundWagerUpPlayoffProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Local critical data path

        // Critical data paths specific to residual credit playoff provider.
        private const string GamePathRoot                             = "RoundWagerUpPlayoffGame";
        private const string RoundWagerUpPlayoffResidualAmountPath    = GamePathRoot + "/RoundWagerUpPlayoffResidualAmount";
        private const string RoundWagerUpPlayoffCreditAmountPath      = GamePathRoot + "/RoundWagerUpPlayoffCreditAmount";
        private const string RoundWagerUpPlayoffExpectedBetAmountPath = GamePathRoot + "/RoundWagerUpPlayoffExpectedBetAmount";
        private const string IsRoundWagerUpPlayoffOfferablePath       = GamePathRoot + "/IsRoundWagerUpPlayoffOfferable";
        private const string IsRoundWagerUpPlayoffWonPath             = GamePathRoot + "/IsRoundWagerUpPlayoffWon";
        private const string RoundWagerUpPlayoffLuckyNumberPath       = GamePathRoot + "/RoundWagerUpPlayoffLuckyNumber";
        private const string IsTriggeredByGamePath                    = GamePathRoot + "/IsTriggeredByGame";

        #endregion

        #region Local members

        // Local reference to GameLib.
        private readonly IGameLib gameLib;
        private bool isTriggeredByGame;

        #endregion

        #region Game Service

        /// <summary>
        /// The residual credit amount which is used to determine if the round wager up playoff is offerable.
        /// The residual symbol on the dashboard in idle state should consume this service.
        /// </summary>
        [AsynchronousGameService]
        public long CreditAmount { get; private set; }

        /// <summary>
        /// The residual bet amount which is committed.
        /// This will be used for evaluation and result presentation.
        /// </summary>
        [AsynchronousGameService]
        public long ResidualAmount { get; private set; }

        /// <summary>
        /// The expected bet amount is same as current bet amount.
        /// </summary>
        [GameService]
        public long ExpectedBetAmount { get; private set; }

        /// <summary>
        /// Service of a flag to tell if round wager up playoff is offerable.
        /// </summary>
        [AsynchronousGameService]
        public bool IsRoundWagerUpPlayoffOfferable { get; private set; }

        /// <summary>
        /// The residual credit playoff result to indicate if it is a win or not.
        /// </summary>
        [GameService]
        public bool IsRoundWagerUpPlayoffWon { get; private set; }

        /// <summary>
        /// The flag indicating if the RoundWagerUpPlayoff is enabled for the current game context.
        /// </summary>
        [GameService]
        public bool IsRoundWagerUpPlayoffEnabled
        {
            get { return gameLib.RoundWagerUpPlayoffEnabled; }
        }

        /// <summary>
        /// The residual credit playoff lucky number used to determine the result.
        /// </summary>
        [GameService]
        public int RoundWagerUpPlayoffLuckyNumber { get; private set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Flag indicates whether credit playoff is triggered by game.
        /// </summary>
        public bool IsTriggeredByGame
        {
            get { return isTriggeredByGame; }
            set
            {
                if(isTriggeredByGame != value)
                {
                    isTriggeredByGame = value;
                    RefreshIsRoundWagerUpPlayoffOfferable();
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsTriggeredByGamePath,
                        isTriggeredByGame);
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath,
                        IsRoundWagerUpPlayoffOfferable);
                    var tempHandler = AsynchronousProviderChanged;
                    if(tempHandler != null)
                    {
                        tempHandler(this, new AsynchronousProviderChangedEventArgs(new List<ServiceSignature>
                        {
                            new ServiceSignature("IsRoundWagerUpPlayoffOfferable")
                        }, true));
                    }
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor to initialize the residual playoff provider.
        /// </summary>
        /// <param name="iGameLib">
        /// Interface to GameLib, GameLib is responsible for communication with the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when iGameLib is null.</exception>
        public RoundWagerUpPlayoffProvider(IGameLib iGameLib)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException("iGameLib", "Parameter cannot be null.");
            }

            // Cache the GameLib interface in provider.
            gameLib = iGameLib;

            gameLib.MoneyEvent += OnMoneyEvent;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Update the round wager up playoff provider with credit amount and expected amount.
        /// This method can be called to update the possible service changes on credit amount and
        /// expected bet amount.
        /// </summary>
        /// <param name="newCreditAmount">The new credit amount.</param>
        /// <param name="newExpectedBetAmount">The new expected bet amount.</param>
        public void UpdateWithAmount(long newCreditAmount, long newExpectedBetAmount)
        {
            CreditAmount = newCreditAmount;
            ExpectedBetAmount = newExpectedBetAmount;

            RefreshIsRoundWagerUpPlayoffOfferable();

            gameLib.WriteCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffCreditAmountPath,
                CreditAmount);
            gameLib.WriteCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffExpectedBetAmountPath,
                ExpectedBetAmount);
            gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath,
                IsRoundWagerUpPlayoffOfferable);
            var tempHandler = AsynchronousProviderChanged;
            if(tempHandler != null)
            {
                tempHandler(this, new AsynchronousProviderChangedEventArgs(new List<ServiceSignature>
                {
                    new ServiceSignature("CreditAmount"),
                    new ServiceSignature("IsRoundWagerUpPlayoffOfferable")
                }, true));
            }
        }

        /// <summary>
        /// Update the residual credit playoff provider with playoff result if it is won or not.
        /// This method can be called at each time right after the evaluation is done with residual credit playoff.
        /// </summary>
        /// <param name="winOrLoss">The round wager up playoff result indicating if it is won or not.</param>
        public void UpdateWithResult(bool winOrLoss)
        {
            UpdateWithResult(winOrLoss, 0);
        }

        /// <summary>
        /// Update the residual credit playoff provider with playoff result if it is won or not.
        /// This method can be called at each time right after the evaluation is done with residual credit playoff.
        /// </summary>
        /// <param name="winOrLoss">The round wager up playoff result indicating if it is won or not.</param>
        /// <param name="luckyNumber">The residual credit playoff lucky number used to determine the result.</param>
        public void UpdateWithResult(bool winOrLoss, int luckyNumber)
        {
            IsRoundWagerUpPlayoffWon = winOrLoss;
            RoundWagerUpPlayoffLuckyNumber = luckyNumber;
            // Save it in critical data.
            gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffWonPath,
                IsRoundWagerUpPlayoffWon);
            gameLib.WriteCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffLuckyNumberPath,
                RoundWagerUpPlayoffLuckyNumber);
        }

        /// <summary>
        /// Restore the residual playoff provider from critical data.
        /// This method can be called before starting any presentation states related to residual credit playoff
        /// states.
        /// </summary>
        public void RestoreData()
        {
            ResidualAmount =
                gameLib.ReadCriticalData<long>(CriticalDataScope.Payvar, RoundWagerUpPlayoffResidualAmountPath);
            CreditAmount =
                gameLib.ReadCriticalData<long>(CriticalDataScope.Payvar, RoundWagerUpPlayoffCreditAmountPath);
            ExpectedBetAmount =
                gameLib.ReadCriticalData<long>(CriticalDataScope.Payvar, RoundWagerUpPlayoffExpectedBetAmountPath);
            IsTriggeredByGame =
                gameLib.ReadCriticalData<bool>(CriticalDataScope.Payvar, IsTriggeredByGamePath);
            IsRoundWagerUpPlayoffOfferable =
                gameLib.ReadCriticalData<bool>(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath);
            IsRoundWagerUpPlayoffWon =
                gameLib.ReadCriticalData<bool>(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffWonPath);
            RoundWagerUpPlayoffLuckyNumber =
                gameLib.ReadCriticalData<int>(CriticalDataScope.Payvar, RoundWagerUpPlayoffLuckyNumberPath);
        }

        /// <summary>
        /// Reset this provider and clear the critical data of residual playoff provider.
        /// This method can be called before a new game cycle is about to start with possible residual credit playoff.
        /// </summary>
        public void ClearData()
        {
            // Reset all the properties of this provider.
            ResidualAmount = 0;
            CreditAmount = 0;
            ExpectedBetAmount = 0;
            IsTriggeredByGame = false;
            IsRoundWagerUpPlayoffOfferable = false;
            IsRoundWagerUpPlayoffWon = false;
            RoundWagerUpPlayoffLuckyNumber = 0;

            // Clear the critical data
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffResidualAmountPath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffCreditAmountPath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffExpectedBetAmountPath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, IsTriggeredByGamePath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffWonPath);
            gameLib.RemoveCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffLuckyNumberPath);
        }

        /// <summary>
        /// Commit a bet with residual amount to foundation.
        /// </summary>
        /// <param name="bet">
        /// The residual bet amount.
        /// </param>
        /// <returns>True if committed successfully. False otherwise.</returns>
        public bool CommitResidualBet(long bet)
        {
            var committed = false;
            if(IsRoundWagerUpPlayoffOfferable)
            {
                committed = gameLib.CommitBet(bet, 1);
                if(committed)
                {
                    SetResidualAmount(bet);
                }
            }
            return committed;
        }

        /// <summary>
        /// Set the ResidualAmount for play.
        /// </summary>
        /// <param name="bet">Bet amount used for residual credits.</param>
        public void SetResidualAmount(long bet)
        {
            ResidualAmount = bet;
            gameLib.WriteCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffResidualAmountPath, 
                ResidualAmount);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Refresh the service of a flag to tell if round wager up playoff is offerable.
        /// </summary>
        private void RefreshIsRoundWagerUpPlayoffOfferable()
        {
            // According to the latest version (1.0) of credit playoff document,
            // CREDIT PLAYOFF can only be activated by the game
            // when the CREDIT meter gets smaller as the TOTAL BET meter.
            IsRoundWagerUpPlayoffOfferable = isTriggeredByGame &&
                                             IsRoundWagerUpPlayoffEnabled &&
                                             CreditAmount > 0 &&
                                             CreditAmount >= gameLib.GameMinBetAmount &&
                                             CreditAmount < ExpectedBetAmount;
        }

        /// <summary>
        /// Event handler for money events from the foundation. This handler is used to raise the
        /// AsynchronousProviderChanged event, so that the round wager up playoff service consumers can update
        /// their presentation status in realtime.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="moneyEvent"><see cref="MoneyEventArgs" /> containing details of the money event.</param>
        /// <devdoc>
        /// Currently all money events other than MoneyBet and MoneyCommittedChanged trigger an update, since a MoneyBet
        /// and/or MoneyCommittedChanged event is posted at each time after committing bet is sent, where, the bank meter
        /// is deducted from residual amount. To avoid updating provider in this case, we should bypass it.                      
        /// </devdoc>
        private void OnMoneyEvent(object sender, MoneyEventArgs moneyEvent)
        {
            var tempHandler = AsynchronousProviderChanged;
            switch(moneyEvent.Type)
            {
                case MoneyEventType.MoneyIn:
                case MoneyEventType.MoneyOut:
                case MoneyEventType.MoneySet:
                case MoneyEventType.MoneyWagerable:
                    isTriggeredByGame = false;
                    IsRoundWagerUpPlayoffOfferable = false;
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsTriggeredByGamePath,
                        isTriggeredByGame);
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath,
                        IsRoundWagerUpPlayoffOfferable);
                    if(tempHandler != null)
                    {
                        tempHandler(this, new AsynchronousProviderChangedEventArgs(new List<ServiceSignature>
                        {
                            new ServiceSignature("IsRoundWagerUpPlayoffOfferable")
                        }, true));
                    }
                    break;
                case MoneyEventType.MoneyWon:
                    CreditAmount = moneyEvent.Meters.Bank;
                    RefreshIsRoundWagerUpPlayoffOfferable();
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, RoundWagerUpPlayoffCreditAmountPath,
                        CreditAmount);
                    gameLib.WriteCriticalData(CriticalDataScope.Payvar, IsRoundWagerUpPlayoffOfferablePath,
                        IsRoundWagerUpPlayoffOfferable);
                    if(tempHandler != null)
                    {
                        tempHandler(this, new AsynchronousProviderChangedEventArgs(new List<ServiceSignature>
                        {
                            new ServiceSignature("CreditAmount"),
                            new ServiceSignature("IsRoundWagerUpPlayoffOfferable")
                        }, true));
                    }
                    break;
            }
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
            iGameLib.MoneyEvent -= OnMoneyEvent;
        }

        #endregion
    }
}
