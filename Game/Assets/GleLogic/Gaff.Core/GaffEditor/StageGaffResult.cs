using System.Collections.Generic;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Engine;

namespace Gaff.Core.GaffEditor
{
	public sealed class StageGaffResult
	{
		public string GaffStep { get; }
		public IReadOnlyList<Decision> Decisions { get; }
		public CycleResult CycleResult { get; }
		public IReadOnlyList<string> Messages { get; }

		public StageGaffResult(string gaffStep, IReadOnlyList<Decision> decisions, CycleResult cycleResult, IReadOnlyList<string> messages)
		{
			GaffStep = gaffStep;
			Decisions = decisions;
			CycleResult = cycleResult;
			Messages = messages;
		}
	}
}