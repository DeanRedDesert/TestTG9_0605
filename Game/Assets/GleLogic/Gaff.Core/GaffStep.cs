using System.Collections.Generic;
using Gaff.Core.Conditions;
using Gaff.Core.DecisionMakers;

namespace Gaff.Core
{
	public sealed class GaffStep
	{
		public GaffStep(string title, IReadOnlyList<DecisionMaker> decisions, IReadOnlyList<ResultCondition> nextResultConditions = null, IReadOnlyList<StepCondition> nextStepConditions = null)
		{
			Title = title;
			Decisions = decisions;
			NextResultConditions = nextResultConditions;
			NextStepConditions = nextStepConditions;
		}

		public string Title { get; }
		public IReadOnlyList<DecisionMaker> Decisions { get; }
		public IReadOnlyList<ResultCondition> NextResultConditions { get; }
		public IReadOnlyList<StepCondition> NextStepConditions { get; }

		public GaffStep WithTitle(string title)
			=> new GaffStep(title, Decisions, NextResultConditions, NextStepConditions);

		public GaffStep WithDecisions(IReadOnlyList<DecisionMaker> decisions)
			=> new GaffStep(Title, decisions, NextResultConditions, NextStepConditions);

		public GaffStep WithResultConditions(IReadOnlyList<ResultCondition> resultConditions)
			=> new GaffStep(Title, Decisions, resultConditions, NextStepConditions);

		public GaffStep WithStepConditions(IReadOnlyList<StepCondition> stepConditions)
			=> new GaffStep(Title, Decisions, NextResultConditions, stepConditions);
	}
}