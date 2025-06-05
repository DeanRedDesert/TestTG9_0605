using System.Collections.Generic;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Core.Conditions
{
	/// <summary>
	/// The outcome of the StepCondition.CheckCondition function.
	/// </summary>
	public enum StepConditionResult
	{
		Found, // This condition is satisfied. If all other step conditions are met move to the next step.
		KeepSearching, // Keep accumulating results in the step.
		Fail // Something has happened that mean this step condition can no longer be satisfied.  Abort the current search and restart.
	}

	/// <summary>
	/// A step condition is used to determine if a the step is complete and valid.
	/// </summary>
	public abstract class StepCondition : IToString
	{
		/// <summary>
		/// Check a cycle result to see if we need to move to the next step.
		/// </summary>
		/// <param name="result">The new result to process.</param>
		/// <param name="initialResultForStep">The result that this current step started with.</param>
		/// <param name="sequenceUpToNow">A set of results gathered so far for this GaffSequence.</param>
		/// <param name="stateData">A state variable that will be preserved between results.  Use this to hold game counts, symbol counts, etc.</param>
		/// <returns>Return true to move to the next state.  Return false to keep gathering results. Return null to stop the current search and start a new one.</returns>
		// ReSharper disable once UnusedParameter.Global
		public abstract StepConditionResult CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow, ref object stateData);

		/// <summary>
		/// Need to implement 'SL' format for display in the UI.
		/// </summary>
		public abstract IResult ToString(string format);
	}
}