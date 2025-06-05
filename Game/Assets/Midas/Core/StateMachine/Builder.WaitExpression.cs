using System;
using System.Linq;

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		public sealed class WaitExpression
		{
			private readonly Builder builder;
			private readonly State[] states;

			public WaitExpression(string name, TimeSpan timeout, Builder builder, State[] states, StateEnterDelegate onEnter, StateExecuteDelegate onExecute,
				StateExitDelegate onExit)
			{
				this.builder = builder;
				this.states = states;

				var rule = this.builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				var ws = new State(name, timeout);
				if (onEnter != null)
				{
					ws.OnEnter += onEnter;
				}

				if (onExecute != null)
				{
					ws.OnExecute += onExecute;
				}

				if (onExit != null)
				{
					ws.OnExit += onExit;
				}

				currentTransition.WaitStates.Add(ws);
			}

			public WaitExpression(string name, Func<TimeSpan> timeout, Builder builder, State[] states, StateEnterDelegate onEnter, StateExecuteDelegate onExecute,
				StateExitDelegate onExit)
			{
				this.builder = builder;
				this.states = states;

				var rule = this.builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				var ws = new State(name, timeout);
				if (onEnter != null)
				{
					ws.OnEnter += onEnter;
				}

				if (onExecute != null)
				{
					ws.OnExecute += onExecute;
				}

				if (onExit != null)
				{
					ws.OnExit += onExit;
				}

				currentTransition.WaitStates.Add(ws);
			}

			public WaitExpression(string name, StateMachine[] dependencies, Builder builder, State[] states, StateEnterDelegate onEnter, StateExecuteDelegate onExecute,
				StateExitDelegate onExit)
			{
				this.builder = builder;
				this.states = states;
				var rule = this.builder.ruleStatements.Last();
				var currentTransition = rule.Transitions.Last();
				var ws = new State(name, dependencies);
				if (onEnter != null)
				{
					ws.OnEnter += onEnter;
				}

				if (onExecute != null)
				{
					ws.OnExecute += onExecute;
				}

				if (onExit != null)
				{
					ws.OnExit += onExit;
				}

				currentTransition.WaitStates.Add(ws);
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

			public WaitExpression Wait(string name, IfExpression.WaitUntilDelegate canExitWait, StateEnterDelegate onEnter = null, StateExecuteDelegate onExecute = null,
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