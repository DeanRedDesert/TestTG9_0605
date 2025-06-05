using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Utility;

// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

namespace Gaff.Core
{
	public static class CycleHelper
	{
		/// <summary>
		/// The next Cycles data.
		/// </summary>
		public static Cycles Next(this CycleResult result)
		{
			return result.Cycles;
		}

		/// <summary>
		/// The current Cycles data.
		/// </summary>
		public static Cycles Current(this CycleResult result)
		{
			return (Cycles)result.Inputs.Single(i => i.Name == "Cycles").Value;
		}

		public static CycleStates GetCycleStates(CycleResult result)
		{
			var state = CycleStates.None;
			var current = result.Current();
			var next = result.Next();

			ProcessCycleState(current, next, OnNonTriggeringBase, OnInitialTrigger, OnTrigger, OnReTrigger, OnFeatureGame, OnPendingFeature, OnNextSubFeature, OnEndSubFeature, OnFeatureComplete);

			return state;

			void OnNonTriggeringBase() => state |= CycleStates.NonTriggeringBase;
			void OnInitialTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool commencingThisCycle) => state |= CycleStates.InitialTrigger;
			void OnTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange) => state |= CycleStates.Trigger;
			void OnReTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange) => state |= CycleStates.ReTrigger;
			void OnFeatureGame(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool featureComplete) => state |= CycleStates.FeatureGame;
			void OnPendingFeature(string stage, string cycleId, int currentPlayed, int totalPlayed) => state |= CycleStates.PendingFeature;
			void OnNextSubFeature(string oldStage, string oldCycleId, int oldCycleStateIndex, string newStage, string newCycleId, int newCycleStateIndex) => state |= CycleStates.NextSubFeature;
			void OnEndSubFeature(string oldStage, string oldCycleId, int oldCycleStateIndex, string newStage, string newCycleId, int newCycleStateIndex) => state |= CycleStates.EndSubFeature;
			void OnFeatureComplete() => state |= CycleStates.FeatureComplete;
		}

		/// <summary>
		/// A state management function used to call off to user code when different game states occur.
		///
		/// Each action have a number of useful parameters as shown below:
		///  void OnNonTriggeringBaseGame()
		///  void OnInitialTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool playingNext)
		///  void OnTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange)
		///  void OnReTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange)
		///  void OnFeatureGame(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool featureComplete)
		///  void OnPendingFeature(string stage, string cycleId, int currentPlayed, int totalPlayed)
		///  void OnNextFeature(string oldStage, string oldCycleId, int oldCycleStateIndex, string newStage, string newCycleId, int newCycleStateIndex) => state |= CycleStates.NextFeature;
		///  void OnFeatureComplete()
		/// </summary>
		/// <param name="currentCycles">The game current cycle information</param>
		/// <param name="nextCycles">The game next cycle information</param>
		/// <param name="onNonTriggeringBaseGame">A function to call on an main game without triggers</param>
		/// <param name="onInitialTrigger">A function to call on an main game trigger</param>
		/// <param name="onTrigger">A function to call on a trigger within features</param>
		/// <param name="onReTrigger">A function to call on a re trigger</param>
		/// <param name="onFeatureGame">A function to call when a non re-triggering feature game is played</param>
		/// <param name="onPendingFeature">A function to call for each pending feature that has not been affected this cycle</param>
		/// <param name="onNextSubFeature">A function to call whenever a sub feature transitions to another.</param>
		/// <param name="onEndSubFeature">A function to call whenever the current sub feature is complete.</param>
		/// <param name="onFeatureComplete">A function to call when all features are completed and about to return to the main game</param>
		// ReSharper disable once MemberCanBePrivate.Global
		public static void ProcessCycleState(Cycles currentCycles, Cycles nextCycles,
			Action onNonTriggeringBaseGame,
			Action<string, string, int, int, int, int, bool> onInitialTrigger,
			Action<string, string, int, int, int, int> onTrigger,
			Action<string, string, int, int, int, int> onReTrigger,
			Action<string, string, int, int, int, int, bool> onFeatureGame,
			Action<string, string, int, int> onPendingFeature,
			Action<string, string, int, string, string, int> onNextSubFeature,
			Action<string, string, int, string, string, int> onEndSubFeature,
			Action onFeatureComplete)
		{
			var deltas = GetCycleDeltas(currentCycles, nextCycles);
			var cs = currentCycles.Current;
			var ncs = nextCycles.Current;
			var isFeatureComplete = ncs == null && nextCycles.Count > 1;
			var isNonTriggeringMainGame = ncs == null && nextCycles.Count == 1;

			if (isNonTriggeringMainGame)
				onNonTriggeringBaseGame();

			var csKey = MakeKey(cs);
			var ncsKey = MakeKey(ncs);

			foreach (var delta in deltas)
			{
				var deltaKey = MakeKey(delta);
				var isCurrentCycleSet = csKey == deltaKey;

				if (cs.Id == 0 && delta.CompletedCycles == 0 && delta.CompletedDelta == 0 && delta.TotalDelta > 0)
					onInitialTrigger(delta.Stage, delta.CycleId, delta.CompletedCycles, delta.TotalCycles, delta.CompletedDelta, delta.TotalDelta, ncs != null && deltaKey == ncsKey);
				// ReSharper disable once ConvertIfStatementToSwitchStatement
				else if (!isCurrentCycleSet && delta.CompletedDelta == 0 && delta.TotalDelta > 0)
					onTrigger(delta.Stage, delta.CycleId, delta.CompletedCycles, delta.TotalCycles, delta.CompletedDelta, delta.TotalDelta);
				else if (isCurrentCycleSet && delta.TotalDelta > 0)
					onReTrigger(delta.Stage, delta.CycleId, delta.CompletedCycles, delta.TotalCycles, delta.CompletedDelta, delta.TotalDelta);
				else if (isCurrentCycleSet && delta.CompletedDelta > 0 && delta.TotalDelta == 0)
					onFeatureGame(delta.Stage, delta.CycleId, delta.CompletedCycles, delta.TotalCycles, delta.CompletedDelta, delta.TotalDelta, isFeatureComplete);
				else if (!isCurrentCycleSet && delta.CompletedDelta == 0 && delta.TotalDelta == 0)
					onPendingFeature(delta.Stage, delta.CycleId, delta.CompletedCycles, delta.TotalCycles);
			}

			var featureDelta = deltas.SingleOrDefault(d => d.Stage == cs.Stage && d.CycleId == cs.CycleId);

			if (!isNonTriggeringMainGame && csKey != ncsKey && featureDelta != null && featureDelta.TotalDelta == 0 && featureDelta.CompletedCycles == featureDelta.TotalCycles)
				onEndSubFeature(cs.Stage, cs.CycleId, cs.Id, ncs?.Stage, ncs?.CycleId, ncs?.Id ?? -1);

			if (isFeatureComplete)
				onFeatureComplete();
			else if (!isNonTriggeringMainGame && csKey != ncsKey)
				onNextSubFeature(cs.Stage, cs.CycleId, cs.Id, ncs?.Stage, ncs?.CycleId, ncs?.Id ?? -1);
		}

		// A collection of empty functions for the helper above when you don't want to use a particular state.
		public static void OnDoNothing(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange) { }
		public static void OnDoNothing(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool featureComplete) { }
		public static void OnDoNothing(string stage, string cycleId, int currentPlayed, int totalPlayed) { }
		public static void OnDoNothing(string oldStage, string oldCycleId, int oldCycleIndex, string newStage, string newCycleId, int newCycleIndex) { }
		public static void OnDoNothing() { }

		private static IReadOnlyList<CycleStateDelta> GetCycleDeltas(Cycles current, Cycles next)
		{
			var baseGameKey = MakeKey(current[0]);
			var preCycle = current.GetCompleteOrPendingCycles().GroupBy(MakeKey).ToDictionary(kv => kv.Key, kv => kv.ToList());
			var postCycle = next.GetCompleteOrPendingCycles().GroupBy(MakeKey);
			var deltas = new List<CycleStateDelta>();
			foreach (var entry in postCycle)
			{
				var first = entry.First();

				if (baseGameKey == entry.Key)
					continue;

				var postTotalCycles = entry.Sum(v => v.TotalCycles);
				var postCompletedCycles = entry.Sum(v => v.CompletedCycles);
				var preTotalCycles = 0;
				var preCompletedCycles = 0;
				if (preCycle.TryGetValue(entry.Key, out var preEntry))
				{
					preTotalCycles = preEntry.Sum(v => v.TotalCycles);
					preCompletedCycles = preEntry.Sum(v => v.CompletedCycles);
				}

				var totalDelta = postTotalCycles - preTotalCycles;
				var compDelta = postCompletedCycles - preCompletedCycles;

				deltas.Add(new CycleStateDelta(first.Stage, first.CycleId, postTotalCycles, postCompletedCycles, totalDelta, compDelta));
			}

			return deltas;
		}

		private static string MakeKey(CycleState c)
		{
			if (c == null)
				return string.Empty;

			return c.Stage + (c.CycleId != null ? $"~{c.CycleId}" : "");
		}

		private static string MakeKey(CycleStateDelta c)
		{
			if (c == null)
				return string.Empty;

			return c.Stage + (c.CycleId != null ? $"~{c.CycleId}" : "");
		}
	}

	public sealed class CycleStateDelta
	{
		public string Stage { get; }
		public string CycleId { get; }
		public int TotalCycles { get; }
		public int CompletedCycles { get; }
		public int TotalDelta { get; }
		public int CompletedDelta { get; }

		public CycleStateDelta(string stage, string cycleId, int totalCycles, int completedCycles, int totalDelta, int completedDelta)
		{
			Stage = stage;
			CycleId = cycleId;
			TotalCycles = totalCycles;
			CompletedCycles = completedCycles;
			TotalDelta = totalDelta;
			CompletedDelta = completedDelta;
		}
	}
}