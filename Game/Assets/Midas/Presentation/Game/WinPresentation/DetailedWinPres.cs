using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Interruption;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.Game.WinPresentation
{
	public sealed class DetailedWinPres : IDetailedWinPres, IInterruptable
	{
		private WinPresentationStatus winPresentationStatus;
		private DetailedWinPresStatus detailedWinPresStatus;
		private GameResultStatus gameResultStatus;
		private GameSpeedStatus gameSpeedStatus;
		private Coroutine coroutine;
		private IReadOnlyList<IWinInfo> wins;
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();

		private bool active;
		private bool interruptButtonPressed;
		private CycleMode cycleMode;
		private InterruptController interruptController;
		private bool stopRequested;

		public void Init()
		{
			winPresentationStatus = StatusDatabase.WinPresentationStatus;
			detailedWinPresStatus = StatusDatabase.DetailedWinPresStatus;
			gameResultStatus = StatusDatabase.QueryStatusBlock<GameResultStatus>();
			gameSpeedStatus = StatusDatabase.QueryStatusBlock<GameSpeedStatus>();
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();

			coroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoDetailedWinPres(), "DetailedWinPres");
			RegisterForEvents();
		}

		public void DeInit()
		{
			UnRegisterFromEvents();

			coroutine?.Stop();
			coroutine = null;

			winPresentationStatus = null;
			detailedWinPresStatus = null;
			gameResultStatus = null;
			gameSpeedStatus = null;
			interruptController = null;
		}

		public void Start(CycleMode newCycleMode)
		{
			cycleMode = newCycleMode;

			wins = gameResultStatus.GetWinInfo();

			if (wins.Count == 0)
				return;

			interruptButtonPressed = false;
			stopRequested = false;

			if (cycleMode == CycleMode.AtLeastOnce)
				interruptController.AddInterruptable(this);

			detailedWinPresStatus.Wins = wins;
			detailedWinPresStatus.IsActive = true;
			detailedWinPresStatus.HighlightedWinIndex = 0;
			detailedWinPresStatus.HighlightedWin = detailedWinPresStatus.Wins[0];
			detailedWinPresStatus.FirstCycleComplete = false;

			active = true;
		}

		public bool CanStop()
		{
			return !active
				|| cycleMode == CycleMode.Forever
				|| detailedWinPresStatus.FirstCycleComplete && cycleMode == CycleMode.AtLeastOnce && !(StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active && winPresentationStatus.WinPresActive)
				|| interruptButtonPressed
				|| detailedWinPresStatus.FirstCycleComplete && StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active && !winPresentationStatus.WinPresActive;
		}

		public void Stop()
		{
			stopRequested = true;
		}

		private void RegisterForEvents()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(GameStatus.CurrentGameState), OnGameStateChanged);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.GameStatus, nameof(GameStatus.SelectedStakeCombinationIndex), OnStakeChanged);
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.DenomConfig), OnDenomConfigChanged);
			autoUnregisterHelper.RegisterMessageHandler<RequestThemeSelectionMenuMessage>(Communication.PresentationDispatcher, OnThemeSelectionMenuRequested);
		}

		private void OnThemeSelectionMenuRequested(RequestThemeSelectionMenuMessage obj)
		{
			if (!active)
				return;

			Log.Instance.Info("Stopping detailed win due to theme menu entry");
			stopRequested = true;
		}

		private void OnGameStateChanged(StatusBlock sender, string propertyName)
		{
			if (!active)
				return;

			var gameState = StatusDatabase.GameStatus.CurrentGameState;
			if (gameState == GameState.Starting || gameState == GameState.StartingCreditPlayoff || gameState == GameState.ShowResult || gameState == GameState.StartingGamble)
			{
				Log.Instance.InfoFormat("Stopping detailed win due to game state change to {0}", gameState);
				stopRequested = true;
			}
		}

		private void OnStakeChanged(StatusBlock sender, string propertyName)
		{
			if (!active)
				return;

			Log.Instance.Info("Stopping detailed win due to bet change");
			stopRequested = true;
		}

		private void OnDenomConfigChanged(StatusBlock sender, string propertyName)
		{
			if (!active)
				return;

			Log.Instance.Info("Stopping detailed win due to denom change");
			stopRequested = true;
		}

		private void UnRegisterFromEvents()
		{
			autoUnregisterHelper.UnRegisterAll();
		}

		// ReSharper disable once IteratorNeverReturns
		private IEnumerator<CoroutineInstruction> DoDetailedWinPres()
		{
			while (!StatusDatabase.GameStatus.IsSetupPresentationDone)
				yield return null;

			var tasks = new List<(VisibilityType Visibility, Func<TimeSpan> GetDuration)>();
			tasks.Add((VisibilityType.Visible, () => winPresentationStatus.DetailedWinPresDisplayTime));
			if (winPresentationStatus.DetailedWinPresFlashTime != TimeSpan.Zero)
			{
				// Add a flash step if the flash time is non-zero.
				tasks.Add((VisibilityType.HiddenBecauseFlashing, () => winPresentationStatus.DetailedWinPresFlashTime));
				tasks.Add((VisibilityType.Visible, () => winPresentationStatus.DetailedWinPresDisplayTime));
			}

			// Time between wins.
			tasks.Add((VisibilityType.HiddenBecauseWaitBetweenWins, () => winPresentationStatus.DetailedWinPresBetweenWinsDisplayTime));

			while (true)
			{
				while (!active)
					yield return null;

				for (;;)
				{
					while (StatusDatabase.GameStatus.GameLogicPaused)
					{
						yield return null;
						if (!StatusDatabase.GameStatus.GameLogicPaused)
							Start();
					}

					foreach (var t in tasks)
					{
						detailedWinPresStatus.Visibility = t.Visibility;
						yield return new CoroutineDelayWithPredicate(t.GetDuration(), ShouldCycleBeInterrupted);

						if (ShouldCycleBeInterrupted())
							break;
					}

					if (StatusDatabase.GameStatus.GameLogicPaused)
					{
						Reset();
						continue;
					}

					if (ShouldCycleBeStopped())
						break;

					var newWinIndex = detailedWinPresStatus.HighlightedWinIndex + 1;
					if (newWinIndex == detailedWinPresStatus.Wins.Count)
					{
						detailedWinPresStatus.FirstCycleComplete = true;

						if (winPresentationStatus.DetailedWinPresCycleTime != TimeSpan.Zero)
						{
							newWinIndex = -1;
							detailedWinPresStatus.HighlightedWinIndex = newWinIndex;
							detailedWinPresStatus.HighlightedWin = null;
							detailedWinPresStatus.Visibility = VisibilityType.HiddenBecauseWaitBetweenCycles;
							yield return new CoroutineDelayWithPredicate(winPresentationStatus.DetailedWinPresCycleTime, ShouldCycleBeInterrupted);
						}

						newWinIndex = 0;

						if (StatusDatabase.GameStatus.GameLogicPaused)
						{
							Reset();
							continue;
						}

						if (ShouldCycleBeStopped())
							break;
					}

					detailedWinPresStatus.HighlightedWinIndex = newWinIndex;
					detailedWinPresStatus.HighlightedWin = detailedWinPresStatus.Wins[newWinIndex];
				}

				Reset();

				if (!interruptButtonPressed)
					interruptController.RemoveInterruptable(this);

				active = false;
				wins = null;
			}
		}

		private void Start()
		{
			detailedWinPresStatus.Wins = wins;
			detailedWinPresStatus.IsActive = true;
			detailedWinPresStatus.HighlightedWinIndex = 0;
			detailedWinPresStatus.HighlightedWin = detailedWinPresStatus.Wins[0];
			detailedWinPresStatus.FirstCycleComplete = false;
		}

		private void Reset()
		{
			detailedWinPresStatus.Visibility = VisibilityType.HiddenBecauseNotActive;
			detailedWinPresStatus.IsActive = false;
			detailedWinPresStatus.HighlightedWinIndex = -1;
			detailedWinPresStatus.HighlightedWin = null;
			detailedWinPresStatus.Wins = null;
		}

		private bool ShouldCycleBeInterrupted()
		{
			return ShouldCycleBeStopped() || StatusDatabase.GameStatus.GameLogicPaused;
		}

		private bool ShouldCycleBeStopped()
		{
			if (winPresentationStatus.CurrentWinLevel == 0 &&
				StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active &&
				gameSpeedStatus.GameSpeed == GameSpeed.SuperFast &&
				detailedWinPresStatus.HighlightedWinIndex == 1)
			{
				return true;
			}

			if (stopRequested)
			{
				return true;
			}

			if (cycleMode == CycleMode.AtLeastOnce && detailedWinPresStatus.FirstCycleComplete && !(StatusDatabase.AutoPlayStatus.State == AutoPlayState.Active && winPresentationStatus.WinPresActive))
			{
				return true;
			}

			return false;
		}

		#region IInterruptable Implementation

		bool IInterruptable.CanBeInterrupted => true;
		public bool CanBeAutoInterrupted => false;
		int IInterruptable.InterruptPriority => InterruptPriorities.Low;

		void IInterruptable.Interrupt()
		{
			interruptButtonPressed = true;
			interruptController.RemoveInterruptable(this);
		}

		#endregion
	}
}