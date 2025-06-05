using System;
using Gaff.Conditions;
using Gaff.Core.DecisionMakers;
using Logic.Core.DecisionGenerator.Decisions;
using Logic.Core.Utility;

namespace Gaff.DecisionMakers
{
	public sealed class SimpleDecisionMaker : DecisionMaker
	{
		public SimpleDecisionMaker(StringCondition contextCondition, bool desiredResult)
		{
			ContextCondition = contextCondition;
			DesiredResult = desiredResult;
		}

		public StringCondition ContextCondition { get; }
		public bool DesiredResult { get; }

		public override IResult ToString(string format)
		{
			return $"If the context {ContextCondition} then set the result to {DesiredResult}".ToSuccess();
		}

		public override bool Valid(string context, Func<DecisionDefinition> decisionData, ref object stateData)
		{
			return ContextCondition.Check(context) && decisionData() is SimpleDecision;
		}

		public override DecisionOutcome Create(DecisionDefinition decisionData, object stateData)
		{
			switch (decisionData)
			{
				case SimpleDecision _: return new Decision(decisionData, DesiredResult);
				default: throw new ArgumentOutOfRangeException(nameof(decisionData));
			}
		}
	}
}