using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Configuration;
using Midas.Core.General;
using Midas.Core.LogicServices;
using Midas.Core.Serialization;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Data.Services;
using Midas.LogicToPresentation.Messages;
using static Midas.LogicToPresentation.Communication;

namespace Midas.Logic
{
	public sealed partial class GameLogic : IGameLogic
	{
		private IFoundationShim foundation;
		private ICreditPlayoff creditPlayoff;
		private IGame game;
		private IGamble gamble;
		private bool waitingForInputGeneric;
		private bool waitingForInputState;
		private bool isSkippingFeature;
		private bool canChangeStakeCombo;
		private bool isGambleOfferable;

		IStakeCombination IGameLogic.CurrentStakeCombination => game.StakeCombinations[logicState.SelectedStakeCombinationIndex];
		IReadOnlyList<(string LevelId, uint GameLevel)> IGameLogic.ProgressiveLevels => game.ProgressiveLevels;

		public GameLogic()
		{
			GameServices.BeginBatch();
		}

		void IGameLogic.Init(IFoundationShim foundationShim)
		{
			foundation = foundationShim;
			NvramSerializer.Init(foundation);

			ToLogicSender.MessageToMessageQueueAdded += OnLogicMessageQueueMessageAdded;

			ToPresentationSender.Send(new RequestLogicLoaderMessage());
			var logicLoader = WaitPresentationMessagesOnly<RequestLogicLoaderResponse>().LogicLoader;

			// Currently only one active paytable is supported.

			if (foundationShim.PaytableConfiguration == null || foundationShim.PaytableConfiguration.ActivePaytables.Count != 1)
			{
				throw new Exception("Unable to load paytable. Exactly one paytable must be active.");
			}

			InitLogicState();
			InitPresentationData();
			InitGameServices();
			InitHistory();
			UpdateLogicStage();
			UpdateChangeDenomState();
			UpdateChooserRequestedState();

			var isGameResetRequired = changeDenomState == ChangeDenomState.Changing || isChooserRequested;

			if (isGameResetRequired)
			{
				logicState.SelectedStakeCombinationIndex = 0;
				SaveLogicState();
				UpdateGameOverState(false);
			}
			else
				UpdateGameOverState();

			creditPlayoff = logicLoader.LoadCreditPlayoff(logicState.Configuration);
			creditPlayoff.Init(foundation, LoadCreditPlayoffHistoryState());
			game = logicLoader.LoadGame(foundationShim.GameMountPoint, foundationShim.PaytableConfiguration.PaytableFilenames[foundationShim.PaytableConfiguration.ActivePaytables[0]], logicState.Configuration);
			game.Init(foundation, isGameResetRequired, LoadGameHistoryState());
			if (logicState.HasWinCapBeenReached)
				game.ApplyWinCapping();

			var denomBetData = logicLoader.GetDenomBetData(foundationShim.GameMountPoint, logicState.Configuration);
			GameServices.ConfigurationService.DenomBetDataService.SetValue(denomBetData);
			gamble = logicLoader.LoadGamble(logicState.Configuration);
			gamble.Init(foundation, LoadGambleHistoryState());
			PresentationLoader.Init(foundation.GameIdentity);

			if (!isGameResetRequired)
				PresentationLoader.UnloadPresentation(true);

			RegisterMessages();

			ProgressiveAwardManager.Init(foundation, this);
		}

		void IGameLogic.Start()
		{
			PresentationLoader.LoadPresentation(true);
			GameServices.RefreshAll();
			GameServices.EndBatch();
			SetupPresentation();

			GameServices.BeginBatch();

			if (changeDenomState == ChangeDenomState.Changing)
			{
				UpdateChangeDenomState(ChangeDenomState.None);
				foundation.SendDenomSelectionActive(false);
			}

			if (isChooserRequested)
				UpdateChooserRequestedState(false);

			UpdateSimulatedUtilityMode();
		}

		void IGameLogic.DeInit(ShutdownReason reason)
		{
			ToPresentationSender.Send(new GameStopGameEngineMessage());

			if (changeDenomState != ChangeDenomState.Changing && !isChooserRequested)
				PresentationLoader.UnloadPresentation(reason != ShutdownReason.ExitingPlayMode);

			ProgressiveAwardManager.DeInit();

			DeregisterMessages();
			PresentationLoader.DeInit();
			ToLogicSender.MessageToMessageQueueAdded -= OnLogicMessageQueueMessageAdded;

			creditPlayoff.DeInit();
			game.DeInit();
			gamble.DeInit();

			game = null;
			gamble = null;

			NvramSerializer.DeInit();
			foundation = null;

			GameServices.AbortBatch();
			GameServices.BeginBatch();
		}

