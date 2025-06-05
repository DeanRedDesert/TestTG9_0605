using System.Collections.Generic;
using Logic.Core.Types;

namespace Logic.Core.WinCheck
{
	/// <summary>
	/// The same as IStrip but with symbol list support for the newer style win check.
	/// </summary>
	public interface ISymbolListStrip : IStrip
	{
		/// <summary>
		/// Gets the (SymbolList) index of the symbol at the index specified.
		/// </summary>
		int GetSymbolIndex(ulong index);

		/// <summary>
		/// The symbol list associated with this strip.
		/// </summary>
		SymbolList GetSymbolList();

		/// <summary>
		/// Get the indexes of all the occurrences of the given symbol index in the strip.
		/// </summary>
		IReadOnlyList<int> GetSymbolPositions(int symbolIndex);
	}
}