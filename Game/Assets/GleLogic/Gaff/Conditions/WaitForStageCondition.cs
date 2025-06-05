using System.Collections.Generic;
using Gaff.Core;
using Gaff.Core.Conditions;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Conditions
{
	/// <summary>
	/// Plays a cycle and if we match the next stage then return with success,
	/// if we have not finished a game sequence return with false,
	/// if we have finished the game sequence return null.
	/// </summary>
	public sealed class WaitForStageCondition : StepCondition
	{
		public string StageName { get; }

		public WaitForStageCondition(string stageName)
		{
			StageName = stageName;
		}

		public override StepConditionResult CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData)
		{
			// If we run to the end of a game then we failed and we should bail.

			return result.Next().CheckStage(StageName) ? StepConditionResult.Found : result.Cycles.IsFinished ? StepConditionResult.Fail : StepConditionResult.KeepSearching;
		}

		public override IResult ToString(string format)
		{
			return $"Play until Stage '{StageName}'".ToSuccess();
		}
	}
}