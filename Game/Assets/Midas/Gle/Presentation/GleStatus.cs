using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Types;
using Midas.Core;
using Midas.Core.General;
using Midas.Gle.LogicToPresentation;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.ExtensionMethods;

namespace Midas.Gle.Presentation
{
	public sealed partial class GleStatus : GameResultStatus
	{
		private StatusProperty<GleResults> previousGameResults;
		private StatusProperty<GleResults> gameResults;
		private StatusProperty<int> currentResultIndex;
		private StatusProperty<Credit> awardedPrize;
		private StatusProperty<Credit> totalAwardedPrize;
		private StatusProperty<IReadOnlyList<GleUserSelection>> playerDecisions;

		public GleResults PreviousGameResults => previousGameResults.Value;
		public GleResults GameResults => gameResults.Value;
		public int CurrentResultIndex => currentResultIndex.Value;
		public GleResult CurrentGameResult => GameResults?[CurrentResultIndex];
		public IReadOnlyList<GleUserSelection> PlayerDecisions => playerDecisions.Value;

		public GleStatus(IComparer<CellPrizeResult> prizeComparer = null) : base(nameof(GleStatus))
		{
			this.prizeComparer = prizeComparer ?? defaultComparer;
		}

		public string GetStageId(bool nextCycle)
		{
			if (nextCycle)
				return CurrentGameResult.IsGameFinished || StatusDatabase.GameStatus.HasWinCapBeenReached ? GameResults[0].CurrentCycle.Stage : CurrentGameResult.NextCycle.Stage;

			return CurrentGameResult.CurrentCycle.Stage;
		}

		public override bool IsBaseGameCycle() => CurrentResultIndex == 0;

		public override bool IsGameFinished() => CurrentGameResult.IsGameFinished;

		/// <summary>
		/// Gets the most recent result for a stage.
		/// </summary>
		private StageResults GetLastResultForStage(string stageId, bool excludeLastCycle)
		{
			var results = GameResults;
			var i = results.Count - 1;

			if (excludeLastCycle)
			{
				if (i == 0)
				{
					results = PreviousGameResults;
					i = results.Count - 1;
				}
				else
					i--;
			}

			for (; i >= 0; i--)
			{
				var r = results[i];
				if (r.CurrentCycle.Stage == stageId)
					return r.Current.StageResults;

				// Need to monitor this condition. If the game has a stage with no "next initial" result then
				// this will cause the previous valid stage result to be returned instead.

				if (r.NextCycle.Stage == stageId && r.NextDefault != null)
					return r.NextDefault;
			}

			return default;
		}

		/// <summary>
		/// Gets the initial stake combination for the current game state.
		/// Use this if you need to recover reels with stake information, eg if there are symbols with values on them.
		/// </summary>
		public IStakeCombination GetInitStakeCombination()
		{
			var state = StatusDatabase.GameStatus.CurrentGameState;

			if (state == GameState.ShowResult && GameResults.Count == 1)
				return PreviousGameResults.StakeCombination;

			return GameResults.StakeCombination;
		}

		public StageResults GetResultForGameState(string stageId)
		{
			StageResults res = null;
			var state = StatusDatabase.GameStatus.CurrentGameState;

			switch (state)
			{
				// case GameState.Idle when stageId == GameResults[0].CurrentCycle.Stage:
				// case GameState.Starting when stageId == GameResults[0].CurrentCycle.Stage:
				// 	// Prior to the start of a game.
				// 	res = GameResults[0].Current.StageResults;
				// 	break;

				case GameState.Idle:
				case GameState.Starting:
				case GameState.OfferGamble:
				case GameState.StartingGamble:
				case GameState.ShowGambleResult:
				case GameState.Continuing:
				case GameState.StartingCreditPlayoff:
				case GameState.ShowCreditPlayoffResult:
					res = GetLastResultForStage(stageId, false);
					break;

				case GameState.ShowResult:
					res = GetLastResultForStage(stageId, true);
					break;

				case GameState.History:
					res = GetHistoryResult();
					break;
			}

			if (res == null)
				Log.Instance.Fatal($"No game result found for {stageId} in state {state}.");

			return res;
		}

