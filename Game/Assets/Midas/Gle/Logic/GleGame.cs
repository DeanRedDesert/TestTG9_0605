using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gaff.Core;
using Gaff.Core.GaffEditor;
using Game;
using Logic.Core.DecisionGenerator;
using Logic.Core.Engine;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.Gle.LogicToPresentation;
using Midas.Logic;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data;
using Midas.LogicToPresentation.Messages;

namespace Midas.Gle.Logic
{
	public sealed partial class GleGame : IGame
	{
		private readonly Runner runner = GleGameData.Runner;
		private readonly IReadOnlyDictionary<string, string> configurations;
		private readonly IReadOnlyList<GleStakeCombination> stakeCombinations;
		private readonly bool doLogicResetOnConfigChange;
		private readonly IReadOnlyList<IGaffSequence> gaffSequences;
		private readonly CircularBuffer<GleHistoryGaffSequence> historyGaffSequences = new CircularBuffer<GleHistoryGaffSequence>(20);
		private readonly IGleOutcomeConverter outcomeConverter;
		private readonly string previousGameStateCriticalData;
		private readonly string gameStateCriticalData;

		private IFoundationShim foundation;
		private GameState gameState;
		private List<GleResult> cycleResults;
		private List<GleResult> previousCycleResults;
		private int? gaffIndex;
		private IDialUpResults currentGaff;
		private bool isSkippingFeature;

		internal GleGame(IReadOnlyDictionary<string, string> configurations, IReadOnlyList<GleStakeCombination> stakeCombinations, bool doLogicResetOnConfigChange, IGleOutcomeConverter outcomeConverter = null)
		{
			NvramSerializer.RegisterCustomSerializer(new GleCustomSerializers());

			this.configurations = configurations;
			this.stakeCombinations = stakeCombinations;
			this.doLogicResetOnConfigChange = doLogicResetOnConfigChange;
			gaffSequences = GetGaffSequences();
			ProgressiveLevels = GetProgressiveLevels();
			this.outcomeConverter = outcomeConverter ?? new DefaultOutcomeConverter();

			var newConfigurationKey = configurations.Aggregate("", (v, i) => v + i.Key + i.Value);
			ConfigurationKey = Regex.Replace(newConfigurationKey, "[^a-zA-Z0-9'.@]", string.Empty).Trim();
			gameStateCriticalData = "GleGame/" + ConfigurationKey + nameof(GameState);
			previousGameStateCriticalData = "GleGame/" + ConfigurationKey + "Previous" + nameof(GameState);
		}

		#region IGame implementation

		public string ConfigurationKey { get; }

		public IReadOnlyList<IStakeCombination> StakeCombinations => stakeCombinations;
		public IReadOnlyList<(string LevelId, uint GameLevel)> ProgressiveLevels { get; }

		public void Init(IFoundationShim foundationShim, bool gameReset, object historyGameState)
		{
			foundation = foundationShim;

			GleStakeCombination prevStakeCombination = null;

			if (foundation.GameMode == FoundationGameMode.History)
			{
				GameState prevGameState;
				(gameState, prevGameState) = ((GameState, GameState))historyGameState;
				if (gameState == null)
					throw new Exception("Unable to load history game state. Has no game been played?");

				var inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;
				cycleResults = InitialiseCycleResult(gameState, inputs);

				if (prevGameState != null)
				{
					inputs = stakeCombinations[prevGameState.SelectedStakeCombination].Inputs;
					previousCycleResults = InitialiseCycleResult(prevGameState, inputs);
				}
				else
					previousCycleResults = new List<GleResult>();
			}
			else
			{
				if (!foundation.TryReadNvram(NvramScope.Variation, gameStateCriticalData, out gameState))
				{
					previousCycleResults = cycleResults;
					cycleResults = GenerateInitialResult(false);
					previousCycleResults = new List<GleResult>();
					SaveGameState(false);
				}
				else
				{
					var inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;
					cycleResults = InitialiseCycleResult(gameState, inputs);

					if (foundation.TryReadNvram(NvramScope.Variation, previousGameStateCriticalData, out GameState prevGameState))
					{
						prevStakeCombination = stakeCombinations[prevGameState.SelectedStakeCombination];
						var prevGameInputs = prevStakeCombination.Inputs;
						previousCycleResults = InitialiseCycleResult(prevGameState, prevGameInputs);
					}
					else
					{
						previousCycleResults = new List<GleResult>();
					}

					if (doLogicResetOnConfigChange && gameReset && !gameState.GameReset)
					{
						(cycleResults, previousCycleResults) = (previousCycleResults, cycleResults);
						prevStakeCombination = stakeCombinations[gameState.SelectedStakeCombination];
						cycleResults = GenerateInitialResult(true);
						SaveGameState(true);
					}
				}
			}

			GleService.Instance.PreviousGameResultsService.SetValue(new GleResults(previousCycleResults.ToArray(), prevStakeCombination));
			GleService.Instance.CurrentGameResultsService.SetValue(new GleResults(cycleResults.ToArray(), stakeCombinations[gameState.SelectedStakeCombination]));
			GleService.Instance.CurrentResultIndexService.SetValue(cycleResults.Count - 1);
			GameServices.GaffService.GaffSequencesService.SetValue(gaffSequences);
			GameServices.GaffService.AreGaffCyclesPendingService.SetValue(false);
			GleService.Instance.PlayerDecisionsService.SetValue(gameState.Selections.Values.ToArray());
			ReadPidData();

			Communication.LogicDispatcher.AddHandler<GleSelectionMessage>(OnGleSelectionMessageReceived);

			if (foundation.ShowMode != FoundationShowMode.None)
				Communication.LogicDispatcher.AddHandler<SkipFeatureMessage>(OnSkipFeatureMessageReceived);
		}

