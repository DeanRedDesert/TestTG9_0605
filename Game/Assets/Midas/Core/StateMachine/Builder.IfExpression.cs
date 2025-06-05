using System;
using System.Linq;
using Midas.Core.General;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public class IfExpression
		{
			public delegate bool WaitUntilDelegate();

			private readonly State[] states;
			private readonly Builder builder;

			public IfExpression(Builder builder, State[] states)
			{
				this.builder = builder;
				this.states = states;
			}

			public IfExpression(Builder builder, Func<bool> condition, State[] states)
			{
				this.builder = builder;
				this.states = states;

				var rule = this.builder.ruleStatements.Last();
				var newTransition = new TransitionStatement();
				rule.Transitions.Add(newTransition);
				newTransition.Cond = condition;
			}

			public IfExpression(Builder builder, ValueRef<bool> condition, State[] states) :
				this(builder, () => condition.Value, states)
			{
			}

			public WaitExpression Wait(TimeSpan timeout, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null, StateExitDelegate onExit = null)
			{
				return new WaitExpression("WaitState", timeout, builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(Func<TimeSpan> timeout, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null, StateExitDelegate onExit = null)
			{
				return new WaitExpression("WaitState", timeout, builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(params StateMachine[] dependencies)
			{
				return new WaitExpression("WaitState", dependencies, builder, states, null, null, null);
			}

			public WaitExpression Wait(StateEnterDelegate onEnter, StateExecuteDelegate onExecute, StateExitDelegate onExit, params StateMachine[] dependencies)
			{
				return new WaitExpression("WaitState", dependencies, builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(string name, TimeSpan timeout, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null, StateExitDelegate onExit = null)
			{
				return new WaitExpression(name, timeout, builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(string name, WaitUntilDelegate canExitWait, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null,
				StateExitDelegate onExit = null)
			{
				return new WaitExpression(name, canExitWait() ? TimeSpan.Zero : TimeSpan.FromDays(6969), builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(string name, Func<TimeSpan> timeout, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null,
				StateExitDelegate onExit = null)
			{
				return new WaitExpression(name, timeout, builder, states, onEnter, onExecute, onExit);
			}

			public WaitExpression Wait(string name, params StateMachine[] dependencies)
			{
				return new WaitExpression(name, dependencies, builder, states, null, null, null);
			}

			public WaitExpression Wait(string name, StateEnterDelegate onEnter, StateExecuteDelegate onExecute, StateExitDelegate onExit, params StateMachine[] dependencies)
			{
				return new WaitExpression(name, dependencies, builder, states, onEnter, onExecute, onExit);
			}

			public ThenExpression Then(State nextState)
			{
				return new ThenExpression(builder, nextState, states);
			}
		}
	}
}