		void IGameLogic.Park(bool park)
		{
			ToPresentationSender.Send(new ParkMessage(park));
		}

		void IGameLogic.Pause(bool pause)
		{
			var message = new GameLogicPauseMessage(pause);
			ToLogicSender.Send(message);
			ToPresentationSender.Send(message);
		}

		Money IGameLogic.WaitForPlay(out IStakeCombination usedStakeCombionation)
		{
			ToPresentationSender.EnqueueMessage(new GameLogicTimingsMessage(foundation.TimingsString));
			SendGameStateAndWait<GameStartMessage>(GameState.Idle);

			Log.Instance.Info("Received game start request");

			if (foundation.ShowMode != FoundationShowMode.None)
			{
				var stakeCombinationOverride = game.CheckGaffStakeCombinationOverride();
				if (stakeCombinationOverride.HasValue && logicState.SelectedStakeCombinationIndex != stakeCombinationOverride.Value)
				{
					logicState.SelectedStakeCombinationIndex = stakeCombinationOverride.Value;
					GameServices.BetService.SelectedStakeComboService.SetValue(stakeCombinationOverride.Value);
				}
			}

			usedStakeCombionation = game.StakeCombinations[logicState.SelectedStakeCombinationIndex];
			var bet = Money.FromCredit(usedStakeCombionation.TotalBet);
			var cash = GameServices.MetersService.CreditsService.Value;

			if (foundation.GameMode == FoundationGameMode.Play && creditPlayoff.TryCommit())
			{
				bet = cash;
				UpdateLogicStage(LogicStage.CreditPlayoff);
			}
			else
			{
				UpdateLogicStage(LogicStage.GameStart);
			}

			logicState.TotalProgressiveAwardedValue = Money.Zero;
			logicState.HasWinCapBeenReached = false;
			logicState.GameTime = DateTime.Now;
			GameServices.MachineStateService.HasWinCapBeenReachedService.SetValue(false);
			GameServices.MachineStateService.IsGambleOfferableService.SetValue(false);
			GameServices.MachineStateService.GameTimeService.SetValue(logicState.GameTime);

			ResetHistory();
			SaveLogicState();
			return bet;
		}

		IOutcome IGameLogic.StartGameCycle()
		{
			var gameState = logicStage switch
			{
				LogicStage.CreditPlayoff => GameState.StartingCreditPlayoff,
				LogicStage.GameStart => GameState.Starting,
				LogicStage.GameFeature => GameState.Continuing,
				_ => throw new InvalidOperationException("Invalid logic stage")
			};

			if (gameState != GameState.Continuing)
				UpdateGameOverState(false);

			SendGameStateAndWait<PresentationCompleteMessage>(gameState);
			Log.Instance.Info("Received start game presentation complete");

			IOutcome outcome;
			switch (gameState)
			{
				case GameState.StartingCreditPlayoff:
					outcome = creditPlayoff.StartCreditPlayoff();
					break;
				case GameState.Starting:
					outcome = game.StartGame(logicState.SelectedStakeCombinationIndex);
					DoWinLimitChecks();
					break;
				case GameState.Continuing:
					outcome = game.ContinueGame();
					DoWinLimitChecks();
					break;
				default:
					throw new InvalidOperationException("Invalid game state, we should never get here.");
			}

			ProgressiveAwardManager.SetPotentialHits(outcome.Prizes, gameState == GameState.Starting);
			SaveLogicState();
			return outcome;

			void DoWinLimitChecks()
			{
				outcome = CheckWinLimit(outcome);
				if (logicState.HasWinCapBeenReached)
					game.ApplyWinCapping();
			}
		}