		public void DeInit()
		{
			if (foundation.ShowMode != FoundationShowMode.None)
				Communication.LogicDispatcher.RemoveHandler<SkipFeatureMessage>(OnSkipFeatureMessageReceived);

			Communication.LogicDispatcher.RemoveHandler<GleSelectionMessage>(OnGleSelectionMessageReceived);
		}

		public IOutcome StartGame(int stakeCombinationIndex)
		{
			var previousCycle = cycleResults[cycleResults.Count - 1];
			var previousStakeCombination = stakeCombinations[stakeCombinationIndex];
			(cycleResults, previousCycleResults) = (previousCycleResults, cycleResults);
			cycleResults.Clear();

			// Special case: If the game was reset, we want to keep the inter game data from the last proper game rather than cycling it.

			if (gameState.GameReset)
				gameState.GameReset = false;
			else
				gameState.InterGameData = CreateNewInterGameData(previousCycle);

			gameState.SelectedStakeCombination = stakeCombinationIndex;
			gameState.CycleData.Clear();

			var previousCycleResult = previousCycle.Current;

			// Win Capping has occurred so reset the cycles.

			if (!previousCycleResult.Cycles.IsFinished)
			{
				var finishedCycles = Cycles.CreateInitial(GleGameData.EntryStage.Name).ReplaceCurrent(1, 1).MoveNext();
				previousCycleResult = new CycleResult(previousCycle.Current.Inputs, finishedCycles, previousCycle.Current.AwardedPrize, previousCycle.Current.TotalAwardedPrize, previousCycle.Current.StageResults, previousCycle.Current.Progressives);
			}

			StageResults stageDefault = null;
			var result = PlayGameCycle(previousCycleResult);
			var outcome = result.Result;

			CleanUpSelections(outcome);

			if (!outcome.Cycles.IsFinished)
				stageDefault = CreateStageDefault(runner, result.Result.Inputs, result.Result);

			gameState.CycleData.Add(new GameCycleData(result.UsedNumbers));
			var gleResult = new GleResult(outcome, stageDefault, GetNextCycleUnresolvedInputs(outcome));
			cycleResults.Add(gleResult);

			SaveGameState(true);

			GleService.Instance.PreviousGameResultsService.SetValue(new GleResults(previousCycleResults.ToArray(), previousStakeCombination));
			GleService.Instance.CurrentGameResultsService.SetValue(new GleResults(cycleResults.ToArray(), stakeCombinations[stakeCombinationIndex]));
			GleService.Instance.CurrentResultIndexService.SetValue(cycleResults.Count - 1);
			GleService.Instance.PlayerDecisionsService.SetValue(gameState.Selections.Values.ToArray());

			var featureIndex = GleGameData.Stages.FindIndex(s => s.Name == gleResult.CurrentCycle.Stage);
			return new GameOutcome(featureIndex, outcomeConverter.GenerateFoundationPrizes(outcome), outcome.Cycles.IsFinished);
		}

