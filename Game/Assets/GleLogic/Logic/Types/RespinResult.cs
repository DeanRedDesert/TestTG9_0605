using System.Collections.Generic;
using Logic.Core.Types;
using Logic.Core.Types.Exits;
using Logic.Core.Utility;

namespace Logic.Types
{
	public sealed class RespinResult
	{
		/// <summary>
		/// Gets the desired exit data.
		/// </summary>
		[NullOut]
		public ICyclesModifier Trigger { get; }

		public RespinState RespinState { get; }

		public IReadOnlyList<CellPrizeResult> Prizes { get; }

		public IReadOnlyList<CellPrizeResult> BonusPrizes { get; }

		public IReadOnlyList<ProgressivePrizeResult> ProgressivePrizes { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public RespinResult(ICyclesModifier trigger, RespinState respinState, IReadOnlyList<CellPrizeResult> prizes, IReadOnlyList<CellPrizeResult> bonusPrizes, IReadOnlyList<ProgressivePrizeResult> progressivePrizes)
		{
			Trigger = trigger;
			RespinState = respinState;
			Prizes = prizes;
			BonusPrizes = bonusPrizes;
			ProgressivePrizes = progressivePrizes;
		}
	}
}