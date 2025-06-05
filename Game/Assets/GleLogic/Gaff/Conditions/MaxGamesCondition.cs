using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

// ReSharper disable MemberCanBePrivate.Global

namespace Gaff.Conditions
{
	/// <summary>
	/// This condition will force a GaffSequence to fail if the sequence length goes above MaxGames count.
	/// </summary>
	public sealed class MaxGamesCondition : StepCondition
	{
		/// <summary>
		/// The max game results to have in a sequence.
		/// </summary>
		public int MaxGames { get; }

		public MaxGamesCondition(int maxGames)
		{
			MaxGames = maxGames;
		}

		/// <inheritdoc />
		public override StepConditionResult CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData)
		{
			return sequenceUpToNow.Count <= MaxGames ? StepConditionResult.Found : StepConditionResult.Fail;
		}

		/// <inheritdoc />
		public override IResult ToString(string format)
		{
			return $"Max games {MaxGames}".ToSuccess();
		}
	}
}