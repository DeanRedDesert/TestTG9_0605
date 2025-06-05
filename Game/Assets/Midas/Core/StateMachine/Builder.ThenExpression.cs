using System;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public class ThenExpression
		{
			private readonly Builder builder;
			private readonly State[] states;

			public ThenExpression(Builder builder, State[] states)
			{
				this.builder = builder;
				this.states = states;
			}

			public ThenExpression(Builder builder, State nextState, State[] states)
			{
				this.builder = builder;
				this.states = states;
				var rule = this.builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				currentTransition.Target = nextState;
			}

			public IfExpression If(Func<bool> condition)
			{
				return new IfExpression(builder, condition, states);
			}

			public IfExpression If(ValueRef<bool> condition)
			{
				return new IfExpression(builder, condition, states);
			}

			public IfNotExpression IfNot(Func<bool> condition)
			{
				return new IfNotExpression(builder, condition, states);
			}

			public IfNotExpression IfNot(ValueRef<bool> condition)
			{
				return new IfNotExpression(builder, condition, states);
			}

			public ElseExpression Else()
			{
				return new ElseExpression(builder, states);
			}

			public OptionsExpression TransitionName(string name)
			{
				return new OptionsExpression(name, builder, states);
			}

			public OptionsExpression TransitionNameAndPriority(string name, int lowerIsBetter)
			{
				return new OptionsExpression(name, lowerIsBetter, builder, states);
			}

			public OptionsExpression Priority(int lowerIsBetter)
			{
				return new OptionsExpression(lowerIsBetter, builder, states);
			}
		}
	}
}