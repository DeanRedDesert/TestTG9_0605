using System.Collections.Generic;
using Gaff.Core.GaffEditor;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Gaff.Core.Conditions
{
	/// <summary>
	/// A result condition is used to determine if a particular result is able to be added to the current step result list.
	/// </summary>
	public abstract class ResultCondition : IToString
	{
		/// <summary>
		/// Check a cycle result to see if we should add it to the step results list.
		/// </summary>
		/// <param name="result">The new result to process.</param>
		/// <param name="initialResultForStep">The result that this current step started with.</param>
		/// <param name="sequenceUpToNow">A set of results gathered so far for this GaffSequence.</param>
		/// <returns>Returns true to store this result. Returns false to keep searching.</returns>
		// ReSharper disable UnusedParameter.Global
		public abstract bool CheckCondition(CycleResult result, CycleResult initialResultForStep, IReadOnlyList<StageGaffResult> sequenceUpToNow);

		/// <summary>
		/// Need to implement 'SL' format for display in the UI.
		/// </summary>
		public abstract IResult ToString(string format);
	}
}