using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.General;
using Midas.Core.ExtensionMethods;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.StageHandling;

namespace Midas.Presentation.Data.StatusBlocks
{
	public enum OfferGambleRequest
	{
		None,
		Gamble,
		TakeWin
	}

	public sealed class GameStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyList<IStakeCombination>> stakeCombinations;
		private StatusProperty<int> selectedStakeCombinationIndex;
		private StatusProperty<FoundationGameMode?> gameMode;
		private StatusProperty<bool> isSetupPresentationDone;
		private StatusProperty<bool> isGameFlowReady;
		private StatusProperty<GameState?> currentGameState;
		private StatusProperty<Stage> currentLogicStage;
		private StatusProperty<Stage> nextLogicStage;
		private StatusProperty<bool> gameIsIdle;
		private StatusProperty<bool> gameIsActive;
		private StatusProperty<bool> logicGameActive;
		private StatusProperty<string> currentlyVisibleStage;
		private StatusProperty<bool> gameLogicPaused;
		private StatusProperty<bool> gamePlayRequested;
		private StatusProperty<OfferGambleRequest> offerGambleRequest;
		private StatusProperty<bool> gamblePlayRequested;
		private StatusProperty<bool> inActivePlay;
		private StatusProperty<bool> inUtilityMode;
		private StatusProperty<string> gameName;
		private StatusProperty<Money> gameMinimumBet;
		private StatusProperty<bool> hasWinCapBeenReached;
		private StatusProperty<DateTime> gameTime;

		public IReadOnlyList<IStakeCombination> StakeCombinations => stakeCombinations.Value;

		/// <summary>
		/// Gets the currently selected stake combination index.
		/// </summary>
		public int SelectedStakeCombinationIndex => selectedStakeCombinationIndex.Value;

		[Expression("Game")]
		public static IStakeCombination SelectedStakeCombination { get; private set; }

		[Expression("Game")]
		public static Credit TotalBet => SelectedStakeCombination?.TotalBet ?? Credit.Zero;

		[Expression("Game")]
		public static long BetMultiplier { get; private set; }

		[Expression("Game")]
		public static long LinesBet { get; private set; }

		[Expression("Game")]
		public static long AnteBet { get; private set; }

		[Expression("Game")]
		public static long MultiwayBet { get; private set; }

		public FoundationGameMode? GameMode => gameMode.Value;

		/// <summary>
		/// Tells us that the logic is ready for presentation messages.
		/// </summary>
		public bool IsSetupPresentationDone
		{
			get => isSetupPresentationDone.Value;
			set
			{
				isSetupPresentationDone.Value = value;
				RefreshIsInActiveGamePlay();
				RefreshIsInUtilityMode();
			}
		}

		/// <summary>
		/// Tells the rest of the presentation that the game flow has initialised, recovered the current scene, and is ready to start the first node.
		/// There is a 50ms delay before game flow starts the engine.
		/// </summary>
		public bool IsGameFlowReady
		{
			get => isGameFlowReady.Value;
			set => isGameFlowReady.Value = value;
		}

		public GameState? CurrentGameState
		{
			get => currentGameState.Value;
			set
			{
				currentGameState.Value = value;
				RefreshGameStateProperties();
			}
		}

		public Stage CurrentLogicStage
		{
			get => currentLogicStage.Value;
			private set => currentLogicStage.Value = value;
		}

		public Stage NextLogicStage
		{
			get => nextLogicStage.Value;
			private set => nextLogicStage.Value = value;
		}

		public bool GameIsIdle
		{
			get => gameIsIdle.Value;
			set => gameIsIdle.Value = value;
		}

		public bool GameIsActive
		{
			get => gameIsActive.Value;
			set => gameIsActive.Value = value;
		}

		public bool LogicGameActive
		{
			get => logicGameActive.Value;
			set => logicGameActive.Value = value;
		}

		public string CurrentlyVisibleStage
		{
			get => currentlyVisibleStage.Value;
			set => currentlyVisibleStage.Value = value;
		}

		public bool GameLogicPaused
		{
			get => gameLogicPaused.Value;
		}

		public bool GamePlayRequested
		{
			get => gamePlayRequested.Value;
			set => gamePlayRequested.Value = value;
		}

		public OfferGambleRequest OfferGambleRequest
		{
			get => offerGambleRequest.Value;
			set => offerGambleRequest.Value = value;
		}

		public bool GamblePlayRequested
		{
			get => gamblePlayRequested.Value;
			set => gamblePlayRequested.Value = value;
		}

		public bool InActivePlay => inActivePlay.Value;

		public bool InUtilityMode => inUtilityMode.Value;

		public string GameName
		{
			get => gameName.Value;
			set => gameName.Value = value;
		}

		public Money GameMinimumBet
		{
			get => gameMinimumBet.Value;
		}

		public bool HasWinCapBeenReached
		{
			get => hasWinCapBeenReached.Value;
		}

		public DateTime GameTime
		{
			get => gameTime.Value;
		}

		public GameStatus() : base(nameof(GameStatus))
		{
		}

