using System.Collections.Generic;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Midas.Gle.LogicToPresentation
{
	public class GleResult
	{
		public CycleResult Current { get; }
		public CycleState CurrentCycle { get; }
		public StageResults NextDefault { get; }
		public CycleState NextCycle { get; }
		public bool IsGameFinished => Current.Cycles.IsFinished;
		public IReadOnlyList<GleDecisionInfo> NextStageDecisions { get; }

		public GleResult(CycleResult current, StageResults nextDefault, IReadOnlyList<GleDecisionInfo> nextStageDecisions)
		{
			Current = current;
			CurrentCycle = current.Inputs.GetCycles().Current;
			NextDefault = nextDefault;
			NextCycle = current.Cycles.Current ?? GleGameData.BaseCycle;
			NextStageDecisions = nextStageDecisions;
		}

		public override string ToString() => $"{CurrentCycle.Stage}{(CurrentCycle.CycleId == null ? "" : $"({CurrentCycle.CycleId})")}, Prize: {Current.AwardedPrize.ToStringOrThrow("SL")}";
	}
}