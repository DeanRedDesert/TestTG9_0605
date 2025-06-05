using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class NextStageCondition : ResultCondition
	{
		public string NextStageName { get; }
		public bool IgnoreCycleId { get; }
		public string NextCycleId { get; }

		public NextStageCondition(string nextStageName, bool ignoreCycleId, string nextCycleId)
		{
			NextStageName = nextStageName;
			NextCycleId = nextCycleId;
			IgnoreCycleId = ignoreCycleId;
		}

		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			return IgnoreCycleId ? result.Cycles.CheckStage(NextStageName) : result.Cycles.CheckStageAndCycleId(NextStageName, NextCycleId);
		}

		public override IResult ToString(string format)
		{
			if (IgnoreCycleId)
				return $"The next stage is {NextStageName}(?)".ToSuccess();

			return NextCycleId == null
				? $"The next stage is {NextStageName}()".ToSuccess()
				: $"The next stage is {NextStageName}({NextCycleId})".ToSuccess();
		}
	}
}