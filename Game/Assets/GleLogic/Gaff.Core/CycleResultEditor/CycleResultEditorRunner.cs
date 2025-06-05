using System;
using System.Collections.Generic;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Engine;

namespace Gaff.Core.CycleResultEditor
{
	public sealed class CycleResultEditorResult
	{
		public CycleResult Result { get; }
		public IReadOnlyList<Decision> Decisions { get; }

		public CycleResultEditorResult(CycleResult result, IReadOnlyList<Decision> decisions)
		{
			Result = result;
			Decisions = decisions;
		}
	}

	public static class CycleResultEditorRunner
	{
		public static CycleResultEditorResult ProcessCycleResultEditorResult(this CycleResult newResult, IDecisionGenerator decisionGeneratorUsed)
		{
			if (!(decisionGeneratorUsed is OverridableDecisionGenerator resultEditorDecisionGenerator))
				throw new Exception("Not a ResultEditorDecisionGenerator");

			return new CycleResultEditorResult(newResult, resultEditorDecisionGenerator.OrderedDecisions);
		}

		public static IReadOnlyList<Decision> GetNewDecisionResults(this IReadOnlyList<Decision> latestResults, IReadOnlyList<Decision> overridesProvided, out IReadOnlyList<Decision> usedOverrides)
		{
			var newOnes = new List<Decision>();
			var usedOnes = new List<Decision>();

			foreach (var decisionResult in latestResults)
			{
				var found = false;
				foreach (var result in overridesProvided)
				{
					if (!decisionResult.Equals(result))
						continue;

					usedOnes.Add(decisionResult);
					found = true;
				}

				if (!found)
					newOnes.Add(decisionResult);
			}

			usedOverrides = usedOnes;

			return newOnes;
		}
	}
}