		private StageResults GetLastResult(Predicate<GleResult> predicate, bool excludeLastCycle)
		{
			var results = GameResults;
			var i = results.Count - 1;

			if (excludeLastCycle)
			{
				if (i == 0)
				{
					results = PreviousGameResults;
					i = results.Count - 1;
				}
				else
					i--;
			}

			for (; i >= 0; i--)
			{
				var r = results[i];
				if (predicate(r))
					return r.Current.StageResults;
			}

			return default;
		}

		/// <summary>
		/// Get the most recent game result that matches the provided predicate.
		/// </summary>
		/// <remarks>Note that this method does not use the next initial result. It expects that you are trying to find a specific initialisation case that may be the result of a different stage (ie, respin initial reels).</remarks>
		public StageResults GetResultForGameState(Predicate<GleResult> predicate)
		{
			StageResults res = null;
			var state = StatusDatabase.GameStatus.CurrentGameState;

			switch (state)
			{
				// case GameState.Idle when predicate(GameResults[0]):
				// case GameState.Starting when predicate(GameResults[0]):
				// 	// Prior to the start of a game.
				// 	res = GameResults[0].Current.StageResults;
				// 	break;

				case GameState.Idle:
				case GameState.Starting:
				case GameState.OfferGamble:
				case GameState.StartingGamble:
				case GameState.ShowGambleResult:
				case GameState.Continuing:
				case GameState.StartingCreditPlayoff:
				case GameState.ShowCreditPlayoffResult:
					res = GetLastResult(predicate, false);
					break;

				case GameState.ShowResult:
					res = GetLastResult(predicate, true);
					break;

				case GameState.History:
					res = GetHistoryResult();
					break;
			}

			if (res == null)
				Log.Instance.Fatal($"No game result found for state {state}.");

			return res;
		}

		private StageResults GetHistoryResult()
		{
			switch (StatusDatabase.HistoryStatus.HistoryStepType)
			{
				case HistoryStepType.CreditPlayoff:
				{
					// If credit playoff was never won, then there are no logic games played.
					// In the winning case, the GLE game state is stored in the previous results.

					if (StatusDatabase.QueryStatusBlock<CreditPlayoffStatusBase>().IsWin)
						return PreviousGameResults[0].Current.StageResults;

					// In the losing case, the GLE game state is stored in the current results.

					return GameResults[0].Current.StageResults;
				}

				case HistoryStepType.Game:
					return GameResults[CurrentResultIndex].Current.StageResults;

				default:
					return GameResults[0].Current.StageResults;
			}
		}

		public void AddOrReplacePlayerDecision(string name, IReadOnlyList<int> values, GleDecisionPersistence persistence = GleDecisionPersistence.Cycle)
		{
			var newDecisions = PlayerDecisions.Where(d => d.Name != name).ToList();
			var decision = new GleUserSelection(name, values, persistence);
			newDecisions.Add(new GleUserSelection(name, values, persistence));
			playerDecisions.Value = newDecisions;

			Communication.ToLogicSender.Send(new GleSelectionMessage(decision));
		}

		protected override void RegisterForEvents(AutoUnregisterHelper autoUnregisterHelper)
		{
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.GetService<GleService>().PreviousGameResults, v => previousGameResults.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.GetService<GleService>().CurrentGameResults, v => gameResults.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.GetService<GleService>().CurrentResultIndex, v => currentResultIndex.Value = v);
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.GetService<GleService>().PlayerDecisions, v => playerDecisions.Value = v);

			var currentResultDeps = new (StatusBlock, string)[]
			{
				(this, nameof(CurrentResultIndex)),
				(this, nameof(GameResults))
			};

			autoUnregisterHelper.RegisterMultiplePropertyChangedHandler(currentResultDeps, _ => currentWinInfo = null);
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();
			previousGameResults = AddProperty(nameof(PreviousGameResults), default(GleResults));
			gameResults = AddProperty(nameof(GameResults), default(GleResults));
			currentResultIndex = AddProperty(nameof(CurrentResultIndex), 0);
			playerDecisions = AddProperty(nameof(PlayerDecisions), (IReadOnlyList<GleUserSelection>)Array.Empty<GleUserSelection>());
		}
	}
}