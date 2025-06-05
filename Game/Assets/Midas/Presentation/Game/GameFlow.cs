using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Cabinet;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.StageHandling;
using static Midas.Core.StateMachine.StateMachineService;
using static Midas.Presentation.Game.GameBase;
using StatusDatabase = Midas.Presentation.Data.StatusDatabase;

namespace Midas.Presentation.Game
{
	public sealed class GameFlow : IPresentationController
	{
		private Coroutine stateMachine;
		private GameFlowStatus gameFlowStatus;
		private bool waitingForGameState = true;
		private GameState? gameState;

		private StageController stageController;
		private IMessage presDataMessage;

		public GameFlow()
		{
			gameFlowStatus = StatusDatabase.GameFlowStatus;
		}

		void IPresentationController.Init()
		{
			StartStateMachine();
			Communication.PresentationDispatcher.AddHandler<GameCycleStepMessage>(OnGameCycleStep);
			Communication.PresentationDispatcher.AddHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);
			stageController = GameBase.GameInstance.GetPresentationController<StageController>();
		}

		void IPresentationController.DeInit()
		{
			stageController.DeactivateAll();
			Communication.PresentationDispatcher.RemoveHandler<GameCycleStepMessage>(OnGameCycleStep);
			Communication.PresentationDispatcher.RemoveHandler<GameStopGameEngineMessage>(OnGameStopGameEngine);
			StopStateMachine();
		}

		void IPresentationController.Destroy()
		{
			gameFlowStatus = null;
		}

		private IEnumerator<CoroutineInstruction> BootstrapGame()
		{
			// Wait for presentation setup to be complete.

			while (!StatusDatabase.GameStatus.IsSetupPresentationDone)
				yield return null;

			// Wait until we start a game step.

			while (!gameState.HasValue)
				yield return null;

			stateMachine = FrameUpdateRoot.StartCoroutine(RunGame, gameState.Value, nameof(GameFlow));
		}

		private IEnumerator<CoroutineInstruction> RunGame(IStateInfo<GameState> stateInfo)
		{
			RecoverStage();

			StatusDatabase.GameStatus.IsGameFlowReady = true;

			// Give the presentation some time to settle.

			yield return new CoroutineDelay(TimeSpan.FromMilliseconds(50));
			CabinetManager.Cabinet.SetReadyState(true);

			while (true)
			{
				while (StatusDatabase.GameStatus.GameLogicPaused)
					yield return null;

				switch (stateInfo.CurrentState)
				{
					case GameState.Idle:
						yield return new CoroutineRun(DoIdle(), "Idle");
						break;
					case GameState.StartingCreditPlayoff:
						yield return new CoroutineRun(DoStarting(), "StartingCreditPlayoff");
						break;
					case GameState.ShowCreditPlayoffResult:
						yield return new CoroutineRun(DoShowResult(), "ShowCreditPlayoffResult");
						break;
					case GameState.Starting:
						yield return new CoroutineRun(DoStarting(), "Starting");
						break;
					case GameState.Continuing:
						yield return new CoroutineRun(DoContinuing(), "Continuing");
						break;
					case GameState.ShowResult:
						yield return new CoroutineRun(DoShowResult(), "Show Result");
						break;
					case GameState.OfferGamble:
						yield return new CoroutineRun(DoOfferGamble(), "Offer Gamble");
						break;
					case GameState.StartingGamble:
						yield return new CoroutineRun(DoGambleIdle(), "Gamble Idle");
						break;
					case GameState.ShowGambleResult:
						yield return new CoroutineRun(DoGambleShowResult(), "Gamble Show Result");
						break;
					case GameState.History:
						yield return new CoroutineRun(DoHistory(), "History");
						break;
				}

				if (stateInfo.CurrentState != gameState)
					yield return stateInfo.SetNextState(gameState!.Value);
			}

			// ReSharper disable once IteratorNeverReturns - By design
		}