		public IOutcome ContinueGame()
		{
			var result = PlayGameCycle(cycleResults[cycleResults.Count - 1].Current);
			var outcome = result.Result;
			StageResults stageDefault = null;

			CleanUpSelections(outcome);

			if (!outcome.Cycles.IsFinished)
				if (outcome.Inputs.CurrentStage() != outcome.Cycles.Current.Stage)
					stageDefault = CreateStageDefault(runner, outcome.Inputs, outcome);

			gameState.CycleData.Add(new GameCycleData(result.UsedNumbers /*, logicInput*/));
			var gleResult = new GleResult(outcome, stageDefault, GetNextCycleUnresolvedInputs(outcome));
			cycleResults.Add(gleResult);

			SaveGameState(false);

			GleService.Instance.CurrentGameResultsService.SetValue(new GleResults(cycleResults.ToArray(), stakeCombinations[gameState.SelectedStakeCombination]));
			GleService.Instance.CurrentResultIndexService.SetValue(cycleResults.Count - 1);
			GleService.Instance.PlayerDecisionsService.SetValue(gameState.Selections.Values.ToArray());

			var featureIndex = GleGameData.Stages.FindIndex(s => s.Name == gleResult.CurrentCycle.Stage);
			return new GameOutcome(featureIndex, outcomeConverter.GenerateFoundationPrizes(outcome), outcome.Cycles.IsFinished);
		}

		public void ApplyWinCapping()
		{
			var lastCycleResult = cycleResults[cycleResults.Count - 1];
			var copy = lastCycleResult.Current.Cycles;

			if (copy.Count > 1)
			{
				// Keep the previous games pending cycles to maintain state and remove any new triggers.

				copy = lastCycleResult.Current.Inputs.GetCycles();
				if (copy.Count > 0)
					copy = copy.PlayOne().MoveNext();
			}

			var winCappedResults = new CycleResult(lastCycleResult.Current.Inputs, copy,
				lastCycleResult.Current.AwardedPrize, lastCycleResult.Current.TotalAwardedPrize,
				lastCycleResult.Current.StageResults,
				lastCycleResult.Current.Progressives);

			lastCycleResult = new GleResult(winCappedResults, null, GetNextCycleUnresolvedInputs(winCappedResults));
			cycleResults[cycleResults.Count - 1] = lastCycleResult;
			GleService.Instance.CurrentGameResultsService.SetValue(new GleResults(cycleResults.ToArray(), stakeCombinations[gameState.SelectedStakeCombination]));
		}

		public object GetHistoryState()
		{
			var cycleResult = cycleResults[cycleResults.Count - 1];
			if (!cycleResult.IsGameFinished)
				return null;

			isSkippingFeature = false;

			// Save the game state in history when the game is over.

			foundation.TryReadNvram(NvramScope.Variation, previousGameStateCriticalData, out GameState prevGameState);

			if (foundation.ShowMode == FoundationShowMode.Development)
			{
				historyGaffSequences.PushFront(new GleHistoryGaffSequence(gameState, stakeCombinations[gameState.SelectedStakeCombination]));
				GameServices.GaffService.GaffSequencesService.SetValue(gaffSequences.Concat(historyGaffSequences).ToArray());
			}

			// Clone so it can safely change after the fact.

			var gameStateClone = new GameState
			{
				GameReset = gameState.GameReset,
				GameConfiguration = gameState.GameConfiguration,
				SelectedStakeCombination = gameState.SelectedStakeCombination,
				InterGameData = gameState.InterGameData,
				Selections = new Dictionary<string, GleUserSelection>(gameState.Selections),
				CycleData = new List<GameCycleData>(gameState.CycleData)
			};

			return (gameStateClone, prevGameState);
		}

		public void ShowHistory(object historyData) => GleService.Instance.CurrentResultIndexService.SetValue((int)historyData);

		public object GetGameCycleHistoryData() => cycleResults.Count - 1;

		public void SetGaffActive(int index)
		{
			gaffIndex = index;
			GameServices.GaffService.AreGaffCyclesPendingService.SetValue(true);
		}

		public void SetGaffActive(IDialUpResults gaffResults)
		{
			currentGaff = gaffResults;
			GameServices.GaffService.AreGaffCyclesPendingService.SetValue(true);
		}

