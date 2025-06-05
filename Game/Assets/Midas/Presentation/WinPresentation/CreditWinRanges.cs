using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.General;

namespace Midas.Presentation.WinPresentation
{
	public sealed class CreditWinRanges : IWinRanges
	{
		private readonly Func<bool> isLevelZeroAllowed;

		// ReSharper disable once MemberCanBePrivate.Global Intended to be visible in the status database
		public IReadOnlyList<(int WinLevel, Credit Credit)> CreditRanges { get; }

		public CreditWinRanges(IReadOnlyList<Credit> creditRanges, Func<bool> isLevelZeroAllowed = null)
		{
			this.isLevelZeroAllowed = isLevelZeroAllowed ?? (() => true);
			CreditRanges = creditRanges.Select((c, i) => (i, c)).OrderByDescending(e => e.c).ToList();
		}

		public int GetWinLevel(Credit winAmount, Credit betAmount)
		{
			if (winAmount.IsZero)
				return -1;

			var winLevel = -1;

			foreach (var entry in CreditRanges)
			{
				if (entry.Credit <= winAmount)
				{
					winLevel = entry.WinLevel;
					break;
				}
			}

			if (winLevel == 0 && !isLevelZeroAllowed())
				winLevel = 1;

			return winLevel;
		}
	}
}