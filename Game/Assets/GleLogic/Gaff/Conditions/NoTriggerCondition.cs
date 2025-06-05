using System.Collections.Generic;
using System.Linq;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Types.Exits;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class NoTriggerCondition : ResultCondition
	{
		/// <summary>
		/// Returns true if there are no triggers and the awarded prize is between min and max (inclusive), otherwise returns false.
		/// </summary>
		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			foreach (var c in result.StageResults.Where(r => r.Type == StageResultType.ExitList))
			{
				if (((IReadOnlyList<DesiredExit>)c.Value).Count > 0)
					return false;
			}

			return true;
		}

		public override IResult ToString(string format)
		{
			return "A game that doesn't trigger an exit".ToSuccess();
		}
	}
}