		void IGameLogic.ShowResult()
		{
			UpdateIsGambleOfferable(GameServices.MetersService.TotalAwardService.Value);
			LogicStage? nextLogicStage = null;
			var gameState = GameState.ShowResult;
			var historyStepType = HistoryStepType.Game;

			switch (logicStage)
			{
				case LogicStage.CreditPlayoff:
					nextLogicStage = LogicStage.GameStart;
					gameState = GameState.ShowCreditPlayoffResult;
					historyStepType = HistoryStepType.CreditPlayoff;
					break;

				case LogicStage.GameStart:
					nextLogicStage = LogicStage.GameFeature;
					break;
			}

			SaveHistoryStep(historyStepType);
			SendGameStateAndWait<PresentationCompleteMessage>(gameState);
			CommitHistoryStep();

			if (nextLogicStage.HasValue)
				UpdateLogicStage(nextLogicStage);

			Log.Instance.Info("Received show result presentation complete");
		}

		void IGameLogic.EndGame()
		{
			UpdateIsGambleOfferable(GameServices.MetersService.TotalAwardService.Value);

			if (foundation.FoundationType == FoundationType.Ascent)
				creditPlayoff.Offer(GameServices.MetersService.CreditsService.Value + GameServices.MetersService.TotalAwardService.Value, Money.FromCredit(game.StakeCombinations[logicState.SelectedStakeCombinationIndex].TotalBet));
		}

		void IGameLogic.Finalise()
		{
			UpdateIsGambleOfferable(GameServices.MetersService.TotalAwardService.Value);
			UpdateAutoPlay();
			UpdateGameOverState(true);
			creditPlayoff.Offer(GameServices.MetersService.CreditsService.Value, Money.FromCredit(game.StakeCombinations[logicState.SelectedStakeCombinationIndex].TotalBet));
		}

		bool IGameLogic.OfferGamble()
		{
			if (isSkippingFeature)
			{
				isSkippingFeature = false;
				PresentationLoader.UnloadPresentation(true);
				((IGameLogic)this).Start();
			}

			UpdateIsGambleOfferable(GameServices.MetersService.TotalAwardService.Value);
			SetWaitingForInputState(true);

			var offerGambleResult = SendGameStateAndWait<OfferGambleCompleteMessage>(GameState.OfferGamble);

			SetWaitingForInputState(false);

			if (offerGambleResult.GambleRequested)
				creditPlayoff.Reset();
			return offerGambleResult.GambleRequested;
		}

		IOutcome IGameLogic.StartGamble(bool isFirstGambleCycle)
		{
			SetWaitingForInputState(true);
			SendGameStateAndWait<PresentationCompleteMessage>(GameState.StartingGamble);

			Log.Instance.Info("Received start gamble presentation complete");

			var outcome = gamble.StartGamble(isFirstGambleCycle);

			SetWaitingForInputState(false);
			SaveLogicState();
			return outcome;
		}

		void IGameLogic.ShowGambleResult()
		{
			SaveHistoryStep(HistoryStepType.Gamble);
			SendGameStateAndWait<PresentationCompleteMessage>(GameState.ShowGambleResult);
			CommitHistoryStep();

			Log.Instance.Info("Received show result presentation complete");
		}

		void IGameLogic.ShowHistory()
		{
			if (!foundation.TryReadNvram(NvramScope.History, nameof(HistorySnapshotType.GameStart), out object historyData))
			{
				Log.Instance.Fatal("Unable to load history game complete snapshot data.");
			}

			var historyStep = LoadHistory();

			GameServices.RestoreHistoryData(HistorySnapshotType.GameStart, historyData);
			GameServices.RestoreHistoryData(HistorySnapshotType.GameCycle, historyStep.ServiceData);
			LoadHistoryPresentationData(historyStep.PresentationData);
			ToPresentationSender.EnqueueMessage(new PresentationDataMessage(presentationData));

			switch (historyStep.HistoryStepType)
			{
				case HistoryStepType.CreditPlayoff:
					creditPlayoff.ShowHistory(historyStep.GameData);
					break;
				case HistoryStepType.Game:
					game.ShowHistory(historyStep.GameData);
					break;
				case HistoryStepType.Gamble:
					gamble.ShowHistory(historyStep.GameData);
					break;
			}

			ToPresentationSender.EnqueueMessage(new HistoryDetailsMessage(currentHistoryStepIndex, historyList.Count, historyStep.HistoryStepType, foundation.DemoGetHistoryRecordCount(), foundation.DemoIsNextHistoryRecordAvailable(), foundation.DemoIsPreviousHistoryRecordAvailable()));
			var historyPresentationCommand = SendGameStateAndWait<HistoryPresentationCommand>(GameState.History);
			switch (historyPresentationCommand.Command)
			{
				case HistoryCommand.FirstStep:
					currentHistoryStepIndex = 0;
					break;

				case HistoryCommand.LastStep:
					currentHistoryStepIndex = historyList.Count - 1;
					break;

				case HistoryCommand.NextStep:
					if (currentHistoryStepIndex < historyList.Count - 1)
						currentHistoryStepIndex++;
					break;

				case HistoryCommand.PreviousStep:
					if (currentHistoryStepIndex > 0)
						currentHistoryStepIndex--;
					break;
			}
		}

