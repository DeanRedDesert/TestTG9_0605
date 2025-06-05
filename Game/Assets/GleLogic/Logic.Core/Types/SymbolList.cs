using System.Collections;
using System.Collections.Generic;

namespace Logic.Core.Types
{
	public sealed class SymbolList : IReadOnlyList<string>
	{
		private readonly IReadOnlyList<string> symbols;
		private readonly Dictionary<string, int> symbolIndexes = new Dictionary<string, int>();
		private readonly int id;

		public SymbolList(IReadOnlyList<string> symbols)
		{
			this.symbols = symbols;

			for (var i = 0; i < symbols.Count; i++)
				symbolIndexes.Add(symbols[i], i);

			id = string.Join(" ", symbols).GetHashCode();
		}

		/// <summary>
		/// A hash code that can be used to compare two symbol lists.
		/// </summary>
		public int GetId() => id;

		/// <summary>
		/// Gets the index of a symbol.
		/// This is a dictionary lookup so should be fairly quick.
		/// Throws an exception if the symbol is not in this symbol list.
		/// </summary>
		public int IndexOf(string symbol) => symbolIndexes[symbol];

		#region IReadOnlyList<string> Implementation

		public int Count => symbols.Count;
		public string this[int index] => symbols[index];
		public IEnumerator<string> GetEnumerator() => symbols.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion
	}
}
