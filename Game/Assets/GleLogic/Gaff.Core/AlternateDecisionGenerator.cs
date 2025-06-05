using System;
using System.Collections.Generic;
using Logic.Core.DecisionGenerator;
using Logic.Core.DecisionGenerator.Decisions;

namespace Gaff.Core
{
	/// <summary>
	/// Records any decisions made using the specified decision generator to provide the values stored with the the DecisionDefinition objects.
	/// A GetDecision abstract function allows for additional logic to be applied when formulating the final result of the decision call.
	/// </summary>
	public abstract class AlternateDecisionGenerator : IDecisionGenerator
	{
		private readonly IDecisionGenerator decisionGenerator;
		private readonly List<Decision> orderedDecisions;

		protected AlternateDecisionGenerator(IDecisionGenerator decisionGenerator)
		{
			this.decisionGenerator = decisionGenerator;
			orderedDecisions = new List<Decision>();
		}

		/// <summary>
		/// An ordered list of all requested decisions.
		/// </summary>
		public IReadOnlyList<Decision> OrderedDecisions => orderedDecisions;

		/// <inheritdoc />
		public bool GetDecision(ulong trueWeight, ulong falseWeight, Func<string> getContext)
		{
			var context = getContext();
			var decision = GetDecisionInternal(context,
				() => new SimpleDecision(context, trueWeight, falseWeight),
				() => decisionGenerator.GetDecision(trueWeight, falseWeight, getContext));

			return (bool)decision.Result;
		}

		/// <inheritdoc />
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			var context = getContext();
			var decision = GetDecisionInternal(context,
				() => new IndexesDecision(context, indexCount, count, allowDuplicates, getName),
				() => decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getName, getContext));

			return (IReadOnlyList<ulong>)decision.Result;
		}

		/// <inheritdoc />
		public IReadOnlyList<ulong> ChooseIndexes(ulong indexCount, uint count, bool allowDuplicates, Func<ulong, ulong> getWeight, ulong totalWeight, Func<ulong, string> getName, Func<string> getContext)
		{
			var context = getContext();
			var decision = GetDecisionInternal(context,
				() => new WeightedIndexesDecision(context, indexCount, count, allowDuplicates, getName, totalWeight, getWeight),
				() => decisionGenerator.ChooseIndexes(indexCount, count, allowDuplicates, getWeight, totalWeight, getName, getContext));

			return (IReadOnlyList<ulong>)decision.Result;
		}

		/// <inheritdoc />
		public IReadOnlyList<ulong> ChooseIndexes(IWeights weights, uint count, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			var context = getContext();
			var decision = GetDecisionInternal(context,
				() => new WeightsIndexesDecision(context, weights, count, allowDuplicates, getName),
				() => decisionGenerator.ChooseIndexes(weights, count, allowDuplicates, getName, getContext));

			return (IReadOnlyList<ulong>)decision.Result;
		}

		public IReadOnlyList<ulong> PickIndexes(ulong indexCount, uint minCount, uint maxCount, bool allowDuplicates, Func<ulong, string> getName, Func<string> getContext)
		{
			var context = getContext();
			var decision = GetDecisionInternal(context,
				() => new PickIndexesDecision(context, indexCount, minCount, maxCount, allowDuplicates, getName),
				() => decisionGenerator.PickIndexes(indexCount, minCount, maxCount, allowDuplicates, getName, getContext));

			return (IReadOnlyList<ulong>)decision.Result;
		}

		/// <summary>
		/// Use the function to select a specific result or call result() to generate a result using the supplied decision generator.
		/// </summary>
		protected abstract Decision GetDecision<T>(string context, Func<T> decisionDefinition, Func<object> result) where T : DecisionDefinition;

		private Decision GetDecisionInternal<T>(string context, Func<T> decisionDefinition, Func<object> result) where T : DecisionDefinition
		{
			var decisionResult = GetDecision(context, decisionDefinition, result);
			orderedDecisions.Add(decisionResult);
			return decisionResult;
		}
	}
}