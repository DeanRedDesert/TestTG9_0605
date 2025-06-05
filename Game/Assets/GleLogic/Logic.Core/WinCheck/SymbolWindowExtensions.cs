using System.Collections.Generic;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// A collection of symbol window helpers.
	/// A symbol window is represented as a collection of mutually exclusive masks.  Each mask represents a different symbol and its index within the list corresponds to the position in a <see cref="SymbolList"/>.
	/// Each bit within each mask represents a cell position in the symbol window and the cells positions are defined in a <see cref="SymbolWindowStructure"/>.
	/// </summary>
	public static class SymbolWindowExtensions
	{
		/// <summary>
		/// This function adds the positions specified in the <param name="positionMask"/> to the mask at index <param name="symbolIndex"/>.
		/// It also ensures that all the masks remain mutually exclusive by removing the additional bits from all other masks in the symbol window.
		/// </summary>
		/// <param name="symbolWindow">The symbol window is a collection of mutually exclusive masks representing the cells in a symbol window.</param>
		/// <param name="symbolIndex">The location of the target mask.</param>
		/// <param name="positionMask">The new positions to apply.</param>
		public static void AddToSymbolPositionMask(this IList<ReadOnlyMask> symbolWindow, int symbolIndex, ReadOnlyMask positionMask)
		{
			var notReplacementSymbolMask = positionMask.Not();

			for (var i = 0; i < symbolWindow.Count; i++)
			{
				var symbolMask = symbolWindow[i];

				if (symbolIndex == i)
				{
					symbolWindow[i] = symbolMask.Or(positionMask);
					continue;
				}

				if (symbolMask == null || symbolMask.IsEmpty)
					continue;

				symbolWindow[i] = symbolMask.And(notReplacementSymbolMask);
			}
		}
	}
}