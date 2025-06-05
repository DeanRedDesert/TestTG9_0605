using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.Utility;

namespace Logic.Types
{
	public sealed class RespinDetectorResult
	{
		/// <summary>
		/// Gets the desired exit data.
		/// </summary>
		[NullOut]
		public ICyclesModifier Trigger { get; }

		[NullOut]
		public RespinState RespinState { get; }

		public IReadOnlyList<CellPrizeResult> Prizes { get; }

		public IReadOnlyList<CellPrizeResult> BonusPrizes { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public RespinDetectorResult(ICyclesModifier trigger, RespinState respinState, IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<CellPrizeResult> bonusPrizes)
		{
			Trigger = trigger;
			RespinState = respinState;
			Prizes = prizes;
			BonusPrizes = bonusPrizes;
		}
	}
}