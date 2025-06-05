using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class CurrentStageCondition : ResultCondition
	{
		// ReSharper disable MemberCanBePrivate.Global

		public string StageName { get; }
		public bool IgnoreCycleId { get; }
		public string CycleId { get; }

		// ReSharper restore MemberCanBePrivate.Global

		public CurrentStageCondition(string stageName, bool ignoreCycleId, string cycleId)
		{
			StageName = stageName;
			IgnoreCycleId = ignoreCycleId;
			CycleId = cycleId;
		}

		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			return IgnoreCycleId ? result.Current().CheckStage(StageName) : result.Current().CheckStageAndCycleId(StageName, CycleId);
		}

		public override IResult ToString(string format)
		{
			if (IgnoreCycleId)
				return $"The current stage is {StageName}(?)".ToSuccess();

			return CycleId == null
				? $"The current stage is {StageName}()".ToSuccess()
				: $"The current stage is {StageName}({CycleId})".ToSuccess();
		}
	}
}