using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class GameCountCondition : StepCondition
	{
		public int GameCount { get; }

		public GameCountCondition(int gameCount)
		{
			GameCount = gameCount;
		}

		public override StepConditionResult CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData)
		{
			if (stateData == null)
				stateData = sequenceUpToNow.LastOrDefault();
			var start = (StageGaffResult)stateData;
			var count = sequenceUpToNow.Count - sequenceUpToNow.IndexOf(start);
			return count == GameCount ? StepConditionResult.Found : count < GameCount ? StepConditionResult.KeepSearching : StepConditionResult.Fail;
		}

		public override IResult ToString(string format)
		{
			return $"Play {GameCount} games".ToSuccess();
		}
	}
}