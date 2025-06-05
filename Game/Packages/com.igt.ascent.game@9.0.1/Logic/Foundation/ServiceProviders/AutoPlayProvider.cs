//-----------------------------------------------------------------------
// <copyright file = "AutoPlayProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// This class provides auto play information. 
    /// </summary>
    public class AutoPlayProvider: INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        /// <summary>
        /// Auto Play Provider Constructor.
        /// </summary>
        /// <param name="iGameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when gameLib is null.</exception>
        public AutoPlayProvider (IGameLib iGameLib)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException("iGameLib", "Parameter cannot be null.");
            }

            gameLib = iGameLib;

            gameLib.BankStatusChangedEvent += OnBankStatusChangedEvent;
            gameLib.MoneyEvent += OnMoneyEvent;
            gameLib.AutoPlayOnRequestEvent += OnAutoPlayOnRequestEvent;
            gameLib.AutoPlayOffEvent += OnAutoPlayOffEvent;
        }

        #region Public Properties

        /// <summary>
        /// The last committed bet amount in game credits, which is used to check 
        /// if there is enough money to start the auto play.
        /// </summary>
        public long CurrentBet { get; set; }

        #endregion

        #region Game Service

        /// <summary>
        /// Get the auto play information for game service.
        /// </summary>
        /// <returns>The auto play information.</returns>
        /// <remarks>The provided AutoPlayInformation is cached for subsequent updates.</remarks>
        [AsynchronousGameService]
        public AutoPlayInformation GetAutoPlayInformation()
        {
            UpdateAutoPlayInformationCache();
            return currentAutoPlayInformation;
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region Private Methods

        /// <summary>
        /// Update the cached fields <see cref="currentAutoPlayInformation"/> and <see cref="isInitiatedByFoundation"/>
        /// for autoplay information with the autoplay information retrieved from GameLib <see cref="GetCurrentAutoPlayInformation"/>.
        /// </summary>
        private void UpdateAutoPlayInformationCache()
        {
            currentAutoPlayInformation = GetCurrentAutoPlayInformation();
            if(!currentAutoPlayInformation.CanCommitBet)
            {
                // Stop the foundation initiated auto play if not enough money.
                isInitiatedByFoundation = false;
            }
        }

        /// <summary>
        /// Get current autoplay information from Gamelib.
        /// </summary>
        /// <returns>AutoPlayInformation.</returns>
        private AutoPlayInformation GetCurrentAutoPlayInformation()
        {
            if(gameLib.GameContextMode == GameMode.Utility)
            {
                // Return default information in utility mode to prevent a crash from the F2L.
                // F2L calls to access the auto play information are not allowed in utility mode.
                return new AutoPlayInformation();
            }

            var canCommitBet = CurrentBet >= GetCommittedCredits() &&
                               CurrentBet <= gameLib.GameMaxBet &&
                               CurrentBet >= gameLib.GameMinBet &&
                               CanCommitBet(gameLib.QueryGameCycleState());

            return new AutoPlayInformation
            {
                CanPlayerInitiate = gameLib.IsPlayerAutoPlayEnabled,
                CanCommitBet = canCommitBet,
                IsOn = (canCommitBet && isInitiatedByFoundation) || gameLib.IsAutoPlayOn(),
                IsConfirmationRequired = gameLib.IsAutoPlayConfirmationRequired,
                IsSpeedIncreaseAllowed = gameLib.IsAutoPlaySpeedIncreaseAllowed
            };
        }

        /// <summary>
        /// Send a async data change to for auto play information if it is not the same.
        /// </summary>
        private void SendAsyncDataChange()
        {
            var asyncHandler = AsynchronousProviderChanged;

            if(asyncHandler != null)
            {
                var previousAutoPlayInformation = currentAutoPlayInformation;
                UpdateAutoPlayInformationCache();
                if(previousAutoPlayInformation != currentAutoPlayInformation)
                {
                    asyncHandler(this, new AsynchronousProviderChangedEventArgs("GetAutoPlayInformation"));
                }
            }
        }

        /// <summary>
        /// Event handler for bank status events from the foundation. This handler is used to raise the
        /// AsynchronousProviderChanged event, so that the auto play controller can update the auto play
        /// buttons status.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="bankStatusEvent"><see cref="BankStatusChangedEventArgs" /> containing details of the bank status event.</param>
        /// <remarks> 
        /// This needs to subscribe to the bank status changed event because it's possible to have the bank locked and
        /// unable to commit a bet at this time. After the bank status becomes unlocked auto play is then allowed to commit.
        /// </remarks>
        private void OnBankStatusChangedEvent(object sender, BankStatusChangedEventArgs bankStatusEvent)
        {
            SendAsyncDataChange();
        }

        /// <summary>
        /// Event handler for money events from the foundation. This handler is used to raise the
        /// AsynchronousProviderChanged event, so that the auto play controller can update the auto play
        /// buttons status.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="moneyEvent"><see cref="MoneyEventArgs" /> containing details of the money event.</param>
        /// <devdoc>
        /// Currently all money events trigger an update; they are not posted very often, so this does not pose a
        /// problem. If the number of situations which result in money events increases, then this function should
        /// be modified to only post a AsynchronousProviderChanged event when the auto play state change.
        /// </devdoc>
        private void OnMoneyEvent(object sender, MoneyEventArgs moneyEvent)
        {
            SendAsyncDataChange();
        }

        /// <summary>
        /// Event handler for the foundation request to turn on the auto play.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="autoPlayOnEvent">The <see cref="AutoPlayOnRequestEventArgs" /> to handle.</param>
        /// <remarks>
        /// autoPlayOnEvent.RequestAccepted is used later in the call stack to see if auto play should be started.
        /// </remarks>
        private void OnAutoPlayOnRequestEvent(object sender, AutoPlayOnRequestEventArgs autoPlayOnEvent)
        {
            var canStartAutoPlay = !gameLib.IsPlayerAutoPlayEnabled;
            var gameCycleState = gameLib.QueryGameCycleState();

            // In Idle or MainPlayComplete state, we can be certain to accept or reject the request.
            // For other states, we have to wait till next game cycle to decide.
            if(canStartAutoPlay && 
                (gameCycleState == GameCycleState.Idle || 
                 gameCycleState == GameCycleState.MainPlayComplete))
            {
                canStartAutoPlay = CanCommitBet(gameCycleState);
            }

            if(canStartAutoPlay)
            {
                isInitiatedByFoundation = true;
                SendAsyncDataChange();
            }

            autoPlayOnEvent.RequestAccepted = canStartAutoPlay;
        }

        /// <summary>
        /// Event handler for the foundation notification to turn off the auto play.
        /// </summary>
        /// <param name="sender">The originator of the event.</param>
        /// <param name="autoPlayOffEvent">The <see cref="AutoPlayOffEventArgs" /> to handle.</param>
        private void OnAutoPlayOffEvent(object sender, AutoPlayOffEventArgs autoPlayOffEvent)
        {
            isInitiatedByFoundation = false;
            SendAsyncDataChange();
        }

        /// <summary>
        /// Check if the current bet can be committed.
        /// </summary>
        /// <returns>True if the current bet can be committed. Otherwise false.</returns>
        private bool CanCommitBet(GameCycleState gameCycleState)
        {
            var canBet = false;
            if(CurrentBet > 0)
            {
                switch (gameCycleState)
                {
                    case GameCycleState.Idle:
                    case GameCycleState.Committed:
                        canBet = gameLib.CanCommitBet(CurrentBet, gameLib.GameDenomination);
                        break;
                    case GameCycleState.MainPlayComplete:
                        canBet = gameLib.CanBetNextGameCycle(CurrentBet, gameLib.GameDenomination);
                        break;
                }
            }
            return canBet;
        }

        /// <summary>
        /// Queries the foundation for the current committed credit amount.
        /// </summary>
        /// <returns>The current committed bet, in game credits.</returns>
        private long GetCommittedCredits()
        {
            long bet, denom;
            gameLib.GetCommittedBet(out bet, out denom);
            checked
            {
                return bet * denom / gameLib.GameDenomination;
            }
        }

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            gameLib.BankStatusChangedEvent -= OnBankStatusChangedEvent;
            iGameLib.MoneyEvent -= OnMoneyEvent;
            iGameLib.AutoPlayOnRequestEvent -= OnAutoPlayOnRequestEvent;
            iGameLib.AutoPlayOffEvent -= OnAutoPlayOffEvent;
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// The reference to GameLib which is used to communicate with the foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Flag indicating if the auto play has been turned on by the foundation.
        /// </summary>
        /// <remarks>
        /// This flag is for the case where AutoPlayOnRequest reply arrives at the foundation 
        /// after the IsAutoPlayOn query, which is triggered by the async game service update.
        /// </remarks>
        private bool isInitiatedByFoundation;

        /// <summary>
        /// Store the current auto play information provided.
        /// </summary>
        /// <remarks>
        /// This is used for checking if the data being sent to the async handler is the same. The value must be updated any
        /// time, AutoPlayInformation is provided to the presentation to make sure that subsequent updates are not skipped.
        /// </remarks>
        private AutoPlayInformation currentAutoPlayInformation;

        #endregion
    }
}
