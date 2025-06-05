using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public class InExpression
		{
			private readonly Builder builder;
			private readonly State[] states;

			[SuppressMessage("ReSharper", "UnusedParameter.Local")]
			public InExpression(Builder builder, bool newRule, params State[] states)
				: this(builder, states)
			{
				this.builder.ruleStatements.Add(new RuleStatement(this.states));
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

			public EnterExpression OnEnterDo(StateEnterDelegate action)
			{
				return new EnterExpression(action, builder, states);
			}

			public ExecExpression AlwaysDo(StateExecuteDelegate action)
			{
				return new ExecExpression(action, builder, states);
			}

			public ExitExpression OnExitDo(StateExitDelegate action)
			{
				return new ExitExpression(action, builder, states);
			}

			public void ReplaceTarget(State targetToReplace, State newTarget)
			{
				foreach (var rStmt in builder.ruleStatements)
				{
					if (rStmt.States.SequenceEqual(states))
					{
						foreach (var tStmt in rStmt.Transitions)
						{
							if (tStmt.Target == targetToReplace)
							{
								tStmt.Target = newTarget;
							}
						}
					}
				}
			}

			protected InExpression(Builder builder, params State[] states)
			{
				this.builder = builder;
				this.states = states;
			}
		}
	}
}