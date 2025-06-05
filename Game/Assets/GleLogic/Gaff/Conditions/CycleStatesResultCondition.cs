using System.Collections.Generic;
using Gaff.Core;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	/// <summary>
	/// Based on the <see cref="CycleStates"/> selected return true when a result matches one of those requested states.
	/// </summary>
	public sealed class CycleStatesResultCondition : ResultCondition
	{
		/// <summary>
		/// The states in which the check condition will return true.
		/// </summary>
		public CycleStates CycleStates { get; }

		public CycleStatesResultCondition(CycleStates cycleStates)
		{
			CycleStates = cycleStates;
		}

		/// <inheritdoc />
		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			var nextCycles = result.Cycles;
			result.Inputs.TryGetInput<Cycles>("Cycles", out var currentCycles);
			var conditionMet = false;

			CycleHelper.ProcessCycleState(currentCycles, nextCycles, OnNonTriggeringBase, OnInitialTrigger, OnTrigger, OnReTrigger, OnFeatureGame, OnPendingFeature, OnNextSubFeature, OnEndSubFeature, OnFeatureComplete);

			return conditionMet;

			void OnNonTriggeringBase() => conditionMet |= (CycleStates & CycleStates.NonTriggeringBase) > 0;
			void OnInitialTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool commencingThisCycle) => conditionMet |= (CycleStates & CycleStates.InitialTrigger) > 0;
			void OnTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange) => conditionMet |= (CycleStates & CycleStates.Trigger) > 0;
			void OnReTrigger(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange) => conditionMet |= (CycleStates & CycleStates.ReTrigger) > 0;
			void OnFeatureGame(string stage, string cycleId, int currentPlayed, int totalPlayed, int playedChange, int totalChange, bool featureComplete) => conditionMet |= (CycleStates & CycleStates.FeatureGame) > 0;
			void OnPendingFeature(string stage, string cycleId, int currentPlayed, int totalPlayed) => conditionMet |= (CycleStates & CycleStates.PendingFeature) > 0;
			void OnNextSubFeature(string oldStage, string oldCycleId, int oldCycleStateIndex, string newStage, string newCycleId, int newCycleStateIndex) => conditionMet |= (CycleStates & CycleStates.NextSubFeature) > 0;
			void OnEndSubFeature(string oldStage, string oldCycleId, int oldCycleStateIndex, string newStage, string newCycleId, int newCycleStateIndex) => conditionMet |= (CycleStates & CycleStates.EndSubFeature) > 0;
			void OnFeatureComplete() => conditionMet |= (CycleStates & CycleStates.FeatureComplete) > 0;
		}

		/// <inheritdoc />
		public override IResult ToString(string format)
		{
			return $"If the result is in one of the following states {CycleStates} the condition is met".ToSuccess();
		}
	}
}