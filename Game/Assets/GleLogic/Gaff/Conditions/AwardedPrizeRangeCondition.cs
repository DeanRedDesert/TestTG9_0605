using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	public sealed class AwardedPrizeRangeCondition : ResultCondition
	{
		public int MinPrize { get; }
		public int MaxPrize { get; }

		public AwardedPrizeRangeCondition(int minPrize, int maxPrize)
		{
			MinPrize = minPrize;
			MaxPrize = maxPrize;
		}

		/// <summary>
		/// Returns true if there are no triggers and the awarded prize is between min and max (inclusive), otherwise returns false.
		/// </summary>
		public override bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow)
		{
			var prize = (int)result.AwardedPrize.ToUInt64();
			return prize >= MinPrize && prize <= MaxPrize;
		}

		public override IResult ToString(string format)
		{
			return $"Awarded prize must be between {MinPrize} and {MaxPrize}".ToSuccess();
		}
	}
}