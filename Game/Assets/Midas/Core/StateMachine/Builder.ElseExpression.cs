using System.Linq;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class ElseExpression : IfExpression
		{
			public ElseExpression(Builder builder, State[] states)
				: base(builder, states)
			{
				var rule = builder.ruleStatements.Last();
				var newTransition = new TransitionStatement();
				rule.Transitions.Add(newTransition);
				newTransition.IsElseTransition = true;
			}
		}
	}
}