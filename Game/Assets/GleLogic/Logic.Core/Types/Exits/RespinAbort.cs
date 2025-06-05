using Logic.Core.Engine;
using Logic.Core.Utility;

namespace Logic.Core.Types.Exits
{
	/// <summary>
	/// Stops the current respins.
	/// </summary>
	// ReSharper disable once UnusedType.Global - Logic class
	public sealed class RespinAbort : ICyclesModifier
	{
		public Cycles ApplyExit(Cycles cycles, CycleState triggeringCycle, string destinationStage)
		{
			var curr = cycles.Current;

			return cycles.ReplaceCurrent(curr.CompletedCycles, curr.CompletedCycles);
		}

		public IResult ToString(string format) => "Abort Respin".ToSuccess();
	}
}