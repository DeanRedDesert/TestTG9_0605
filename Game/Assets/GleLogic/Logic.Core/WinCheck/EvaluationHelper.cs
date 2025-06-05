using System.Collections.Generic;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	public static class EvaluationHelper
	{
		/// <summary>
		/// Checks a prize to see if it meets the symbol count restrictions and returns if the prize should be added as a win.
		/// </summary>
		/// <param name="symbolWindow">The symbol window containing the win.</param>
		/// <param name="prize">The prize definition.</param>
		/// <param name="winMaskCells">The cells involved in the win.</param>
		public static bool CheckAddPrize(this SymbolWindowResult symbolWindow, MaskPrize prize, IReadOnlyList<Cell> winMaskCells)
		{
			var winMask = symbolWindow.SymbolWindowStructure.CellsToMask(winMaskCells);

			for (var i = 0; i < prize.Symbols.Count; i++)
			{
				var requiredCount = prize.RequiredSymbolCounts[i];

				if (requiredCount == 0)
					continue;

				var symbolMask = symbolWindow.SymbolMasks[prize.Symbols[i]];

				if (symbolMask.AndTrueCount(winMask) < requiredCount)
					return false;
			}

			return true;
		}
	}
}