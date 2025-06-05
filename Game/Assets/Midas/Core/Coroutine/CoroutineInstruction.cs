using System;

namespace Midas.Core.Coroutine
{
	/// <summary>
	/// Abstract base for a coroutine instruction.
	/// </summary>
	public abstract class CoroutineInstruction
	{
		private readonly string name;

		protected CoroutineInstruction(string name)
		{
			this.name = name;
		}

		/// <summary>
		/// Check function for the instruction. Returns true to allow the coroutine to continue.
		/// </summary>
		/// <param name="currentTime">The current game time.</param>
		/// <param name="deltaTime">The delta time from the previous frame.</param>
		/// <returns>True to allow the coroutine to continue, otherwise false to keep waiting.</returns>
		public abstract bool Check(TimeSpan currentTime, TimeSpan deltaTime);

		public override string ToString() => name;
	}
}