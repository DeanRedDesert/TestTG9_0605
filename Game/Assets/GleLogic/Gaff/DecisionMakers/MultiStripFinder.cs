using System;
using System.Collections.Generic;
using System.Linq;
using Gaff.Conditions;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Types;
using Logic.Core.Utility;

namespace Gaff.DecisionMakers
{
	/// <summary>
	/// The selection strategies available for decisions.
	/// </summary>
	public enum SelectionStrategy
	{
		/// <summary>
		/// Decisions are overridden from the left.
		/// </summary>
		Left,

		/// <summary>
		/// Decisions are overridden randomly.
		/// </summary>
		Any,

		/// <summary>
		/// Decisions are overridden from the right.
		/// </summary>
		Right
	}

	public sealed class MultiStripFinder : DecisionMaker
	{
		private sealed class MsState
		{
			public MsState(ReadOnlyMask expectedChoiceMask, IReadOnlyList<string> chosenContexts, int currentCount)
			{
				ExpectedChoiceMask = expectedChoiceMask;
				ChosenContexts = chosenContexts;
				CurrentCount = currentCount;
			}

			public ReadOnlyMask ExpectedChoiceMask { get; }
			public IReadOnlyList<string> ChosenContexts { get; }
			public int CurrentCount { get; }
		}

		public MultiStripFinder(StringCondition contextCondition, StringCondition symbolCondition, uint symbolWindow, int expectedDecisions, int minCount, int maxCount, SelectionStrategy strategy)
		{
			ContextCondition = contextCondition;
			SymbolCondition = symbolCondition;
			SymbolWindow = symbolWindow;
			ExpectedDecisions = expectedDecisions;
			MinCount = minCount;
			MaxCount = maxCount;
			Strategy = strategy;
		}

		public StringCondition ContextCondition { get; }

		public StringCondition SymbolCondition { get; }
		public uint SymbolWindow { get; }
		public int ExpectedDecisions { get; }
		public int MinCount { get; }
		public int MaxCount { get; }
		public SelectionStrategy Strategy { get; }

		#region Overrides of DecisionResultMaker

		public override IResult ToString(string format)
		{
			return $"{MinCount}-{MaxCount} out of {ExpectedDecisions} 'context {ContextCondition}' {SymbolCondition} within {SymbolWindow} stops with an {Strategy} strategy".ToSuccess();
		}

		public override bool Valid(string context, Func<DecisionDefinition> decisionData, ref object stateData)
		{
			if (decisionData() is SimpleDecision || !ContextCondition.Check(context))
				return false;

			if (stateData == null)
			{
				var count = MinCount == MaxCount ? MinCount : MinCount + GaffDecisionHelper.ChooseRandomIndexes(MaxCount - MinCount + 1, 1, false)[0];

				ReadOnlyMask mask;
				switch (Strategy)
				{
					case SelectionStrategy.Left:
					{
						mask = ReadOnlyMask.CreateFromIndexes(ExpectedDecisions, Enumerable.Range(0, count).ToArray());
						break;
					}
					case SelectionStrategy.Any:
					{
						var chosenIndexes = GaffDecisionHelper.ChooseRandomIndexes(ExpectedDecisions, (uint)count, false);
						mask = ReadOnlyMask.CreateFromIndexes(ExpectedDecisions, chosenIndexes);
						break;
					}
					case SelectionStrategy.Right:
					{
						mask = ReadOnlyMask.CreateFromIndexes(ExpectedDecisions, Enumerable.Range(ExpectedDecisions - count, count).ToArray());
						break;
					}
					default: throw new NotSupportedException();
				}

				stateData = new MsState(mask, new string[ExpectedDecisions], 0);
			}
			else
			{
				var oldState = (MsState)stateData;

				if (oldState.CurrentCount + 1 > ExpectedDecisions)
					return false;

				if (oldState.ExpectedChoiceMask[oldState.CurrentCount])
				{
					var chosenContexts = oldState.ChosenContexts.ToArray();

					chosenContexts[oldState.CurrentCount] = context;

					stateData = new MsState(oldState.ExpectedChoiceMask, chosenContexts, oldState.CurrentCount + 1);
				}
				else
					stateData = new MsState(oldState.ExpectedChoiceMask, oldState.ChosenContexts, oldState.CurrentCount + 1);
			}

			return true;
		}

		public override DecisionOutcome Create(DecisionDefinition decisionData, object stateData)
		{
			var state = (MsState)stateData;
			var isSpecial = state.ExpectedChoiceMask[state.CurrentCount];

			switch (decisionData)
			{
				case WeightedIndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (DecisionOutcome(d.IndexCount, d.GetName, isSpecial, out var decision))
						return decision;

					break;
				}
				case IndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (DecisionOutcome(d.IndexCount, d.GetName, isSpecial, out var decision))
						return decision;

					break;
				}
				case WeightsIndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (DecisionOutcome(d.Weights.GetLength(), d.GetName, isSpecial, out var decision))
						return decision;

					break;
				}
				case PickIndexesDecision d:
				{
					if (d.MinCount != 1 || d.MaxCount != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (DecisionOutcome(d.IndexCount, d.GetName, isSpecial, out var decision))
						return decision;

					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(decisionData));
			}

			return new DecisionOutcome("This decision maker could not find the requested symbol.");

			bool DecisionOutcome(ulong indexCount, Func<ulong, string> getName, bool selectIfTrue, out DecisionOutcome decision)
			{
				var conditionMet = GaffDecisionHelper.ConditionalIndexSelection(getName, s => selectIfTrue ? SymbolCondition.Check(s) : !SymbolCondition.Check(s), indexCount, SymbolWindow, out var decisionIndex);

				decision = new Decision(decisionData, new[] { decisionIndex });
				return conditionMet;
			}
		}

		#endregion
	}
}