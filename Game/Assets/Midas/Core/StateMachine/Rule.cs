using System.Collections.Generic;
using System.Linq;

namespace Midas.Core.StateMachine
{
	public sealed class Rule
	{
		internal HashSet<State> States { get; }

		public string Name { get; }

		public IReadOnlyList<Transition> Transitions { get; }

		public Rule(HashSet<State> states, params Transition[] transitions)
		{
			States = states;
			Transitions = transitions.ToArray();
		}

		public Rule(State state, params Transition[] transitions) :
			this(new HashSet<State> { state }, transitions)
		{
		}

		public Rule(string name, State state, params Transition[] transitions) :
			this(name, new HashSet<State> { state }, transitions)
		{
		}

		public Rule(string name, HashSet<State> states, params Transition[] transitions) : this(states, transitions)
		{
			Name = name;
		}
	}
}