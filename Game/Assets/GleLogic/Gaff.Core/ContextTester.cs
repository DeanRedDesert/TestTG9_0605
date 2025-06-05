using System;
using System.Collections.Generic;
using Logic.Core.DecisionGenerator;

namespace Gaff.Core
{
	/// <summary>
	/// A decision generator wrapper to detect duplicate context names in a set of decision calls.
	/// </summary>
	public sealed class ContextTester : IDecisionGenerator
	{
		private readonly IDecisionGenerator decisionGenerator;
		private readonly HashSet<string> usedContextNames = new HashSet<string>();

		public ContextTester(IDecisionGenerator decisionGenerator)
		{
			this.decisionGenerator = decisionGenerator;
		}

		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
		{
			ProcessContextName(getContext());
			return decisionGenerator.GetDecision(trueWeight, falseWeight, getContext);
		}

		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			ProcessContextName(getContext());
			return decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext);
		}

		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
		{
			ProcessContextName(getContext());
			return decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext);
		}

		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			ProcessContextName(getContext());
			return decisionGenerator.ChooseIndexes(weights, count, allowDuplicates, getName, getContext);
		}

		public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			ProcessContextName(getContext());
			return decisionGenerator.PickIndexes(indexCount, minCount, maxCount, allowDuplicates, getName, getContext);
		}

		private void ProcessContextName(string context)
		{
			if (!usedContextNames.Add(context))
				throw new Exception($"The context {context} has been used more than once to create the logic result.\nPlease check the call stack of this exception to find out where this issue occurred.");
		}
	}
}