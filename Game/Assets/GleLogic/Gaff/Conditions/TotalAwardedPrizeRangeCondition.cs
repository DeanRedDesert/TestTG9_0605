using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	/// <summary>
	/// This condition will return true if the awarded prize is between <see cref="MinPrize"/> and <see cref="MaxPrize"/> (inclusive), otherwise returns false.
	/// </summary>
	public sealed class TotalAwardedPrizeRangeCondition : StepCondition
	{
		/// <summary>
		/// The minimum total awarded credits.
		/// </summary>
		public int MinPrize { get; }

		/// <summary>
		/// The maximum total awarded credits.
		/// </summary>
		public int MaxPrize { get; }

		public TotalAwardedPrizeRangeCondition(int minPrize, int maxPrize)
		{
			MinPrize = minPrize;
			MaxPrize = maxPrize;
		}

		/// <inheritdoc />
		public override StepConditionResult CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData)
		{
			var prize = (int)result.TotalAwardedPrize.ToUInt64();

			if (prize > MaxPrize)
				return StepConditionResult.Fail;

			return prize < MinPrize ? StepConditionResult.KeepSearching : StepConditionResult.Found;
		}

		/// <inheritdoc />
		public override IResult ToString(string format)
		{
			return $"Total Awarded prize must be between {MinPrize} and {MaxPrize} (inclusive)".ToSuccess();
		}
	}
}