		public Random GetPresentationRng(string scope)
		{
			var seed = (int)(StatusDatabase.GameStatus.GameTime.Ticks & 0x7FFFFFFF) ^ scope.GetHashCode();
			return new Random(seed);
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.GameMode, OnGameModeChanged);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.BetService.StakeCombos, v =>
			{
				stakeCombinations.Value = v;
				if (v != null)
					gameMinimumBet.Value = Money.FromCredit(v.Min(sc => sc.TotalBet));
			});
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.BetService.SelectedStakeCombo, v => selectedStakeCombinationIndex.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.DisplayState, OnDisplayStateChanged);

			var stakeInfoDeps = new (StatusBlock, string)[]
			{
				(StatusDatabase.GameStatus, nameof(StakeCombinations)),
				(StatusDatabase.GameStatus, nameof(SelectedStakeCombinationIndex))
			};

			autoUnregisterHelper.RegisterMultiplePropertyChangedHandler(stakeInfoDeps, _ => RefreshStakeInformation());
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.HasWinCapBeenReached, v => hasWinCapBeenReached.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.GameTime, v => gameTime.Value = v);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			stakeCombinations = AddProperty(nameof(StakeCombinations), default(IReadOnlyList<IStakeCombination>));
			selectedStakeCombinationIndex = AddProperty(nameof(SelectedStakeCombinationIndex), 0);
			gameMode = AddProperty(nameof(GameMode), default(FoundationGameMode?));
			isSetupPresentationDone = AddProperty(nameof(IsSetupPresentationDone), false);
			isGameFlowReady = AddProperty(nameof(IsGameFlowReady), false);
			currentGameState = AddProperty(nameof(CurrentGameState), default(GameState?));
			currentLogicStage = AddProperty(nameof(CurrentLogicStage), Stages.Undefined);
			nextLogicStage = AddProperty(nameof(NextLogicStage), Stages.Undefined);
			gameIsIdle = AddProperty(nameof(GameIsIdle), false);
			gameIsActive = AddProperty(nameof(GameIsActive), false);
			logicGameActive = AddProperty(nameof(LogicGameActive), false);
			currentlyVisibleStage = AddProperty(nameof(CurrentlyVisibleStage), default(string));
			gameLogicPaused = AddProperty(nameof(GameLogicPaused), true);
			gamePlayRequested = AddProperty(nameof(GamePlayRequested), false);
			offerGambleRequest = AddProperty(nameof(OfferGambleRequest), OfferGambleRequest.None);
			gamblePlayRequested = AddProperty(nameof(GamblePlayRequested), false);
			inActivePlay = AddProperty(nameof(InActivePlay), false);
			inUtilityMode = AddProperty(nameof(InUtilityMode), false);
			gameName = AddProperty(nameof(GameName), default(string));
			gameMinimumBet = AddProperty(nameof(GameMinimumBet), default(Money));
			hasWinCapBeenReached = AddProperty(nameof(HasWinCapBeenReached), false);
			gameTime = AddProperty(nameof(GameTime), default(DateTime));
		}

		public override void ResetForNewGame()
		{
			base.ResetForNewGame();
			OfferGambleRequest = OfferGambleRequest.None;
		}

		private void OnGameModeChanged(FoundationGameMode v)
		{
			gameMode.Value = v;
			RefreshIsInActiveGamePlay();
			RefreshIsInUtilityMode();
		}

		private void OnDisplayStateChanged(DisplayState v)
		{
			gameLogicPaused.Value = v != DisplayState.Normal;
			RefreshIsInActiveGamePlay();
		}

		private void RefreshStakeInformation()
		{
			var stakeCombo = StakeCombinations?[SelectedStakeCombinationIndex];
			SelectedStakeCombination = stakeCombo;

			long stakeMultiplier;

			if (stakeCombo == null || (stakeMultiplier = stakeCombo.Values.GetValueOrNull(Stake.BetMultiplier) ?? 0) == 0)
				return;

			BetMultiplier = stakeMultiplier;
			LinesBet = stakeCombo.Values.GetValueOrNull(Stake.LinesBet) ?? 0;
			MultiwayBet = stakeCombo.Values.GetValueOrNull(Stake.Multiway) ?? 0;
			AnteBet = stakeCombo.Values.GetValueOrNull(Stake.AnteBet) ?? 0;
		}

		private void RefreshGameStateProperties()
		{
			GameIsIdle = CurrentGameState == GameState.Idle;
			LogicGameActive = CurrentGameState == GameState.Starting || CurrentGameState == GameState.Continuing || CurrentGameState == GameState.ShowResult;
			GameIsActive = LogicGameActive || CurrentGameState == GameState.OfferGamble;

			CurrentLogicStage = GameBase.GameInstance.GetLogicStage(false);
			NextLogicStage = GameBase.GameInstance.GetLogicStage(true);
		}

		private void RefreshIsInActiveGamePlay()
		{
			inActivePlay.Value = GameMode == FoundationGameMode.Play && !GameLogicPaused && IsSetupPresentationDone;
		}

		private void RefreshIsInUtilityMode()
		{
			inUtilityMode.Value = GameMode == FoundationGameMode.Utility && IsSetupPresentationDone;
		}

#if UNITY_EDITOR

		public void OverrideStakeCombinations(IReadOnlyList<IStakeCombination> fakeStakeCombinations, int selectedStakeCombination = 0)
		{
			stakeCombinations.Value = fakeStakeCombinations;
			selectedStakeCombinationIndex.Value = selectedStakeCombination;
		}

#endif
	}
}