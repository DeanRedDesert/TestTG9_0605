using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.StageHandling;
using Midas.Presentation.WinPresentation;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private Coroutine gameMessagesCoroutine;
		private IGameMessages messageOverride;

		public void SetGameMessageOverride(IGameMessages gameMessages)
		{
			messageOverride = gameMessages;
		}

		private void InitGameMessages()
		{
			gameMessagesCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(GameInfoCoroutine(), "GameMessages");
		}

		private void DeInitGameMessages()
		{
			gameMessagesCoroutine?.Stop();
			gameMessagesCoroutine = null;
		}

		private IEnumerator<CoroutineInstruction> GameInfoCoroutine()
		{
			while (!StatusDatabase.IsInitialised)
				yield return null;

			while (StatusDatabase.ConfigurationStatus.GameIdentity == null || StatusDatabase.ConfigurationStatus.AncillaryConfig == null || StatusDatabase.GameStatus.StakeCombinations == null)
				yield return null;

			if (!StatusDatabase.ConfigurationStatus.GameIdentity.Value.IsGlobalGi())
				yield break;

			var playoffStatus = StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>();
			var gameStatus = StatusDatabase.GameStatus;
			var detailedWinPresStatus = StatusDatabase.DetailedWinPresStatus;
			var stageStatus = StatusDatabase.StageStatus;
			var autoPlayStatus = StatusDatabase.AutoPlayStatus;
			var configStatus = StatusDatabase.ConfigurationStatus;
			var winPresStatus = StatusDatabase.WinPresentationStatus;
			var gameSpeedStatus = StatusDatabase.GameSpeedStatus;
			while (true)
			{
				dashboardStatus.GameMessageLeft = UpdateLeft(gameStatus, playoffStatus, stageStatus, winPresStatus, detailedWinPresStatus, autoPlayStatus, gameSpeedStatus);
				dashboardStatus.GameMessageRight = UpdateRight(configStatus, gameStatus, detailedWinPresStatus, autoPlayStatus);
				yield return null;
			}
		}

		private static GameMessageRight UpdateRight(ConfigurationStatus configStatus, GameStatus gameStatus, DetailedWinPresStatus detailedWinPresStatus, AutoPlayStatus autoPlayStatus)
		{
			var hasWin = StatusDatabase.BankStatus.WinMeter > Money.Zero;
			var gameIsIdle = gameStatus.GameIsIdle;
			var gameIsRevealing = gameStatus.CurrentGameState == GameState.Starting || gameStatus.CurrentGameState == GameState.Continuing;
			var gambleIsEnabled = configStatus.AncillaryConfig.Enabled;
			var autoPlayActive = autoPlayStatus.State == AutoPlayState.Active;
			var isDetailWindPresActive = detailedWinPresStatus.IsActive;

			// Game is idle, with no win, no detailed win presentation and autoplay is not active.
			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (gameIsIdle && !hasWin && !autoPlayActive && !isDetailWindPresActive)
				return GameMessageRight.GameOver;

			// Game is playing, with win, autoplay is not active and gamble is enabled.
			if (!gameIsIdle && hasWin && !autoPlayActive && gambleIsEnabled)
				return GameMessageRight.GamePays;

			// Game is playing, with win, autoplay is not active and gamble is enabled.
			if (autoPlayActive && isDetailWindPresActive && !gameIsRevealing)
				return GameMessageRight.GamePays;

			// Game is playing, with win, autoplay is not active and gamble isn't enabled.
			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (!gameIsIdle && hasWin && !autoPlayActive)
				return GameMessageRight.GamePaysGameOver;

			// Game is idle, with win, autoplay is not active.
			if (gameIsIdle && hasWin && !autoPlayActive)
				return GameMessageRight.GamePaysGameOver;

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (gameIsRevealing || autoPlayActive)
				return GameMessageRight.GoodLuck;

			return GameMessageRight.Nothing;
		}

		private GameMessageLeft UpdateLeft(GameStatus gameStatus, CreditPlayoffStatusBase playoffStatus, StageStatus stageStatus, WinPresentationStatus winPresentationStatus,
			DetailedWinPresStatus detailedWinPresStatus, AutoPlayStatus autoPlayStatus, GameSpeedStatus gameSpeedStatus)
		{
			var isSuperFastMode = autoPlayStatus.State == AutoPlayState.Active && winPresentationStatus.CurrentWinLevel == 0 && winPresentationStatus.CurrentWinLevel == 0 && gameSpeedStatus.GameSpeed == GameSpeed.SuperFast;

			if (!isSuperFastMode && CheckForPlayoffPlayFromOfferGamble(playoffStatus))
				return GameMessageLeft.Nothing;

			if (messageOverride != null && messageOverride.GetGameSpecificGameMessage(out var m))
				return m;

			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (!isSuperFastMode && CheckForGameWinMessages(detailedWinPresStatus, out var gameWinMessage))
				return gameWinMessage;

			if (!isSuperFastMode && CheckForGamblePlayoffChangeBet(gameStatus, playoffStatus, out var gameFlowMessage))
				return gameFlowMessage;

			// ReSharper disable once ConvertIfStatementToReturnStatement
			if (CheckForPlayMessages(gameStatus, stageStatus, out var playMessage))
				return playMessage;

			return GameMessageLeft.Nothing;
		}

		private static bool CheckForPlayoffPlayFromOfferGamble(CreditPlayoffStatusBase playoffStatus)
		{
			return StatusDatabase.GameStatus.GamePlayRequested && (playoffStatus.IsAvailable || playoffStatus.IsPlaying);
		}

		private bool CheckForGameWinMessages(DetailedWinPresStatus detailedWinPresStatus, out GameMessageLeft gameWinMessage)
		{
			gameWinMessage = GameMessageLeft.Nothing;
			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (!detailedWinPresStatus.IsActive)
				return false;

			if (detailedWinPresStatus.Visibility == VisibilityType.HiddenBecauseWaitBetweenCycles || detailedWinPresStatus.HighlightedWin == null)
				return false;

			if (detailedWinPresStatus.Visibility == VisibilityType.HiddenBecauseWaitBetweenWins || detailedWinPresStatus.HighlightedWin == null)
				return true;

			if (messageOverride != null && messageOverride.IsFeatureTriggerPrize(detailedWinPresStatus.HighlightedWin, out var isRetrigger))
			{
				gameWinMessage = isRetrigger ? GameMessageLeft.FreeGameReTriggerPays : GameMessageLeft.FreeGameTriggerPays;
				return true;
			}

			if (detailedWinPresStatus.HighlightedWin.PatternName.StartsWith("line", StringComparison.InvariantCultureIgnoreCase))
			{
				gameWinMessage = GameMessageLeft.LinePay;
				return true;
			}

			// ReSharper disable once InvertIf
			if (detailedWinPresStatus.HighlightedWin.PatternName.StartsWith("scatter", StringComparison.InvariantCultureIgnoreCase))
			{
				gameWinMessage = GameMessageLeft.ScatterPay;
				return true;
			}

			return false;
		}

		private bool CheckForPlayMessages(GameStatus gameStatus, StageStatus stageStatus, out GameMessageLeft gameMessage)
		{
			gameMessage = GameMessageLeft.Nothing;
			if (messageOverride != null && messageOverride.UseBonusPlay())
			{
				gameMessage = GameMessageLeft.BonusPlay;
				return true;
			}

			var stakeCombination = gameStatus.StakeCombinations[gameStatus.SelectedStakeCombinationIndex];
			if (stakeCombination.Values.TryGetValue(Stake.LinesBet, out var lb))
			{
				if (stageStatus.DesiredStage != null && stageStatus.DesiredStage.Id != Stages.GameSpecificStartId)
				{
					gameMessage = lb == 1 ? GameMessageLeft.BonusLinePlay : GameMessageLeft.BonusLinesPlay;
					return true;
				}

				gameMessage = lb == 1 ? GameMessageLeft.LinePlay : GameMessageLeft.LinesPlay;
				return true;
			}

			if (stakeCombination.Values.TryGetValue(Stake.Multiway, out _))
			{
				if (stageStatus.DesiredStage != null && stageStatus.DesiredStage.Id != Stages.GameSpecificStartId)
				{
					gameMessage = GameMessageLeft.BonusMultiwayPlay;
					return true;
				}

				gameMessage = GameMessageLeft.MultiwayPlay;
				return true;
			}

			return false;
		}

		private static bool CheckForGamblePlayoffChangeBet(GameStatus gameStatus, CreditPlayoffStatusBase playoffStatus, out GameMessageLeft gameFlowMessage)
		{
			gameFlowMessage = GameMessageLeft.Nothing;

			var gambleActive = gameStatus.CurrentGameState == GameState.OfferGamble || gameStatus.CurrentGameState == GameState.StartingGamble || gameStatus.CurrentGameState == GameState.ShowGambleResult;
			var gambleOrTakeWinOffering = gameStatus.CurrentGameState == GameState.OfferGamble || gameStatus.CurrentGameState == GameState.StartingGamble;
			var creditPlayOffOffered = playoffStatus.IsAvailable;
			var creditPlayOffRevaIsRevealingOutcome = playoffStatus.IsPlaying;

			var lowerBetWithEnoughMoneyExists = false;
			if (creditPlayOffOffered)
			{
				var currentBet = StatusDatabase.GameStatus.StakeCombinations[StatusDatabase.GameStatus.SelectedStakeCombinationIndex].TotalBet;
				var minBet = Money.FromCredit(StatusDatabase.ConfigurationStatus.GameConfig.MinBetLimit);
				lowerBetWithEnoughMoneyExists = Money.FromCredit(currentBet) >= minBet;
			}

			// ReSharper disable once ConvertIfStatementToSwitchStatement
			if (creditPlayOffOffered && !lowerBetWithEnoughMoneyExists && !gambleOrTakeWinOffering && !gambleActive)
			{
				gameFlowMessage = GameMessageLeft.StartPlayOff;
				return true;
			}

			if (creditPlayOffOffered && lowerBetWithEnoughMoneyExists && !gambleOrTakeWinOffering && !gambleActive)
			{
				gameFlowMessage = GameMessageLeft.StartPlayOffOrChangeBet;
				return true;
			}

			if (creditPlayOffOffered && !lowerBetWithEnoughMoneyExists && gambleOrTakeWinOffering)
			{
				gameFlowMessage = GameMessageLeft.PressGambleOrStartPlayOff;
				return true;
			}

			if (creditPlayOffOffered && lowerBetWithEnoughMoneyExists && gambleOrTakeWinOffering)
			{
				gameFlowMessage = GameMessageLeft.PressGambleOrStartPlayOffOrChangeBet;
				return true;
			}

			if (creditPlayOffRevaIsRevealingOutcome)
			{
				gameFlowMessage = GameMessageLeft.PlayingPlayOffWithCredits;
				return true;
			}

			return false;
		}
	}
}
