using System.Linq;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class OptionsExpression : ThenExpression
		{
			public OptionsExpression(string name, Builder builder, State[] states) : base(builder, states)
			{
				var rule = builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				currentTransition.Name = name;
			}

			public OptionsExpression(string name, int priority, Builder builder, State[] states) : base(builder, states)
			{
				var rule = builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				currentTransition.Name = name;
				currentTransition.Priority = priority;
			}

			public OptionsExpression(int priority, Builder builder, State[] states) : base(builder, states)
			{
				var rule = builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				currentTransition.Priority = priority;
			}
		}
	}
}