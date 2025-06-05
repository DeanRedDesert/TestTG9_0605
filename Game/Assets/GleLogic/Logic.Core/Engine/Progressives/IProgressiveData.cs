using System.Collections.Generic;

namespace Logic.Core.Engine.Progressives
{
	/// <summary>
	/// Provides a function to get the user defined progressives in the core progressive format.
	/// </summary>
	public interface IProgressiveData
	{
		/// <summary>
		/// Get the progressive sets for the specified input set.  Proved error information on failure.
		/// </summary>
		/// <param name="inputSet">The input set to base the progressive levels on.</param>
		/// <param name="progressiveLevels">The resultant progressive sets.</param>
		/// <param name="errorMsg">Any errors that occured.</param>
		/// <returns>If the operation was successful.</returns>
		bool TryGetProgressiveSets(IReadOnlyList<Input> inputSet, out IReadOnlyList<ProgressiveLevel> progressiveLevels, out string errorMsg);
	}
}