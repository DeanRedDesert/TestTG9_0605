using System;
using System.Collections.Generic;

namespace Midas.Core.Coroutine
{
	/// <summary>
	/// Provides the ability to control a coroutine.
	/// </summary>
	public abstract class Coroutine
	{
		private readonly CoroutineRun runner;

		/// <summary>
		/// Gets whether the coroutine is running.
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routine">The routine to run.</param>
		protected Coroutine(IEnumerator<CoroutineInstruction> routine)
		{
			runner = new CoroutineRun(routine, $"Root({routine})", false);
			IsRunning = true;
		}

		/// <summary>
		/// Call this to stop the coroutine.
		/// </summary>
		public void Stop()
		{
			RemoveCoroutine();
			IsRunning = false;
		}

		/// <summary>
		/// When implemented in derived classes, removes the coroutine from whatever subsystem it is attached to.
		/// </summary>
		protected abstract void RemoveCoroutine();

		/// <summary>
		/// Called by derived classes to step the coroutine at whatever interval the attached subsystem allows.
		/// </summary>
		protected void DoStep(TimeSpan deltaTime)
		{
			if (runner.Check(FrameTime.CurrentTime, deltaTime))
				Stop();
		}

		public override string ToString() => runner.ToString();
	}
}