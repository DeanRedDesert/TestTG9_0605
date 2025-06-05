//-----------------------------------------------------------------------
// <copyright file = "GameControlCategory.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2L
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using PlatformInterfaces = Ascent.Communication.Platform.Interfaces;
    using Ascent.OutcomeList.Interfaces;
    using F2XTransport;
    using Schemas.Internal;
    using Transport;

    /// <summary>
    /// Implementation of the F2L game control API category.
    /// </summary>
    public sealed class GameControlCategory : F2LTransactionalCategoryBase<GameControl>,
                                              IMultiVersionSupport,
                                              IGameControlCategory
    {
        #region Versioning Information

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<VersionType> supportedVersions = new List<VersionType>
        {
            new VersionType(4, 9),
            new VersionType(4, 10),
            new VersionType(4, 11),
            // NOTE: 4.12 is no longer supported in The foundation.
            new VersionType(4, 13),
            new VersionType(4, 14),
            new VersionType(4, 15),
            // NOTE: 4.16 is not supported by the foundation.
            new VersionType(4, 17),
        };

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>.
        /// </summary>
        private const string MethodGetAvailableProgressiveDenominations = "GetAvailableProgressiveDenominations";

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>.
        /// </summary>
        private const string MethodGetDefaultDenomination = "GetDefaultDenomination";

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>.
        /// </summary>
        private const string MethodSetDefaultCulture = "SetDefaultCulture";

        /// <summary>
        /// The name of the method that will appear in <see cref="methodSupportingVersions"/>
        /// </summary>
        private const string MethodGetLineSelection = "GetLineSelection";

        /// <summary>
        /// A look up table for the methods that are NOT available in all supported versions.
        /// Keyed by the method name, the value is the version where the method becomes available.
        /// </summary>
        private readonly Dictionary<string, VersionType> methodSupportingVersions = new Dictionary<string, VersionType>
            {
                { MethodGetAvailableProgressiveDenominations, new VersionType(4, 10) },
                { MethodGetDefaultDenomination, new VersionType(4, 13) },
                { MethodSetDefaultCulture, new VersionType(4, 13) },
                { MethodGetLineSelection, new VersionType(4, 17) },
            };

        #endregion

        #region Fields

        /// <summary>
        /// Object which implements the game control category callbacks.
        /// </summary>
        private readonly IGameControlCategoryCallbacks callbackHandler;

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0.  Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private VersionType effectiveVersion = new VersionType(0, 0);

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Create an instance of the game control category.
        /// </summary>
        /// <param name="transport">Transport that this handler will be installed in.</param>
        /// <param name="callbackHandler">Game control category callback handler.</param>
        /// <exception cref="ArgumentNullException">Thrown when the callback handler is null.</exception>
        public GameControlCategory(IF2XTransport transport, IGameControlCategoryCallbacks callbackHandler)
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
            AddMessagehandler<GameControlSetDisplayControlStateSend>(HandleGameControlSetDisplayControlStateSend);
            AddMessagehandler<GameControlTransactionCompleteSend>(HandleGameControlTransactionCompleteSend);
            AddMessagehandler<GameControlNewThemeContextSend>(HandleGameControlNewThemeContextSend);
            AddMessagehandler<GameControlSwitchThemeContextSend>(HandleGameControlSwitchThemeContextSend);
            AddMessagehandler<GameControlActivateThemeContextSend>(HandleGameControlActivateThemeContextSend);
            AddMessagehandler<GameControlActionResponseSend>(HandleGameControlActionResponseSend);
            AddMessagehandler<GameControlInactivateThemeContextSend>(HandleGameControlInactivateThemeContextSend);
            AddMessagehandler<GameControlEnrollResponseSend>(HandleGameControlEnrollResponseSend);
            AddMessagehandler<GameControlEvalOutcomeResponseSend>(HandleGameControlEvalOutcomeResponseSend);
            AddMessagehandler<GameControlFinalizeAwardResponseSend>(HandleGameControlFinalizeAwardResponseSend);
            AddMessagehandler<GameControlMoneyWagerableSend>(HandleGameControlMoneyWagerableSend);
            AddMessagehandler<GameControlMoneyBetSend>(HandleGameControlMoneyBetSend);
            AddMessagehandler<GameControlMoneyCommittedChangedSend>(HandleGameControlMoneyCommittedChangedSend);
            AddMessagehandler<GameControlMoneyOutSend>(HandleGameControlMoneyOutSend);
            AddMessagehandler<GameControlMoneyWonSend>(HandleGameControlMoneyWonSend);
            AddMessagehandler<GameControlMoneySetSend>(HandleGameControlMoneySetSend);
            AddMessagehandler<GameControlMoneyInSend>(HandleGameControlMoneyInSend);
            AddMessagehandler<GameControlBankStatusChangedSend>(HandleGameControlBankStatusChangedSend);
            AddMessagehandler<GameControlThemeSelectionMenuOfferableStatusChangedSend>(
                HandleGameControlThemeSelectionMenuOfferableStatusChangedSend);
            AddMessagehandler<GameControlDisableAncillaryGameOfferSend>(
                HandleGameControlDisableAncillaryGameOfferSend);
            AddMessagehandler<GameControlCultureChangedSend>(HandleGameControlCultureChangedSend);
            AddMessagehandler<GameControlDenominationChangeCancelledSend>(HandleGameControlDenominationCancelledSend);
        }

        #endregion

        #region IApiCategory Overrides

        /// <inheritdoc />
        public override uint MajorVersion => effectiveVersion.MajorVersion;

        /// <inheritdoc />
        public override uint MinorVersion => effectiveVersion.MinorVersion;

        /// <inheritdoc />
        public override MessageCategory Category => MessageCategory.GameControl;

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the GameControlSetDisplayControlStateSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlSetDisplayControlStateSend(GameControlSetDisplayControlStateSend message)
        {
            callbackHandler.ProcessSetDisplayControlState(message.DisplayControlState);
            var reply = CreateReply<GameControlSetDisplayControlStateReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlTransactionCompleteSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlTransactionCompleteSend(GameControlTransactionCompleteSend message)
        {
            var reply = CreateReply<GameControlTransactionCompleteReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlNewThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlNewThemeContextSend(GameControlNewThemeContextSend message)
        {
            // The GameSubMode defaults to Standard in the schema for older version of this category
            // that does not support the GameSubMode.
            callbackHandler.ProcessNewThemeContext(message.GameMode, message.GameSubMode, message.Denom,
                                                   message.PayvarTag, message.PayvarTagDataFile,
                                                   message.NewlySelectedForPlay);

            var reply = CreateReply<GameControlNewThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlSwitchThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlSwitchThemeContextSend(GameControlSwitchThemeContextSend message)
        {
            callbackHandler.ProcessSwitchThemeContext(message.ThemeTag, message.ThemeTagDataFile,
                                                      message.ResourcePaths, message.Denom, message.PayvarTag,
                                                      message.PayvarTagDataFile);

            var reply = CreateReply<GameControlSwitchThemeContextReply>(0, string.Empty);
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlActivateThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlActivateThemeContextSend(GameControlActivateThemeContextSend message)
        {
            callbackHandler.ProcessActivateThemeContext();

            var reply = CreateReply<GameControlActivateThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlActionResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlActionResponseSend(GameControlActionResponseSend message)
        {
            callbackHandler.ProcessActionResponse(message.Payload);

            var reply = CreateReply<GameControlActionResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlInactivateThemeContextSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlInactivateThemeContextSend(GameControlInactivateThemeContextSend message)
        {
            callbackHandler.ProcessInactivateThemeContext();
            var reply = CreateReply<GameControlInactivateThemeContextReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handle the GameControlEnrollResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlEnrollResponseSend(GameControlEnrollResponseSend message)
        {
            callbackHandler.ProcessEnrollResponse(message.EnrollSuccess, message.HostEnrollmentData);
            var reply = CreateReply<GameControlEnrollResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlEvalOutcomeResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlEvalOutcomeResponseSend(GameControlEvalOutcomeResponseSend message)
        {
            callbackHandler.ProcessEvalOutcomeResponse(message.OutcomeList, message.IsPlayComplete);
            var reply = CreateReply<GameControlEvalOutcomeResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlFinalizeAwardResponseSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlFinalizeAwardResponseSend(GameControlFinalizeAwardResponseSend message)
        {
            callbackHandler.ProcessFinalizeAwardResponse();
            var reply = CreateReply<GameControlFinalizeAwardResponseReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyWagerableSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyWagerableSend(GameControlMoneyWagerableSend message)
        {
            callbackHandler.ProcessMoneyWagerable(message.AmountMoved, message.WagerableDirection.ToString(),
                                           message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyWagerableReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyBetSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyBetSend(GameControlMoneyBetSend message)
        {
            callbackHandler.ProcessMoneyBet(message.AmountBet, message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyBetReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlMoneyCommittedChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyCommittedChangedSend(GameControlMoneyCommittedChangedSend message)
        {
            callbackHandler.ProcessMoneyCommittedChanged(message.AmountCommitted, message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyCommittedChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyInSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyInSend(GameControlMoneyInSend message)
        {
            FundsTransferAccountSource? transferSource = null;
            if(message.FundsTransferAccountSourceSpecified)
            {
                transferSource = message.FundsTransferAccountSource;
            }
            callbackHandler.ProcessMoneyIn(message.AmountIn,
                                           message.MoneyInSource,
                                           transferSource,
                                           message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyInReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyOutSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyOutSend(GameControlMoneyOutSend message)
        {
            callbackHandler.ProcessMoneyOut(message.AmountOut, message.MoneyOutSource, message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyOutReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneyWonSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneyWonSend(GameControlMoneyWonSend message)
        {
            callbackHandler.ProcessMoneyWon(message.AmountWon, message.PlayerMeters);
            var reply = CreateReply<GameControlMoneyWonReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlMoneySetSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlMoneySetSend(GameControlMoneySetSend message)
        {
            var playerMeters = message.PlayerMeters;

            callbackHandler.ProcessMoneySet(playerMeters);
            var reply = CreateReply<GameControlMoneySetReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        ///  Handler for the GameControlBankStatusChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlBankStatusChangedSend(GameControlBankStatusChangedSend message)
        {
            var bankStatus = message.BankStatus;

            callbackHandler.ProcessBankStatusChanged(bankStatus);
            var reply = CreateReply<GameControlBankStatusChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlThemeSelectionMenuOfferableStatusChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlThemeSelectionMenuOfferableStatusChangedSend(
            GameControlThemeSelectionMenuOfferableStatusChangedSend message)
        {
            callbackHandler.ProcessThemeSelectionMenuOfferableStatusChanged(message.IsThemeSelectionMenuOfferable);
            var reply = CreateReply<GameControlThemeSelectionMenuOfferableStatusChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlDisableAncillaryGameOfferSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlDisableAncillaryGameOfferSend(
            GameControlDisableAncillaryGameOfferSend message)
        {
            callbackHandler.ProcessDisableAncillaryGameOffer();
            var reply = CreateReply<GameControlDisableAncillaryGameOfferReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlCultureChangedSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlCultureChangedSend(GameControlCultureChangedSend message)
        {
            callbackHandler.ProcessCultureChanged(message.Culture);
            var reply = CreateReply<GameControlCultureChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the GameControlDenominationChangeCancelledSend message.
        /// </summary>
        /// <param name="message">Message to handle.</param>
        private void HandleGameControlDenominationCancelledSend(GameControlDenominationChangeCancelledSend message)
        {
            callbackHandler.ProcessDenominationChangeCancelled();
            var reply = CreateReply<GameControlDenominationChangeCancelledReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IMultiVersionSupport Members

        /// <inheritdoc />
        public void SetVersion(uint major, uint minor)
        {
            var version = new VersionType(major, minor);

            if(!supportedVersions.Contains(version))
            {
                throw new ArgumentException(
                    $"{version} is not supported by GameControlCategory class.");
            }

            effectiveVersion = version;
        }

        #endregion

        #region IGameControlCategory Members

        /// <inheritdoc />
        public bool ActionRequest(byte[] payload)
        {
            var request = CreateBasicRequest<GameControlActionRequestSend>();
            var actionRequest = (GameControlActionRequestSend)request.Message.Item;
            actionRequest.Payload = payload;

            var reply = SendMessageAndGetReply<GameControlActionRequestReply>(Channel.Game, request);

            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public bool CanCommitBet(long betAmount)
        {
            var request = CreateTransactionalRequest<GameControlCanCommitBetSend>();
            var content = (GameControlCanCommitBetSend)request.Message.Item;
            content.BetAmount = betAmount;

            var reply = SendMessageAndGetReply<GameControlCanCommitBetReply>(Channel.Foundation, request);
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

            var request = CreateTransactionalRequest<GameControlCanCommitBetsSend>();
            var content = (GameControlCanCommitBetsSend)request.Message.Item;
            content.BetAmount = (from bet in bets select new AmountType(bet)).ToList();

            var reply = SendMessageAndGetReply<GameControlCanCommitBetsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitAllowed;
        }

        /// <inheritdoc />
        public bool CommitBet(long betAmount)
        {
            var request = CreateTransactionalRequest<GameControlCommitBetSend>();
            var content = (GameControlCommitBetSend)request.Message.Item;
            content.BetAmount = betAmount;

            var reply = SendMessageAndGetReply<GameControlCommitBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetCommitted;
        }

        /// <inheritdoc />
        public long GetCommittedBet()
        {
            var request = CreateTransactionalRequest<GameControlGetCommittedBetSend>();

            var reply = SendMessageAndGetReply<GameControlGetCommittedBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommittedBetAmount;
        }

        /// <inheritdoc />
        public void UncommitBet()
        {
            var request = CreateTransactionalRequest<GameControlUncommitBetSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlUncommitBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void PlaceStartingBet(bool maxBet)
        {
            var request = CreateTransactionalRequest<GameControlPlaceStartingBetSend>();
            var content = (GameControlPlaceStartingBetSend)request.Message.Item;
            content.IsMaxBet = maxBet;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlPlaceStartingBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool CanPlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                                long pendingWinsAvailableForBet)
        {
            var request = CreateTransactionalRequest<GameControlCanPlaceBetSend>();
            var content = (GameControlCanPlaceBetSend)request.Message.Item;

            content.BetAmount = betAmount;
            content.BetFromCreditsAmount = betFromCredits;
            content.BetFromUncommittedGameWinsAmount = betFromPendingWins;
            content.PendingWinsAvailableForThisBet = pendingWinsAvailableForBet;

            var reply = SendMessageAndGetReply<GameControlCanPlaceBetReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetAllowed;
        }

        /// <inheritdoc />
        public bool PlaceBet(long betAmount, long betFromCredits, long betFromPendingWins,
                             long pendingWinsAvailableForBet)
        {
            var request = CreateTransactionalRequest<GameControlPlaceBetSend>();
            var content = (GameControlPlaceBetSend)request.Message.Item;
            content.BetAmount = betAmount;
            content.BetFromCreditsAmount = betFromCredits;
            content.BetFromUncommittedGameWinsAmount = betFromPendingWins;
            content.PendingWinsAvailableForThisBet = pendingWinsAvailableForBet;

            var reply = SendMessageAndGetReply<GameControlPlaceBetReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public IEnumerable<bool> CanBetNextGameCycle(IEnumerable<long> bets)
        {
            if(bets == null)
            {
                throw new ArgumentNullException(nameof(bets), "The list of bets should not be null.");
            }

            var request = CreateTransactionalRequest<GameControlCanBetNextGameCycleSend>();
            var content = (GameControlCanBetNextGameCycleSend)request.Message.Item;
            content.BetAmount = (from bet in bets select new AmountType(bet)).ToList();

            var reply = SendMessageAndGetReply<GameControlCanBetNextGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BetAllowed;
        }

        /// <inheritdoc />
        public PlayerMeters GetPlayerMeters()
        {
            var request = CreateTransactionalRequest<GameControlGetPlayerMetersSend>();

            var reply = SendMessageAndGetReply<GameControlGetPlayerMetersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.PlayerMeters;
        }

        /// <inheritdoc />
        public BankStatus QueryBankStatus()
        {
            var request = CreateTransactionalRequest<GameControlQueryBankStatusSend>();

            var reply = SendMessageAndGetReply<GameControlQueryBankStatusReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.BankStatus;
        }

        /// <inheritdoc />
        public bool IsPlayerWagerOfferable()
        {
            // F2L message keeps the name of BankLocked for sake of 
            // backward compatibility. IsPlayerWagerOfferable should be the opposite of isBankLocked.
            var request = CreateTransactionalRequest<GameControlIsBankLockedSend>();

            var reply = SendMessageAndGetReply<GameControlIsBankLockedReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return !reply.IsBankLocked;
        }

        /// <inheritdoc />
        public bool IsPlayerCashoutOfferable()
        {
            var request = CreateTransactionalRequest<GameControlIsPlayerCashoutOfferableSend>();

            var reply = SendMessageAndGetReply<GameControlIsPlayerCashoutOfferableReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.IsPlayerCashoutOfferable;
        }

        /// <inheritdoc />
        public bool IsPlayerBankToWagerableOfferable()
        {
            var request = CreateTransactionalRequest<GameControlIsPlayerBankToWagerableOfferableSend>();

            var reply = SendMessageAndGetReply<GameControlIsPlayerBankToWagerableOfferableReply>(Channel.Foundation,
                                                                                                 request);
            CheckReply(reply.Reply);

            return reply.IsPlayerBankToWagerableOfferable;
        }

        /// <inheritdoc />
        public void PlayerCashoutRequest()
        {
            var request = CreateTransactionalRequest<GameControlPlayerCashoutRequestSend>();

            var reply = SendMessageAndGetReply<GameControlPlayerCashoutRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool TransferWagerableToBankRequest()
        {
            var request = CreateTransactionalRequest<GameControlTransferWagerableToBankRequestSend>();

            var reply = SendMessageAndGetReply<GameControlTransferWagerableToBankRequestReply>(Channel.Foundation,
                                                                                               request);
            CheckReply(reply.Reply);

            return reply.TransferAccepted;
        }

        /// <inheritdoc />
        public void TransferBankToWagerableRequest()
        {
            var request = CreateTransactionalRequest<GameControlTransferBankToWagerableRequestSend>();

            var reply = SendMessageAndGetReply<GameControlTransferBankToWagerableRequestReply>(Channel.Foundation,
                                                                                               request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public GameCycleState QueryGameCycleState()
        {
            var request = CreateTransactionalRequest<GameControlQueryGameCycleStateSend>();

            var reply = SendMessageAndGetReply<GameControlQueryGameCycleStateReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CurrentGameCycleState;
        }

        /// <inheritdoc />
        public bool CommitGameCycle()
        {
            var request = CreateTransactionalRequest<GameControlCommitGameCycleSend>();

            var reply = SendMessageAndGetReply<GameControlCommitGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitSuccessful;
        }

        /// <inheritdoc />
        public bool CanCommitGameCycle()
        {
            var request = CreateTransactionalRequest<GameControlCanCommitGameCycleSend>();

            var reply = SendMessageAndGetReply<GameControlCanCommitGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.CommitAllowed;
        }

        /// <inheritdoc />
        public bool UncommitGameCycle()
        {
            var request = CreateTransactionalRequest<GameControlUncommitGameCycleSend>();

            var reply = SendMessageAndGetReply<GameControlUncommitGameCycleReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public void EnrollGameCycle(byte[] enrollmentData)
        {
            var request = CreateTransactionalRequest<GameControlEnrollGameCycleSend>();
            var enrollContent = (GameControlEnrollGameCycleSend)request.Message.Item;
            enrollContent.HostEnrollmentData = enrollmentData;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEnrollGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void UnenrollGameCycle()
        {
            var request = CreateTransactionalRequest<GameControlUnenrollGameCycleSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlUnenrollGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool CanStartPlaying()
        {
            var request = CreateTransactionalRequest<GameControlCanStartPlayingSend>();

            var reply = SendMessageAndGetReply<GameControlCanStartPlayingReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.StartAllowed;
        }

        /// <inheritdoc />
        public bool StartPlaying()
        {
            var request = CreateTransactionalRequest<GameControlStartPlayingSend>();

            var reply = SendMessageAndGetReply<GameControlStartPlayingReply>(Channel.Foundation, request);
            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public void EvalOutcomeRequest(IOutcomeList outcomeList)
        {
            var request = CreateTransactionalRequest<GameControlEvalOutcomeRequestSend>();
            var content = (GameControlEvalOutcomeRequestSend)request.Message.Item;
            content.OutcomeList = new OutcomeList(outcomeList);

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEvalOutcomeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void LastEvalOutcomeRequest(IOutcomeList outcomeList,
                                           ReadOnlyCollection<WagerCatOutcome> wagerCategoryOutcomes)
        {
            var request = CreateTransactionalRequest<GameControlEvalLastOutcomeRequestSend>();
            var content = (GameControlEvalLastOutcomeRequestSend)request.Message.Item;

            content.OutcomeList = new OutcomeList(outcomeList);
            content.WagerCatOutcomes = new GameControlEvalLastOutcomeRequestSendWagerCatOutcomes
            {
                WagerCatOutcome = wagerCategoryOutcomes.ToList()
            };

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEvalLastOutcomeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void LastEvalAncillaryOutcomeRequest(IOutcomeList outcomeList)
        {
            var request = CreateTransactionalRequest<GameControlEvalLastAncillaryOutcomeRequestSend>();
            var content = (GameControlEvalLastAncillaryOutcomeRequestSend)request.Message.Item;
            content.OutcomeList = new OutcomeList(outcomeList);

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEvalLastAncillaryOutcomeRequestReply>(Channel.Foundation,
                                                                                                request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool OfferAncillaryGame()
        {
            var request = CreateTransactionalRequest<GameControlOfferAncillaryGameRequestSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlOfferAncillaryGameRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.OfferAncillaryGame;
        }

        /// <inheritdoc />
        public bool StartAncillaryPlaying()
        {
            var request = CreateTransactionalRequest<GameControlStartAncillaryPlayingSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlStartAncillaryPlayingReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.StartSuccessful;
        }

        /// <inheritdoc />
        public bool StartBonusPlaying()
        {
            var request = CreateTransactionalRequest<GameControlStartBonusPlayingSend>();

            var reply = SendMessageAndGetReply<GameControlStartBonusPlayingReply>(Channel.Foundation, request);

            return reply.Reply.ReplyCode == 0;
        }

        /// <inheritdoc />
        public void LastEvalBonusOutcomeRequest(IOutcomeList outcomeList)
        {
            var request = CreateTransactionalRequest<GameControlEvalLastBonusOutcomeRequestSend>();
            var content = (GameControlEvalLastBonusOutcomeRequestSend)request.Message.Item;
            content.OutcomeList = new OutcomeList(outcomeList);

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEvalLastBonusOutcomeRequestReply>(Channel.Foundation,
                                                                                            request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool GetBonusPlayEnabled()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataBonusPlayEnabledSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataBonusPlayEnabledReply>(Channel.Foundation,
                                                                                              request);

            CheckReply(reply.Reply);

            return reply.BonusPlayEnabled;
        }

        /// <inheritdoc />
        public void FinalizeAwardRequest()
        {
            var request = CreateTransactionalRequest<GameControlFinalizeAwardRequestSend>();

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlFinalizeAwardRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public void EndGameCycle(uint historySteps)
        {
            var request = CreateTransactionalRequest<GameControlEndGameCycleSend>();
            var content = (GameControlEndGameCycleSend)request.Message.Item;
            content.NumberOfSteps = historySteps;

            //Get the reply so that the call blocks.
            var reply = SendMessageAndGetReply<GameControlEndGameCycleReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public int GetRandomNumber(int low, int high, string rngName)
        {
            var request = CreateTransactionalRequest<GameControlGetRandomNumbersSend>();
            var content = (GameControlGetRandomNumbersSend)request.Message.Item;
            content.MaximumValue = high;
            content.MinimumValue = low;
            content.Count = 1;
            content.AlreadyPickedNumbers = new GameControlGetRandomNumbersSendAlreadyPickedNumbers
            {
                AlreadyPickedNumber = new List<int>()
            };
            content.Duplicates = 0;
            content.Name = rngName;

            var reply = SendMessageAndGetReply<GameControlGetRandomNumbersReply>(Channel.Foundation, request);
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
            var request = CreateTransactionalRequest<GameControlGetRandomNumbersSend>();
            var content = (GameControlGetRandomNumbersSend)request.Message.Item;
            content.MaximumValue = high;
            content.MinimumValue = low;
            content.Count = count;
            content.AlreadyPickedNumbers = new GameControlGetRandomNumbersSendAlreadyPickedNumbers
            {
                AlreadyPickedNumber = prePicked.ToList()
            };
            content.Duplicates = allowedDuplicates;
            content.Name = rngName;

            var reply = SendMessageAndGetReply<GameControlGetRandomNumbersReply>(Channel.Foundation, request);
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
            var request = CreateTransactionalRequest<GameControlGetListLimitedRandomNumbersSend>();
            var content = (GameControlGetListLimitedRandomNumbersSend)request.Message.Item;

            content.Count = count;
            content.RangeList = new List<GameControlGetListLimitedRandomNumbersSendRange>();

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
                content.RangeList.Add(new GameControlGetListLimitedRandomNumbersSendRange
                { MaximumValue = highRanges[rangeIndex], MinimumValue = lowRanges[rangeIndex] });
            }

            content.Name = rngName;

            var reply = SendMessageAndGetReply<GameControlGetListLimitedRandomNumbersReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            if(reply.RandomNumbers == null)
            {
                throw new InvalidMessageException("Random number response contained a null RandomNumbers list.");
            }

            return reply.RandomNumbers.RandomNumber;
        }

        /// <inheritdoc />
        public void WriteCriticalData(CriticalDataScope scope, string itemName, byte[] data)
        {
            var request = CreateTransactionalRequest<GameControlWriteCritDataSend>();
            var content = (GameControlWriteCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;
            content.Data = data;

            var reply = SendMessageAndGetReply<GameControlWriteCritDataReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public byte[] ReadCriticalData(CriticalDataScope scope, string itemName)
        {
            var request = CreateTransactionalRequest<GameControlReadCritDataSend>();
            var content = (GameControlReadCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;

            var reply = SendMessageAndGetReply<GameControlReadCritDataReply>(Channel.Foundation, request);

            CheckReply(reply.Reply);

            return reply.ReadCritDataSuccess ? reply.Data : new byte[0];
        }

        /// <inheritdoc />
        public bool RemoveCriticalData(CriticalDataScope scope, string itemName)
        {
            var request = CreateTransactionalRequest<GameControlRemoveCritDataSend>();
            var content = (GameControlRemoveCritDataSend)request.Message.Item;
            content.CriticalDataScope = scope;
            content.ItemName = itemName;

            var reply = SendMessageAndGetReply<GameControlRemoveCritDataReply>(Channel.Foundation, request);

            CheckReply(reply.Reply);

            return reply.RemoveCritDataSuccess;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemReferencedEnumeration(string name, CustomConfigItemScope scope,
            IList<string> values)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemReferencedEnumerationSend>();
            var content = (GameControlGetCustomConfigItemReferencedEnumerationSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;

            var reply =
                SendMessageAndGetReply<GameControlGetCustomConfigItemReferencedEnumerationReply>(Channel.Foundation,
                    request);

            if(reply.EnumerationData.Item != null)
            {
                var enumeration = reply.EnumerationData;

                foreach(var value in enumeration.Item)
                {
                    values.Add(value);
                }
            }

            CheckReply(reply.Reply);

            return reply.EnumerationData.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemAmount(string name, CustomConfigItemScope scope, out long value,
                                              out long lowRange, out long highRange)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Amount;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var amount = (GameControlGetCustomConfigItemReplyCustomConfigItemAmountData)reply.CustomConfigItem.Item;
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
        public bool GetCustomConfigItemBoolean(string name, CustomConfigItemScope scope, out bool value)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Boolean;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

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
        public bool GetCustomConfigItemEnumeration(string name, CustomConfigItemScope scope, IList<string> values)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Enumeration;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var enumeration =
                    (GameControlGetCustomConfigItemReplyCustomConfigItemEnumerationData)reply.CustomConfigItem.Item;

                foreach(var value in enumeration.Item)
                {
                    values.Add(value);
                }
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem.Item != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemFlagList(string name, CustomConfigItemScope scope,
                                                IDictionary<string, bool> values)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.FlagList;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem != null)
            {
                var flagList =
                    (GameControlGetCustomConfigItemReplyCustomConfigItemFlagListData)reply.CustomConfigItem.Item;

                foreach(var flag in flagList.FlagList)
                {
                    values.Add(flag.Item, flag.Value);
                }
            }

            CheckReply(reply.Reply);

            return reply.CustomConfigItem != null;
        }

        /// <inheritdoc />
        public bool GetCustomConfigItemFloat(string name, CustomConfigItemScope scope, out float value)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Float;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var floatData =
                    (GameControlGetCustomConfigItemReplyCustomConfigItemFloatData)reply.CustomConfigItem.Item;

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
        public bool GetCustomConfigItemInt64(string name, CustomConfigItemScope scope, out long value)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Int64;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var int64Data =
                    (GameControlGetCustomConfigItemReplyCustomConfigItemInt64Data)reply.CustomConfigItem.Item;

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
        public bool GetCustomConfigItem(string name, CustomConfigItemScope scope, out string value)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.Item;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var itemData = (GameControlGetCustomConfigItemReplyCustomConfigItemItemData)reply.CustomConfigItem.Item;

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
        public bool GetCustomConfigItemString(string name, CustomConfigItemScope scope, out string value)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemSend>();
            var content = (GameControlGetCustomConfigItemSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;
            content.Type = CustomConfigItemType.String;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemReply>(Channel.Foundation, request);

            if(reply.CustomConfigItem.Item != null)
            {
                var itemData =
                    (GameControlGetCustomConfigItemReplyCustomConfigItemStringData)reply.CustomConfigItem.Item;

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
        public bool GetCustomConfigItemType(string name, CustomConfigItemScope scope, out CustomConfigItemType type)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemTypeSend>();
            var content = (GameControlGetCustomConfigItemTypeSend)request.Message.Item;
            content.Name = name;
            content.Scope = scope;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemTypeReply>(Channel.Foundation, request);
            type = reply.Type;
            CheckReply(reply.Reply);

            return type != CustomConfigItemType.Invalid;
        }

        /// <inheritdoc />
        public IDictionary<string, CustomConfigItemType> GetCustomConfigItemTypes(CustomConfigItemScope scope)
        {
            var request = CreateTransactionalRequest<GameControlGetCustomConfigItemTypesSend>();
            var content = (GameControlGetCustomConfigItemTypesSend)request.Message.Item;
            content.Scope = scope;

            var reply = SendMessageAndGetReply<GameControlGetCustomConfigItemTypesReply>(Channel.Foundation, request);

            var types = new Dictionary<string, CustomConfigItemType>();

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
            var request = CreateTransactionalRequest<GameControlGetConfigDataMaxBetAmountSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataMaxBetAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.MaxBetAmount;
        }

        /// <inheritdoc />
        public long GetMinBetAmount()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataMinBetAmountSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataMinBetAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.MinBetAmount;
        }

        /// <inheritdoc />
        public long GetButtonPanelMinBetAmount()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataButtonPanelMinBetAmountSend>();

            var reply =
                SendMessageAndGetReply<GameControlGetConfigDataButtonPanelMinBetAmountReply>(Channel.Foundation,
                    request);
            CheckReply(reply.Reply);

            return reply.ButtonPanelMinBetAmount;
        }

        /// <inheritdoc />
        public long GetWinCapLimitAmount()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataWinCapAmountSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataWinCapAmountReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.WinCapAmount;
        }

        /// <summary>
        ///Gets the minimum base game time from the foundation
        /// </summary>
        /// <returns>The minimum base game time</returns>
        public int GetMinimumBaseGameTimeInMs()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataMinimumBaseGamePresentationTimeSend>();

            var reply =
                SendMessageAndGetReply<GameControlGetConfigDataMinimumBaseGamePresentationTimeReply>(
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
            var request = CreateTransactionalRequest<GameControlGetConfigDataMinimumFreeSpinPresentationTimeSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataMinimumFreeSpinPresentationTimeReply>(
                Channel.Foundation,
                request);
            CheckReply(reply.Reply);

            return (int)reply.MinimumFreeSpinPresentationTime;
        }

        /// <inheritdoc />
        public uint GetAncillaryCycleLimit()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataAncillaryCycleLimitSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataAncillaryCycleLimitReply>(Channel.Foundation,
                                                                                                 request);
            CheckReply(reply.Reply);

            return reply.AncillaryCycleLimit;
        }

        /// <inheritdoc />
        public bool GetAncillaryGameEnabled()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataAncillaryGameEnabledSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataAncillaryGameEnabledReply>(Channel.Foundation,
                                                                                                  request);
            CheckReply(reply.Reply);

            return reply.AncillaryGameEnabled;
        }

        /// <inheritdoc />
        public AmountType GetAncillaryMonetaryLimit()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataAncillaryMonetaryLimitSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataAncillaryMonetaryLimitReply>(Channel.Foundation,
                                                                                                    request);
            CheckReply(reply.Reply);

            return reply.AncillaryMonetaryLimit;
        }

        /// <inheritdoc />
        public uint GetHistoryPlaySteps()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataHistoryPlayStepsSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataHistoryPlayStepsReply>(Channel.Foundation,
                                                                                              request);
            CheckReply(reply.Reply);

            return reply.NumberOfPlaySteps;
        }

        /// <inheritdoc />
        public CreditMeterDisplayBehaviorType GetCreditMeterBehavior()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataCreditMeterDisplayBehaviorSend>();
            var reply =
                SendMessageAndGetReply<GameControlGetConfigDataCreditMeterDisplayBehaviorReply>(Channel.Foundation,
                                                                                                request);
            CheckReply(reply.Reply);
            return reply.CreditMeterDisplayBehavior;
        }

        /// <inheritdoc />
        public MaxBetButtonBehavior GetMaxBetButtonBehavior()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataMaxBetButtonBehaviorSend>();
            var reply =
                SendMessageAndGetReply<GameControlGetConfigDataMaxBetButtonBehaviorReply>(Channel.Foundation,
                                                                                          request);
            CheckReply(reply.Reply);
            return reply.MaxBetButtonBehavior;
        }

        /// <inheritdoc />
        public IDictionary<long, IEnumerable<SystemProgressiveData>> GetProgressiveLevels(
            IEnumerable<long> denominations)
        {
            var request = CreateBasicRequest<GameControlGetProgressiveGameLevelValuesSend>();

            var content = (GameControlGetProgressiveGameLevelValuesSend)request.Message.Item;
            var denomConversion = denominations.Select(i => (uint)i);
            content.Denominations = denomConversion.ToList();

            var reply = SendMessageAndGetReply<GameControlGetProgressiveGameLevelValuesReply>(Channel.Game, request);
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
            var request = CreateTransactionalRequest<GameControlGetConfigDataRoundWagerUpPlayoffEnabledSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataRoundWagerUpPlayoffEnabledReply>(
                Channel.Foundation,
                request);

            CheckReply(reply.Reply);

            return reply.RoundWagerUpPlayoffEnabled;
        }

        /// <inheritdoc />
        public PlatformInterfaces.WinCapBehaviorInfo GetWinCapBehavior()
        {
            var request = CreateTransactionalRequest<GameControlGetConfigDataWinCapBehaviorSend>();

            var reply = SendMessageAndGetReply<GameControlGetConfigDataWinCapBehaviorReply>(
                Channel.Foundation, request);

            CheckReply(reply.Reply);

            return new PlatformInterfaces.WinCapBehaviorInfo((PlatformInterfaces.WinCapBehavior)reply.WinCapBehavior,
                                          reply.WinCapAmount,
                                          reply.WinCapMultiplier);
        }

        /// <inheritdoc />
        public bool GetThemeSelectionMenuOfferable()
        {
            var request = CreateTransactionalRequest<GameControlIsThemeSelectionMenuOfferableSend>();
            var reply = SendMessageAndGetReply<GameControlIsThemeSelectionMenuOfferableReply>(Channel.Foundation,
                                                                                              request);
            CheckReply(reply.Reply);
            return reply.IsThemeSelectionMenuOfferable;
        }

        /// <inheritdoc />
        public void RequestThemeSelectionMenu()
        {
            var request = CreateTransactionalRequest<GameControlRequestThemeSelectionMenuSend>();

            var reply = SendMessageAndGetReply<GameControlRequestThemeSelectionMenuReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public bool RequestDenominationChange(long newDenomination)
        {
            var request = CreateTransactionalRequest<GameControlDenominationChangeRequestSend>();
            var content = (GameControlDenominationChangeRequestSend)request.Message.Item;
            content.Denomination = checked((uint)newDenomination);

            var reply = SendMessageAndGetReply<GameControlDenominationChangeRequestReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.RequestAccepted;
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableDenominations()
        {
            var request = CreateTransactionalRequest<GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            return reply.DenominationList.Denom.ConvertAll(denom => (long)denom);
        }

        /// <inheritdoc />
        public ICollection<long> GetAvailableProgressiveDenominations()
        {
            var request = CreateTransactionalRequest<GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation,
                request);
            CheckReply(reply.Reply);

            if(IsMethodSupported(MethodGetAvailableProgressiveDenominations))
            {
                return reply.ProgressiveDenominationList.Denom.ConvertAll(denom => (long)denom);
            }

            var availableDenominations = reply.DenominationList.Denom.ConvertAll(denom => (long)denom);
            var levels = GetProgressiveLevels(availableDenominations);
            return levels.Keys;
        }

        /// <inheritdoc />
        public GameDenominationInfo GetGameDenominationInfo()
        {
            var request = CreateTransactionalRequest<GameControlQueryPlayerSelectableDenomsSend>();

            var reply = SendMessageAndGetReply<GameControlQueryPlayerSelectableDenomsReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);

            var availableDenominations = reply.DenominationList.Denom.ConvertAll(denom => (long)denom);

            // From 4.13, return the default denomination replied from the foundation; otherwise, return 0.
            var defaultGameDenomination = IsMethodSupported(MethodGetDefaultDenomination) ? reply.DefaultDenom : default(long);

            // From 4.10, return the available progressive denominations directly received in the same message.
            // Otherwise, retrieve the available progressive denominations by a separate game control category message.
            var availableProgressiveDenominations = IsMethodSupported(MethodGetAvailableProgressiveDenominations)
                ? reply.ProgressiveDenominationList.Denom.ConvertAll(denom => (long)denom)
                : GetProgressiveLevels(availableDenominations).Keys;

            return new GameDenominationInfo(availableDenominations, availableProgressiveDenominations, defaultGameDenomination);
        }

        /// <inheritdoc />
        public string GetCulture()
        {
            var request = CreateTransactionalRequest<GameControlGetCultureSend>();
            var reply = SendMessageAndGetReply<GameControlGetCultureReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.CurrentCulture;
        }

        /// <inheritdoc />
        public ICollection<string> GetAvailableCultures()
        {
            var request = CreateTransactionalRequest<GameControlGetAvailableCulturesSend>();
            var reply = SendMessageAndGetReply<GameControlGetAvailableCulturesReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.AvailableCulture;
        }

        /// <inheritdoc />
        public void SetCulture(string culture)
        {
            var request = CreateTransactionalRequest<GameControlSetCultureSend>();
            var content = (GameControlSetCultureSend)request.Message.Item;
            content.Culture = culture;
            var reply = SendMessageAndGetReply<GameControlSetCultureReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
        }

        /// <inheritdoc />
        public string SetDefaultCulture()
        {
            if(IsMethodSupported(MethodSetDefaultCulture))
            {

                var request = CreateTransactionalRequest<GameControlSetDefaultCultureSend>();
                var reply = SendMessageAndGetReply<GameControlSetDefaultCultureReply>(Channel.Foundation, request);
                CheckReply(reply.Reply);
                return reply.Culture;
            }

            return default;
        }

        /// <inheritdoc />
        public CreditFormatterInfoType GetCreditFormatting()
        {
            var request = CreateTransactionalRequest<GameControlGetCreditFormatterInfoSend>();
            var reply = SendMessageAndGetReply<GameControlGetCreditFormatterInfoReply>(Channel.Foundation, request);
            CheckReply(reply.Reply);
            return reply.CreditFormatterInfo;
        }

        /// <inheritdoc/>
        public LineSelectionType GetConfiguredLineSelection()
        {
            if(IsMethodSupported(MethodGetLineSelection))
            {
                var request = CreateTransactionalRequest<GameControlGetConfigDataLineSelectionSend>();
                var reply =
                    SendMessageAndGetReply<GameControlGetConfigDataLineSelectionReply>(Channel.Foundation, request);
                CheckReply(reply.Reply);
                return reply.LineSelection;
            }

            return LineSelectionType.Undefined;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check if a method is supported by the effective version of the category.
        /// </summary>
        /// <param name="methodName">The name of the method to check.</param>
        /// <returns>True if the method is supported.  False otherwise.</returns>
        private bool IsMethodSupported(string methodName)
        {
            // Methods not in the dictionary are available in all versions.
            var result = true;

            if(methodSupportingVersions.ContainsKey(methodName))
            {
                result = effectiveVersion >= methodSupportingVersions[methodName];
            }

            return result;
        }

        #endregion
    }
}
