using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Midas.Core.General;
using Midas.Presentation.WinPresentation;

namespace Game.GameIdentity.Anz
{
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Intended to be visible in the status database")]
	public sealed class WinRangeBasedWinCountRanges : IWinCountRanges
	{
		public IWinRanges WinRanges { get; }

		public IReadOnlyList<TimeSpan> Durations { get; }

		public IReadOnlyList<TimeSpan> Delays{ get; }

		public WinRangeBasedWinCountRanges(IWinRanges winRanges, IReadOnlyList<TimeSpan> durations, IReadOnlyList<TimeSpan> delays)
		{
			WinRanges = winRanges;
			Durations = durations;
			Delays = delays;
		}

		public (int WinCountLevel, TimeSpan Duration, TimeSpan Delay) GetWinCountLevel(Credit winAmount, Credit betAmount)
		{
			var winLevel = WinRanges.GetWinLevel(winAmount, betAmount);
			return (winLevel, winLevel == -1 ? TimeSpan.Zero : Durations[winLevel], winLevel == -1 ? TimeSpan.Zero : Delays[winLevel]);
		}

		public bool IsSequenceEligible(int sequenceId) => true;
	}
}