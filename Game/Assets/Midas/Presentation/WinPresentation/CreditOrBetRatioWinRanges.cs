using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Midas.Core.General;

namespace Midas.Presentation.WinPresentation
{
	/// <summary>
	/// Win ranges that use a credit range if bet is less than or equal to CreditBetThreshold, otherwise uses bet ratio.
	/// </summary>
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public sealed class CreditOrBetRatioWinRanges : IWinRanges
	{
		public Money CreditBetThreshold { get; }
		public CreditWinRanges CreditWinRanges { get; }
		public BetRatioWinRanges BetRatioWinRanges { get; }

		public CreditOrBetRatioWinRanges(Money creditBetThreshold, IReadOnlyList<Credit> creditRanges, IReadOnlyList<float> betRatios, Func<bool> isLevelZeroAllowed = null)
		{
			CreditBetThreshold = creditBetThreshold;
			CreditWinRanges = new CreditWinRanges(creditRanges, isLevelZeroAllowed);
			BetRatioWinRanges = new BetRatioWinRanges(betRatios, isLevelZeroAllowed);
		}

		public int GetWinLevel(Credit winAmount, Credit betAmount)
		{
			return betAmount.ToMoney() <= CreditBetThreshold
				? CreditWinRanges.GetWinLevel(winAmount, betAmount)
				: BetRatioWinRanges.GetWinLevel(winAmount, betAmount);
		}
	}
}