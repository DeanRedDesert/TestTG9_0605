//-----------------------------------------------------------------------
// <copyright file = "UcGameControlCategory.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;
    using F2XTransport;
    using Transport;
    using F2LInternal = Schemas.Internal;

    /// <summary>
    /// Implementation of the F2L game control API category for Universal Controller.
    /// </summary>
    public sealed class UcGameControlCategory : F2LTransactionalCategoryBase<F2LInternal.GameControl>,
                                              IGameControlCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the game control category callbacks.
        /// </summary>
        private readonly IGameControlCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the game control category.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        /// <param name="callbackHandler">Game control category callback handler.</param>
        /// <exception cref="ArgumentNullException">Thrown when the callback handler is null.</exception>
        public UcGameControlCategory(IF2XTransport transport, IGameControlCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler), "Argument may not be null.");

            ConfigureHandlers();
        }

        /// <summary>
        /// Configure the handler table for all game control category messages which can be received.
        /// </summary>
        private void ConfigureHandlers()
        {
            AddMessagehandler<F2LInternal.GameControlSetDisplayControlStateSend>(HandleGameControlSetDisplayControlStateSend);
            AddMessagehandler<F2LInternal.GameControlTransactionCompleteSend>(HandleGameControlTransactionCompleteSend);
            AddMessagehandler<F2LInternal.GameControlNewThemeContextSend>(HandleGameControlNewThemeContextSend);
            AddMessagehandler<F2LInternal.GameControlActivateThemeContextSend>(HandleGameControlActivateThemeContextSend);
            AddMessagehandler<F2LInternal.GameControlActionResponseSend>(HandleGameControlActionResponseSend);
            AddMessagehandler<F2LInternal.GameControlInactivateThemeContextSend>(HandleGameControlInactivateThemeContextSend);
            AddMessagehandler<F2LInternal.GameControlEnrollResponseSend>(HandleGameControlEnrollResponseSend);
            AddMessagehandler<F2LInternal.GameControlEvalOutcomeResponseSend>(HandleGameControlEvalOutcomeResponseSend);
            AddMessagehandler<F2LInternal.GameControlFinalizeAwardResponseSend>(HandleGameControlFinalizeAwardResponseSend);
            AddMessagehandler<F2LInternal.GameControlMoneyWagerableSend>(HandleGameControlMoneyWagerableSend);
            AddMessagehandler<F2LInternal.GameControlMoneyBetSend>(HandleGameControlMoneyBetSend);
            AddMessagehandler<F2LInternal.GameControlMoneyCommittedChangedSend>(HandleGameControlMoneyCommittedChangedSend);
            AddMessagehandler<F2LInternal.GameControlMoneyOutSend>(HandleGameControlMoneyOutSend);
            AddMessagehandler<F2LInternal.GameControlMoneyWonSend>(HandleGameControlMoneyWonSend);
            AddMessagehandler<F2LInternal.GameControlMoneySetSend>(HandleGameControlMoneySetSend);
            AddMessagehandler<F2LInternal.GameControlMoneyInSend>(HandleGameControlMoneyInSend);
            AddMessagehandler<F2LInternal.GameControlBankStatusChangedSend>(HandleGameControlBankStatusChangedSend);
            AddMessagehandler<F2LInternal.GameControlThemeSelectionMenuOfferableStatusChangedSend>(
                HandleGameControlThemeSelectionMenuOfferableStatusChangedSend);
            AddMessagehandler<F2LInternal.GameControlDisableAncillaryGameOfferSend>(
                HandleGameControlDisableAncillaryGameOfferSend);
            AddMessagehandler<F2LInternal.GameControlCultureChangedSend>(HandleGameControlCultureChangedSend);
            AddMessagehandler<F2LInternal.GameControlDenominationChangeCancelledSend>(HandleGameControlDenominationCancelledSend);
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion => 4;

        /// <inheritdoc />
        public override uint MinorVersion => 1;

        /// <inheritdoc />
        public override MessageCategory Category => MessageCategory.GameControl;

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the GameControlSetDisplayControlStateSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlSetDisplayControlStateSend(F2LInternal.GameControlSetDisplayControlStateSend message)
        {
            callbackHandler.ProcessSetDisplayControlState(message.DisplayControlState);
            var reply = CreateReply<F2LInternal.GameControlSetDisplayControlStateReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlTransactionCompleteSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlTransactionCompleteSend(F2LInternal.GameControlTransactionCompleteSend message)
        {
            var reply = CreateReply<F2LInternal.GameControlTransactionCompleteReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlNewThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlNewThemeContextSend(F2LInternal.GameControlNewThemeContextSend message)
        {
            // The GameSubMode defaults to Standard for games running on the UC platform.
            callbackHandler.ProcessNewThemeContext(message.GameMode,
                F2LInternal.GameSubMode.Standard, message.Denom,
                message.PayvarTag, message.PayvarTagDataFile,
                message.NewlySelectedForPlay);
            var reply = CreateReply<F2LInternal.GameControlNewThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlActivateThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlActivateThemeContextSend(F2LInternal.GameControlActivateThemeContextSend message)
        {
            callbackHandler.ProcessActivateThemeContext();

            var reply = CreateReply<F2LInternal.GameControlActivateThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlActionResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlActionResponseSend(F2LInternal.GameControlActionResponseSend message)
        {
            callbackHandler.ProcessActionResponse(message.Payload);

            var reply = CreateReply<F2LInternal.GameControlActionResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlInactivateThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlInactivateThemeContextSend(F2LInternal.GameControlInactivateThemeContextSend message)
        {
            callbackHandler.ProcessInactivateThemeContext();
            var reply = CreateReply<F2LInternal.GameControlInactivateThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handle the GameControlEnrollResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlEnrollResponseSend(F2LInternal.GameControlEnrollResponseSend message)
        {
            callbackHandler.ProcessEnrollResponse(message.EnrollSuccess, message.HostEnrollmentData);
            var reply = CreateReply<F2LInternal.GameControlEnrollResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlEvalOutcomeResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlEvalOutcomeResponseSend(F2LInternal.GameControlEvalOutcomeResponseSend message)
        {
            callbackHandler.ProcessEvalOutcomeResponse(message.OutcomeList, message.IsPlayComplete);
            var reply = CreateReply<F2LInternal.GameControlEvalOutcomeResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlFinalizeAwardResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlFinalizeAwardResponseSend(F2LInternal.GameControlFinalizeAwardResponseSend message)
        {
            callbackHandler.ProcessFinalizeAwardResponse();
            var reply = CreateReply<F2LInternal.GameControlFinalizeAwardResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyWagerableSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyWagerableSend(F2LInternal.GameControlMoneyWagerableSend message)
        {
            callbackHandler.ProcessMoneyWagerable(message.AmountMoved, message.WagerableDirection.ToString(),
                                           message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyWagerableReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyBetSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyBetSend(F2LInternal.GameControlMoneyBetSend message)
        {
            callbackHandler.ProcessMoneyBet(message.AmountBet, message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyBetReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlMoneyCommittedChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyCommittedChangedSend(F2LInternal.GameControlMoneyCommittedChangedSend message)
        {
            callbackHandler.ProcessMoneyCommittedChanged(message.AmountCommitted, message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyCommittedChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyInSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyInSend(F2LInternal.GameControlMoneyInSend message)
        {
            F2LInternal.FundsTransferAccountSource? transferSource = null;
            if(message.FundsTransferAccountSourceSpecified)
            {
                transferSource = message.FundsTransferAccountSource;
            }
            callbackHandler.ProcessMoneyIn(message.AmountIn,
                                           message.MoneyInSource,
                                           transferSource,
                                           message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyInReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyOutSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyOutSend(F2LInternal.GameControlMoneyOutSend message)
        {
            callbackHandler.ProcessMoneyOut(message.AmountOut, message.MoneyOutSource, message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyOutReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyWonSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyWonSend(F2LInternal.GameControlMoneyWonSend message)
        {
            callbackHandler.ProcessMoneyWon(message.AmountWon, message.PlayerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneyWonReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneySetSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneySetSend(F2LInternal.GameControlMoneySetSend message)
        {
            var playerMeters = message.PlayerMeters;

            callbackHandler.ProcessMoneySet(playerMeters);
            var reply = CreateReply<F2LInternal.GameControlMoneySetReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlBankStatusChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlBankStatusChangedSend(F2LInternal.GameControlBankStatusChangedSend message)
        {
            var bankStatus = message.BankStatus;

            callbackHandler.ProcessBankStatusChanged(bankStatus);
            var reply = CreateReply<F2LInternal.GameControlBankStatusChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlThemeSelectionMenuOfferableStatusChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlThemeSelectionMenuOfferableStatusChangedSend(
            F2LInternal.GameControlThemeSelectionMenuOfferableStatusChangedSend message)
        {
            callbackHandler.ProcessThemeSelectionMenuOfferableStatusChanged(message.IsThemeSelectionMenuOfferable);
            var reply = CreateReply<F2LInternal.GameControlThemeSelectionMenuOfferableStatusChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlDisableAncillaryGameOfferSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlDisableAncillaryGameOfferSend(
            F2LInternal.GameControlDisableAncillaryGameOfferSend message)
        {
            callbackHandler.ProcessDisableAncillaryGameOffer();
            var reply = CreateReply<F2LInternal.GameControlDisableAncillaryGameOfferReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlCultureChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlCultureChangedSend(F2LInternal.GameControlCultureChangedSend message)
        {
            callbackHandler.ProcessCultureChanged(message.Culture);
            var reply = CreateReply<F2LInternal.GameControlCultureChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlDenominationChangeCancelledSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlDenominationCancelledSend(F2LInternal.GameControlDenominationChangeCancelledSend message)
        {
            callbackHandler.ProcessDenominationChangeCancelled();
            var reply = CreateReply<F2LInternal.GameControlDenominationChangeCancelledReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IGameControlCategory Members

        /// <inheritdoc />
        public bool ActionRequest(byte[] payload)
        {
            var request = CreateBasicRequest<F2LInternal.GameControlActionRequestSend>();
            var actionRequest = (F2LInternal.GameControlActionRequestSend)request.Message.Item;
            actionRequest.Payload = payload;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlActionRequestReply>(Channel.Game, request);

            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public bool CanCommitBet(long betAmount)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCanCommitBetSend>();
            var content = (F2LInternal.GameControlCanCommitBetSend)request.Message.Item;
            content.BetAmount = betAmount;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanCommitBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitAllowed;
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanCommitBets(IEnumerable<long> bets)
        {
            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            var request = CreateTransactionalRequest<F2LInternal.GameControlCanCommitBetsSend>();
            var content = (F2LInternal.GameControlCanCommitBetsSend)request.Message.Item;
            content.BetAmount = (from bet in bets select new F2LInternal.AmountType(bet)).ToList();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanCommitBetsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitAllowed;
        }

        /// <inheritdoc />
        public bool CommitBet(long betAmount)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCommitBetSend>();
            var content = (F2LInternal.GameControlCommitBetSend)request.Message.Item;
            content.BetAmount = betAmount;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCommitBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetCommitted;
        }

        /// <inheritdoc />
        public long GetCommittedBet()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCommittedBetSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCommittedBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommittedBetAmount;
        }

        /// <inheritdoc />
        public void UncommitBet()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlUncommitBetSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlUncommitBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void PlaceStartingBet(bool maxBet)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlPlaceStartingBetSend>();
            var content = (F2LInternal.GameControlPlaceStartingBetSend)request.Message.Item;
            content.IsMaxBet = maxBet;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlPlaceStartingBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool CanPlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                                long pendingWinsAvailableForBet)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCanPlaceBetSend>();
            var content = (F2LInternal.GameControlCanPlaceBetSend)request.Message.Item;

            content.BetAmount = betAmount;
            content.BetFromCreditsAmount = betFromCredits;
            content.BetFromUncommittedGameWinsAmount = betFromPendingWins;
            content.PendingWinsAvailableForThisBet = pendingWinsAvailableForBet;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanPlaceBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetAllowed;
        }

        /// <inheritdoc />
        public bool PlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                             long pendingWinsAvailableForBet)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlPlaceBetSend>();
            var content = (F2LInternal.GameControlPlaceBetSend)request.Message.Item;
            content.BetAmount = betAmount;
            content.BetFromCreditsAmount = betFromCredits;
            content.BetFromUncommittedGameWinsAmount = betFromPendingWins;
            content.PendingWinsAvailableForThisBet = pendingWinsAvailableForBet;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlPlaceBetReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets)
        {
            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            var request = CreateTransactionalRequest<F2LInternal.GameControlCanBetNextGameCycleSend>();
            var content = (F2LInternal.GameControlCanBetNextGameCycleSend)request.Message.Item;
            content.BetAmount = (from bet in bets select new F2LInternal.AmountType(bet)).ToList();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanBetNextGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetAllowed;
        }

        /// <inheritdoc />
        public F2LInternal.PlayerMeters GetPlayerMeters()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetPlayerMetersSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetPlayerMetersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.PlayerMeters;
        }

        /// <inheritdoc />
        public F2LInternal.BankStatus QueryBankStatus()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlQueryBankStatusSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlQueryBankStatusReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BankStatus;
        }

        /// <inheritdoc />
        public bool IsPlayerWagerOfferable()
        {
            // F2L message keeps the name of BankLocked for sake of 
            // backward compatibility.IsPlayerWagerOfferable should be the opposite of isBankLocked.
            var request = CreateTransactionalRequest<F2LInternal.GameControlIsBankLockedSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlIsBankLockedReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return !reply.IsBankLocked;
        }

        /// <inheritdoc />
        public bool IsPlayerCashoutOfferable()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlIsPlayerCashoutOfferableSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlIsPlayerCashoutOfferableReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.IsPlayerCashoutOfferable;
        }

        /// <inheritdoc />
        public bool IsPlayerBankToWagerableOfferable()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlIsPlayerBankToWagerableOfferableSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlIsPlayerBankToWagerableOfferableReply>(Channel.Foundation,
                                                                                                 request);
            CheckReply(reply.Reply);

            return reply.IsPlayerBankToWagerableOfferable;
        }

        /// <inheritdoc />
        public void PlayerCashoutRequest()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlPlayerCashoutRequestSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlPlayerCashoutRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool TransferWagerableToBankRequest()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlTransferWagerableToBankRequestSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlTransferWagerableToBankRequestReply>(Channel.Foundation,
                                                                                               request);
            CheckReply(reply.Reply);

            return reply.TransferAccepted;
        }

        /// <inheritdoc />
        public void TransferBankToWagerableRequest()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlTransferBankToWagerableRequestSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlTransferBankToWagerableRequestReply>(Channel.Foundation,
                                                                                               request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public F2LInternal.GameCycleState QueryGameCycleState()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlQueryGameCycleStateSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlQueryGameCycleStateReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CurrentGameCycleState;
        }

        /// <inheritdoc />
        public bool CommitGameCycle()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCommitGameCycleSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCommitGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitSuccessful;
        }

        /// <inheritdoc />
        public bool CanCommitGameCycle()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCanCommitGameCycleSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanCommitGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitAllowed;
        }

        /// <inheritdoc />
        public bool UncommitGameCycle()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlUncommitGameCycleSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlUncommitGameCycleReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public void EnrollGameCycle(byte[] enrollmentData)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlEnrollGameCycleSend>();
            var enrollContent = (F2LInternal.GameControlEnrollGameCycleSend)request.Message.Item;
            enrollContent.HostEnrollmentData = enrollmentData;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlEnrollGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void UnenrollGameCycle()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlUnenrollGameCycleSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlUnenrollGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool CanStartPlaying()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlCanStartPlayingSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlCanStartPlayingReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.StartAllowed;
        }

        /// <inheritdoc />
        public bool StartPlaying()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlStartPlayingSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlStartPlayingReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public void EvalOutcomeRequest(IOutcomeList outcomeList)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlEvalOutcomeRequestSend>();
            var content = (F2LInternal.GameControlEvalOutcomeRequestSend)request.Message.Item;
            content.OutcomeList = new F2LInternal.OutcomeList(outcomeList);

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlEvalOutcomeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void LastEvalOutcomeRequest(IOutcomeList outcomeList,
                                           ReadOnlyCollection<F2LInternal.WagerCatOutcome> wagerCategoryOutcomes)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlEvalLastOutcomeRequestSend>();
            var content = (F2LInternal.GameControlEvalLastOutcomeRequestSend)request.Message.Item;

            content.OutcomeList = new F2LInternal.OutcomeList(outcomeList);
            content.WagerCatOutcomes = new F2LInternal.GameControlEvalLastOutcomeRequestSendWagerCatOutcomes
            {
                WagerCatOutcome = wagerCategoryOutcomes.ToList()
            };

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlEvalLastOutcomeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void LastEvalAncillaryOutcomeRequest(IOutcomeList outcomeList)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlEvalLastAncillaryOutcomeRequestSend>();
            var content = (F2LInternal.GameControlEvalLastAncillaryOutcomeRequestSend)request.Message.Item;
            content.OutcomeList = new F2LInternal.OutcomeList(outcomeList);

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlEvalLastAncillaryOutcomeRequestReply>(Channel.Foundation,
                                                                                                request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool OfferAncillaryGame()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlOfferAncillaryGameRequestSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlOfferAncillaryGameRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.OfferAncillaryGame;
        }

        /// <inheritdoc />
        public bool StartAncillaryPlaying()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlStartAncillaryPlayingSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlStartAncillaryPlayingReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.StartSuccessful;
        }

        /// <inheritdoc />
        public bool StartBonusPlaying()
        {
            return false;
        }

        /// <inheritdoc />
        public void LastEvalBonusOutcomeRequest(IOutcomeList outcomeList)
        {

        }

        /// <inheritdoc />
        public bool GetBonusPlayEnabled()
        {
            return false;
        }

        /// <inheritdoc />
        public void FinalizeAwardRequest()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlFinalizeAwardRequestSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlFinalizeAwardRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void EndGameCycle(uint historySteps)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlEndGameCycleSend>();
            var content = (F2LInternal.GameControlEndGameCycleSend)request.Message.Item;
            content.NumberOfSteps = historySteps;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<F2LInternal.GameControlEndGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public int GetRandomNumber(int low, int high, string rngName)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetRandomNumbersSend>();
            var content = (F2LInternal.GameControlGetRandomNumbersSend)request.Message.Item;
            content.MaximumValue = high;
            content.MinimumValue = low;
            content.Count = 1;
            content.AlreadyPickedNumbers = new F2LInternal.GameControlGetRandomNumbersSendAlreadyPickedNumbers
            {
                AlreadyPickedNumber = new List<int>()
            };
            content.Duplicates = 0;
            content.Name = rngName;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetRandomNumbersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            if(reply.RandomNumbers == null)
            {
                throw new InvalidMessageException("Random number response contained a null RandomNumbers list.");
            }

            return reply.RandomNumbers.RandomNumber[0];
        }

        /// <inheritdoc />
        public IList<int> GetRandomNumbers(uint count, int low, int high, ReadOnlyCollection<int> prePicked,
                                           uint allowedDuplicates, string rngName)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetRandomNumbersSend>();
            var content = (F2LInternal.GameControlGetRandomNumbersSend)request.Message.Item;
            content.MaximumValue = high;
            content.MinimumValue = low;
            content.Count = count;
            content.AlreadyPickedNumbers = new F2LInternal.GameControlGetRandomNumbersSendAlreadyPickedNumbers
            {
                AlreadyPickedNumber = prePicked.ToList(),
            };
            content.Duplicates = allowedDuplicates;
            content.Name = rngName;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetRandomNumbersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            if(reply.RandomNumbers == null)
            {
                throw new InvalidMessageException("Random number response contained a null RandomNumbers list.");
            }

            return reply.RandomNumbers.RandomNumber;
        }

        /// <inheritdoc />
        public IList<int> GetRandomNumbers(uint count, ReadOnlyCollection<int> lowRanges,
                                           ReadOnlyCollection<int> highRanges, string rngName)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetListLimitedRandomNumbersSend>();
            var content = (F2LInternal.GameControlGetListLimitedRandomNumbersSend)request.Message.Item;

            content.Count = count;
            content.RangeList = new List<F2LInternal.GameControlGetListLimitedRandomNumbersSendRange>();

            //The ranges should be valid from the GameLib, so this is just a sanity check.
            if(highRanges.Count != count)
            {
                throw new ArgumentException("Range list must match requested count.", nameof(highRanges));
            }

            if(lowRanges.Count != count)
            {
                throw new ArgumentException("Range list must match requested count.", nameof(lowRanges));
            }

            for(var rangeIndex = 0; rangeIndex < count; rangeIndex++)
            {
                content.RangeList.Add(new F2LInternal.GameControlGetListLimitedRandomNumbersSendRange
                { MaximumValue = highRanges[rangeIndex], MinimumValue = lowRanges[rangeIndex] });
            }

            content.Name = rngName;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetListLimitedRandomNumbersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            if(reply.RandomNumbers == null)
            {
                throw new InvalidMessageException("Random number response contained a null RandomNumbers list.");
            }

            return reply.RandomNumbers.RandomNumber;
        }

        /// <inheritdoc />
        public void WriteCriticalData(F2LInternal.CriticalDataScope scope, string itemName, byte[] data)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlWriteCritDataSend>();
            var content = (F2LInternal.GameControlWriteCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;
            content.Data = data;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlWriteCritDataReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public byte[] ReadCriticalData(F2LInternal.CriticalDataScope scope, string itemName)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlReadCritDataSend>();
            var content = (F2LInternal.GameControlReadCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlReadCritDataReply>(Channel.Foundation, request);

            CheckReply(reply.Reply);

            return reply.ReadCritDataSuccess ? reply.Data : new byte[0];
        }

        /// <inheritdoc />
        public bool RemoveCriticalData(F2LInternal.CriticalDataScope scope, string itemName)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlRemoveCritDataSend>();
            var content = (F2LInternal.GameControlRemoveCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlRemoveCritDataReply>(Channel.Foundation, request);

            CheckReply(reply.Reply);

            return reply.RemoveCritDataSuccess;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemReferencedEnumeration(string name, F2LInternal.CustomConfigItemScope scope,
            IList<string> values)
        {
            return false;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemAmount(string name, F2LInternal.CustomConfigItemScope scope, out long value,
                                              out long lowRange, out long highRange)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Amount;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var amount = (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemAmountData)reply.CustomConfigItem.Item;
                value = amount.Value;
                lowRange = amount.Min;
                highRange = amount.Max;
            }
            else
            {
                value = 0;
                lowRange = 0;
                highRange = 0;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemBoolean(string name, F2LInternal.CustomConfigItemScope scope, out bool value)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Boolean;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                value = (bool)reply.CustomConfigItem.Item;
            }
            else
            {
                value = false;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemEnumeration(string name, F2LInternal.CustomConfigItemScope scope, IList<string> values)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Enumeration;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var enumeration =
                    (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemEnumerationData)reply.CustomConfigItem.Item;

                foreach(var value in enumeration.Item)
                {
                    values.Add(value);
                }
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemFlagList(string name, F2LInternal.CustomConfigItemScope scope,
                                                IDictionary<string, bool> values)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.FlagList;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem != null)
            {
                var flagList =
                    (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemFlagListData)reply.CustomConfigItem.Item;

                foreach(var flag in flagList.FlagList)
                {
                    values.Add(flag.Item, flag.Value);
                }
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemFloat(string name, F2LInternal.CustomConfigItemScope scope, out float value)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Float;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var floatData =
                    (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemFloatData)reply.CustomConfigItem.Item;

                value = floatData.Value;
            }
            else
            {
                value = 0;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemInt64(string name, F2LInternal.CustomConfigItemScope scope, out long value)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Int64;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var int64Data =
                    (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemInt64Data)reply.CustomConfigItem.Item;

                value = int64Data.Value;
            }
            else
            {
                value = 0;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItem(string name, F2LInternal.CustomConfigItemScope scope, out string value)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.Item;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var itemData = (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemItemData)reply.CustomConfigItem.Item;

                value = itemData.Value;
            }
            else
            {
                value = null;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemString(string name, F2LInternal.CustomConfigItemScope scope, out string value)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = F2LInternal.CustomConfigItemType.String;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var itemData =
                    (F2LInternal.GameControlGetCustomConfigItemReplyCustomConfigItemStringData)reply.CustomConfigItem.Item;

                value = itemData.Value;
            }
            else
            {
                value = null;
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemType(string name, F2LInternal.CustomConfigItemScope scope, out F2LInternal.CustomConfigItemType type)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemTypeSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemTypeSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemTypeReply>(Channel.Foundation, request);
            type = reply.Type;
            CheckReply(reply.Reply);

            return type != F2LInternal.CustomConfigItemType.Invalid;
        }

        /// <inheritdoc />
        public IDictionary<string, F2LInternal.CustomConfigItemType> GetCustomConfigItemTypes(F2LInternal.CustomConfigItemScope scope)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCustomConfigItemTypesSend>();
            var content = (F2LInternal.GameControlGetCustomConfigItemTypesSend)request.Message.Item;
            content.Scope = scope;

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCustomConfigItemTypesReply>(Channel.Foundation, request);

            var types = new Dictionary<string, F2LInternal.CustomConfigItemType>();

            if(reply.CustomConfigItemTypePairs != null)
            {
                foreach(var customConfigItemPair in reply.CustomConfigItemTypePairs)
                {
                    types.Add(customConfigItemPair.Name, customConfigItemPair.Type);
                }
            }

            CheckReply(reply.Reply);

            return types;
        }

        /// <inheritdoc />
        public long GetMaxBetAmount()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataMaxBetAmountSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataMaxBetAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.MaxBetAmount;
        }

        /// <inheritdoc />
        public long GetMinBetAmount()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataMinBetAmountSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataMinBetAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.MinBetAmount;
        }

        /// <inheritdoc />
        public long GetButtonPanelMinBetAmount()
        {
            long result = 0;
            return result;
        }

        /// <inheritdoc />
        public long GetWinCapLimitAmount()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataWinCapAmountSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataWinCapAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.WinCapAmount;
        }

        /// <summary>
        ///Gets the minimum base game time from the foundation
        /// </summary>
        /// <returns>The minimum base game time</returns>
        public int GetMinimumBaseGameTimeInMs()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataMinimumBaseGamePresentationTimeSend>();

            var reply =
                SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataMinimumBaseGamePresentationTimeReply>(
                    Channel.Foundation, request);
            CheckReply(reply.Reply);

            return (int)reply.MinimumBaseGamePresentationTime;
        }

        /// <summary>
        /// Gets the minimum free spin game time from the foundation.
        /// </summary>
        /// <returns>The minimum free spin game time.</returns>
        public int GetMinimumFreeSpinTimeInMs()
        {
            var result = 0;
            return result;
        }

        /// <inheritdoc />
        public F2LInternal.LineSelectionType GetConfiguredLineSelection()
        {
            return F2LInternal.LineSelectionType.Undefined;
        }

        /// <inheritdoc />
        public uint GetAncillaryCycleLimit()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataAncillaryCycleLimitSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataAncillaryCycleLimitReply>(Channel.Foundation,
                                                                                                 request);
            CheckReply(reply.Reply);

            return reply.AncillaryCycleLimit;
        }

        /// <inheritdoc />
        public bool GetAncillaryGameEnabled()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataAncillaryGameEnabledSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataAncillaryGameEnabledReply>(Channel.Foundation,
                                                                                                  request);
            CheckReply(reply.Reply);

            return reply.AncillaryGameEnabled;
        }

        /// <inheritdoc />
        public F2LInternal.AmountType GetAncillaryMonetaryLimit()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataAncillaryMonetaryLimitSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataAncillaryMonetaryLimitReply>(Channel.Foundation,
                                                                                                    request);
            CheckReply(reply.Reply);

            return reply.AncillaryMonetaryLimit;
        }

        /// <inheritdoc />
        public uint GetHistoryPlaySteps()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataHistoryPlayStepsSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataHistoryPlayStepsReply>(Channel.Foundation,
                                                                                              request);
            CheckReply(reply.Reply);

            return reply.NumberOfPlaySteps;
        }

        /// <inheritdoc />
        public F2LInternal.CreditMeterDisplayBehaviorType GetCreditMeterBehavior()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetConfigDataCreditMeterDisplayBehaviorSend>();
            var reply =
                SendMessageAndGetReply<F2LInternal.GameControlGetConfigDataCreditMeterDisplayBehaviorReply>(Channel.Foundation,
                                                                                                request);
            CheckReply(reply.Reply);
            return reply.CreditMeterDisplayBehavior;
        }

        /// <inheritdoc />
        public F2LInternal.MaxBetButtonBehavior GetMaxBetButtonBehavior()
        {
            return F2LInternal.MaxBetButtonBehavior.BetMaxCreditsOnly;
        }

        /// <inheritdoc />
        public IDictionary<long, IEnumerable<SystemProgressiveData>> GetProgressiveLevels(
            IEnumerable<long> denominations)
        {
            var request = CreateBasicRequest<F2LInternal.GameControlGetProgressiveGameLevelValuesSend>();

            var content = (F2LInternal.GameControlGetProgressiveGameLevelValuesSend)request.Message.Item;
            var denomConversion = denominations.Select(i => (uint)i);
            content.Denominations = denomConversion.ToList();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetProgressiveGameLevelValuesReply>(Channel.Game, request);
            CheckReply(reply.Reply);

            var progressiveReport = new Dictionary<long, IEnumerable<SystemProgressiveData>>();


            if(reply.ProgressiveGameLevelValues != null)
            {
                foreach(var denominationConfiguration in reply.ProgressiveGameLevelValues)
                {
                    // If the amount and prize string are both null it means the progressive isn't linked.
                    var progressiveInformation =
                        denominationConfiguration.GameLevelProgressives.Where(
                            progressive =>
                                progressive.Amount != null || progressive.PrizeString != null
                        )
                        .Select(
                            progressive =>
                            new SystemProgressiveData(progressive.Amount?.Value ?? 0,
                                                      progressive.GameLevel,
                                                      progressive.PrizeString)).ToList();

                    progressiveReport[denominationConfiguration.Denomination] = progressiveInformation;
                }
            }

            return progressiveReport;
        }

        /// <inheritdoc />
        public IEnumerable<SystemProgressiveData> GetProgressiveLevels(long denomination)
        {
            var denominations = new List<long> { denomination };
            var progressiveReport = GetProgressiveLevels(denominations);

            if(progressiveReport.ContainsKey(denomination))
            {
                if(progressiveReport[denomination] != null)
                    return progressiveReport[denomination];
            }

            return new List<SystemProgressiveData>();
        }

        /// <inheritdoc />
        public bool GetRoundWagerUpPlayoffEnabled()
        {
            return false;
        }

        /// <inheritdoc />
        public WinCapBehaviorInfo GetWinCapBehavior()
        {
            return new WinCapBehaviorInfo(WinCapBehavior.FixedWinCapAmount,
                    GetWinCapLimitAmount(), 0);
        }

        /// <inheritdoc />
        public bool GetThemeSelectionMenuOfferable()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlIsThemeSelectionMenuOfferableSend>();
            var reply = SendMessageAndGetReply<F2LInternal.GameControlIsThemeSelectionMenuOfferableReply>(Channel.Foundation,
                                                                                              request);
            CheckReply(reply.Reply);
            return reply.IsThemeSelectionMenuOfferable;
        }

        /// <inheritdoc />
        public void RequestThemeSelectionMenu()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlRequestThemeSelectionMenuSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlRequestThemeSelectionMenuReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool RequestDenominationChange(long newDenomination)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlDenominationChangeRequestSend>();
            var content = (F2LInternal.GameControlDenominationChangeRequestSend)request.Message.Item;
            content.Denomination = checked((uint)newDenomination);

            var reply = SendMessageAndGetReply<F2LInternal.GameControlDenominationChangeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.RequestAccepted;
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableDenominations()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.DenominationList.Denom.ConvertAll(denom => (long)denom);
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableProgressiveDenominations()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation,
                request);
            CheckReply(reply.Reply);

            var availableDenominations = reply.DenominationList.Denom.ConvertAll(denom => (long)denom);
            var levels = GetProgressiveLevels(availableDenominations);
            return levels.Keys;
        }

        /// <inheritdoc />
        public GameDenominationInfo GetGameDenominationInfo()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<F2LInternal.GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation,
                request);
            CheckReply(reply.Reply);

            var availableDenominations = reply.DenominationList.Denom.ConvertAll(denom => (long)denom);
            var levels = GetProgressiveLevels(availableDenominations);

            return new GameDenominationInfo(availableDenominations, levels.Keys, 0);
        }

        /// <inheritdoc />
        public string GetCulture()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCultureSend>();
            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCultureReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.CurrentCulture;
        }

        /// <inheritdoc />
        public ICollection<string> GetAvailableCultures()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetAvailableCulturesSend>();
            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetAvailableCulturesReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.AvailableCulture;
        }

        /// <inheritdoc />
        public void SetCulture(string culture)
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlSetCultureSend>();
            var content = (F2LInternal.GameControlSetCultureSend)request.Message.Item;
            content.Culture = culture;
            var reply = SendMessageAndGetReply<F2LInternal.GameControlSetCultureReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public string SetDefaultCulture()
        {
            // The UC foundation doesn't support this message, so simply return an empty string.
            return "";
        }

        /// <inheritdoc />
        public F2LInternal.CreditFormatterInfoType GetCreditFormatting()
        {
            var request = CreateTransactionalRequest<F2LInternal.GameControlGetCreditFormatterInfoSend>();
            var reply = SendMessageAndGetReply<F2LInternal.GameControlGetCreditFormatterInfoReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.CreditFormatterInfo;
        }

        #endregion
    }
}
