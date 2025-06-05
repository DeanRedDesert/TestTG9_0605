using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Interrupts the current Cycle to play respins.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class Respin : ICyclesModifier
	{
		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public int Cycles { get; }

		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public string CycleId { get; }

		public Respin(int cycles, string cycleId)
		{
			Cycles = cycles;
			CycleId = cycleId;
		}

		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage) => cycles.InsertImmediately(triggeringCycle, destinationStage, CycleId, Cycles);

		public IResult ToString(string format) => $"Respin {Cycles} {CycleId}".ToSuccess();
	}
}