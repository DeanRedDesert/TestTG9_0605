using System.Collections.Generic;

namespace Logic.Core.Types.WeightScaling
{
	/// <summary>
	/// A result for applying symbol mask changes to a <see cref="SymbolWindowResult"/> object.
	/// The final result and a dictionary of symbol index and replacement mask pairs.
	/// </summary>
	public sealed class ApplyReplacementsResult
	{
		/// <summary>
		/// The final symbol window result.
		/// </summary>
		public SymbolWindowResult SymbolWindow { get; }

		/// <summary>
		/// A dictionary of entries with a key of symbol index and the value a mask of cells holding that symbol as a replacement.
		/// </summary>
		public IReadOnlyDictionary<int, ReadOnlyMask> Masks { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="symbolWindow">The final symbol window result.</param>
		/// <param name="masks">A dictionary of entries with a key of symbol index and the value a mask of cells holding that symbol as a replacement.</param>
		public ApplyReplacementsResult(SymbolWindowResult symbolWindow, IReadOnlyDictionary<int, ReadOnlyMask> masks)
		{
			SymbolWindow = symbolWindow;
			Masks = masks;
		}
	}
}