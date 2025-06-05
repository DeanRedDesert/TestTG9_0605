//-----------------------------------------------------------------------
// <copyright file = "BankStatusProvider.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Communication.Foundation;
    using Services;
    using Session;

    /// <summary>
    /// Core logic service provider for information about the bank.
    /// </summary>
    public class BankStatusProvider : IGameLibEventListener, INotifyAsynchronousProviderChanged
    {
        private readonly IGameLib cachedGameLib;
        private bool? cashOutOfferable;
        private bool? isBankToWagerableOfferable;
        private bool? isPlayerWagerOfferable;

        #region Constructor

        /// <summary>
        /// Constructor for bank status provider.
        /// </summary>
        /// <param name="gameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="gameLib"/> is null.</exception>
        public BankStatusProvider(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib", "Parameter cannot be null.");
            }

            cachedGameLib = gameLib;
            cachedGameLib.MoneyEvent += HandleMoneyEvent;
            cachedGameLib.BankStatusChangedEvent += HandleBankStatusChangedEvent;

            MoneyIn = null;
            MoneyOut = null;
        }

        #endregion

        #region Game Services

        /// <summary>
        /// Indicates if cash out can be offered to the player.
        /// </summary>
        [AsynchronousGameService]
        public bool CashOutOfferable
        {
            get
            {
                if(!cashOutOfferable.HasValue)
                {
                    cashOutOfferable = cachedGameLib.IsCashOutOfferable;
                }

                return cashOutOfferable.Value;
            }
            private set
            {
                cashOutOfferable = value;
            }
        }

        /// <summary>
        /// The flag indicates whether or not the player requested transfers of funds
        /// from the Player Bank Meter to the Player Wagerable Meter are currently
        /// allowed.
        /// </summary>
        [AsynchronousGameService]
        public bool IsBankToWagerableOfferable
        {
            get
            {
                if(!isBankToWagerableOfferable.HasValue)
                {
                    isBankToWagerableOfferable = cachedGameLib.IsBankToWagerableOfferable;
                }

                return isBankToWagerableOfferable.Value;
            }
            private set
            {
                isBankToWagerableOfferable = value;
            }
        }

        /// <summary>
        /// The flag indicates whether the player wager is offerable and that a bet can be made.
        /// Player wager can be unavailable due to the bank being locked or other such conditions. 
        /// </summary>
        /// <remarks>
        /// Cached the value to avoid querying the Foundation if it is not necessary.
        /// </remarks>
        [AsynchronousGameService]
        public bool IsPlayerWagerOfferable
        {
            get
            {
                if (!isPlayerWagerOfferable.HasValue)
                {
                    isPlayerWagerOfferable = cachedGameLib.IsPlayerWagerOfferable;
                }

                return isPlayerWagerOfferable.Value;
            }
            private set
            {
                isPlayerWagerOfferable = value;
            }
        }

        /// <summary>
        /// This service indicates when money is added onto the machine and provides
        /// the amount added and money in source.
        /// </summary>
        /// <remarks>
        /// Each Money In service should be consumed only once. Before using the payload,
        /// be sure to check it is not null and whether it has a new session id.
        /// </remarks>
        [AsynchronousGameService]
        public MoneyInInformation MoneyIn
        {
            get;
            private set;
        }

        /// <summary>
        /// This service indicates when money is taken away from the machine and provides
        /// the amount taken away and the money out source.
        /// </summary>
        /// <remarks>
        /// Each Money Out service should be consumed only once. Before using the payload,
        /// be sure to check it is not null and whether it has a new session id.
        /// </remarks>
        [AsynchronousGameService]
        public MoneyOutInformation MoneyOut
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Handler for bank status events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="eventArgs"><see cref="BankStatusChangedEventArgs"/> containing the current bank status.</param>
        private void HandleBankStatusChangedEvent(object sender, BankStatusChangedEventArgs eventArgs)
        {
            if(CashOutOfferable != eventArgs.Status.IsCashOutOfferable)
            {
                CashOutOfferable = eventArgs.Status.IsCashOutOfferable;
                ServiceUpdated("CashOutOfferable");
            }

            if(IsBankToWagerableOfferable != eventArgs.Status.IsBankToWagerableOfferable)
            {
                IsBankToWagerableOfferable = eventArgs.Status.IsBankToWagerableOfferable;
                ServiceUpdated("IsBankToWagerableOfferable");
            }

            if(IsPlayerWagerOfferable != eventArgs.Status.IsPlayerWagerOfferable)
            {
                IsPlayerWagerOfferable = eventArgs.Status.IsPlayerWagerOfferable;
                ServiceUpdated("IsPlayerWagerOfferable");
            }
        }

        /// <summary>
        /// Handler for money events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="moneyEvent"><see cref="MoneyEventArgs"/> containing details of the money movement.</param>
        private void HandleMoneyEvent(object sender, MoneyEventArgs moneyEvent)
        {
            var amountValue = Utility.ConvertToCents(moneyEvent.Value, moneyEvent.Denomination);

            switch (moneyEvent.Type)
            {
                case MoneyEventType.MoneyIn:
                    {
                        MoneyIn = new MoneyInInformation(moneyEvent.MoneyInSource, amountValue, UniqueIdentifier.New());
                        ServiceUpdated("MoneyIn");
                    }
                    break;

                case MoneyEventType.MoneyOut:
                    {
                        MoneyOut = new MoneyOutInformation(moneyEvent.MoneyOutSource, amountValue, UniqueIdentifier.New());
                        ServiceUpdated("MoneyOut");
                    }
                    break;
            }
        }

        /// <summary>
        /// Calls the asynchronous provider changed event with the specified service name.
        /// </summary>
        /// <param name="serviceName">The name of the service that has been updated.</param>
        private void ServiceUpdated(string serviceName)
        {
            var handler = AsynchronousProviderChanged;
            if(handler != null)
            {
                handler(this, new AsynchronousProviderChangedEventArgs(serviceName));
            }
        }

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            cachedGameLib.MoneyEvent -= HandleMoneyEvent;
            cachedGameLib.BankStatusChangedEvent -= HandleBankStatusChangedEvent;
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion
    }
}