		public IDialUpData GetGaffData()
		{
			return new GleDialUpData(GleGameData.EntryStage.Name, stakeCombinations[gameState.SelectedStakeCombination].Inputs, gameState.GameConfiguration, gameState.InterGameData, gameState.Selections, gameState.CycleData.Select(cd => cd.Decisions.ToList()).ToList(), cycleResults.Last().Current);
		}

		public int? CheckGaffStakeCombinationOverride()
		{
			if (gaffIndex.HasValue && gaffIndex >= gaffSequences.Count)
			{
				var historyIndex = gaffIndex.Value - gaffSequences.Count;
				if (historyIndex < historyGaffSequences.Size)
					return historyGaffSequences[historyIndex].SelectedStakeCombination;
			}

			return null;
		}

		#endregion

		#region Private Methods

		private IReadOnlyList<(string LevelId, uint gameLevel)> GetProgressiveLevels()
		{
			var pl = GleGameData.Progressives.GetProgressiveLevels(stakeCombinations.First().Inputs);
			return pl.Select((l, i) => (l.LevelName, (uint)i)).ToList();
		}

		private (CycleResult Result, IReadOnlyList<ulong> UsedNumbers) PlayGameCycle(CycleResult previousCycle)
		{
			var decisionRecorder = new DecisionRecorder();

			var inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;

			if (gaffIndex != null && currentGaff == null)
			{
				if (gaffIndex < gaffSequences.Count)
					currentGaff = GenerateGaffSequenceRngs(((GleGaffSequence)gaffSequences[gaffIndex.Value]).GaffSequence, new GaffRuntimeDecisionGen(new QuickRng()));
				else
				{
					gaffIndex -= gaffSequences.Count;
					if (gaffIndex < historyGaffSequences.Size)
					{
						var historyGaff = historyGaffSequences[gaffIndex.Value];

						// Historical games need to use their own game state info

						gameState.InterGameData = historyGaff.InterGameData;
						gameState.SelectedStakeCombination = historyGaff.SelectedStakeCombination;
						inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;
						inputs = inputs.ReplaceOrAdd(gameState.InterGameData.Select(d => new Input(d.Key, d.Value)).ToArray());
						currentGaff = historyGaffSequences[gaffIndex.Value].GetGaffResults(runner, inputs);
						previousCycle = null;
					}
				}

				gaffIndex = null;
			}

			if (currentGaff != null)
			{
				var val = ((GleDialUpResults)currentGaff).GetNext();
				var tempRng = new GleGaffChecker(val.Numbers.ToList());
				runner.EvaluateCycle(new GaffRuntimeDecisionGen(tempRng), inputs, previousCycle);
				if (!tempRng.CheckIfRngsOk())
				{
					Log.Instance.Warn("Invalid Gaff Numbers: " + string.Join(",", val.Numbers));
					decisionRecorder.ChangeDecisionGenerator(new RuntimeDecisionGen(GleRandomNumbers.Create(foundation, null), gameState.Selections, isSkippingFeature));
					currentGaff = null;
				}
				else
				{
					decisionRecorder.ChangeDecisionGenerator(new GaffRuntimeDecisionGen(GleRandomNumbers.Create(foundation, val.Numbers)));
				}

				if (val.Finished)
					currentGaff = null;
			}
			else
			{
				decisionRecorder.ChangeDecisionGenerator(new RuntimeDecisionGen(GleRandomNumbers.Create(foundation, null), gameState.Selections, isSkippingFeature));
			}

			GameServices.GaffService.AreGaffCyclesPendingService.SetValue(currentGaff != null);

			return (runner.EvaluateCycle(decisionRecorder, inputs, previousCycle), decisionRecorder.Decisions);
		}

		private IReadOnlyList<GleDecisionInfo> GetNextCycleUnresolvedInputs(CycleResult previousCycle)
		{
			var decisionRecorder = new NextCyclePicksDecisionGen(((GleDialUpResults)currentGaff)?.PeekNext());
			var inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;
			var nextCycleInputs = runner.CreateInputs(inputs, previousCycle);

			runner.ResolveMissingVariables(decisionRecorder, nextCycleInputs, previousCycle);

			if (decisionRecorder.PreGeneratedPicks != null)
			{
				foreach (var pick in decisionRecorder.PreGeneratedPicks)
					gameState.Selections[pick.Key] = new GleUserSelection(pick.Key, pick.Value, GleDecisionPersistence.Cycle);

				SaveGameState(false);
				GleService.Instance.PlayerDecisionsService.SetValue(gameState.Selections.Values.ToArray());
			}

			return decisionRecorder.OrderedPicks.Select(v => new GleDecisionInfo(v)).ToArray();
		}