		private IEnumerator<CoroutineInstruction> DoIdle()
		{
			Log.Instance.Debug("Entering Idle State");

			if (StatusDatabase.GaffStatus.IsSelfPlayActive)
			{
				var totalBetAsMoney = Money.FromCredit(GameStatus.TotalBet);
				if (totalBetAsMoney > StatusDatabase.BankStatus.BankMeter)
				{
					if (StatusDatabase.GaffStatus.IsSelfPlayAddCreditsActive)
					{
						var amountToAdd = StatusDatabase.GaffStatus.AddCreditsAmount;
						if (amountToAdd < totalBetAsMoney)
							amountToAdd = totalBetAsMoney * new RationalNumber(100, 1);
						Communication.ToLogicSender.Send(new ShowAddMoneyMessage(amountToAdd));
						StatusDatabase.GameStatus.GamePlayRequested = true;
					}
					else
					{
						StatusDatabase.GaffStatus.IsSelfPlayActive = false;
					}
				}
				else
				{
					StatusDatabase.GameStatus.GamePlayRequested = true;
				}
			}

			gameFlowStatus.AutoPlayCanStart = true;

			while (!StatusDatabase.GameStatus.GamePlayRequested)
				yield return null;

			gameFlowStatus.AutoPlayCanStart = false;

			if (StatusDatabase.ConfigurationStatus.MachineConfig.AreShowFeaturesEnabled && StatusDatabase.GaffStatus.SelectedGaffIndex.HasValue)
				Communication.ToLogicSender.Send(new DemoActivateGaffMessage(StatusDatabase.GaffStatus.SelectedGaffIndex!.Value));

			ResetForNewGame();

			yield return new CoroutineRun(SendPresCompleteAndWait(new GameStartMessage()), "Wait for game state");
			StatusDatabase.GameStatus.GamePlayRequested = false;
		}

		private IEnumerator<CoroutineInstruction> DoStarting()
		{
			Log.Instance.Debug("Entering Starting State");

			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			var gh = GameInstance.GetFirstEnabledGaffHandler();
			if (gh != null)
				yield return new CoroutineRun(gh.Run(), "Wait for Gaff");

			var uh = GameInstance.GetUtilityHandler();
			if (uh is { IsEnable: true })
				yield return new CoroutineRun(uh.Run(), "Wait for Utility");

			yield return new CoroutineRun(SendPresCompleteAndWait(new PresentationCompleteMessage()), "Wait for game state");
		}

		private IEnumerator<CoroutineInstruction> DoContinuing()
		{
			Log.Instance.Debug("Entering Continuing State");

			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			var gh = GameInstance.GetFirstEnabledGaffHandler();
			if (gh != null)
				yield return new CoroutineRun(gh.Run(), "Wait for Gaff");

			var uh = GameInstance.GetUtilityHandler();
			if (uh is { IsEnable: true })
				yield return new CoroutineRun(uh.Run(), "Wait for Utility");

			yield return new CoroutineRun(SendPresCompleteAndWait(new PresentationCompleteMessage()), "Wait for game state");
		}

		private IEnumerator<CoroutineInstruction> DoShowResult()
		{
			Log.Instance.Debug("Entering ShowResult State");

			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			yield return new CoroutineRun(SendPresCompleteAndWait(new PresentationCompleteMessage()), "Wait for game state");
			StatusDatabase.WinPresentationStatus.WinPresentationComplete = false;
		}

		private IEnumerator<CoroutineInstruction> DoOfferGamble()
		{
			Log.Instance.Debug("Entering OfferGamble State");

			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			yield return new CoroutineRun(SendPresCompleteAndWait(new OfferGambleCompleteMessage(StatusDatabase.GameStatus.OfferGambleRequest == OfferGambleRequest.Gamble)), "Wait for game state");
		}

		private IEnumerator<CoroutineInstruction> DoGambleIdle()
		{
			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			yield return new CoroutineRun(SendPresCompleteAndWait(new PresentationCompleteMessage()), "Wait for game state");

			if (gameState != GameState.ShowGambleResult)
			{
				stageController.SwitchTo(StatusDatabase.GameStatus.NextLogicStage);

				while (stageController.IsTransitioning())
					yield return null;
			}
		}

