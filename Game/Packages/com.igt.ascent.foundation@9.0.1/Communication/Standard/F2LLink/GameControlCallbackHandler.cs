//-----------------------------------------------------------------------
// <copyright file = "GameControlCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standard.F2LLink
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;
    using F2L;
    using F2XCallbacks;
    using F2XTransport;
    using F2LInternal = F2L.Schemas.Internal;

    /// <summary>
    /// This class is responsible for handling callbacks from the game control category.
    /// </summary>
    class GameControlCallbackHandler : IGameControlCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactions.
        /// </summary>
        private readonly ITransactionCallbacks transactionCallbackInterface;

        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbackInterface;

        /// <summary>
        /// Initialize an instance of <see cref="GameControlCallbackHandler"/>.
        /// </summary>
        /// <param name="transactionCallbacks">The callback interface for the handling transactions</param>
        /// <param name="eventEventCallbacks">The callback interface for the handling events</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="transactionCallbacks"/> or <paramref name="eventEventCallbacks"/> is null.
        /// </exception>
        public GameControlCallbackHandler(ITransactionCallbacks transactionCallbacks,
                                          IEventCallbacks eventEventCallbacks)
        {
            if(transactionCallbacks == null)
            {
                throw new ArgumentNullException("transactionCallbacks");
            }

            if(eventEventCallbacks == null)
            {
                throw new ArgumentNullException("eventEventCallbacks");
            }

            transactionCallbackInterface = transactionCallbacks;
            eventCallbackInterface = eventEventCallbacks;
        }

        #region IGameCategoryCallbacks

        /// <summary>
        /// Information for the next session.
        /// </summary>
        private ThemeContext nextSession;

        /// <inheritdoc />
        public void ProcessActivateThemeContext()
        {
            eventCallbackInterface.PostEvent(new ActivateThemeContextEventArgs(nextSession));
        }

        /// <inheritdoc />
        public void ProcessInactivateThemeContext()
        {
            eventCallbackInterface.PostEvent(new InactivateThemeContextEventArgs());
        }

        /// <inheritdoc />
        public void ProcessNewThemeContext(F2LInternal.GameMode gameMode, F2LInternal.GameSubMode gameSubMode, uint denomination,
                                           string paytable, string paytableFile, bool newlySelectedForPlay)
        {
            if(gameMode == F2LInternal.GameMode.Invalid)
            {
                throw new InvalidGameModeException("Foundation attempted to set the GameMode to GameMode.Invalid");
            }

            nextSession = new ThemeContext((GameMode)gameMode, denomination, paytable, paytableFile,
                                           newlySelectedForPlay, (GameSubMode)gameSubMode);
            eventCallbackInterface.PostEvent(new NewThemeContextEventArgs(nextSession));
        }

        /// <inheritdoc />
        public void ProcessSwitchThemeContext(string themeTag, string themeTagDataFile,
                                              IEnumerable<F2LInternal.GameControlSwitchThemeContextSendResourcePaths> resourcePaths,
                                              uint denom, string payvarTag, string payvarTagDataFile)

        {
            nextSession = new ThemeContext(nextSession.GameContextMode, denom, payvarTag, payvarTagDataFile,
                                           true, nextSession.GameSubMode);

            // Manually instantiating a dictionary and adding data via [key]=value instead of ToDictionary
            // to prevent possible same key System.ArgumentExceptions from being thrown.
            var convertedResourcePaths = new Dictionary<string, string>();
            foreach(var resourcePath in resourcePaths)
            {
                convertedResourcePaths[resourcePath.Tag] = resourcePath.Path;
            }

            eventCallbackInterface.PostEvent(new SwitchThemeContextEventArgs(themeTag, themeTagDataFile,
                                                                             convertedResourcePaths, denom,
                                                                             payvarTag, payvarTagDataFile));
        }

        /// <inheritdoc />
        public void ProcessActionResponse(byte[] payload)
        {
            transactionCallbackInterface.ProcessActionResponse(payload);
        }

        /// <inheritdoc />
        public void ProcessEnrollResponse(bool enrollSuccess, byte[] enrollmentData)
        {
            eventCallbackInterface.PostEvent(new EnrollResponseEventArgs(enrollSuccess, enrollmentData));
        }

        /// <inheritdoc />
        public void ProcessEvalOutcomeResponse(IOutcomeList outcomeList, bool isFinal)
        {
            eventCallbackInterface.PostEvent(new OutcomeResponseEventArgs(outcomeList, isFinal));
        }

        /// <inheritdoc />
        public void ProcessFinalizeAwardResponse()
        {
            eventCallbackInterface.PostEvent(new FinalizeOutcomeEventArgs());
        }

        /// <inheritdoc />
        public void ProcessSetDisplayControlState(F2LInternal.DisplayControlState controlState)
        {
            eventCallbackInterface.PostEvent(new DisplayControlEventArgs((DisplayControlState)controlState));
        }

        /// <inheritdoc />
        public void ProcessMoneyWagerable(long amountMoved, string wagerableDirection, F2LInternal.PlayerMeters playerMeters)
        {
            //The amount moved is in base units, should it be converted to an amount with a denom, or is this fine?
            eventCallbackInterface.PostEvent(new MoneyEventArgs(MoneyEventType.MoneyWagerable, amountMoved, 1,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessMoneyBet(long amountBet, F2LInternal.PlayerMeters playerMeters)
        {
            eventCallbackInterface.PostEvent(new MoneyEventArgs(MoneyEventType.MoneyBet, amountBet, 1,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessMoneyCommittedChanged(long committedBet, F2LInternal.PlayerMeters playerMeters)
        {
            eventCallbackInterface.PostEvent(new MoneyEventArgs(MoneyEventType.MoneyCommittedChanged, committedBet, 1,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessMoneyIn(long amountIn,
                                   F2LInternal.MoneyInSource moneyInSource,
                                   F2LInternal.FundsTransferAccountSource? fundsTransferAccountSource,
                                   F2LInternal.PlayerMeters playerMeters)
        {
            if(fundsTransferAccountSource != null)
            {
                eventCallbackInterface.PostEvent(new MoneyEventArgs((MoneyInSource)moneyInSource,
                                                                    (FundsTransferAccountSource)fundsTransferAccountSource,
                                                                    amountIn,
                                                                    1,
                                                                    playerMeters.ToPublic()));
            }
            else
            {
                eventCallbackInterface.PostEvent(new MoneyEventArgs((MoneyInSource)moneyInSource,
                                                                    amountIn,
                                                                    1,
                                                                    playerMeters.ToPublic()));
            }
        }

        /// <inheritdoc />
        public void ProcessMoneyOut(long amountOut, F2LInternal.MoneyOutSource moneyOutSource, F2LInternal.PlayerMeters playerMeters)
        {
            eventCallbackInterface.PostEvent(new MoneyEventArgs((MoneyOutSource)moneyOutSource, amountOut, 1,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessMoneyWon(long amountWon, F2LInternal.PlayerMeters playerMeters)
        {
            eventCallbackInterface.PostEvent(new MoneyEventArgs(MoneyEventType.MoneyWon, amountWon, 1,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessMoneySet(F2LInternal.PlayerMeters playerMeters)
        {
            eventCallbackInterface.PostEvent(new MoneyEventArgs(MoneyEventType.MoneySet,
                                                                playerMeters.ToPublic()));
        }

        /// <inheritdoc />
        public void ProcessBankStatusChanged(F2LInternal.BankStatus bankStatus)
        {
            // F2L message keeps the name of BankLocked for sake of 
            // backward compatibility. IsPlayerWagerOfferable should be the opposite of isBankLocked.
            eventCallbackInterface.PostEvent(
                new BankStatusChangedEventArgs(new BankStatus(!bankStatus.BankLocked, bankStatus.PlayerCashoutOfferable, bankStatus.BankToWagerableOfferable)));
        }

        /// <inheritdoc />
        public void ProcessThemeSelectionMenuOfferableStatusChanged(bool offerable)
        {
            eventCallbackInterface.PostEvent(new ThemeSelectionMenuOfferableStatusChangedEventArgs(offerable));
        }

        /// <inheritdoc />
        public void ProcessDisableAncillaryGameOffer()
        {
            eventCallbackInterface.PostEvent(new DisableAncillaryGameOfferEventArgs());
        }

        /// <inheritdoc />
        public void ProcessCultureChanged(string culture)
        {
            eventCallbackInterface.PostEvent(new LanguageChangedEventArgs(culture));
        }

        /// <inheritdoc />
        public void ProcessDenominationChangeCancelled()
        {
            eventCallbackInterface.PostEvent(new DenominationChangeCancelledEventArgs());
        }

        #endregion
    }
}