		private static StageResults CreateStageDefault(IRunner runner, Inputs inputs, CycleResult previousCycle)
		{
			var decisionGenerator = new SceneInitDecisionGenerator();

			// TODO: If the stage requires inputs then we can't get initial data.

			var cycleResult = runner.EvaluateCycle(decisionGenerator, inputs, previousCycle);

			return cycleResult.StageResults;
		}

		private void SaveGameState(bool savePrevious)
		{
			if (savePrevious)
			{
				if (!foundation.TryReadNvram(NvramScope.Variation, gameStateCriticalData, out var data))
					throw new InvalidOperationException();

				foundation.WriteNvram(NvramScope.Variation, previousGameStateCriticalData, data);
			}

			foundation.WriteNvram(NvramScope.Variation, gameStateCriticalData, gameState, 20000);
		}

		private List<GleResult> InitialiseCycleResult(GameState gs, Inputs initialInputs)
		{
			var results = new List<GleResult>();
			var decisionGenerator = new ReplayDecisionGen(gs.CycleData[0].Decisions);
			var inputsWithIntergame = initialInputs.ReplaceOrAdd(gs.InterGameData.Select(d => new Input(d.Key, d.Value)).ToArray());
			var cycleResult = runner.EvaluateCycle(decisionGenerator, inputsWithIntergame, null);
			StageResults nextStageInitialData = null;

			if (!cycleResult.Cycles.IsFinished)
				nextStageInitialData = CreateStageDefault(runner, cycleResult.Inputs, cycleResult);

			results.Add(new GleResult(cycleResult, nextStageInitialData, GetNextCycleUnresolvedInputs(cycleResult)));

			for (var i = 1; i < gs.CycleData.Count; i++)
			{
				var gcd = gs.CycleData[i];
				decisionGenerator = new ReplayDecisionGen(gcd.Decisions);

				if (cycleResult.Cycles.IsFinished)
					throw new Exception("There are more random numbers but the game is over!");

				cycleResult = runner.EvaluateCycle(decisionGenerator, inputsWithIntergame, cycleResult);

				var currentStage = cycleResult.Inputs.CurrentStage();
				var nextCycle = cycleResult.Cycles.Current;

				if (cycleResult.Cycles.IsFinished || currentStage == nextCycle.Stage)
					nextStageInitialData = null;
				else
					nextStageInitialData = CreateStageDefault(runner, inputsWithIntergame, cycleResult);

				results.Add(new GleResult(cycleResult, nextStageInitialData, GetNextCycleUnresolvedInputs(cycleResult)));
			}

			return results;
		}

		private List<GleResult> GenerateInitialResult(bool gameReset)
		{
			gameState = new GameState
			{
				GameReset = gameReset,
				GameConfiguration = configurations,
				InterGameData = gameReset ? gameState.InterGameData : new Dictionary<string, object>(),
				Selections = new Dictionary<string, GleUserSelection>(),
				CycleData = new List<GameCycleData>(),
				SelectedStakeCombination = 0
			};

			var stakeCombination = stakeCombinations[0];

			var inputs = stakeCombination.Inputs;
			inputs = inputs.ReplaceOrAdd(gameState.InterGameData.Select(d => new Input(d.Key, d.Value)).ToArray());
			var rng = GleRandomNumbers.Create(foundation, null);
			var dr = new DecisionRecorder();

			dr.ChangeDecisionGenerator(new PersistentInitDecisionGen(rng));
			inputs = inputs.ReplaceOrAdd(runner.ResolveMissingVariables(dr, inputs, null));

			dr.ChangeDecisionGenerator(new SceneInitDecisionGen());
			var cycleResult = runner.EvaluateCycle(dr, inputs, null);

			var results = new List<GleResult> { new GleResult(cycleResult, null, GetNextCycleUnresolvedInputs(cycleResult)) };
			gameState.CycleData.Add(new GameCycleData(dr.Decisions));

			return results;
		}

