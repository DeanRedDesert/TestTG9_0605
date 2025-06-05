using System;
using System.Collections.Generic;

namespace Midas.Core.Coroutine
{
	public sealed class CoroutineRun : CoroutineInstruction
	{
		private readonly bool finishedImmediately;
		private readonly IEnumerator<CoroutineInstruction> routine;
		private CoroutineInstruction nextInstruction;

		internal CoroutineRun(IEnumerator<CoroutineInstruction> child, string name, bool runFirstStep)
			: base(name ?? "Run Child")
		{
			routine = child;

			if (runFirstStep)
			{
				// The routine could in theory finish immediately

				finishedImmediately = !routine.MoveNext();
				if (!finishedImmediately)
					nextInstruction = routine.Current;
			}
		}

		public CoroutineRun(IEnumerator<CoroutineInstruction> child, string name = null)
			: this(child, name ?? "Run Child", true)
		{
		}

		public override bool Check(TimeSpan currentTime, TimeSpan deltaTime)
		{
			if (finishedImmediately)
				return true;

			if (nextInstruction == null || nextInstruction.Check(FrameTime.CurrentTime, deltaTime))
			{
				if (!routine.MoveNext())
					return true;

				nextInstruction = routine.Current;
			}

			return false;
		}

		public override string ToString()
		{
			return $"{base.ToString()} -> {nextInstruction?.ToString() ?? "Wait One Frame"}";
		}
	}

	/// <summary>
	/// Coroutine instruction that delays the specified amount of time before allowing the coroutine to continue.
	/// </summary>
	public sealed class CoroutineDelay : CoroutineInstruction
	{
		private TimeSpan waitRemaining;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delay">The delay time.</param>
		public CoroutineDelay(TimeSpan delay)
			: base($"Delay {delay}")
		{
			waitRemaining = delay;
		}

		public CoroutineDelay(double seconds)
			: this(TimeSpan.FromSeconds(seconds))
		{
		}

		/// <inheritdoc />
		public override bool Check(TimeSpan currentTime, TimeSpan deltaTime)
		{
			waitRemaining -= deltaTime;

			return waitRemaining <= TimeSpan.Zero;
		}
	}

	/// <summary>
	/// Coroutine instruction that delays the specified amount of time before allowing the coroutine to continue.
	/// </summary>
	public sealed class CoroutineDelayWithPredicate : CoroutineInstruction
	{
		private TimeSpan waitRemaining;
		private readonly Func<bool> shouldContinue;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="delay">The delay time.</param>
		/// <param name="shouldContinue">A predicate that indicates if the coroutine should continue immediately.</param>
		public CoroutineDelayWithPredicate(TimeSpan delay, Func<bool> shouldContinue)
			: base("CoroutineDelayWithPredicate")
		{
			waitRemaining = delay;
			this.shouldContinue = shouldContinue;
		}

		public CoroutineDelayWithPredicate(double seconds, Func<bool> shouldContinue)
			: this(TimeSpan.FromSeconds(seconds), shouldContinue)
		{
		}

		/// <inheritdoc />
		public override bool Check(TimeSpan currentTime, TimeSpan deltaTime)
		{
			waitRemaining -= deltaTime;

			return waitRemaining <= TimeSpan.Zero || shouldContinue();
		}
	}
}