		void IGameLogic.SetDisplayState(DisplayState displayState)
		{
			GameServices.MachineStateService.DisplayStateService.SetValue(displayState);
		}

		void IGameLogic.SetBankMeters(MoneyEvent moneyEvent, Money bank, Money paid)
		{
			var creditsChanged = GameServices.MetersService.CreditsService.SetValue(bank);

			if (moneyEvent != MoneyEvent.MoneyWon && !foundation.IsInitialising && creditsChanged
				&& GameServices.MachineStateService.GameModeService.Value == FoundationGameMode.Play)
			{
				creditPlayoff?.Decline();
			}

			GameServices.MetersService.PaidService.SetValue(paid);
		}

		void IGameLogic.SetWagerableMeter(Money wagerable)
		{
			GameServices.MetersService.WagerableService.SetValue(wagerable);
		}

		void IGameLogic.MoneyIn(Money amount, MoneySource source)
		{
			ToPresentationSender.Send(new MoneyInMessage(amount, source));
		}

		void IGameLogic.MoneyOut(Money amount, MoneyTarget target)
		{
			ToPresentationSender.Send(new MoneyOutMessage(amount, target));
		}

		void IGameLogic.SetAwardValues(Money cycleAward, Money totalAward)
		{
			GameServices.MetersService.CycleAwardService.SetValue(cycleAward);
			GameServices.MetersService.TotalAwardService.SetValue(totalAward);
		}

		void IGameLogic.SetBankStatus(bool isPlayerWagerOfferable, bool isCashOutOfferable)
		{
			GameServices.MachineStateService.IsPlayerWagerAvailableService.SetValue(isPlayerWagerOfferable);
			GameServices.MachineStateService.IsCashoutAvailableService.SetValue(isCashOutOfferable);
		}

		void IGameLogic.SetIsChooserAvailable(bool isAvailable)
		{
			GameServices.MachineStateService.IsChooserAvailableService.SetValue(isAvailable);
		}

		void IGameLogic.SetReserveConfig(bool allowedWithCredits, bool allowedWithoutCredits, TimeSpan timeoutWithCredits, TimeSpan timeoutWithoutCredits)
		{
			GameServices.ConfigurationService.ReserveParametersService.SetValue(new ReserveParameters(allowedWithCredits, allowedWithoutCredits, timeoutWithCredits, timeoutWithoutCredits));
		}

		void IGameLogic.SetMessages(IReadOnlyList<string> messages)
		{
			GameServices.MachineStateService.MessagesService.SetValue(messages.ToList());
		}

		void IGameLogic.SetSlamSpinConfig(bool allowed, bool allowedInFeature, bool isImmediate, bool allowDppButton, bool recordUsage)
		{
			GameServices.ConfigurationService.IsSlamSpinAllowedService.SetValue(allowed);
			GameServices.ConfigurationService.IsSlamSpinAllowedInFeaturesService.SetValue(allowedInFeature);
			GameServices.ConfigurationService.IsSlamSpinImmediateService.SetValue(isImmediate);
			GameServices.ConfigurationService.AllowSlamSpinDPPButtonService.SetValue(allowDppButton);
			GameServices.ConfigurationService.RecordSlamSpinUsedService.SetValue(recordUsage);
		}

		void IGameLogic.SetClockConfig(bool isClockVisible, string clockFormat)
		{
			GameServices.ConfigurationService.IsClockVisibleService.SetValue(isClockVisible);
			GameServices.ConfigurationService.ClockFormatService.SetValue(clockFormat);
		}

		void IGameLogic.SetPlayConfig(TimeSpan baseGameTime, TimeSpan freeGameTime, bool isContinuousPlayAllowed, bool isFeatureAutoStartEnabled, Credit maxBet)
		{
			GameServices.ConfigurationService.BaseGameCycleTimeService.SetValue(baseGameTime);
			GameServices.ConfigurationService.FreeGameCycleTimeService.SetValue(freeGameTime);
			GameServices.ConfigurationService.IsContinuousPlayAllowedService.SetValue(isContinuousPlayAllowed);
			GameServices.ConfigurationService.IsFeatureAutoStartEnabledService.SetValue(isFeatureAutoStartEnabled);
			GameServices.ConfigurationService.GameMaximumBetService.SetValue(maxBet);
		}