		private IReadOnlyDictionary<string, object> CreateNewInterGameData(GleResult previousCycle)
		{
			var newInterGameData = previousCycle.Current.StageResults
				.Where(r => r.Type == StageResultType.VariablePermanent)
				.ToDictionary(r => r.Name, r => r.Value);

			if (newInterGameData.Count == 0)
				return gameState.InterGameData;

			foreach (var kvp in gameState.InterGameData)
			{
				// If we have reset the game, we need to freeze all the original inter game data so it doesn't advance for this cycle

				if (gameState.GameReset || !newInterGameData.ContainsKey(kvp.Key))
					newInterGameData[kvp.Key] = kvp.Value;
			}

			return newInterGameData;
		}

		private void OnGleSelectionMessageReceived(GleSelectionMessage msg)
		{
			gameState.Selections[msg.UserSelection.Name] = msg.UserSelection;
			SaveGameState(false);
			GleService.Instance.PlayerDecisionsService.SetValue(gameState.Selections.Values.ToArray());
		}

		private void OnSkipFeatureMessageReceived(SkipFeatureMessage obj)
		{
			if (foundation.ShowMode != FoundationShowMode.None)
				isSkippingFeature = true;
		}

		private void CleanUpSelections(CycleResult outcome)
		{
			if (gameState.Selections.Count > 0)
			{
				var toRemove = new List<string>(gameState.Selections.Count);
				foreach (var kvp in gameState.Selections)
				{
					if (kvp.Value.Persistence == GleDecisionPersistence.Cycle ||
						kvp.Value.Persistence == GleDecisionPersistence.Game && outcome.Cycles.IsFinished)
						toRemove.Add(kvp.Key);
				}

				foreach (var key in toRemove)
					gameState.Selections.Remove(key);
			}
		}

		private IDialUpResults GenerateGaffSequenceRngs(GaffSequence gaffSequence, IDecisionGenerator decisionGenerator)
		{
			var inputs = stakeCombinations[gameState.SelectedStakeCombination].Inputs;

			var task = gaffSequence.OrderedSteps.RunGaffAsync(runner, decisionGenerator, inputs, false);
			var results = task.GetAwaiter().GetResult();
			var orderedRngs = new List<List<ulong>>();

			for (var i = 0; i < results.OrderedResults.Count; i++)
			{
				var or = results.OrderedResults[i];
				var list = new List<ulong>();
				foreach (var decision in or.Decisions)
					list.AddRange(DecisionHelper.ConvertToRng(decision));

				orderedRngs.Add(list);
			}

			return new GleDialUpResults(orderedRngs);
		}

		private void ReadPidData()
		{
			try
			{
				var inputs = stakeCombinations.First().Inputs;
				var gamesPerWin = GleGameData.PidProvider.GetGamesPerWin(inputs);
				var largestPrizes = GleGameData.PidProvider.GetLargestPrizes(inputs);
				var smallestPrizes = GleGameData.PidProvider.GetSmallestPrizes(inputs);
				GameServices.PidService.LargestPrizesService.SetValue(largestPrizes.ToArray());
				GameServices.PidService.SmallestPrizesService.SetValue(smallestPrizes.ToArray());
				GameServices.PidService.GamesPerWinService.SetValue(gamesPerWin);
			}
			catch
			{
				GameServices.PidService.LargestPrizesService.SetValue(Array.Empty<(string, int)>());
				GameServices.PidService.SmallestPrizesService.SetValue(Array.Empty<(string, int)>());
				GameServices.PidService.GamesPerWinService.SetValue(0);
			}
		}

		private IReadOnlyList<IGaffSequence> GetGaffSequences()
		{
			var gaffs = GleGameData.GaffSequences.GetSequences().Select(s => (IGaffSequence)new GleGaffSequence(s)).ToList();

			gaffs.Add(NoTriggerPrizeResultCondition.CreateSequence("No Trigger, No Prize", prize => prize == Credit.Zero));
			gaffs.Add(NoTriggerPrizeResultCondition.CreateSequence("No Trigger, Any Prize", prize => prize != Credit.Zero));
			gaffs.Add(NoTriggerPrizeResultCondition.CreateSequence("No Trigger, Prize >= Bet", prize => prize >= stakeCombinations[gameState.SelectedStakeCombination].TotalBet));

			return gaffs;
		}

		#endregion
	}
}