		private IEnumerator<CoroutineInstruction> RunNodes()
		{
			while (true)
			{
				var nodes = GameInstance.GetReadyNodes();
				if (nodes.Count == 0)
					yield break;

				presDataMessage = gameFlowStatus.AddToPresentationNodeHistory(nodes);
				gameFlowStatus.RunningNodes = nodes;
				nodes.Show();

				// Wait one frame for nodes to settle

				yield return null;

				while (!nodes.AreAllComplete())
					yield return null;
				gameFlowStatus.RunningNodes = Array.Empty<IPresentationNode>();
			}
		}

		private IEnumerator<CoroutineInstruction> DoGambleShowResult()
		{
			yield return new CoroutineRun(RunNodes(), "Running Nodes");

			if (StatusDatabase.GameStatus.GameLogicPaused)
				yield break;

			yield return new CoroutineRun(SendPresCompleteAndWait(new PresentationCompleteMessage()), "Wait for game state");

			if (gameState != GameState.StartingGamble)
			{
				stageController.SwitchTo(StatusDatabase.GameStatus.NextLogicStage);

				while (stageController.IsTransitioning())
					yield return null;
			}
		}

		private IEnumerator<CoroutineInstruction> DoHistory()
		{
			Log.Instance.Debug("Entering History State");
			var historyStatus = StatusDatabase.HistoryStatus;

			var historyNodes = GameInstance.GetReadyHistoryNodes();
			historyNodes.ShowHistory();

			while (!StatusDatabase.GameStatus.GameLogicPaused)
			{
				if (historyStatus.RefreshRequired)
				{
					historyNodes.HideHistory();
					RecoverStage();
					historyStatus.RefreshRequired = false;

					// Give the presentation some time to settle.

					yield return new CoroutineDelay(TimeSpan.FromMilliseconds(50));

					historyNodes = GameInstance.GetReadyHistoryNodes();
					historyNodes.ShowHistory();
				}

				yield return null;
			}

			historyNodes.HideHistory();
		}

		private IEnumerator<CoroutineInstruction> SendPresCompleteAndWait(IMessage presCompleteMessage)
		{
			waitingForGameState = true;

			if (presDataMessage != null)
			{
				Communication.ToLogicSender.EnqueueMessage(presDataMessage);
				presDataMessage = null;
			}

			Communication.ToLogicSender.EnqueueMessage(presCompleteMessage);
			Communication.ToLogicSender.SendEnqueuedMessages();

			while (waitingForGameState && !StatusDatabase.GameStatus.GameLogicPaused)
				yield return null;
		}

		private static void ResetForNewGame()
		{
			StatusDatabase.ResetForNewGame();
		}

		private void RecoverStage()
		{
			// Recovering the game state (ie, what result to show on the reels depending on the game step) is to be handled by the SceneActivator.

			Stage stage;

			switch (gameState)
			{
				case GameState.Idle:
				case GameState.Starting:
					stage = StatusDatabase.GameStatus.NextLogicStage;
					break;

				case GameState.StartingGamble:
				case GameState.ShowGambleResult:
				case GameState.History when StatusDatabase.HistoryStatus.HistoryStepType == HistoryStepType.Gamble:
					stage = Stages.Gamble;
					break;

				default:
					stage = StatusDatabase.GameStatus.CurrentLogicStage;
					break;
			}

			stageController.ActivateStage(stage, true);
		}

		private void OnGameCycleStep(GameCycleStepMessage msg)
		{
			waitingForGameState = false;
			gameState = msg.GameState;
			StatusDatabase.GameStatus.CurrentGameState = msg.GameState;
		}

		private void StartStateMachine() => stateMachine = FrameUpdateRoot.StartCoroutine(BootstrapGame(), nameof(GameFlow) + "Bootstrap");

		private void StopStateMachine()
		{
			stateMachine?.Stop();
			stateMachine = null;
			StatusDatabase.GameStatus.IsGameFlowReady = false;
		}

		private void OnGameStopGameEngine(GameStopGameEngineMessage obj)
		{
			// Restart the state machine.

			gameState = null;
			StopStateMachine();
			StartStateMachine();
		}
	}
}