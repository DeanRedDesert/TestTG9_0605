using System;
using Gaff.Conditions;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;

namespace Gaff.DecisionMakers
{
	public sealed class StripFinder : DecisionMaker
	{
		public StripFinder(StringCondition contextCondition, StringCondition symbolCondition, uint symbolWindow)
		{
			ContextCondition = contextCondition;
			SymbolCondition = symbolCondition;
			SymbolWindow = symbolWindow;
		}

		public StringCondition ContextCondition { get; }
		public StringCondition SymbolCondition { get; }

		public uint SymbolWindow { get; }

		public override IResult ToString(string format)
		{
			return $"If the context {ContextCondition} then find a stop that has a symbol that {SymbolCondition} within {SymbolWindow} stops".ToSuccess();
		}

		public override bool Valid(string context, Func<DecisionDefinition> decisionData, ref object stateData)
		{
			return ContextCondition.Check(context) && !(decisionData() is SimpleDecision);
		}

		public override DecisionOutcome Create(DecisionDefinition decisionData, object stateData)
		{
			switch (decisionData)
			{
				case WeightedIndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (GaffDecisionHelper.ConditionalIndexSelection(i => d.GetName(i), s => SymbolCondition.Check(s), d.IndexCount, SymbolWindow, out var decision))
						return new Decision(decisionData, new[] { decision });

					break;
				}
				case IndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (GaffDecisionHelper.ConditionalIndexSelection(i => d.GetName(i), s => SymbolCondition.Check(s), d.IndexCount, SymbolWindow, out var decision))
						return new Decision(decisionData, new[] { decision });

					break;
				}
				case WeightsIndexesDecision d:
				{
					if (d.Count != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (GaffDecisionHelper.ConditionalIndexSelection(i => d.GetName(i), s => SymbolCondition.Check(s), d.Weights.GetLength(), SymbolWindow, out var decision))
						return new Decision(decisionData, new[] { decision });

					break;
				}
				case PickIndexesDecision d:
				{
					if (d.MinCount != 1 || d.MaxCount != 1)
						return new DecisionOutcome("This decision maker only supports decisions with 1 index requested.");

					if (GaffDecisionHelper.ConditionalIndexSelection(i => d.GetName(i), s => SymbolCondition.Check(s), d.IndexCount, SymbolWindow, out var decision))
						return new Decision(decisionData, new[] { decision });

					break;
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(decisionData));
			}

			return new DecisionOutcome("This decision maker could not find the requested symbol.");
		}
	}
}