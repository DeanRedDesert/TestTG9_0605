using System;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	public enum TriggerPosition
	{
		/// <summary>
		/// Insert the trigger the end of all the existing CycleStates.
		/// </summary>
		AtEnd,

		/// <summary>
		/// Insert the trigger the after the current CycleState.
		/// </summary>
		AtNext,

		/// <summary>
		/// Insert the trigger before current CycleState, essentially interrupting the current
		/// CycleState, which will be completed after the triggered CycleState is completed.
		/// </summary>
		Immediately
	}

	/// <summary>
	/// Creates a new CycleState and adds it to the end of the existing cycles.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class Trigger : ICyclesModifier
	{
		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public int CycleCount { get; }

		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public string CycleId { get; }

		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public TriggerPosition TriggerPosition { get; }

		public Trigger(int cycleCount, string cycleId, TriggerPosition triggerPosition = TriggerPosition.AtEnd)
		{
			CycleCount = cycleCount;
			CycleId = cycleId;
			TriggerPosition = triggerPosition;
		}

		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage)
		{
			switch (TriggerPosition)
			{
				case TriggerPosition.AtEnd: return cycles.InsertAtEnd(triggeringCycle, destinationStage, CycleId, CycleCount);
				case TriggerPosition.AtNext: return cycles.InsertAtNext(triggeringCycle, destinationStage, CycleId, CycleCount);
				case TriggerPosition.Immediately: return cycles.InsertImmediately(triggeringCycle, destinationStage, CycleId, CycleCount);
				default: throw new NotSupportedException();
			}
		}

		public IResult ToString(string format) => $"Trigger {CycleCount} {CycleId} {TriggerPosition}".ToSuccess();
	}
}