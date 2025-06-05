using System;
using Midas.Core.General;

namespace Midas.Presentation.WinPresentation
{
	/// <summary>
	/// Converts win and bet values into a win count level and duration to support win counts for different game identities.
	/// </summary>
	public interface IWinCountRanges
	{
		(int WinCountLevel, TimeSpan Duration, TimeSpan Delay) GetWinCountLevel(Credit winAmount, Credit betAmount);

		bool IsSequenceEligible(int sequenceId);
	}
}