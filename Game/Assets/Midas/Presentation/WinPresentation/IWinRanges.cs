using Midas.Core.General;

namespace Midas.Presentation.WinPresentation
{
	/// <summary>
	/// Converts win and bet values into a win level to support big win.
	/// </summary>
	public interface IWinRanges
	{
		/// <summary>
		/// Returns the win level associated with the prize information.
		/// </summary>
		/// <param name="winAmount">The win amount to count</param>
		/// <param name="betAmount">The bet amount for this game</param>
		/// <returns>The win level for the prize.</returns>
		int GetWinLevel(Credit winAmount, Credit betAmount);
	}
}