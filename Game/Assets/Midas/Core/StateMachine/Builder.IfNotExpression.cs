using System;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class IfNotExpression : IfExpression
		{
			public IfNotExpression(Builder builder, Func<bool> condition, State[] states) :
				base(builder, states)
			{
				var rule = builder.ruleStatements.Last();
				var newTransition = new TransitionStatement();
				rule.Transitions.Add(newTransition);
				newTransition.Cond = () => !condition();
			}

			public IfNotExpression(Builder builder, ValueRef<bool> condition, State[] states) :
				this(builder, () => condition.Value, states)
			{
			}
		}
	}
}