using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Stops the current CycleStage and inserts one cycle of the respin award stage.
	/// Retains the CycleId of the current spins.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class RespinAward : ICyclesModifier
	{
		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage)
		{
			var curr = cycles.Current;

			return cycles
				.ReplaceCurrent(curr.CompletedCycles, curr.CompletedCycles)
				.InsertAtNext(triggeringCycle, destinationStage, curr.CycleId, 1);
		}

		public IResult ToString(string format) => "Award Respin".ToSuccess();
	}
}