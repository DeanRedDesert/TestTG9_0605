using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Types.Exits;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class TriggerCountCondition : ResultCondition
	{
		public uint MinCount { get; }
		public uint MaxCount { get; }

		public TriggerCountCondition(uint minCount, uint maxCount)
		{
			MinCount = minCount;
			MaxCount = maxCount;
		}

		/// <summary>
		/// Returns true if the total trigger count is between MinCount and MaxCount (inclusive), otherwise false.
		/// </summary>
		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			var exitCount = result.StageResults.Where(r => r.Type == StageResultType.ExitList)
				.SelectMany(c => (IReadOnlyList<DesiredExit>)c.Value).Count();

			return exitCount >= MinCount && exitCount <= MaxCount;
		}

		public override IResult ToString(string format)
		{
			return $"A game with the total coinciding triggers between {MinCount} and {MaxCount} (inclusive)".ToSuccess();
		}
	}
}