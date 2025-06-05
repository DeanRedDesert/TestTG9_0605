using System.Collections.Generic;

namespace Logic.Core.Engine
{
	/// <summary>
	/// Interface to access types in dynamically loaded dlls.
	/// </summary>
	// ReSharper disable UnusedMember.Global - Used in unity
	public interface IPid
	{
		/// <summary>
		/// Get the games per win.
		/// </summary>
		double GetGamesPerWin(Inputs inputs);

		/// <summary>
		/// Get the largest prizes.
		/// </summary>
		IReadOnlyList<(string Prize, int Odds)> GetLargestPrizes(Inputs inputs);

		/// <summary>
		/// Get the smallest prizes.
		/// </summary>
		IReadOnlyList<(string Prize, int Odds)> GetSmallestPrizes(Inputs inputs);
	}
}