using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Adds cycles to the running respins to ensure there are ResetAmount cycles to run.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class RespinReset : ICyclesModifier
	{
		// ReSharper disable once MemberCanBePrivate.Global - Used by presentation
		public int ResetAmount { get; }

		public RespinReset(int resetAmount) => ResetAmount = resetAmount;

		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage)
		{
			var curr = cycles.Current;
			return cycles.ReplaceCurrent(curr.CompletedCycles + ResetAmount, curr.CompletedCycles);
		}

		public IResult ToString(string format) => $"Reset Respin to {ResetAmount}".ToSuccess();
	}
}