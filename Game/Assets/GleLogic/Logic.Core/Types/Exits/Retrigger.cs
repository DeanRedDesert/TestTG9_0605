using System;
using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Looks for a pre-existing CycleStage with a matching CycleId and increases its TotalCycles.
	/// If it doesn't already exist a new CycleStage is created and added to the end of the existing cycles.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class Retrigger : ICyclesModifier
	{
		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public int AdditionalCycleCount { get; }

		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public string CycleId { get; }

		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public TriggerPosition TriggerPosition { get; }

		public Retrigger(int additionalCycleCount, string cycleId, TriggerPosition triggerPosition = TriggerPosition.AtEnd)
		{
			AdditionalCycleCount = additionalCycleCount;
			CycleId = cycleId;
			TriggerPosition = triggerPosition;
		}

		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage)
		{
			// Need to start looking at the current cycle.
			// We don't want to apply a retrigger to a cycle state that is already finished.
			var startIndex = cycles.IndexOf(cycles.Current);

			for (var i = startIndex; i < cycles.Count; i++)
			{
				var cycle = cycles[i];
				if (cycle.Stage == destinationStage && cycle.CycleId == CycleId)
				{
					i++;

					while (i < cycles.Count)
					{
						cycle = cycles[i];

						if (cycle.Stage != destinationStage || cycle.CycleId != CycleId)
							break;

						i++;
					}

					return cycles.InsertAtIndex(i, triggeringCycle, destinationStage, CycleId, cycle.TotalCycles);
				}
			}

			// Existing cycle state not found.
			switch (TriggerPosition)
			{
				case TriggerPosition.AtEnd: return cycles.InsertAtEnd(triggeringCycle, destinationStage, CycleId, AdditionalCycleCount);
				case TriggerPosition.AtNext: return cycles.InsertAtNext(triggeringCycle, destinationStage, CycleId, AdditionalCycleCount);
				case TriggerPosition.Immediately: return cycles.InsertImmediately(triggeringCycle, destinationStage, CycleId, AdditionalCycleCount);
				default: throw new NotSupportedException();
			}
		}

		public IResult ToString(string format) => $"Retrigger {AdditionalCycleCount} {CycleId} {TriggerPosition}".ToSuccess();
	}
}