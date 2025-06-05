using System;

namespace Midas.Core.StateMachine
{
	public delegate void TransitionDelegate(State currentState, State nextState);

	public sealed class Transition
	{
		public string Name { get; }

		public State Target { get; }

		public int Priority { get; }

		public bool IsElse { get; }

		internal Func<bool> Condition { get; }

		public Transition(Func<bool> condition, State targetState)
		{
			Condition = condition;
			Target = targetState;
		}

		public Transition(string name, Func<bool> condition, State targetState)
			: this(condition, targetState)
		{
			Name = name;
		}

		public Transition(Func<bool> condition, State targetState, int priority)
			: this(condition, targetState)
		{
			Priority = priority;
		}

		public Transition(string name, Func<bool> condition, State targetState, int priority)
			: this(condition, targetState, priority)
		{
			Name = name;
		}

		public Transition(string name, Func<bool> condition, State targetState, int priority, bool isElse) :
			this(name, condition, targetState, priority)
		{
			IsElse = isElse;
		}
	}
}