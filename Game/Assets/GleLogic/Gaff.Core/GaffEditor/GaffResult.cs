using System;
using System.Collections.Generic;

namespace Gaff.Core.GaffEditor
{
	public sealed class GaffResult
	{
		public static readonly GaffResult Default = new GaffResult(Array.Empty<StageGaffResult>(), Array.Empty<int>(), Array.Empty<int>(), false);

		public bool Success { get; }
		public string Message { get; }
		public IReadOnlyList<int> StepSearchCounts { get; }
		public IReadOnlyList<int> StepFailCounts { get; }
		public IReadOnlyList<StageGaffResult> OrderedResults { get; }

		public GaffResult(IReadOnlyList<StageGaffResult> orderedResults, IReadOnlyList<int> stepSearchCounts, IReadOnlyList<int> stepFailCounts, bool success = true, string message = null)
		{
			OrderedResults = orderedResults;
			StepSearchCounts = stepSearchCounts;
			StepFailCounts = stepFailCounts;
			Success = success;
			Message = message;
		}
	}
}