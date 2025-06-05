// Copyright (c) 2021 IGT

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace Midas.Core.StateMachine
{
	public sealed partial class Builder
	{
		private sealed class RuleStatement
		{
			#region Public

			public RuleStatement(State[] states)
			{
				States = states;
			}

			public State[] States { get; }

			public List<TransitionStatement> Transitions { get; } = new List<TransitionStatement>();

			#endregion
		}

		private sealed class TransitionStatement
		{
			#region Public

			public Func<bool> Cond { get; set; }
			public string Name { get; set; }
			public int Priority { get; set; } = 1000;
			public State Target { get; set; }
			public bool IsElseTransition { get; set; }
			public List<State> WaitStates { get; } = new List<State>();

			#endregion
		}

		private readonly List<RuleStatement> ruleStatements = new List<RuleStatement>();
		private readonly string name;
		private readonly StateMachine parent;
		private readonly int priority;

		public Builder(string name, StateMachine parent = null, int priority = StateMachines.DefaultPriority)
		{
			this.name = name;
			this.parent = parent;
			this.priority = priority;
		}

		public InExpression In(params State[] states)
		{
			return new InExpression(this, true, states);
		}

		public StateMachine CreateStateMachine()
		{
			return StateMachines.Create(name, CreateRules(), parent, priority);
		}

		public List<Rule> CreateRules()
		{
			var rules = new List<Rule>();
			foreach (var rStmt in ruleStatements)
			{
				var transitions = new List<Transition>();
				foreach (var tStmt in rStmt.Transitions)
				{
					if (tStmt.WaitStates.Count == 0)
					{
						transitions.Add(new Transition(tStmt.Name, tStmt.Cond, tStmt.Target, tStmt.Priority, tStmt.IsElseTransition));
					}
					else
					{
						//insert wait states and rules
						transitions.Add(new Transition(tStmt.Name, tStmt.Cond, tStmt.WaitStates[0], tStmt.Priority, tStmt.IsElseTransition));
						var currentState = tStmt.WaitStates[0];
						//make rules for rest of wait states
						for (var i = 0; i < tStmt.WaitStates.Count; ++i)
						{
							var waitTrans = i + 1 < tStmt.WaitStates.Count ? new Transition(() => true, tStmt.WaitStates[i + 1]) : new Transition(() => true, tStmt.Target);

							rules.Add(new Rule(currentState, waitTrans));
							currentState = i + 1 < tStmt.WaitStates.Count ? tStmt.WaitStates[i + 1] : currentState;
						}
					}
				}

				var states = new HashSet<State>();
				foreach (var state in rStmt.States)
				{
					states.Add(state);
				}

				rules.Add(new Rule(states, transitions.ToArray()));
			}

			return rules;
		}
	}
}