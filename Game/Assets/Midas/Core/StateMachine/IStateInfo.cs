using System;
using Midas.Core.Coroutine;

namespace Midas.Core.StateMachine
{
	/// <summary>
	/// Used for state machine coroutines, giving the coroutine the ability to switch states.
	/// </summary>
	/// <typeparam name="TEnum"></typeparam>
	public interface IStateInfo<TEnum> where TEnum : Enum
	{
		/// <summary>
		/// Gets the current state that the state machine is in.
		/// </summary>
		TEnum CurrentState { get; }

		/// <summary>
		/// Set the next state.
		/// </summary>
		/// <remarks>Once set, the coroutine will advance states on the next yield.</remarks>
		/// <param name="nextState">The next state to enter.</param>
		CoroutineInstruction SetNextState(TEnum nextState);
	}
}