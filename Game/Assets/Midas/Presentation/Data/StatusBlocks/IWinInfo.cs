using System.Collections.Generic;
using Midas.Core.General;

namespace Midas.Presentation.Data.StatusBlocks
{
	public interface IWinInfo
	{
		/// <summary>
		/// Get the name of the prize for this win.
		/// </summary>
		string PrizeName { get; }

		/// <summary>
		/// Get the pattern name that this win is on.
		/// </summary>
		string PatternName { get; }

		/// <summary>
		/// Get the credit value for this win.
		/// </summary>
		Credit Value { get; }

		/// <summary>
		/// Get the winning positions for this prize.
		/// </summary>
		public IReadOnlyList<(int Column, int Row)> WinningPositions { get; }

		/// <summary>
		/// Get the line number for the win, or null if it's not a line win.
		/// </summary>
		public int? LineNumber { get; }

		/// <summary>
		/// Get the line pattern if this is a line win, otherwise null.
		/// </summary>
		public IReadOnlyList<(int Column, int Row)> LinePattern { get; }
	}

	public static class WinInfoExtensions
	{
		[Expression("DetailedWinInfo")] public static int WinningPositionsCount
		{
			get
			{
				return StatusDatabase.DetailedWinPresStatus.HighlightedWin == null ? 0 : StatusDatabase.DetailedWinPresStatus.HighlightedWin.WinningPositions.Count;
			}
		}

		public static bool IsLineWin(this IWinInfo winInfo) => winInfo.LineNumber.HasValue;
	}
}