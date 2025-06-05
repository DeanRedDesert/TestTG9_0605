using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.Coroutine;

namespace Midas.Core.StateMachine
{
	public static class CoroutineExtensionMethods
	{
		public sealed class StateMachineCoroutineArgs : StateArgs
		{
			public Coroutine.Coroutine Coroutine { get; internal set; }
		}

		private sealed class StateInfo<TEnum> : IStateInfo<TEnum> where TEnum : Enum
		{
			public TEnum CurrentState { get; private set; }

			public TEnum NextState { get; private set; }

			public CoroutineInstruction SetNextState(TEnum nextState)
			{
				NextState = nextState;
				return null;
			}

			public void SetCurrentState(TEnum current)
			{
				CurrentState = current;
				NextState = current;
			}
		}

		private sealed class StateMachineCoroutine : Coroutine.Coroutine
		{
			private readonly StateMachine coroutineStateMachine;
			private TimeSpan? lastStepTime;
			private bool removed;

			public StateMachineCoroutine(StateMachine stateMachine, IEnumerator<CoroutineInstruction> routine) : base(routine)
			{
				coroutineStateMachine = stateMachine;
				foreach (var state in stateMachine.States)
				{
					state.OnExecute += _ => DoStep();
				}
			}

			protected override void RemoveCoroutine()
			{
				StateMachines.Destroy(coroutineStateMachine);
				removed = true;
			}

			private void DoStep()
			{
				if (removed)
					return;

				// State machine coroutines may enter multiple times per frame to allow them to handle
				// status database changes before the frame is drawn, so we set delta to zero on re-entry.

				TimeSpan delta;
				if (lastStepTime == FrameTime.CurrentTime)
				{
					delta = TimeSpan.Zero;
				}
				else
				{
					delta = FrameTime.DeltaTime;
					lastStepTime = FrameTime.CurrentTime;
				}

				DoStep(delta);
			}
		}

		/// <summary>
		/// Init a coroutine as a child of a state machine. This kind of coroutine will potentially get many steps per frame
		/// until the status database stabilises.
		/// </summary>
		/// <param name="parentStateMachine">The parent state machine. If the parent is running then this coroutine will run.</param>
		/// <param name="routine">The routine to run.</param>
		/// <param name="name">The name of the coroutine. This is used for debugging purposes.</param>
		/// <returns>The new coroutine.</returns>
		public static Coroutine.Coroutine StartCoroutine(this StateMachine parentStateMachine, IEnumerator<CoroutineInstruction> routine, string name = null)
		{
			var coroutineArgs = new StateMachineCoroutineArgs();
			var stateStep = new State("Coroutine Step", true, coroutineArgs);
			var stayInIdle = new Rule(stateStep, new Transition(() => false, stateStep));
			var coroutineStateMachine = StateMachines.Create(name ?? "UnnamedCoroutine", new List<Rule> { stayInIdle }, parentStateMachine);
			var cor = new StateMachineCoroutine(coroutineStateMachine, routine);
			coroutineArgs.Coroutine = cor;
			return cor;
		}

		/// <summary>
		/// Initialise a coroutine as a child of a state machine. This coroutine will allow you to provide state information to the framework
		/// giving a more traditional state machine behaviour, as well as allowing for the editor to show where the coroutine is at.
		/// </summary>
		/// <param name="parentStateMachine">The parent state machine. If the parent is running then this coroutine will run.</param>
		/// <param name="routine">The routine to run.</param>
		/// <param name="startState">The initial state the state machine should be in.</param>
		/// <param name="name">The name of the coroutine. This is used for debugging purposes.</param>
		/// <typeparam name="TEnum">The enum type to generate states from.</typeparam>
		/// <returns>The new coroutine.</returns>
		public static Coroutine.Coroutine StartCoroutine<TEnum>(this StateMachine parentStateMachine, Func<IStateInfo<TEnum>, IEnumerator<CoroutineInstruction>> routine, TEnum startState, string name = null) where TEnum : Enum
		{
			var stateInfo = new StateInfo<TEnum>();
			var actualRoutine = routine(stateInfo);

			var states = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToDictionary(e => e, e => new State(e.ToString(), e.Equals(startState)));
			foreach (var state in states)
				state.Value.OnEnter += s => stateInfo.SetCurrentState(state.Key);

			var transitions = states.Values.Select(s => new Transition(() => !stateInfo.CurrentState.Equals(stateInfo.NextState) && states[stateInfo.NextState].Equals(s), s)).ToArray();
			var stateMachine = StateMachines.Create(name, new List<Rule> { new Rule(new HashSet<State>(states.Values), transitions) }, parentStateMachine);
			return new StateMachineCoroutine(stateMachine, actualRoutine);
		}
	}
}