		void IGameLogic.SetQcomConfig(int qcomJurisdiction)
		{
			GameServices.ConfigurationService.QcomJurisdictionService.SetValue(qcomJurisdiction);
		}

		void IGameLogic.SetHardwareId(string cabinetId, string brainboxId, string gpu)
		{
			GameServices.ConfigurationService.CabinetIdService.SetValue(cabinetId);
			GameServices.ConfigurationService.BrainboxIdService.SetValue(brainboxId);
			GameServices.ConfigurationService.GpuService.SetValue(gpu);
		}

		void IGameLogic.SetProgressiveValues(IReadOnlyList<(string LevelId, Money Value)> broadcastList)
		{
			broadcastList = ProgressiveAwardManager.GetBroadcastData(broadcastList);
			GameServices.ProgressiveService.BroadcastDataService.SetValue(broadcastList);
		}

		void IGameLogic.SetProgressiveHits(IReadOnlyList<ProgressiveHit> hits)
		{
			ProgressiveAwardManager.SetHits(hits);
		}

		void IGameLogic.SetProgressiveVerified(int awardIndex, string levelId, ProgressiveAwardPayType payType, Money verifiedAmount)
		{
			ProgressiveAwardManager.SetVerified(awardIndex, levelId, payType, verifiedAmount);
		}

		void IGameLogic.SetProgressivePaid(int awardIndex, string levelId, Money paidAmount)
		{
			ProgressiveAwardManager.SetPaid(awardIndex, levelId, paidAmount);
		}

		ProgressiveAwardWaitState IGameLogic.GetProgressiveAwardWaitState(out int awardIndex)
		{
			return ProgressiveAwardManager.GetProgressiveAwardWaitState(out awardIndex);
		}

		void IGameLogic.SetProgressiveLevels(IReadOnlyList<ProgressiveLevel> levels)
		{
			GameServices.ProgressiveService.ProgressiveLevelsService.SetValue(levels);
			CalculateRtps(levels);
		}

		void IGameLogic.SetExternalJackpots(bool isVisible, int iconId, IReadOnlyList<ExternalJackpot> jackpots)
		{
			GameServices.ExternalJackpotService.IsVisibleService.SetValue(isVisible);
			GameServices.ExternalJackpotService.IconIdService.SetValue(iconId);
			GameServices.ExternalJackpotService.JackpotsService.SetValue(jackpots);
		}

		void IGameLogic.SetPidSession(PidSession pidSession)
		{
			GameServices.PidService.PidSessionService.SetValue(pidSession);
		}

		void IGameLogic.SetPidConfiguration(PidConfiguration pidConfiguration)
		{
			GameServices.PidService.PidConfigurationService.SetValue(pidConfiguration);
		}

		void IGameLogic.SetIsServiceRequested(bool requested)
		{
			GameServices.PidService.IsServiceRequestedService.SetValue(requested);
		}

		void IGameLogic.SetGameButtonBehaviours(IReadOnlyList<GameButtonBehaviour> behaviours)
		{
			GameServices.GameFunctionStatusService.GameButtonBehavioursService.SetValue(behaviours);
		}

		void IGameLogic.SetDenomPlayableStatus(IReadOnlyList<DenominationPlayableStatus> denomPlayableStatus)
		{
			GameServices.GameFunctionStatusService.DenominationPlayableStatusService.SetValue(denomPlayableStatus);
		}

		void IGameLogic.SetDenominationMenuTimeoutConfiguration(bool isActive, TimeSpan timeout)
		{
			GameServices.GameFunctionStatusService.TimeoutService.SetValue(timeout);
			GameServices.GameFunctionStatusService.IsTimeoutActiveService.SetValue(isActive);
		}

		public void ChangeDenom(Money newDenom, bool foundationInitiated)
		{
			if (foundation.ShouldGameLogicExit || foundation.GameMode != FoundationGameMode.Play)
				return;

			creditPlayoff?.Decline();

			if (newDenom != logicState.Configuration.DenomConfig.CurrentDenomination)
			{
				if (foundationInitiated || foundation.ChangeGameDenom(newDenom))
					UpdateChangeDenomState(ChangeDenomState.Changing);
				else
					ToPresentationSender.Send(new ChangeGameDenomCancelledMessage());
			}
		}

