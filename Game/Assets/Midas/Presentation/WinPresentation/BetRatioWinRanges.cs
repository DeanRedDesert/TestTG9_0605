using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core.General;

namespace Midas.Presentation.WinPresentation
{
	public sealed class BetRatioWinRanges : IWinRanges
	{
		private readonly Func<bool> isLevelZeroAllowed;

		// ReSharper disable once MemberCanBePrivate.Global intended to make it visible in the status database
		public IReadOnlyList<(int WinLevel, float BetRatio)> BetRatios { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="betRatios">The list of bet ratios</param>
		/// <param name="isLevelZeroAllowed">An optional method to enable or disable win level 0 at runtime.</param>
		public BetRatioWinRanges(IReadOnlyList<float> betRatios, Func<bool> isLevelZeroAllowed = null)
		{
			DoWinRangesValueCheck(betRatios);
			BetRatios = betRatios.Select((r, i) => (i, r)).OrderByDescending(e => e.r).ToList();
			this.isLevelZeroAllowed = isLevelZeroAllowed ?? (() => true);
		}

		public int GetWinLevel(Credit winAmount, Credit betAmount)
		{
			var winLevel = -1;
			var betRatio = winAmount.Value.Numerator > 0 && betAmount.Value.Numerator > 0
				? (float)(winAmount.Value / betAmount.Value).ToDouble()
				: -1f;

			if (betRatio > 0)
			{
				foreach (var entry in BetRatios)
				{
					if (entry.BetRatio <= betRatio)
					{
						winLevel = entry.WinLevel;
						break;
					}
				}
			}

			if (winLevel == 0 && !isLevelZeroAllowed())
				winLevel = 1;

			return winLevel;
		}

		private static void DoWinRangesValueCheck(IReadOnlyList<float> betRatios)
		{
			// Do a sanity check

			var prev = betRatios[0];
			for (var i = 1; i < betRatios.Count; i++)
			{
				var item = betRatios[i];
				if (item <= prev)
					throw new Exception("WinLevelTable: Integrity error, ranges must be strictly ordered and must not overlap!");

				prev = item;
			}
		}
	}
}