		void IGameLogic.SetLanguage(string language, string flag)
		{
			GameServices.ConfigurationService.CurrentLanguageService.SetValue(language);
			GameServices.ConfigurationService.CurrentFlagService.SetValue(flag);
		}

		void IGameLogic.SetPlayerSession(PlayerSession playerSession, bool isSessionTimerDisplayEnabled)
		{
			GameServices.PlayerSessionService.SessionService.SetValue(playerSession);
			GameServices.PlayerSessionService.IsSessionTimerDisplayEnabledService.SetValue(isSessionTimerDisplayEnabled);
		}

		void IGameLogic.SetPlayerSessionParameters(PlayerSessionParameters playerSessionParameters)
		{
			GameServices.PlayerSessionService.ParametersService.SetValue(playerSessionParameters);
		}

		void IGameLogic.SetFlashingPlayerClock(FlashingPlayerClock flashingPlayerClock)
		{
			GameServices.ConfigurationService.FlashingPlayerClockService.SetValue(flashingPlayerClock);
		}

		void IGameLogic.SetIsGambleOfferable(bool value)
		{
			isGambleOfferable = value;
		}

		private void UpdateIsGambleOfferable(Money totalWin)
		{
			GameServices.MachineStateService.IsGambleOfferableService.SetValue(isGambleOfferable && gamble.IsPlayPossible(totalWin));
		}

		private void SetupPresentation()
		{
			SendOverallStakeSettings();
			ToPresentationSender.SendEnqueuedMessages();

			// Any custom game setup should be done here.

			ToPresentationSender.EnqueueMessage(new PresentationDataMessage(presentationData));
			ToPresentationSender.EnqueueMessage(new GameSetupPresentationMessage());

			// Send all enqueued data to presentation.

			ToPresentationSender.SendEnqueuedMessages();

			Log.Instance.Info("SetupPresentationOnLogic done. Waiting for presentation to complete");

			// Wait for presentation to complete setup presentation

			var message = WaitPresentationMessagesOnly<GameSetupPresentationDoneMessage>();
			if (message != null)
			{
				// Send message back to presentation so that everyone gets informed about setup presentation done.
				ToPresentationSender.Send(message);
			}
		}

		private T SendGameStateAndWait<T>(GameState state) where T : class, IMessage
		{
			if (isSkippingFeature)
				return null;

			try
			{
				canChangeStakeCombo = state == GameState.Idle || state == GameState.OfferGamble;

				ToPresentationSender.EnqueueMessage(new GameCycleStepMessage(state));
				return WaitPresentation<T>();
			}
			finally
			{
				canChangeStakeCombo = false;
			}
		}

		private void SendOverallStakeSettings()
		{
			GameServices.BetService.StakeCombosService.SetValue(game.StakeCombinations);
			GameServices.BetService.SelectedStakeComboService.SetValue(logicState.SelectedStakeCombinationIndex);
		}

		private void CalculateRtps(IReadOnlyList<ProgressiveLevel> levels)
		{
			var prtp = levels.Where(level => level.IsStandalone).Sum(level => level.Rtp);
			var minimumRtp = prtp + double.Parse((string)logicState.Configuration.CustomConfig.PayVarConfigItems["MinimumBaseGameRtp"]);
			var maximumRtp = prtp + double.Parse((string)logicState.Configuration.CustomConfig.PayVarConfigItems["MaximumBaseGameRtp"]);
			GameServices.PidService.MinGameRtpService.SetValue(minimumRtp);
			GameServices.PidService.MaxGameRtpService.SetValue(maximumRtp);
		}

		private void UpdateSimulatedUtilityMode()
		{
#if UNITY_EDITOR
			var utilityThemes = foundation.DemoGetRegistrySupportedThemes();
			var enabled = utilityThemes.Count > 0;
			var denoms = enabled ? foundation.DemoGetRegistrySupportedDenominations(utilityThemes.First()) : new Dictionary<KeyValuePair<string, string>, IReadOnlyList<long>>();
			ToPresentationSender.EnqueueMessage(new UtilityModeDetailsMessage(enabled, utilityThemes, denoms));
#endif
		}
	}
}